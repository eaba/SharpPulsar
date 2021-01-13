﻿using Akka.Actor;
using Akka.Event;
using SharpPulsar.Configuration;
using SharpPulsar.Auth;
using SharpPulsar.Interfaces;
using SharpPulsar.Model;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using SharpPulsar.SocketImpl;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using BAMCIS.Util.Concurrent;
using System.Collections.Concurrent;
using SharpPulsar.Exceptions;
using Akka.Util.Internal;
using SharpPulsar.Messages;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Akka.Network;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Messages.Consumer;

namespace SharpPulsar
{
    public sealed class ClientCnx: ReceiveActor
    {
        private readonly SocketClient _socketClient;
		private readonly IAuthentication _authentication;
		private State _state;

		private readonly Dictionary<long, (ReadOnlySequence<byte> Message, IActorRef Requester)> _pendingRequests = new Dictionary<long, (ReadOnlySequence<byte> Message, IActorRef Requester)>();
		// LookupRequests that waiting in client side.
		private readonly LinkedList<KeyValuePair<long, KeyValuePair<byte[], LookupDataResult>>> _waitingLookupRequests;

		private readonly Dictionary<long, IActorRef> _producers = new Dictionary<long, IActorRef>();
		
		private readonly Dictionary<long, IActorRef> _consumers = new Dictionary<long, IActorRef>();
		private readonly Dictionary<long, IActorRef> _transactionMetaStoreHandlers = new Dictionary<long, IActorRef>();

		private readonly ConcurrentQueue<RequestTime> _requestTimeoutQueue = new ConcurrentQueue<RequestTime>();

		private volatile int _numberOfRejectRequests = 0;

		private static int _maxMessageSize = Commands.DefaultMaxMessageSize;

		private readonly int _maxNumberOfRejectedRequestPerConnection;
		private readonly int _rejectedRequestResetTimeSec = 60;
		private int _protocolVersion;
		private readonly long _operationTimeoutMs;

		private readonly ILoggingAdapter _log;
		private readonly IActorContext _actorContext;

		private string _proxyToTargetBrokerAddress;
		
		private string _remoteHostName;
		private bool _isTlsHostnameVerificationEnable;

		private static readonly TlsHostnameVerifier _hostnameVerifier = new TlsHostnameVerifier();

		private ICancelable _timeoutTask;

		// Added for mutual authentication.
		private IAuthenticationDataProvider _authenticationDataProvider;
		private TransactionBufferHandler _transactionBufferHandler;
		public ClientCnx(ClientConfigurationData conf, Uri endPoint, string targetBroker = "") : this(conf, endPoint, Commands.CurrentProtocolVersion, targetBroker)
		{
		}

		public ClientCnx(ClientConfigurationData conf, Uri endPoint, int protocolVersion, string targetBroker = "")
		{
			_actorContext = Context;
			_proxyToTargetBrokerAddress = targetBroker;
			_socketClient = (SocketClient)SocketClient.CreateClient(conf, endPoint, endPoint.Host, Context.System.Log);
			_socketClient.OnConnect += OnConnected;
			_socketClient.OnDisconnect += OnDisconnected;
			_socketClient.ReceiveMessageObservable.Subscribe(a => OnCommandReceived(a));
			Condition.CheckArgument(conf.MaxLookupRequest > conf.ConcurrentLookupRequest);
			_waitingLookupRequests = new LinkedList<KeyValuePair<long, KeyValuePair<byte[], LookupDataResult>>>();
			_authentication = conf.Authentication;
			_maxNumberOfRejectedRequestPerConnection = conf.MaxNumberOfRejectedRequestPerConnection;
			_operationTimeoutMs = conf.OperationTimeoutMs;
			_state = State.None;
			_isTlsHostnameVerificationEnable = conf.TlsHostnameVerificationEnable;
			_protocolVersion = protocolVersion;
			_socketClient.Connect();
		}
		private void OnConnected()
		{
			_timeoutTask = _actorContext.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(_operationTimeoutMs)), CheckRequestTimeout);

			if (string.IsNullOrWhiteSpace(_proxyToTargetBrokerAddress))
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"{_remoteHostName} Connected to broker");
				}
			}
			else
			{
				_log.Info($"{_remoteHostName} Connected through proxy to target broker at {_proxyToTargetBrokerAddress}");
			}
			// Send CONNECT command
			_socketClient.SendMessageAsync(NewConnectCommand()).ContinueWith(task => 
			{
				if (task.IsCompletedSuccessfully)
				{
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"Complete: {task.IsCompletedSuccessfully}");
					}
					_state = State.SentConnectFrame;
				}
				else
				{
					_log.Warning($"Error during handshake: {task.Exception}");
					//ctx.close();
				}

			});
		}
		private void OnDisconnected()
		{
			_log.Info($"{_remoteHostName} Disconnected");
			PulsarClientException e = new PulsarClientException("Disconnected from server at " + _remoteHostName);


			// Notify all attached producers/consumers so they have a chance to reconnect
			_producers.ForEach(p => p.Value.Tell(new ConnectionClosed(this)));
			_consumers.ForEach(c => c.Value.Tell(new ConnectionClosed(this)));
			_transactionMetaStoreHandlers.ForEach(t => t.Value.Tell(new ConnectionClosed(this)));

			_pendingRequests.Clear();
			_waitingLookupRequests.Clear();

			_producers.Clear();
			_consumers.Clear();

			_timeoutTask.Cancel(true);
		}
		private void ExceptionCaught(Exception cause)
		{
			if (_state != State.Failed)
			{
				// No need to report stack trace for known exceptions that happen in disconnections
				_log.Warning("[{}] Got exception {}", RemoteAddress, ClientCnx.IsKnownException(cause) ? cause : ExceptionUtils.getStackTrace(cause));
				_state = State.Failed;
			}
			else
			{
				// At default info level, suppress all subsequent exceptions that are thrown when the connection has already
				// failed
				if (_log.DebugEnabled)
				{
					_log.debug("[{}] Got exception: {}", RemoteAddress, cause.Message, cause);
				}
			}

			_socketClient.Dispose();
		}
		protected override void PostStop()
        {
			_timeoutTask?.Cancel();
            base.PostStop();
        }
		private void HandleConnected(CommandConnected connected)
		{

			if (_isTlsHostnameVerificationEnable && !string.ReferenceEquals(_remoteHostName, null) && !VerifyTlsHostName(_remoteHostName, Ctx))
			{
				// close the connection if host-verification failed with the broker
				_log.Warning($"Failed to verify hostname of {_remoteHostName}");
				_socketClient.Dispose();
				return;
			}

			Condition.CheckArgument(_state == State.SentConnectFrame || _state == State.Connecting);
			if (connected.MaxMessageSize > 0)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"{connected.MaxMessageSize} Connection has max message size setting");
				}
				_maxMessageSize = connected.MaxMessageSize;
				//Ctx.pipeline().replace("frameDecoder", "newFrameDecoder", new LengthFieldBasedFrameDecoder(connected.MaxMessageSize + Commands.MessageSizeFramePadding, 0, 4, 0, 4));
			}
			if (_log.IsDebugEnabled)
			{
				_log.Debug("Connection is ready");
			}
			// set remote protocol version to the correct version before we complete the connection future
			_protocolVersion = connected.ProtocolVersion;
			_state = State.Ready;
		}

		private void HandleAuthChallenge(CommandAuthChallenge authChallenge)
		{
			// mutual authn. If auth not complete, continue auth; if auth complete, complete connectionFuture.
			try
			{
				var assemblyName = Assembly.GetCallingAssembly().GetName();
				var authData = _authenticationDataProvider.Authenticate(new Auth.AuthData(authChallenge.Challenge.auth_data));
				var auth = new Protocol.Proto.AuthData { auth_data = ((byte[])(object)authData.Bytes) };
				var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);
				var request = Commands.NewAuthResponse(_authentication.AuthMethodName, auth, _protocolVersion, clientVersion);

				if (_log.IsDebugEnabled)
				{
					_log.Debug($"Mutual auth {_authentication.AuthMethodName}");
				}
				_socketClient.SendMessageAsync(request).ContinueWith(task => {
                    if (task.IsFaulted)
                    {
						_log.Warning($"Failed to send request for mutual auth to broker: {task.Exception}");
					}
				});
				if (_state == State.SentConnectFrame)
				{
					_state = State.Connecting;
				}
			}
			catch (Exception e)
			{
				_log.Error($"Error mutual verify: {e}");
			}
		}

		private void HandleSendReceipt(CommandSendReceipt sendReceipt)
		{
			Condition.CheckArgument(_state == State.Ready);

			long producerId = (long)sendReceipt.ProducerId;
			long sequenceId = (long)sendReceipt.SequenceId;
			long highestSequenceId = (long)sendReceipt.HighestSequenceId;
			long ledgerId = -1;
			long entryId = -1;
			if (sendReceipt.MessageId != null)
			{
				ledgerId = (long)sendReceipt.MessageId.ledgerId;
				entryId = (long)sendReceipt.MessageId.entryId;
			}

			if (ledgerId == -1 && entryId == -1)
			{
				_log.Warning($"Message has been dropped for non-persistent topic producer-id {producerId}-{sequenceId}");
			}

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Got receipt for producer: {producerId} -- msg: {sequenceId} -- id: {ledgerId}:{entryId}");
			}

			_producers[producerId].Tell(new AckReceived(this, sequenceId, highestSequenceId, ledgerId, entryId));
		}

		private void HandleMessage(CommandMessage msg, ReadOnlySequence<byte> frame, uint commandSize)
		{
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received a message from the server: {msg}");
			}
			var message = new MessageReceived((long)msg.ConsumerId, new MessageIdReceived((long)msg.MessageId.ledgerId, (long)msg.MessageId.entryId, msg.MessageId.BatchIndex, msg.MessageId.Partition, msg.MessageId.AckSets), frame.Slice(commandSize + 4), (int)msg.RedeliveryCount, this);
			if (_consumers.TryGetValue((long)msg.ConsumerId, out var consumer))
			{
				consumer.Tell(message);
			}
		}

		private void HandleActiveConsumerChange(CommandActiveConsumerChange change)
		{
			Condition.CheckArgument(_state == State.Ready);

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received a consumer group change message from the server : {change}");
			}
			if (_consumers.TryGetValue((long)change.ConsumerId, out var consumer))
			{
				consumer.Tell(new ActiveConsumerChanged(change.IsActive));
			}
		}

		private void HandleSuccess(CommandSuccess success)
		{
			Condition.CheckArgument(_state == State.Ready);

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received success response from server: {success.RequestId}");
			}
			long requestId = (long)success.RequestId;
			var req = _pendingRequests.Remove(requestId);
			if (!req)
			{
				_log.Warning($"Received unknown request id from server: {success.RequestId}");
			}
		}

		private void HandleGetLastMessageIdSuccess(CommandGetLastMessageIdResponse success)
		{
			Condition.CheckArgument(_state == State.Ready);

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Received success GetLastMessageId response from server: {success.RequestId}");
			}
			long requestId = (long)success.RequestId;
			if (_pendingRequests.TryGetValue(requestId, out var request))
			{
				var consumer = request.Requester;
				var req = _pendingRequests.Remove(requestId);
				var lid = success.LastMessageId;
				consumer.Tell(new LastMessageIdResponse((long)lid.ledgerId, (long)lid.entryId, lid.Partition, lid.BatchIndex, lid.BatchSize, lid.AckSets));
			}
			else
			{
				_log.Warning($"Received unknown request id from server: {requestId}");
			}
		}

		private void HandleProducerSuccess(CommandProducerSuccess success)
		{
			checkArgument(_state == State.Ready);

			if (_log.DebugEnabled)
			{
				_log.debug("{} Received producer success response from server: {} - producer-name: {}", Ctx.channel(), success.RequestId, success.ProducerName);
			}
			long requestId = success.RequestId;
			CompletableFuture<ProducerResponse> requestFuture = (CompletableFuture<ProducerResponse>)_pendingRequests.Remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.complete(new ProducerResponse(success.ProducerName, success.LastSequenceId, success.SchemaVersion.toByteArray()));
			}
			else
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), success.RequestId);
			}
		}

		private void HandleLookupResponse(CommandLookupTopicResponse lookupResult)
		{
			if (_log.DebugEnabled)
			{
				_log.debug("Received Broker lookup response: {}", lookupResult.Response);
			}

			long requestId = lookupResult.RequestId;
			CompletableFuture<LookupDataResult> requestFuture = GetAndRemovePendingLookupRequest(requestId);

			if (requestFuture != null)
			{
				if (requestFuture.CompletedExceptionally)
				{
					if (_log.DebugEnabled)
					{
						_log.debug("{} Request {} already timed-out", Ctx.channel(), lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!lookupResult.HasResponse() || CommandLookupTopicResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult.HasError())
					{
						CheckServerError(lookupResult.Error, lookupResult.Message);
						requestFuture.completeExceptionally(GetPulsarClientException(lookupResult.Error, lookupResult.Message));
					}
					else
					{
						requestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
					}
				}
				else
				{
					requestFuture.complete(new LookupDataResult(lookupResult));
				}
			}
			else
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), lookupResult.RequestId);
			}
		}

		private void HandlePartitionResponse(CommandPartitionedTopicMetadataResponse lookupResult)
		{
			if (_log.DebugEnabled)
			{
				_log.debug("Received Broker Partition response: {}", lookupResult.Partitions);
			}

			long requestId = lookupResult.RequestId;
			CompletableFuture<LookupDataResult> requestFuture = GetAndRemovePendingLookupRequest(requestId);

			if (requestFuture != null)
			{
				if (requestFuture.CompletedExceptionally)
				{
					if (_log.DebugEnabled)
					{
						_log.debug("{} Request {} already timed-out", Ctx.channel(), lookupResult.RequestId);
					}
					return;
				}
				// Complete future with exception if : Result.response=fail/null
				if (!lookupResult.HasResponse() || CommandPartitionedTopicMetadataResponse.LookupType.Failed.Equals(lookupResult.Response))
				{
					if (lookupResult.HasError())
					{
						CheckServerError(lookupResult.Error, lookupResult.Message);
						requestFuture.completeExceptionally(GetPulsarClientException(lookupResult.Error, lookupResult.Message));
					}
					else
					{
						requestFuture.completeExceptionally(new PulsarClientException.LookupException("Empty lookup response"));
					}
				}
				else
				{
					// return LookupDataResult when Result.response = success/redirect
					requestFuture.complete(new LookupDataResult(lookupResult.Partitions));
				}
			}
			else
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), lookupResult.RequestId);
			}
		}

		private void HandleReachedEndOfTopic(CommandReachedEndOfTopic commandReachedEndOfTopic)
		{
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final long consumerId = commandReachedEndOfTopic.getConsumerId();
			long consumerId = commandReachedEndOfTopic.ConsumerId;

			_log.info("[{}] Broker notification reached the end of topic: {}", RemoteAddress, consumerId);

			//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
			//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> consumer = _consumers.Get(consumerId);
			if (consumer != null)
			{
				consumer.SetTerminated();
			}
		}

		// caller of this method needs to be protected under pendingLookupRequestSemaphore
		private void AddPendingLookupRequests(long requestId, CompletableFuture<LookupDataResult> future)
		{
			_pendingRequests.Put(requestId, future);
			_eventLoopGroup.schedule(() =>
			{
				if (!future.Done)
				{
					future.completeExceptionally(new PulsarClientException.TimeoutException(requestId + " lookup request timedout after ms " + _operationTimeoutMs));
				}
			}, _operationTimeoutMs, TimeUnit.MILLISECONDS);
		}

		private CompletableFuture<LookupDataResult> GetAndRemovePendingLookupRequest(long requestId)
		{
			CompletableFuture<LookupDataResult> result = (CompletableFuture<LookupDataResult>)_pendingRequests.Remove(requestId);
			if (result != null)
			{
				Pair<long, Pair<ByteBuf, CompletableFuture<LookupDataResult>>> firstOneWaiting = _waitingLookupRequests.RemoveFirst();
				if (firstOneWaiting != null)
				{
					_maxLookupRequestSemaphore.release();
					// schedule a new lookup in.
					_eventLoopGroup.submit(() =>
					{
						long newId = firstOneWaiting.Left;
						CompletableFuture<LookupDataResult> newFuture = firstOneWaiting.Right.Right;
						AddPendingLookupRequests(newId, newFuture);
						Ctx.writeAndFlush(firstOneWaiting.Right.Left).addListener(writeFuture =>
						{
							if (!writeFuture.Success)
							{
								_log.warn("{} Failed to send request {} to broker: {}", Ctx.channel(), newId, writeFuture.cause().Message);
								GetAndRemovePendingLookupRequest(newId);
								newFuture.completeExceptionally(writeFuture.cause());
							}
						});
					});
				}
				else
				{
					_pendingLookupRequestSemaphore.release();
				}
			}
			return result;
		}

		private void HandleSendError(CommandSendError sendError)
		{
			_log.warn("{} Received send error from server: {} : {}", Ctx.channel(), sendError.Error, sendError.Message);

			long producerId = sendError.ProducerId;
			long sequenceId = sendError.SequenceId;

			switch (sendError.Error)
			{
				case ChecksumError:
					_producers.Get(producerId).recoverChecksumError(this, sequenceId);
					break;

				case TopicTerminatedError:
					_producers.Get(producerId).terminated(this);
					break;

				default:
					// By default, for transient error, let the reconnection logic
					// to take place and re-establish the produce again
					Ctx.close();
					break;
			}
		}

		private void HandleError(CommandError error)
		{
			checkArgument(_state == State.SentConnectFrame || _state == State.Ready);

			_log.warn("{} Received error from server: {}", Ctx.channel(), error.Message);
			long requestId = error.RequestId;
			if (error.Error == ServerError.ProducerBlockedQuotaExceededError)
			{
				_log.warn("{} Producer creation has been blocked because backlog quota exceeded for producer topic", Ctx.channel());
			}
			if (error.Error == ServerError.AuthenticationError)
			{
				_connectionFuture.completeExceptionally(new PulsarClientException.AuthenticationException(error.Message));
				_log.error("{} Failed to authenticate the client", Ctx.channel());
			}
			//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
			//ORIGINAL LINE: java.util.concurrent.CompletableFuture<?> requestFuture = pendingRequests.remove(requestId);
			CompletableFuture<object> requestFuture = _pendingRequests.Remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.completeExceptionally(GetPulsarClientException(error.Error, error.Message));
			}
			else
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), error.RequestId);
			}
		}

		private void HandleCloseProducer(CommandCloseProducer closeProducer)
		{
			_log.info("[{}] Broker notification of Closed producer: {}", RemoteAddress, closeProducer.ProducerId);
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final long producerId = closeProducer.getProducerId();
			long producerId = closeProducer.ProducerId;
			//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
			//ORIGINAL LINE: ProducerImpl<?> producer = producers.get(producerId);
			ProducerImpl<object> producer = _producers.Get(producerId);
			if (producer != null)
			{
				producer.ConnectionClosed(this);
			}
			else
			{
				_log.warn("Producer with id {} not found while closing producer ", producerId);
			}
		}

		private void HandleCloseConsumer(CommandCloseConsumer closeConsumer)
		{
			_log.info("[{}] Broker notification of Closed consumer: {}", RemoteAddress, closeConsumer.ConsumerId);
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final long consumerId = closeConsumer.getConsumerId();
			long consumerId = closeConsumer.ConsumerId;
			//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
			//ORIGINAL LINE: ConsumerImpl<?> consumer = consumers.get(consumerId);
			ConsumerImpl<object> consumer = _consumers.Get(consumerId);
			if (consumer != null)
			{
				consumer.ConnectionClosed(this);
			}
			else
			{
				_log.warn("Consumer with id {} not found while closing consumer ", consumerId);
			}
		}

		private bool HandshakeCompleted
		{
			get
			{
				return _state == State.Ready;
			}
		}

		public virtual CompletableFuture<LookupDataResult> NewLookup(ByteBuf request, long requestId)
		{
			CompletableFuture<LookupDataResult> future = new CompletableFuture<LookupDataResult>();

			if (_pendingLookupRequestSemaphore.tryAcquire())
			{
				AddPendingLookupRequests(requestId, future);
				Ctx.writeAndFlush(request).addListener(writeFuture =>
				{
					if (!writeFuture.Success)
					{
						_log.warn("{} Failed to send request {} to broker: {}", Ctx.channel(), requestId, writeFuture.cause().Message);
						GetAndRemovePendingLookupRequest(requestId);
						future.completeExceptionally(writeFuture.cause());
					}
				});
			}
			else
			{
				if (_log.DebugEnabled)
				{
					_log.debug("{} Failed to add lookup-request into pending queue", requestId);
				}

				if (_maxLookupRequestSemaphore.tryAcquire())
				{
					_waitingLookupRequests.AddLast(Pair.of(requestId, Pair.of(request, future)));
				}
				else
				{
					if (_log.DebugEnabled)
					{
						_log.debug("{} Failed to add lookup-request into waiting queue", requestId);
					}
					future.completeExceptionally(new PulsarClientException.TooManyRequestsException(string.Format("Requests number out of config: There are {{{0}}} lookup requests outstanding and {{{1}}} requests pending.", _pendingLookupRequestSemaphore.availablePermits(), _waitingLookupRequests.Count)));
				}
			}
			return future;
		}

		public virtual CompletableFuture<IList<string>> NewGetTopicsOfNamespace(ByteBuf request, long requestId)
		{
			return SendRequestAndHandleTimeout(request, requestId, RequestType.GetTopics);
		}

		private void HandleGetTopicsOfNamespaceSuccess(CommandGetTopicsOfNamespaceResponse success)
		{
			checkArgument(_state == State.Ready);

			long requestId = success.RequestId;
			IList<string> topics = success.TopicsList;

			if (_log.DebugEnabled)
			{
				_log.debug("{} Received get topics of namespace success response from server: {} - topics.size: {}", Ctx.channel(), success.RequestId, topics.Count);
			}

			CompletableFuture<IList<string>> requestFuture = (CompletableFuture<IList<string>>)_pendingRequests.Remove(requestId);
			if (requestFuture != null)
			{
				requestFuture.complete(topics);
			}
			else
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), success.RequestId);
			}
		}

		private void HandleGetSchemaResponse(CommandGetSchemaResponse commandGetSchemaResponse)
		{
			checkArgument(_state == State.Ready);

			long requestId = commandGetSchemaResponse.RequestId;

			CompletableFuture<CommandGetSchemaResponse> future = (CompletableFuture<CommandGetSchemaResponse>)_pendingRequests.Remove(requestId);
			if (future == null)
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), requestId);
				return;
			}
			future.complete(commandGetSchemaResponse);
		}

		private void HandleGetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
		{
			checkArgument(_state == State.Ready);
			long requestId = commandGetOrCreateSchemaResponse.RequestId;
			CompletableFuture<CommandGetOrCreateSchemaResponse> future = (CompletableFuture<CommandGetOrCreateSchemaResponse>)_pendingRequests.Remove(requestId);
			if (future == null)
			{
				_log.warn("{} Received unknown request id from server: {}", Ctx.channel(), requestId);
				return;
			}
			future.complete(commandGetOrCreateSchemaResponse);
		}

		internal virtual Promise<Void> NewPromise()
		{
			return Ctx.newPromise();
		}

		public virtual ChannelHandlerContext Ctx()
		{
			return Ctx;
		}

		internal virtual Channel Channel()
		{
			return Ctx.channel();
		}

		internal virtual SocketAddress ServerAddrees()
		{
			return RemoteAddress;
		}

		internal virtual CompletableFuture<Void> ConnectionFuture()
		{
			return _connectionFuture;
		}

		internal virtual CompletableFuture<ProducerResponse> SendRequestWithId(ByteBuf cmd, long requestId)
		{
			return SendRequestAndHandleTimeout(cmd, requestId, RequestType.Command);
		}

		private CompletableFuture<T> SendRequestAndHandleTimeout<T>(ByteBuf requestMessage, long requestId, RequestType requestType)
		{
			CompletableFuture<T> future = new CompletableFuture<T>();
			_pendingRequests.Put(requestId, future);
			Ctx.writeAndFlush(requestMessage).addListener(writeFuture =>
			{
				if (!writeFuture.Success)
				{
					_log.warn("{} Failed to send {} to broker: {}", Ctx.channel(), requestType.Description, writeFuture.cause().Message);
					_pendingRequests.Remove(requestId);
					future.completeExceptionally(writeFuture.cause());
				}
			});
			_requestTimeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId, requestType));
			return future;
		}

		public virtual CompletableFuture<MessageIdData> SendGetLastMessageId(ByteBuf request, long requestId)
		{
			return SendRequestAndHandleTimeout(request, requestId, RequestType.GetLastMessageId);
		}

		public virtual CompletableFuture<Optional<SchemaInfo>> SendGetSchema(ByteBuf request, long requestId)
		{
			return SendGetRawSchema(request, requestId).thenCompose(commandGetSchemaResponse =>
			{
				if (commandGetSchemaResponse.hasErrorCode())
				{
					ServerError rc = commandGetSchemaResponse.ErrorCode;
					if (rc == ServerError.TopicNotFound)
					{
						return CompletableFuture.completedFuture(null);
					}
					else
					{
						return FutureUtil.FailedFuture(GetPulsarClientException(rc, commandGetSchemaResponse.ErrorMessage));
					}
				}
				else
				{
					return CompletableFuture.completedFuture(SchemaInfoUtil.NewSchemaInfo(commandGetSchemaResponse.Schema));
				}
			});
		}

		public virtual CompletableFuture<CommandGetSchemaResponse> SendGetRawSchema(ByteBuf request, long requestId)
		{
			return SendRequestAndHandleTimeout(request, requestId, RequestType.GetSchema);
		}

		public virtual CompletableFuture<sbyte[]> SendGetOrCreateSchema(ByteBuf request, long requestId)
		{
			CompletableFuture<CommandGetOrCreateSchemaResponse> future = SendRequestAndHandleTimeout(request, requestId, RequestType.GetOrCreateSchema);
			return future.thenCompose(response =>
			{
				if (response.hasErrorCode())
				{
					ServerError rc = response.ErrorCode;
					if (rc == ServerError.TopicNotFound)
					{
						return CompletableFuture.completedFuture(SchemaVersion.Empty.bytes());
					}
					else
					{
						return FutureUtil.FailedFuture(GetPulsarClientException(rc, response.ErrorMessage));
					}
				}
				else
				{
					return CompletableFuture.completedFuture(response.SchemaVersion.toByteArray());
				}
			});
		}

		private void HandleNewTxnResponse(CommandNewTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleNewTxnResponse(command);
			}
		}

		private void HandleAddPartitionToTxnResponse(CommandAddPartitionToTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleAddPublishPartitionToTxnResponse(command);
			}
		}

		private void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleAddSubscriptionToTxnResponse(command);
			}
		}

		private void HandleEndTxnOnPartitionResponse(CommandEndTxnOnPartitionResponse command)
		{
			_log.info("handleEndTxnOnPartitionResponse");
			TransactionBufferHandler handler = CheckAndGetTransactionBufferHandler();
			if (handler != null)
			{
				handler.HandleEndTxnOnTopicResponse(command.RequestId, command);
			}
		}

		private void HandleEndTxnOnSubscriptionResponse(CommandEndTxnOnSubscriptionResponse command)
		{
			TransactionBufferHandler handler = CheckAndGetTransactionBufferHandler();
			if (handler != null)
			{
				handler.HandleEndTxnOnSubscriptionResponse(command.RequestId, command);
			}
		}

		private void HandleEndTxnResponse(CommandEndTxnResponse command)
		{
			TransactionMetaStoreHandler handler = CheckAndGetTransactionMetaStoreHandler(command.TxnidMostBits);
			if (handler != null)
			{
				handler.HandleEndTxnResponse(command);
			}
		}

		private TransactionMetaStoreHandler CheckAndGetTransactionMetaStoreHandler(long tcId)
		{
			TransactionMetaStoreHandler handler = _transactionMetaStoreHandlers.Get(tcId);
			if (handler == null)
			{
				Channel().close();
				_log.warn("Close the channel since can't get the transaction meta store handler, will reconnect later.");
			}
			return handler;
		}

		private TransactionBufferHandler CheckAndGetTransactionBufferHandler()
		{
			if (_transactionBufferHandler == null)
			{
				Channel().close();
				_log.warn("Close the channel since can't get the transaction buffer handler.");
			}
			return _transactionBufferHandler;
		}

		/// <summary>
		/// check serverError and take appropriate action
		/// <ul>
		/// <li>InternalServerError: close connection immediately</li>
		/// <li>TooManyRequest: received error count is more than maxNumberOfRejectedRequestPerConnection in
		/// #rejectedRequestResetTimeSec</li>
		/// </ul>
		/// </summary>
		/// <param name="error"> </param>
		/// <param name="errMsg"> </param>
		private void CheckServerError(ServerError error, string errMsg)
		{
			if (ServerError.ServiceNotReady.Equals(error))
			{
				_log.error("{} Close connection because received internal-server error {}", Ctx.channel(), errMsg);
				Ctx.close();
			}
			else if (ServerError.TooManyRequests.Equals(error))
			{
				long rejectedRequests = _numberOfRejectedRequestsUpdater.getAndIncrement(this);
				if (rejectedRequests == 0)
				{
					// schedule timer
					_eventLoopGroup.schedule(() => _numberOfRejectedRequestsUpdater.set(ClientCnx.this, 0), _rejectedRequestResetTimeSec, TimeUnit.SECONDS);
				}
				else if (rejectedRequests >= _maxNumberOfRejectedRequestPerConnection)
				{
					_log.error("{} Close connection because received {} rejected request in {} seconds ", Ctx.channel(), _numberOfRejectedRequestsUpdater.get(ClientCnx.this), _rejectedRequestResetTimeSec);
					Ctx.close();
				}
			}
		}

		/// <summary>
		/// verifies host name provided in x509 Certificate in tls session
		/// 
		/// it matches hostname with below scenarios
		/// 
		/// <pre>
		///  1. Supports IPV4 and IPV6 host matching
		///  2. Supports wild card matching for DNS-name
		///  eg:
		///     HostName                     CN           Result
		/// 1.  localhost                    localhost    PASS
		/// 2.  localhost                    local*       PASS
		/// 3.  pulsar1-broker.com           pulsar*.com  PASS
		/// </pre>
		/// </summary>
		/// <param name="ctx"> </param>
		/// <returns> true if hostname is verified else return false </returns>
		private bool VerifyTlsHostName(string hostname, ChannelHandlerContext ctx)
		{
			ChannelHandler sslHandler = ctx.channel().pipeline().get("tls");

			SSLSession sslSession = null;
			if (sslHandler != null)
			{
				sslSession = ((SslHandler)sslHandler).engine().Session;
				if (_log.DebugEnabled)
				{
					_log.debug("Verifying HostName for {}, Cipher {}, Protocols {}", hostname, sslSession.CipherSuite, sslSession.Protocol);
				}
				return _hostnameVerifier.Verify(hostname, sslSession);
			}
			return false;
		}
		private void OnCommandReceived(ReadOnlySequence<byte> frame)
        {
			var commandSize = frame.ReadUInt32(0, true);
			var cmd = Serializer.Deserialize(frame.Slice(4, commandSize));
			var t = cmd.type;
			switch (cmd.type)
			{
				case BaseCommand.Type.AuthChallenge:
					var auth = cmd.authChallenge;
					HandleAuthChallenge(auth);
					break;
				case BaseCommand.Type.Message:
					var msg = cmd.Message;
					HandleMessage(msg, frame, commandSize);
					break;
			}
		}
		private void RegisterConsumer(long consumerId, IActorRef consumer)
		{
			_consumers.Add(consumerId, consumer);
		}

		private void RegisterProducer(long producerId, IActorRef producer)
		{
			_producers.Add(producerId, producer);
		}
		private void RegisterTransactionMetaStoreHandler(long transactionMetaStoreId, IActorRef handler)
		{
			_transactionMetaStoreHandlers.Add(transactionMetaStoreId, handler);
		}
		private void RegisterTransactionBufferHandler(TransactionBufferHandler handler)
		{
			_transactionBufferHandler = handler;
		}

		private void RemoveProducer(long producerId)
		{
			_producers.Remove(producerId);
		}

		private void RemoveConsumer(long consumerId)
		{
			_consumers.Remove(consumerId);
		}
		private void CheckRequestTimeout()
		{
			while (!_requestTimeoutQueue.IsEmpty)
			{
				var req = _requestTimeoutQueue.TryPeek(out var request);
				if (!req || (DateTimeHelper.CurrentUnixTimeMillis() - request.CreationTimeMs) < _operationTimeoutMs)
				{
					// if there is no request that is timed out then exit the loop
					break;
				}
				req = _requestTimeoutQueue.TryDequeue(out request);
				if (_pendingRequests.Remove(request.RequestId))
				{
					string timeoutMessage = string.Format("{0:D} {1} timedout after ms {2:D}", request.RequestId, request.RequestType.Description, _operationTimeoutMs);
					_log.Warning(timeoutMessage);
				}
			}
			_timeoutTask = _actorContext.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(_operationTimeoutMs)), CheckRequestTimeout);

		}
		public byte[] NewConnectCommand()
		{
			// mutual authentication is to auth between `remoteHostName` and this client for this channel.
			// each channel will have a mutual client/server pair, mutual client evaluateChallenge with init data,
			// and return authData to server.
			_authenticationDataProvider = _authentication.GetAuthData(_remoteHostName);
			var authData = _authenticationDataProvider.Authenticate(_authentication.AuthMethodName.ToLower() == "sts" ? null : new AuthData(AuthData.InitAuthData));
			var assemblyName = Assembly.GetCallingAssembly().GetName();
			var auth = new Protocol.Proto.AuthData { auth_data = ((byte[])(object)authData.Bytes) };
			var clientVersion = assemblyName.Name + " " + assemblyName.Version.ToString(3);

			return Commands.NewConnect(_authentication.AuthMethodName, auth, _protocolVersion, clientVersion, _proxyToTargetBrokerAddress, string.Empty, null, string.Empty);
		}
		#region privates
		internal enum State
		{
			None,
			SentConnectFrame,
			Ready,
			Failed,
			Connecting
		}

		private class RequestTime
		{
			internal readonly long CreationTimeMs;
			internal readonly long RequestId;
			internal readonly RequestType RequestType;

			internal RequestTime(long creationTime, long requestId, RequestType requestType)
			{
				CreationTimeMs = creationTime;
				RequestId = requestId;
				RequestType = requestType;
			}
		}

		private sealed class RequestType
		{
			public static readonly RequestType Command = new RequestType("Command", InnerEnum.Command);
			public static readonly RequestType GetLastMessageId = new RequestType("GetLastMessageId", InnerEnum.GetLastMessageId);
			public static readonly RequestType GetTopics = new RequestType("GetTopics", InnerEnum.GetTopics);
			public static readonly RequestType GetSchema = new RequestType("GetSchema", InnerEnum.GetSchema);
			public static readonly RequestType GetOrCreateSchema = new RequestType("GetOrCreateSchema", InnerEnum.GetOrCreateSchema);

			private static readonly List<RequestType> valueList = new List<RequestType>();

			static RequestType()
			{
				valueList.Add(Command);
				valueList.Add(GetLastMessageId);
				valueList.Add(GetTopics);
				valueList.Add(GetSchema);
				valueList.Add(GetOrCreateSchema);
			}

			public enum InnerEnum
			{
				Command,
				GetLastMessageId,
				GetTopics,
				GetSchema,
				GetOrCreateSchema
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			private RequestType(string name, InnerEnum innerEnum)
			{
				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			internal string Description
			{
				get
				{
					if (this == Command)
					{
						return "request";
					}
					else
					{
						return Name() + " request";
					}
				}
			}

			public static RequestType[] Values()
			{
				return valueList.ToArray();
			}

			public int Ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static RequestType ValueOf(string name)
			{
				foreach (RequestType enumInstance in RequestType.valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		#endregion


	}
}