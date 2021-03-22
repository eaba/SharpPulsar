﻿using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using BAMCIS.Util.Concurrent;
using DotNetty.Common.Utilities;
using IdentityModel;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Entity;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Crypto;
using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Queues;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using SharpPulsar.Stats.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SharpPulsar.Exceptions.PulsarClientException;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar
{

    internal class ProducerActor<T> : ProducerActorBase<T>, IWithUnboundedStash
	{

		// Producer id, used to identify a producer within a single connection
		private readonly long _producerId;

		// Variable is used through the atomic updater
		private long _msgIdGenerator;
		private ICancelable _sendTimeout;
		private long _createProducerTimeout;
		private readonly IBatchMessageContainerBase<T> _batchMessageContainer;
		private Queue<OpSendMsg<T>> _pendingMessages;
		private IActorRef _generator;

		private readonly ICancelable _regenerateDataKeyCipherCancelable;

		// Globally unique producer name
		private string _producerName;
		private bool _userProvidedProducerName = false;

		private ILoggingAdapter _log;

		private string _connectionId;
		private string _connectedSince;
		private readonly int _partitionIndex;

		private readonly IProducerStatsRecorder _stats;

		private readonly CompressionCodec _compressor;

		private long _lastSequenceIdPublished;

		protected internal long LastSequenceIdPushed;
		private bool _isLastSequenceIdPotentialDuplicated;

		private readonly IMessageCrypto _msgCrypto;

		private ICancelable _keyGeneratorTask = null;

		private readonly IDictionary<string, string> _metadata;

		private Option<sbyte[]> _schemaVersion = null;

		private readonly IActorRef _connectionHandler;
		private readonly IActorRef _self;

		private ICancelable _batchTimerTask;
		private Dictionary<long, IActorRef> _txnSequence;

		private readonly IScheduler _scheduler;
		private readonly Commands _commands;

		public ProducerActor(long producerid, IActorRef client, IActorRef lookup, IActorRef idGenerator, string topic, ProducerConfigurationData conf, int partitionIndex, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, ProducerQueueCollection<T> queue) : base(client, lookup, topic, conf, schema, interceptors, clientConfiguration, queue)
		{
			_commands = new Commands();
			_txnSequence = new Dictionary<long, IActorRef>();
			_self = Self;
			_generator = idGenerator;
			_scheduler = Context.System.Scheduler;
			_log = Context.GetLogger();
			_producerId = producerid;
			_producerName = conf.ProducerName;
			if(!string.IsNullOrWhiteSpace(_producerName))
			{
				_userProvidedProducerName = true;
			}
			_partitionIndex = partitionIndex;
			_pendingMessages = new Queue<OpSendMsg<T>>(conf.MaxPendingMessages);

			_compressor = CompressionCodecProvider.GetCompressionCodec((int)conf.CompressionType);

			if (conf.InitialSequenceId != null)
			{
				long initialSequenceId = conf.InitialSequenceId.Value;
				_lastSequenceIdPublished = initialSequenceId;
				LastSequenceIdPushed = initialSequenceId;
				_msgIdGenerator = initialSequenceId + 1L;
			}
			else
			{
				_lastSequenceIdPublished = -1L;
				LastSequenceIdPushed = -1L;
				_msgIdGenerator = 0L;
			}

			if(conf.EncryptionEnabled)
			{
				string logCtx = "[" + topic + "] [" + _producerName + "] [" + _producerId + "]";

				if(conf.MessageCrypto != null)
				{
					_msgCrypto = conf.MessageCrypto;
				}
				else
				{
					// default to use MessageCryptoBc;
					IMessageCrypto msgCryptoBc;
					try
					{
						msgCryptoBc = new MessageCrypto(logCtx, true, _log);
					}
					catch(Exception e)
					{
						_log.Error($"MessageCryptoBc may not included in the jar in Producer: {e}");
						msgCryptoBc = null;
					}
					_msgCrypto = msgCryptoBc;
				}
			}
			else
			{
				_msgCrypto = null;
			}

			if(_msgCrypto != null)
			{
				// Regenerate data key cipher at fixed interval
				_regenerateDataKeyCipherCancelable = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromHours(0), TimeSpan.FromHours(4), () =>
				{
					try
					{
						_msgCrypto.AddPublicKeyCipher(conf.EncryptionKeys, conf.CryptoKeyReader);
					}
					catch (CryptoException e)
					{
						Context.System.Log.Warning($"[{Topic}] [{ProducerName().GetAwaiter().GetResult()}] [{_producerId}] Failed to add public key cipher.");
						Context.System.Log.Error(e.ToString());
					}
				});
			}

			if(conf.SendTimeoutMs > 0)
			{
				_sendTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(conf.SendTimeoutMs), Self, RunSendTimeout.Instance, ActorRefs.NoSender);
			}

			_createProducerTimeout = DateTimeHelper.CurrentUnixTimeMillis() + clientConfiguration.OperationTimeoutMs;
			if(conf.BatchingEnabled)
			{
				var containerBuilder = conf.BatcherBuilder;
				if(containerBuilder == null)
				{
					containerBuilder = IBatcherBuilder.Default(Context.System);
				}
				_batchMessageContainer = (IBatchMessageContainerBase<T>)containerBuilder.Build<T>();
				_batchMessageContainer.Producer = Self;
			}
			else
			{
				_batchMessageContainer = null;
			}
			if (clientConfiguration.StatsIntervalSeconds > 0)
			{
				_stats = new ProducerStatsRecorder(Context.System, ProducerName().GetAwaiter().GetResult(), topic, Configuration.MaxPendingMessages);
			}
			else
			{
				_stats = ProducerStatsDisabled.Instance;
			}

			if (Configuration.Properties == null)
			{
				_metadata = new Dictionary<string, string>();
			}
			else
			{
				_metadata = new SortedDictionary<string, string>(Configuration.Properties);
			}
			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(clientConfiguration, State, new BackoffBuilder().SetInitialTime(clientConfiguration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(clientConfiguration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(0, TimeUnit.MILLISECONDS).Create(), Self));

			GrabCnx();
		}
		internal virtual void GrabCnx()
		{
			_connectionHandler.Tell(new GrabCnx($"Create connection from producer: {ProducerName().GetAwaiter().GetResult()}"));
			Become(Connection);
		}
		private void Connection()
		{
			ReceiveAsync<ConnectionOpened>(async c => {
				await ConnectionOpened(c.ClientCnx);
			});
			Receive<ConnectionFailed>(c => {
				ConnectionFailed(c.Exception);
			});
			Receive<Failure>(c => {
				_log.Error($"Connection to the server failed: {c.Exception}/{c.Timestamp}");
			});
			Receive<ConnectionAlreadySet>(_ => {
				Become(Ready);
			});
			ReceiveAny(a => {
				var message = a;
				Stash.Stash();
			});
		}
		private void Ready()
        {
			ReceiveAsync<AckReceived>(async a => 
			{				
				await AckReceived(a.SequenceId, a.HighestSequenceId, a.LedgerId, a.EntryId);
				ProducerQueue.Receipt.Add(a);
			});
			ReceiveAsync<RunSendTimeout>(async _ => {
				await FailTimedoutMessages();
			});
			ReceiveAsync<BatchTask>(async _ => {
				await RunBatchTask();
			});
			Receive<ConnectionClosed>(m => {
				ConnectionClosed(m.ClientCnx);
			});
			ReceiveAsync<Flush>(async _ => {
				await Flush();
			});
			Receive<GetProducerName>(_ => Sender.Tell(_producerName));
			Receive<GetLastSequenceId>(_ => Sender.Tell(_lastSequenceIdPublished));
			Receive<GetTopic>(_ => Sender.Tell(Topic));
			ReceiveAsync<IsConnected>(async _ => Sender.Tell(await Connected()));
			Receive<GetStats>(_ => Sender.Tell(_stats));
			ReceiveAsync<GetLastDisconnectedTimestamp>(async _ => 
			{
				var l = await LastDisconnectedTimestamp();
				Sender.Tell(l);
			});
			ReceiveAsync<InternalSend<T>>(async m => 
			{
                try
                {
					//get excepyion vai out
					await InternalSend(m.Message);
				}
                catch(Exception ex)
                {
					_log.Error(ex.ToString());
				}
			});
			ReceiveAsync<InternalSendWithTxn<T>>(async m =>
			{
				try
				{
					_txnSequence.Add(_msgIdGenerator, m.Txn);
					await InternalSendWithTxn(m.Message, m.Txn);
				}
				catch (Exception ex)
				{
					_log.Error(ex.ToString());
				}
			});
		}
		private async ValueTask ConnectionOpened(IActorRef cnx)
		{
			// we set the cnx reference before registering the producer on the cnx, so if the cnx breaks before creating the
			// producer, it will try to grab a new cnx
			_connectionHandler.Tell(new SetCnx(cnx));

			if(_batchMessageContainer != null)
            {
				var maxMessageSize = await cnx.Ask<int>(MaxMessageSize.Instance);
                _batchMessageContainer.Container = new ProducerContainer(Self, Configuration, maxMessageSize, Context.System)
                {
                    ProducerId = _producerId
                };
            }

			cnx.Tell(new RegisterProducer(_producerId, _self)); 
			_log.Info($"[{Topic}] [{_producerName}] Creating producer on cnx {cnx.Path.Name}");

			var requestId = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);

			ISchemaInfo schemaInfo = null;
			if (Schema != null)
			{
				if (Schema.SchemaInfo != null)
				{
					if (Schema.SchemaInfo.Type == SchemaType.JSON)
					{
						// for backwards compatibility purposes
						// JSONSchema originally generated a schema for pojo based of of the JSON schema standard
						// but now we have standardized on every schema to generate an Avro based schema
						var protocolVersion = await cnx.Ask<int>(RemoteEndpointProtocolVersion.Instance);
						if (_commands.PeerSupportJsonSchemaAvroFormat(protocolVersion))
						{
							schemaInfo = Schema.SchemaInfo;
						}
						else if (Schema is JSONSchema<T> jsonSchema)
						{
							schemaInfo = jsonSchema.BackwardsCompatibleJsonSchemaInfo;
						}
						else
						{
							schemaInfo = Schema.SchemaInfo;
						}
					}
					else if (Schema.SchemaInfo.Type == SchemaType.BYTES || Schema.SchemaInfo.Type == SchemaType.NONE)
					{
						// don't set schema info for Schema.BYTES
						schemaInfo = null;
					}
					else
					{
						schemaInfo = Schema.SchemaInfo;
					}
				}
			}
			try
            {
				var epoch = await _connectionHandler.Ask<long>(GetEpoch.Instance);
				var cmd = _commands.NewProducer(Topic, _producerId, requestId.Id, _producerName, Conf.EncryptionEnabled, _metadata, schemaInfo, epoch, _userProvidedProducerName);
				var payload = new Payload(cmd, requestId.Id, "NewProducer");

				var response = await cnx.Ask<ProducerResponse>(payload);

				string producerName = response.ProducerName;
				long lastSequenceId = response.LastSequenceId;

				_schemaVersion = new Option<sbyte[]>(response.SchemaVersion.ToSBytes());

				if (_schemaVersion.HasValue)
					SchemaCache.Add(SchemaHash.Of(Schema), _schemaVersion.Value);
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					cnx.Tell(new RemoveProducer(_producerId));
					await cnx.GracefulStop(TimeSpan.FromSeconds(5));
					return;
				}
				if(_batchMessageContainer != null)
					_batchMessageContainer.Container.ProducerName = producerName;
				_connectionHandler.Tell(ResetBackoff.Instance);
				_log.Info($"[{Topic}] [{producerName}] Created producer on cnx {cnx.Path}");
				_connectionId = cnx.Path.ToString();
				_connectedSince = DateTime.Now.ToLongDateString();
				if (string.IsNullOrWhiteSpace(_producerName))
				{
					_producerName = producerName;
				}
				if (_msgIdGenerator == 0 && Conf.InitialSequenceId == null)
				{
					_lastSequenceIdPublished = lastSequenceId;
					_msgIdGenerator = lastSequenceId + 1;
				}
				if (BatchMessagingEnabled)
				{
					var interval = TimeUnit.MICROSECONDS.ToMicroseconds(Conf.BatchingMaxPublishDelayMicros);
					_batchTimerTask =_scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(0), TimeSpan.FromTicks(interval), Self, BatchTask.Instance, ActorRefs.NoSender);
				}

				await ResendMessages(response);
			}
			catch(Exception ex)
            {
				cnx.Tell(new RemoveProducer(_producerId));
				if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
				{
					await cnx.GracefulStop(TimeSpan.FromSeconds(5));
					ProducerQueue.Producer.Add(new ProducerCreation(ex));
					return;
				}
				_log.Error($"[{Topic}] [{_producerName}] Failed to create producer: {ex}");
				if (ex is TopicDoesNotExistException e)
				{
					_log.Error($"Failed to close producer on TopicDoesNotExistException: {Topic}");
					ProducerQueue.Producer.Add(new ProducerCreation(e));
					return;
				}
				if (ex is ProducerBlockedQuotaExceededException)
				{
					_log.Warning($"[{Topic}] [{_producerName}] Topic backlog quota exceeded. Throwing Exception on producer.");
					var pe = new ProducerBlockedQuotaExceededException($"The backlog quota of the topic {Topic} that the producer {_producerName} produces to is exceeded");
					ProducerQueue.Producer.Add(new ProducerCreation(pe));
				}
				else if (ex is ProducerBlockedQuotaExceededError pexe)
				{
					_log.Warning($"[{_producerName}] [{Topic}] Producer is blocked on creation because backlog exceeded on topic.");
					ProducerQueue.Producer.Add(new ProducerCreation(pexe));
				}
				if (ex is TopicTerminatedException tex)
				{
					State.ConnectionState = HandlerState.State.Terminated;
					Client.Tell(new CleanupProducer(Self));
					ProducerQueue.Producer.Add(new ProducerCreation(tex));
				}
				else if ((ex is PulsarClientException && IsRetriableError(ex) && DateTimeHelper.CurrentUnixTimeMillis() < _createProducerTimeout))
				{
					ReconnectLater(ex);
				}
				else
				{
					State.ConnectionState = HandlerState.State.Failed;
					Client.Tell(new CleanupProducer(Self));
					ProducerQueue.Producer.Add(new ProducerCreation(new Exception()));
					var timeout = _sendTimeout;
					if (timeout != null)
					{
						timeout.Cancel();
						_sendTimeout = null;
					}
				}
			}			
		}

		private void ConnectionFailed(PulsarClientException exception)
		{
		bool nonRetriableError = !IsRetriableError(exception);
		bool producerTimeout = DateTimeHelper.CurrentUnixTimeMillis() > _createProducerTimeout;
		if ((nonRetriableError || producerTimeout))
		{
			if (nonRetriableError)
			{
				_log.Info($"[{Topic}] Producer creation failed for producer {_producerId} with unretriableError = {exception}");
			}
			else
			{
				_log.Info($"[{Topic}] Producer creation failed for producer {_producerId} after producerTimeout");
			}
			State.ConnectionState = HandlerState.State.Failed;
			Client.Tell(new CleanupProducer(Self));
		}
	}

		private void ReconnectLater(Exception exception)
		{
			_connectionHandler.Tell(new ReconnectLater(exception));
		}
		private async ValueTask RunBatchTask()
        {
			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				return;
			}
			_log.Info($"[{Topic}] [{_producerName}] Batching the messages from the batch container from timer thread");

			await BatchMessageAndSend();
		}
		private bool BatchMessagingEnabled
		{
			get
			{
				return Conf.BatchingEnabled;
			}
		}

		private bool IsMultiSchemaEnabled(bool autoEnable)
		{
			if(_multiSchemaMode != MultiSchemaMode.Auto)
			{
				return _multiSchemaMode == MultiSchemaMode.Enabled;
			}
			if(autoEnable)
			{
				_multiSchemaMode = MultiSchemaMode.Enabled;
				return true;
			}
			return false;
		}

		protected internal override async ValueTask<long> LastSequenceId()
		{
			return await Task.FromResult(_lastSequenceIdPublished);
		}

		internal override async ValueTask InternalSend(IMessage<T> message)
		{

			var interceptorMessage = (Message<T>) BeforeSend(message);
			if(Interceptors != null)
			{

				_ = interceptorMessage.Properties;
			}
			await Send(interceptorMessage);
		}

		internal override async ValueTask InternalSendWithTxn(IMessage<T> message, IActorRef txn)
		{
			if(txn == null)
			{
				await InternalSend(message);
			}
			else
			{
				var registered = await txn.Ask<bool>(new RegisterProducedTopic(Topic));
				if(registered)
					await InternalSend(message);
			}
		}

		private async ValueTask Send(IMessage<T> message)
		{
			var cnx = await ClientCnx();
			Condition.CheckArgument(message is Message<T>);
			var maxMessageSize = await cnx.Ask<int>(MaxMessageSize.Instance).ConfigureAwait(false);
			if (Conf.ChunkingEnabled && Conf.MaxMessageSize > 0)
				maxMessageSize = Math.Min(Conf.MaxMessageSize, maxMessageSize);
			if (!IsValidProducerState(message.SequenceId))
			{
				return;
			}

			var msg = (Message<T>) message;
			MessageMetadata msgMetadata = msg.Metadata;
			var payload = msg.Data;

			// If compression is enabled, we are compressing, otherwise it will simply use the same buffer
			int uncompressedSize = payload.Length;
			var compressedPayload = payload;
			// Batch will be compressed when closed
			// If a message has a delayed delivery time, we'll always send it individually
			if(!BatchMessagingEnabled || msgMetadata.ShouldSerializeDeliverAtTime())
			{
				compressedPayload = _compressor.Encode(payload.ToBytes()).ToSBytes();

				// validate msg-size (For batching this will be check at the batch completion size)
				int compressedSize = compressedPayload.Length;
				
				if (compressedSize > maxMessageSize && !Conf.ChunkingEnabled)
				{
					string compressedStr = (!BatchMessagingEnabled && Conf.CompressionType != CompressionType.None) ? "Compressed" : "";
					var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} sends a {compressedStr} message with {compressedSize:d} bytes that exceeds {maxMessageSize:d} bytes");
					_log.Error(invalidMessageException.ToString());
					return;
				}
			}

			if(!msg.Replicated && !string.IsNullOrWhiteSpace(msgMetadata.ProducerName))
			{
				var invalidMessageException = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} can not reuse the same message {msgMetadata.SequenceId}");
				_log.Error(invalidMessageException.ToString());
				return;
			}

			if(!PopulateMessageSchema(msg, out _))
			{
				return;
			}

			// send in chunks
			var totalChunks = CanAddToBatch(msg) ? 1 : Math.Max(1, compressedPayload.Length) / maxMessageSize + (Math.Max(1, compressedPayload.Length) % maxMessageSize == 0 ? 0 : 1);
			// chunked message also sent individually so, try to acquire send-permits
			for(int i = 0; i < (totalChunks - 1); i++)
			{
				if(!CanEnqueueRequest(message.SequenceId))
				{
					return;
				}
			}

			try
			{
				int readStartIndex = 0;
				long sequenceId = _msgIdGenerator;
				msgMetadata.SequenceId = (ulong)sequenceId;
				_msgIdGenerator++;
				string uuid = totalChunks > 1 ? string.Format("{0}-{1:D}", _producerName, sequenceId) : null;
				for (int chunkId = 0; chunkId < totalChunks; chunkId++)
				{
					await SerializeAndSendMessage(msg, msgMetadata, payload.ToBytes(), sequenceId, uuid, chunkId, totalChunks, readStartIndex, maxMessageSize, compressedPayload.ToBytes(), compressedPayload.Length, uncompressedSize);
					readStartIndex = ((chunkId + 1) * maxMessageSize);
				}
			}
			catch (PulsarClientException e)
			{
				e.SequenceId = msg.SequenceId;
				SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), e);
			}
			catch (Exception t)
			{
				SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), t);
			}
		}
		private bool CanEnqueueRequest(long sequenceId)
		{
			return true;
		}
		private async ValueTask SerializeAndSendMessage(Message<T> msg, MessageMetadata msgMetadata, byte[] payload, long sequenceId, string uuid, int chunkId, int totalChunks, int readStartIndex, int chunkMaxSizeInBytes, byte[] compressedPayload, int compressedPayloadSize, int uncompressedSize)
		{
			var chunkPayload = compressedPayload;
			var chunkMsgMetadata = msgMetadata;
			if(totalChunks > 1 && TopicName.Get(Topic).Persistent)
			{
				chunkPayload = compressedPayload.Slice(readStartIndex, Math.Min(chunkMaxSizeInBytes, chunkPayload.Length - readStartIndex));
				if (chunkId != totalChunks - 1)
				{
					chunkMsgMetadata = msgMetadata;
				}
				if(!string.IsNullOrWhiteSpace(uuid))
				{
					chunkMsgMetadata.Uuid = uuid;
				}
				chunkMsgMetadata.ChunkId = chunkId;
				chunkMsgMetadata.NumChunksFromMsg = totalChunks;
				chunkMsgMetadata.TotalChunkMsgSize = compressedPayloadSize;
			}
			if(!HasPublishTime(chunkMsgMetadata.PublishTime))
			{
				chunkMsgMetadata.PublishTime = (ulong)ClientConfiguration.Clock.ToEpochTime();

				if (!string.IsNullOrWhiteSpace(chunkMsgMetadata.ProducerName))
				{
					_log.Warning($"changing producer name from '{chunkMsgMetadata.ProducerName}' to ''. Just helping out ;)");
					chunkMsgMetadata.ProducerName = string.Empty;
				}

				chunkMsgMetadata.ProducerName = _producerName;

				if(Conf.CompressionType != CompressionType.None)
				{
					chunkMsgMetadata.Compression = CompressionCodecProvider.ConvertToWireProtocol(Conf.CompressionType);
				}
				chunkMsgMetadata.UncompressedSize = (uint)uncompressedSize;
			}

			if(CanAddToBatch(msg) && totalChunks <= 1)
			{
				if(CanAddToCurrentBatch(msg))
				{
					// should trigger complete the batch message, new message will add to a new batch and new batch
					// sequence id use the new message, so that broker can handle the message duplication
					if(sequenceId <= LastSequenceIdPushed)
					{
						_isLastSequenceIdPotentialDuplicated = true;
						if(sequenceId <= _lastSequenceIdPublished)
						{
							_log.Warning($"Message with sequence id {sequenceId} is definitely a duplicate");
						}
						else
						{
							_log.Info($"Message with sequence id {sequenceId} might be a duplicate but cannot be determined at this time.");
						}
						await DoBatchSendAndAdd(msg, payload);
					}
					else
					{
						// Should flush the last potential duplicated since can't combine potential duplicated messages
						// and non-duplicated messages into a batch.
						if(_isLastSequenceIdPotentialDuplicated)
						{
							await DoBatchSendAndAdd(msg, payload);
						}
						else
						{
							// handle boundary cases where message being added would exceed
							// batch size and/or max message size
							bool isBatchFull = _batchMessageContainer.Add(msg, (a, e) =>
							{
								if (a != null)
									_lastSequenceIdPublished = (long)a;
								if (e != null)
									SendComplete(msg, DateTimeHelper.CurrentUnixTimeMillis(), e);
							});
							if (isBatchFull)
							{
								await BatchMessageAndSend();
							}
						}
						_isLastSequenceIdPotentialDuplicated = false;
					}
				}
				else
				{
					await DoBatchSendAndAdd (msg, payload);
				}
			}
			else
			{
				var encryptedPayload = EncryptMessage(chunkMsgMetadata, chunkPayload);

				msgMetadata = chunkMsgMetadata;
				// When publishing during replication, we need to set the correct number of message in batch
				// This is only used in tracking the publish rate stats
				int numMessages = msg.Metadata.ShouldSerializeNumMessagesInBatch() ? msg.Metadata.NumMessagesInBatch : 1;

				OpSendMsg<T> op;
				if(msg.GetSchemaState() == Message<T>.SchemaState.Ready)
				{
					var cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
					op = OpSendMsg<T>.Create(msg, cmd, sequenceId);
				}
				else
				{
					op = OpSendMsg<T>.Create(msg, null, sequenceId);
					op.Cmd = SendMessage(_producerId, sequenceId, numMessages, msgMetadata, encryptedPayload);
				}
				op.NumMessagesInBatch = numMessages;
				op.BatchSizeByte = encryptedPayload.Length;
				if(totalChunks > 1)
				{
					op.TotalChunks = totalChunks;
					op.ChunkId = chunkId;
				}
				await ProcessOpSendMsg(op);
			}
		}

		private bool PopulateMessageSchema(Message<T> msg, out Exception exception)
		{
			exception = new Exception();
			var msgMetadata = new MessageMetadata();
			if(msg.Schema == Schema)
			{
				if (_schemaVersion.HasValue)
					msgMetadata.SchemaVersion = _schemaVersion.Value.ToBytes();
				msg.SetSchemaState(Message<T>.SchemaState.Ready);
				return true;
			}
			if(!IsMultiSchemaEnabled(true))
			{
				exception = new PulsarClientException.InvalidMessageException($"The producer {_producerName} of the topic {Topic} is disabled the `MultiSchema`: Seq:{msg.SequenceId}");
				return false;
			}
			SchemaHash schemaHash = SchemaHash.Of(msg.Schema);
			sbyte[] schemaVersion = SchemaCache[schemaHash];
			if(schemaVersion != null)
			{
				msgMetadata.SchemaVersion = schemaVersion.ToBytes();
				msg.SetSchemaState(Message<T>.SchemaState.Ready);
			}
			return true;
		}

		private bool RePopulateMessageSchema(Message<T> msg)
		{
			SchemaHash schemaHash = SchemaHash.Of(msg.Schema);
			sbyte[] schemaVersion = SchemaCache[schemaHash];
			if(schemaVersion == null)
			{
				return false;
			}
			msg.Metadata.SchemaVersion = schemaVersion.ToBytes();
			msg.SetSchemaState(Message<T>.SchemaState.Ready);
			return true;
		}

		private async ValueTask TryRegisterSchema(IActorRef cnx, Message<T> msg)
		{
			
			if(!State.ChangeToRegisteringSchemaState())
			{
				return;
			}
			ISchemaInfo schemaInfo;
			if (msg.Schema != null && msg.Schema.SchemaInfo.Type.Value > 0)
			{
				schemaInfo = (SchemaInfo)msg.Schema.SchemaInfo;
			}
			else
			{
				schemaInfo = (SchemaInfo)ISchema<T>.Bytes.SchemaInfo;
			}
			var obj = await GetOrCreateSchema(cnx, schemaInfo);
			if(obj is GetOrCreateSchemaResponse r)
            {
				if(!Enum.IsDefined(typeof(ServerError), r.Response.ErrorCode))
                {
					_log.Warning($"[{Topic}] [{_producerName}] GetOrCreateSchema succeed");
					SchemaHash schemaHash = SchemaHash.Of(msg.Schema);
					if (!SchemaCache.ContainsKey(schemaHash))
						SchemaCache.Add(schemaHash, r.Response.SchemaVersion.ToSBytes());
					msg.Metadata.SchemaVersion = r.Response.SchemaVersion;
					msg.SetSchemaState(Message<T>.SchemaState.Ready);
				}
                else
                {
					if(r.Response.ErrorCode == ServerError.IncompatibleSchema)
                    {
						_log.Warning($"[{Topic}] [{_producerName}] GetOrCreateSchema error: [{r.Response.ErrorCode}:{r.Response.ErrorMessage}]");
						msg.SetSchemaState(Message<T>.SchemaState.Broken);
						var ex = new IncompatibleSchemaException(r.Response.ErrorMessage);
					}
                    else
                    {
						_log.Error($"[{Topic}] [{_producerName}] GetOrCreateSchema failed: [{r.Response.ErrorCode}:{r.Response.ErrorMessage}]");
						msg.SetSchemaState(Message<T>.SchemaState.None);
					}					
				}
			}
			await RecoverProcessOpSendMsgFrom(msg);
		}

		private async ValueTask<object> GetOrCreateSchema(IActorRef cnx, ISchemaInfo schemaInfo)
		{
			var protocolVersion = await cnx.Ask<int>(RemoteEndpointProtocolVersion.Instance);
			if(!_commands.PeerSupportsGetOrCreateSchema(protocolVersion))
			{
				throw new PulsarClientException.NotSupportedException($"The command `GetOrCreateSchema` is not supported for the protocol version {protocolVersion}. The producer is {_producerName}, topic is {Topic}");
			}
			long requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).Id;
			var request = _commands.NewGetOrCreateSchema(requestId, Topic, schemaInfo);
			var payload = new Payload(request, requestId, "SendGetOrCreateSchema");
			_log.Info($"[{Topic}] [{_producerName}] GetOrCreateSchema request", Topic, _producerName);
			return await cnx.Ask(payload);
		}

		private byte[] EncryptMessage(MessageMetadata msgMetadata, byte[] compressedPayload)
		{

			var encryptedPayload = compressedPayload;
			if(!Conf.EncryptionEnabled || _msgCrypto == null)
			{
				return encryptedPayload;
			}
			try
			{
				encryptedPayload = _msgCrypto.Encrypt(Conf.EncryptionKeys, Conf.CryptoKeyReader, msgMetadata, compressedPayload);
			}
			catch(PulsarClientException e)
			{
				// Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
				if(Conf.CryptoFailureAction == ProducerCryptoFailureAction.Send)
				{
					_log.Warning($"[{Topic}] [{_producerName}] Failed to encrypt message {e}. Proceeding with publishing unencrypted message");
					return compressedPayload;
				}
				throw e;
			}
			return encryptedPayload;
		}

		private byte[] SendMessage(long producerId, long sequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
		{
			_log.Info($"Send message with {_producerName}:{producerId}");
			return new Commands().NewSend(producerId, sequenceId, numMessages, msgMetadata, compressedPayload);
		}

		private byte[] SendMessage(long producerId, long lowestSequenceId, long highestSequenceId, int numMessages, MessageMetadata msgMetadata, byte[] compressedPayload)
		{
			return _commands.NewSend(producerId, lowestSequenceId, highestSequenceId, numMessages, msgMetadata, compressedPayload);
		}

        public IStash Stash { get; set; }

        private bool CanAddToBatch(Message<T> msg)
		{
			return msg.GetSchemaState() == Message<T>.SchemaState.Ready && BatchMessagingEnabled && !msg.Metadata.ShouldSerializeDeliverAtTime();
		}

		private bool CanAddToCurrentBatch(Message<T> msg)
		{
			return _batchMessageContainer.HaveEnoughSpace(msg) && (!IsMultiSchemaEnabled(false) || _batchMessageContainer.HasSameSchema(msg)) && _batchMessageContainer.HasSameTxn(msg);
		}

		private async ValueTask DoBatchSendAndAdd(Message<T> msg, byte[] payload)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{_producerName}] Closing out batch to accommodate large message with size {msg.Data.Length}");
			}
			try
			{
				await BatchMessageAndSend();
			}
			catch(Exception ex)
            {
				_log.Error(ex.ToString());
			}
		}

		private bool IsValidProducerState(long sequenceId)
		{
			switch(State.ConnectionState)
			{
				case HandlerState.State.Ready:
					// OK
				case HandlerState.State.Connecting:
					// We are OK to queue the messages on the client, it will be sent to the broker once we get the connection
				case HandlerState.State.RegisteringSchema:
					// registering schema
					return true;
				case HandlerState.State.Closing:
				case HandlerState.State.Closed:
					_log.Error($"Producer already closed sequenceId: {sequenceId}");
					return false;
				case HandlerState.State.Terminated:
						_log.Error($"Topic was terminated sequenceId: {sequenceId}");
					return false;
				case HandlerState.State.Failed:
				case HandlerState.State.Uninitialized:
				default:
						_log.Error(new NotConnectedException(sequenceId).ToString());
					return false;
			}
		}
		private void SendCommand(OpSendMsg<T> op, IActorRef cnx)
		{
			var pay = new Payload(op.Cmd, -1, "CommandMessage");
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{Topic}] [{_producerName}] Sending message cnx, sequenceId {op.SequenceId}");
			}
			cnx.Tell(pay);
		}
		protected override void PostStop()
        {
			if (_sendTimeout != null)
            {
				_sendTimeout.Cancel();
				_sendTimeout = null;
			}

			if (_batchTimerTask != null)
			{
				_batchTimerTask.Cancel();
				_batchTimerTask = null;
			}
			if (_keyGeneratorTask != null && !_keyGeneratorTask.IsCancellationRequested)
			{
				_keyGeneratorTask.Cancel();
			}
			Close().ConfigureAwait(false);
			base.PostStop();
        }
        private async ValueTask Close()
		{
			HandlerState.State currentState = State.GetAndUpdateState(State.ConnectionState == HandlerState.State.Closed ? HandlerState.State.Closed : HandlerState.State.Closing);

			if(currentState == HandlerState.State.Closed || currentState == HandlerState.State.Closing)
			{
				return;
			}
					

			_stats.CancelStatsTimeout();

			var cnx = await Cnx();
			if(cnx == null || currentState != HandlerState.State.Ready)
			{
				_log.Info("[{}] [{}] Closed Producer (not connected)", Topic, _producerName);
				State.ConnectionState = HandlerState.State.Closed;
				Client.Tell(new CleanupProducer(Self));
				var ex = new AlreadyClosedException($"The producer {_producerName} of the topic {Topic} was already closed when closing the producers");
				_pendingMessages.ForEach(msg =>
				{
					msg.Recycle();
				});
				_pendingMessages.Clear();
			}

			var requestId = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).Id;
			var cmd = _commands.NewCloseProducer(_producerId, requestId);
			var response = await cnx.Ask(new SendRequestWithId(cmd, requestId));
			if (!(response is Exception))
			{
				_log.Info($"[{Topic}] [{_producerName}] Closed Producer", Topic, _producerName);
				State.ConnectionState = HandlerState.State.Closed;
				_pendingMessages.ForEach(msg =>
				{
					msg.Recycle();
				});
				_pendingMessages.Clear();
				Client.Tell(new CleanupProducer(Self));
			}
		}

		protected internal override async ValueTask<bool> Connected()
		{
			var cnx = await Cnx();
			return cnx != null && (State.ConnectionState == HandlerState.State.Ready);
		}

		protected internal override async ValueTask<long> LastDisconnectedTimestamp()
		{
			return await _connectionHandler.Ask<long>(LastConnectionClosedTimestamp.Instance);
		}


		public virtual void Terminated(IActorRef cnx)
		{
			HandlerState.State previousState = State.GetAndUpdateState(State.ConnectionState == HandlerState.State.Closed ? HandlerState.State.Closed : HandlerState.State.Terminated);
			if(previousState != HandlerState.State.Terminated && previousState != HandlerState.State.Closed)
			{
				_log.Info($"[{Topic}] [{_producerName}] The topic has been terminated");
				
				FailPendingMessages(cnx, new TopicTerminatedException($"The topic {Topic} that the producer {_producerName} produces to has been terminated"));
			}
		}
		/// <summary>
		/// This fails and clears the pending messages with the given exception. This method should be called from within the
		/// ProducerImpl object mutex.
		/// </summary>
		private void FailPendingMessages(IActorRef cnx, PulsarClientException ex)
		{
			if (cnx == null)
			{
				int releaseCount = 0;

				bool batchMessagingEnabled = BatchMessagingEnabled;
				_pendingMessages.ForEach(op =>
				{
					releaseCount += batchMessagingEnabled ? op.NumMessagesInBatch : 1;
					try
					{
						ex.SequenceId = op.SequenceId;
						if (op.TotalChunks <= 1 || (op.ChunkId == op.TotalChunks - 1))
						{
						 _log.Error(ex?.ToString());
						}
					}
					catch (Exception t)
					{
						_log.Warning($"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {op.SequenceId}:");
					}
					op.Recycle();
				});

				_pendingMessages.Clear();
				if (batchMessagingEnabled)
				{
					FailPendingBatchMessages(ex);
				}

			}
			else
			{
				FailPendingMessages(null, ex);
			}
	}

	/// <summary>
	/// fail any pending batch messages that were enqueued, however batch was not closed out
	/// 
	/// </summary>
	private void FailPendingBatchMessages(PulsarClientException ex)
	{
		if (_batchMessageContainer.Empty)
		{
			return;
		}
		int numMessagesInBatch = _batchMessageContainer.NumMessagesInBatch;
		_batchMessageContainer.Discard(ex);
	}
	private async ValueTask AckReceived(long sequenceId, long highestSequenceId, long ledgerId, long entryId)
	{
			var cnx = await ClientCnx();
			if (_txnSequence.TryGetValue(sequenceId, out var txn))
				txn.Tell(new RegisterSendOp(new MessageId(ledgerId, entryId, _partitionIndex)));

			if (!_pendingMessages.TryDequeue(out var op))
			{
				var msg = $"[{Topic}] [{_producerName}] Got ack for timed out msg {sequenceId} - {highestSequenceId}";
				if (_log.IsDebugEnabled)
				{
					_log.Debug(msg);
				}
				return;
			}
			if (sequenceId > op.SequenceId)
			{
				var msg = $"[{Topic}] [{_producerName}] Got ack for msg. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {highestSequenceId} - queue-size: {_pendingMessages.Count}";
				_log.Warning(msg);
				// Force connection closing so that messages can be re-transmitted in a new connection
				cnx.Tell(Messages.Requests.Close.Instance);
			}
			else if (sequenceId < op.SequenceId)
			{
				var msg = $"[{Topic}] [{_producerName}] Got ack for timed out msg. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {highestSequenceId}";
				// Ignoring the ack since it's referring to a message that has already timed out.
				if (_log.IsDebugEnabled)
				{
					_log.Debug(msg);
				}
			}
			else
			{
				// Add check `sequenceId >= highestSequenceId` for backward compatibility.
				if (sequenceId >= highestSequenceId || highestSequenceId == op.HighestSequenceId)
				{
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}] [{_producerName}] Received ack for msg {sequenceId} ");
					}
					_lastSequenceIdPublished = Math.Max(_lastSequenceIdPublished, GetHighestSequenceId(op));
					op.SetMessageId(ledgerId, entryId, _partitionIndex);
					try
					{
						// if message is chunked then call callback only on last chunk
						if (op.TotalChunks <= 1 || (op.ChunkId == op.TotalChunks - 1))
						{
							if(op.Msg != null)
								SendComplete(op.Msg, DateTimeHelper.CurrentUnixTimeMillis(), null);
						}
					}
					catch (Exception t)
					{
						var msg = $"[{Topic}] [{_producerName}] Got exception while completing the callback for msg {sequenceId}";
						_log.Warning($"{msg}:{t}");
					}
				}
				else
				{
					var msg = $"[{Topic}] [{_producerName}] Got ack for batch msg error. expecting: {op.SequenceId} - {op.HighestSequenceId} - got: {sequenceId} - {highestSequenceId} - queue-size: {_pendingMessages.Count}";
					_log.Warning(msg);
					// Force connection closing so that messages can be re-transmitted in a new connection
					cnx.Tell(Messages.Requests.Close.Instance);
				}
			}
		}

		private long GetHighestSequenceId(OpSendMsg<T> op)
		{
			return Math.Max(op.HighestSequenceId, op.SequenceId);
		}

		private async ValueTask ResendMessages(ProducerResponse response)
		{
			var messagesToResend = _pendingMessages.Count;
			if (messagesToResend == 0)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[{Topic}] [{await ProducerName()}] No pending messages to resend {messagesToResend}");
				}

				if (State.ChangeToReadyState())
				{
					ProducerQueue.Producer.Add(new ProducerCreation(response));
					Become(Ready);
					return;
				}
				return;
			}
			_log.Info($"[{Topic}] [{await ProducerName()}] Re-Sending {messagesToResend} messages to server");
			await RecoverProcessOpSendMsgFrom(null);
		}
		
		private int BrokerChecksumSupportedVersion()
		{
			return (int)ProtocolVersion.V6;
		}

		/// <summary>
		/// Process sendTimeout events
		/// </summary>
		private async ValueTask FailTimedoutMessages()
		{
			if(_sendTimeout.IsCancellationRequested)
			{
				return;
			}

			long timeToWaitMs;
			// If it's closing/closed we need to ignore this timeout and not schedule next timeout.
			if (State.ConnectionState == HandlerState.State.Closing || State.ConnectionState == HandlerState.State.Closed)
			{
				return;
			}

			if (!_pendingMessages.TryPeek(out var firstMsg))
			{
				// If there are no pending messages, reset the timeout to the configured value.
				timeToWaitMs = Conf.SendTimeoutMs;
			}
			else
			{
				// If there is at least one message, calculate the diff between the message timeout and the elapsed
				// time since first message was created.
				long diff = Conf.SendTimeoutMs - (DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.CreatedAt);
				if (diff <= 0)
				{
					// The diff is less than or equal to zero, meaning that the message has been timed out.
					// Set the callback to timeout on every message, then clear the pending queue.
					_log.Info("[{}] [{}] Message send timed out. Failing {} messages", Topic, _producerName, _pendingMessages.Count);

					var te = new PulsarClientException.TimeoutException($"The producer {_producerName} can not send message to the topic {Topic} within given timeout: {firstMsg.SequenceId}");
					FailPendingMessages(await Cnx(), te);
					_stats.IncrementSendFailed(_pendingMessages.Count);
					// Since the pending queue is cleared now, set timer to expire after configured value.
					timeToWaitMs = Conf.SendTimeoutMs;
				}
				else
				{
					// The diff is greater than zero, set the timeout to the diff value
					timeToWaitMs = diff; 
				}
			}
			_sendTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), Self, RunSendTimeout.Instance, ActorRefs.NoSender);

		}


		private async ValueTask Flush()
		{
			if (BatchMessagingEnabled)
			{
				await BatchMessageAndSend();
			}
		}

		private async ValueTask TriggerFlush()
		{
			if(BatchMessagingEnabled)
			{
				await BatchMessageAndSend();
			}
		}

		// must acquire semaphore before enqueuing
		private async ValueTask BatchMessageAndSend()
		{
			if(_log.IsDebugEnabled)
			{
				_log.Info($"[{Topic}] [{_producerName}] Batching the messages from the batch container with {_batchMessageContainer.NumMessagesInBatch} messages");
			}
			if(!_batchMessageContainer.Empty)
			{
				try
				{
					IList<OpSendMsg<T>> opSendMsgs;
					if(_batchMessageContainer.MultiBatches)
					{
						opSendMsgs = _batchMessageContainer.CreateOpSendMsgs();
					}
					else
					{
						opSendMsgs = new List<OpSendMsg<T>> { _batchMessageContainer.CreateOpSendMsg() };
					}
					_batchMessageContainer.Clear();
					foreach(var opSendMsg in opSendMsgs)
					{
						await ProcessOpSendMsg(opSendMsg);
					}
				}
				catch(Exception t)
				{
					_log.Warning($"[{Topic}] [{_producerName}] error while create opSendMsg by batch message container: {t}");
				}
			}
		}
		private async ValueTask ProcessOpSendMsg(OpSendMsg<T> op)
		{
			try
			{
				if (op.Msg != null && BatchMessagingEnabled)
				{
					await BatchMessageAndSend();
				}
				_pendingMessages.Enqueue(op);
				if (op.Msg != null)
				{
					LastSequenceIdPushed = Math.Max(LastSequenceIdPushed, GetHighestSequenceId(op));
				}
				if(await Connected())
				{
					var cnx = await ClientCnx();
					if (op.Msg != null && op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
					{
						await TryRegisterSchema(cnx, op.Msg);
					}

					_stats.UpdateNumMsgsSent(op.NumMessagesInBatch, op.BatchSizeByte);
					SendCommand(op, cnx);
				}
				else
				{
					//stash message
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}] [{_producerName}] Connection is not ready -- sequenceId {op.SequenceId}");
					}
				}
			}
			catch (Exception t)
			{
				_log.Warning($"[{Topic}] [{await ProducerName()}] error while closing out batch -- {t}");
				SendComplete(op.Msg, DateTimeHelper.CurrentUnixTimeMillis(), new PulsarClientException(t.ToString(), op.SequenceId));
			}
		}
		private void SendComplete(Message<T> interceptorMessage, long createdAt, Exception e)
		{
			if (e != null)
			{
				_stats.IncrementSendFailed();
				OnSendAcknowledgement(interceptorMessage, null, e);
				_log.Error(e.ToString());
			}
			else
			{
				OnSendAcknowledgement(interceptorMessage, interceptorMessage.MessageId, null);
				_stats.IncrementNumAcksReceived(DateTimeHelper.CurrentUnixTimeMillis() - createdAt);
			}

		}
		private async ValueTask RecoverProcessOpSendMsgFrom(Message<T> from)
		{
			var pendingMessages = _pendingMessages;
			OpSendMsg<T> pendingRegisteringOp = null;
			foreach (var op in pendingMessages)
			{
				if (from != null)
				{
					if (op?.Msg == from)
					{
						from = null;
					}
					else
					{
						continue;
					}
					if (op?.Msg != null)
					{
						if (op.Msg.GetSchemaState() == Message<T>.SchemaState.None)
						{
							if (!RePopulateMessageSchema(op.Msg))
							{
								pendingRegisteringOp = op;
								break;
							}
						}
						else if (op.Msg.GetSchemaState() == Message<T>.SchemaState.Broken)
						{
							_pendingMessages = new Queue<OpSendMsg<T>>(_pendingMessages.Where(x=> x != op));
							op.Recycle();
							continue;
						}
					}

					if (_log.IsDebugEnabled)
					{
						_log.Debug($"[{Topic}] [{await ProducerName()}] Re-Sending message in sequenceId {op.SequenceId}");
					}
				}
			}
			if (pendingRegisteringOp != null)
			{
				var cnx = await ClientCnx();
				await TryRegisterSchema(cnx, pendingRegisteringOp.Msg);
			}
		}

		public virtual long DelayInMillis
		{
			get
			{
				var firstMsg = _pendingMessages.FirstOrDefault();
				if (firstMsg != null)
				{
					return DateTimeHelper.CurrentUnixTimeMillis() - firstMsg.CreatedAt;
				}
				return 0L;
			}
		}

		public virtual string ConnectionId
		{
			get
			{
				return Cnx() != null ? _connectionId : null;
			}
		}
		private bool HasPublishTime(ulong seq)
		{
			if (seq > 0)
				return true;
			return false;
		}
		private bool HasEventTime(ulong seq)
		{
			if (seq > 0)
				return true;
			return false;
		}
		public virtual string ConnectedSince
		{
			get
			{
				return Cnx() != null ? _connectedSince : null;
			}
		}

		public virtual int PendingQueueSize
		{
			get
			{
				return _pendingMessages.Count;
			}
		}

		protected internal override async ValueTask<string> ProducerName()
		{
			return await Task.FromResult(_producerName);
		}

		// wrapper for connection methods
		private async ValueTask<IActorRef> Cnx()
		{
			return await _connectionHandler.Ask<IActorRef>(GetCnx.Instance);
		}

		private void ConnectionClosed(IActorRef cnx)
		{
			_connectionHandler.Tell(new ConnectionClosed(cnx));
		}

		internal async ValueTask<IActorRef> ClientCnx()
		{
			return await Cnx();
		}

		protected internal override async ValueTask<IProducerStats> Stats() => await Task.FromResult(_stats);
		protected internal sealed class OpSendMsg<T1>
		{
			internal Message<T1> Msg;
			internal IList<Message<T1>> Msgs;
			internal byte[] Cmd;
			internal long SequenceId;
			internal long CreatedAt;
			internal long HighestSequenceId;

			internal int TotalChunks = 0;
			internal int ChunkId = -1;

			internal static OpSendMsg<T1> Create(Message<T1> msg, byte[] cmd, long sequenceId)
			{
				var op = new OpSendMsg<T1>
				{
					Msg = msg,
					Cmd = cmd,
					SequenceId = sequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
				};
				return op;
			}

			internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, byte[] cmd, long sequenceId)
			{
				var op = new OpSendMsg<T1>
				{
					Msgs = msgs,
					Cmd = cmd,
					SequenceId = sequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
				};
				return op;
			}

			internal static OpSendMsg<T1> Create(IList<Message<T1>> msgs, byte[] cmd, long lowestSequenceId, long highestSequenceId)
			{
				var op = new OpSendMsg<T1>
				{
					Msgs = msgs,
					Cmd = cmd,
					SequenceId = lowestSequenceId,
					HighestSequenceId = highestSequenceId,
					CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
				};
				return op;
			}

			internal int NumMessagesInBatch { get; set; } = 1;

			internal long BatchSizeByte { get; set; } = 0;

			internal void SetMessageId(long ledgerId, long entryId, int partitionIndex)
			{
				if (Msg != null)
				{
					Msg.SetMessageId(new MessageId(ledgerId, entryId, partitionIndex));
				}
				else
				{
					for (var batchIndex = 0; batchIndex < Msgs.Count; batchIndex++)
					{
						Msgs[batchIndex].SetMessageId(new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex));
					}
				}
			}


			internal void Recycle()
			{
				Msg = null;
				Msgs = null;
				Cmd = null;
				SequenceId = -1L;
				CreatedAt = -1L;
				HighestSequenceId = -1L;
				TotalChunks = 0;
				ChunkId = -1;
			}

		}
	}
	internal sealed class RunSendTimeout
	{
		internal static RunSendTimeout Instance = new RunSendTimeout();
    }
	internal sealed class BatchTask
	{
		internal static BatchTask Instance = new BatchTask();
    }
}