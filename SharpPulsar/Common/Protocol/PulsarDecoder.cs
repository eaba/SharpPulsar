﻿/// <summary>
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
namespace org.apache.pulsar.common.protocol
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;

	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using BaseCommand = org.apache.pulsar.common.api.proto.PulsarApi.BaseCommand;
	using CommandAck = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck;
	using CommandActiveConsumerChange = org.apache.pulsar.common.api.proto.PulsarApi.CommandActiveConsumerChange;
	using CommandAddPartitionToTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddPartitionToTxn;
	using CommandAddPartitionToTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddPartitionToTxnResponse;
	using CommandAddSubscriptionToTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddSubscriptionToTxn;
	using CommandAddSubscriptionToTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAddSubscriptionToTxnResponse;
	using CommandAuthChallenge = org.apache.pulsar.common.api.proto.PulsarApi.CommandAuthChallenge;
	using CommandAuthResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandAuthResponse;
	using CommandCloseConsumer = org.apache.pulsar.common.api.proto.PulsarApi.CommandCloseConsumer;
	using CommandCloseProducer = org.apache.pulsar.common.api.proto.PulsarApi.CommandCloseProducer;
	using CommandConnect = org.apache.pulsar.common.api.proto.PulsarApi.CommandConnect;
	using CommandConnected = org.apache.pulsar.common.api.proto.PulsarApi.CommandConnected;
	using CommandConsumerStats = org.apache.pulsar.common.api.proto.PulsarApi.CommandConsumerStats;
	using CommandConsumerStatsResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandConsumerStatsResponse;
	using CommandEndTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxn;
	using CommandEndTxnOnPartition = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnPartition;
	using CommandEndTxnOnPartitionResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnPartitionResponse;
	using CommandEndTxnOnSubscription = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnSubscription;
	using CommandEndTxnOnSubscriptionResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnOnSubscriptionResponse;
	using CommandEndTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandEndTxnResponse;
	using CommandError = org.apache.pulsar.common.api.proto.PulsarApi.CommandError;
	using CommandFlow = org.apache.pulsar.common.api.proto.PulsarApi.CommandFlow;
	using CommandGetOrCreateSchema = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetOrCreateSchema;
	using CommandGetOrCreateSchemaResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetOrCreateSchemaResponse;
	using CommandGetSchema = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetSchema;
	using CommandGetSchemaResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetSchemaResponse;
	using CommandGetTopicsOfNamespace = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace;
	using CommandGetTopicsOfNamespaceResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespaceResponse;
	using CommandLookupTopic = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopic;
	using CommandLookupTopicResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopicResponse;
	using CommandMessage = org.apache.pulsar.common.api.proto.PulsarApi.CommandMessage;
	using CommandNewTxn = org.apache.pulsar.common.api.proto.PulsarApi.CommandNewTxn;
	using CommandNewTxnResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandNewTxnResponse;
	using CommandPartitionedTopicMetadata = org.apache.pulsar.common.api.proto.PulsarApi.CommandPartitionedTopicMetadata;
	using CommandPartitionedTopicMetadataResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandPartitionedTopicMetadataResponse;
	using CommandPing = org.apache.pulsar.common.api.proto.PulsarApi.CommandPing;
	using CommandPong = org.apache.pulsar.common.api.proto.PulsarApi.CommandPong;
	using CommandProducer = org.apache.pulsar.common.api.proto.PulsarApi.CommandProducer;
	using CommandProducerSuccess = org.apache.pulsar.common.api.proto.PulsarApi.CommandProducerSuccess;
	using CommandReachedEndOfTopic = org.apache.pulsar.common.api.proto.PulsarApi.CommandReachedEndOfTopic;
	using CommandRedeliverUnacknowledgedMessages = org.apache.pulsar.common.api.proto.PulsarApi.CommandRedeliverUnacknowledgedMessages;
	using CommandSeek = org.apache.pulsar.common.api.proto.PulsarApi.CommandSeek;
	using CommandSend = org.apache.pulsar.common.api.proto.PulsarApi.CommandSend;
	using CommandSendError = org.apache.pulsar.common.api.proto.PulsarApi.CommandSendError;
	using CommandSendReceipt = org.apache.pulsar.common.api.proto.PulsarApi.CommandSendReceipt;
	using CommandSubscribe = org.apache.pulsar.common.api.proto.PulsarApi.CommandSubscribe;
	using CommandSuccess = org.apache.pulsar.common.api.proto.PulsarApi.CommandSuccess;
	using CommandUnsubscribe = org.apache.pulsar.common.api.proto.PulsarApi.CommandUnsubscribe;
	using ByteBufCodedInputStream = org.apache.pulsar.common.util.protobuf.ByteBufCodedInputStream;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// Basic implementation of the channel handler to process inbound Pulsar data.
	/// </summary>
	public abstract class PulsarDecoder : ChannelInboundHandlerAdapter
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void channelRead(io.netty.channel.ChannelHandlerContext ctx, Object msg) throws Exception
		public override void channelRead(ChannelHandlerContext ctx, object msg)
		{
			// Get a buffer that contains the full frame
			ByteBuf buffer = (ByteBuf) msg;
			PulsarApi.BaseCommand cmd = null;
			PulsarApi.BaseCommand.Builder cmdBuilder = null;

			try
			{
				// De-serialize the command
				int cmdSize = (int) buffer.readUnsignedInt();
				int writerIndex = buffer.writerIndex();
				buffer.writerIndex(buffer.readerIndex() + cmdSize);
				ByteBufCodedInputStream cmdInputStream = ByteBufCodedInputStream.get(buffer);
				cmdBuilder = PulsarApi.BaseCommand.newBuilder();
				cmd = cmdBuilder.mergeFrom(cmdInputStream, null).build();
				buffer.writerIndex(writerIndex);

				cmdInputStream.recycle();

				if (log.DebugEnabled)
				{
					log.debug("[{}] Received cmd {}", ctx.channel().remoteAddress(), cmd.getType());
				}

				messageReceived();

				switch (cmd.getType())
				{
				case PARTITIONED_METADATA:
					checkArgument(cmd.hasPartitionMetadata());
					handlePartitionMetadataRequest(cmd.PartitionMetadata);
					cmd.PartitionMetadata.recycle();
					break;

				case PARTITIONED_METADATA_RESPONSE:
					checkArgument(cmd.hasPartitionMetadataResponse());
					handlePartitionResponse(cmd.PartitionMetadataResponse);
					cmd.PartitionMetadataResponse.recycle();
					break;

				case LOOKUP:
					checkArgument(cmd.hasLookupTopic());
					handleLookup(cmd.LookupTopic);
					cmd.LookupTopic.recycle();
					break;

				case LOOKUP_RESPONSE:
					checkArgument(cmd.hasLookupTopicResponse());
					handleLookupResponse(cmd.LookupTopicResponse);
					cmd.LookupTopicResponse.recycle();
					break;

				case ACK:
					checkArgument(cmd.hasAck());
					PulsarApi.CommandAck ack = cmd.Ack;
					handleAck(ack);
					for (int i = 0; i < ack.MessageIdCount; i++)
					{
						ack.getMessageId(i).recycle();
					}
					ack.recycle();
					break;

				case CLOSE_CONSUMER:
					checkArgument(cmd.hasCloseConsumer());
					handleCloseConsumer(cmd.CloseConsumer);
					cmd.CloseConsumer.recycle();
					break;

				case CLOSE_PRODUCER:
					checkArgument(cmd.hasCloseProducer());
					handleCloseProducer(cmd.CloseProducer);
					cmd.CloseProducer.recycle();
					break;

				case CONNECT:
					checkArgument(cmd.hasConnect());
					handleConnect(cmd.Connect);
					cmd.Connect.recycle();
					break;
				case CONNECTED:
					checkArgument(cmd.hasConnected());
					handleConnected(cmd.Connected);
					cmd.Connected.recycle();
					break;

				case ERROR:
					checkArgument(cmd.hasError());
					handleError(cmd.Error);
					cmd.Error.recycle();
					break;

				case FLOW:
					checkArgument(cmd.hasFlow());
					handleFlow(cmd.Flow);
					cmd.Flow.recycle();
					break;

				case MESSAGE:
				{
					checkArgument(cmd.hasMessage());
					handleMessage(cmd.Message, buffer);
					cmd.Message.recycle();
					break;
				}
				case PRODUCER:
					checkArgument(cmd.hasProducer());
					handleProducer(cmd.Producer);
					cmd.Producer.recycle();
					break;

				case SEND:
				{
					checkArgument(cmd.hasSend());

					// Store a buffer marking the content + headers
					ByteBuf headersAndPayload = buffer.markReaderIndex();
					handleSend(cmd.Send, headersAndPayload);
					cmd.Send.recycle();
					break;
				}
				case SEND_ERROR:
					checkArgument(cmd.hasSendError());
					handleSendError(cmd.SendError);
					cmd.SendError.recycle();
					break;

				case SEND_RECEIPT:
					checkArgument(cmd.hasSendReceipt());
					handleSendReceipt(cmd.SendReceipt);
					cmd.SendReceipt.recycle();
					break;

				case SUBSCRIBE:
					checkArgument(cmd.hasSubscribe());
					handleSubscribe(cmd.Subscribe);
					cmd.Subscribe.recycle();
					break;

				case SUCCESS:
					checkArgument(cmd.hasSuccess());
					handleSuccess(cmd.Success);
					cmd.Success.recycle();
					break;

				case PRODUCER_SUCCESS:
					checkArgument(cmd.hasProducerSuccess());
					handleProducerSuccess(cmd.ProducerSuccess);
					cmd.ProducerSuccess.recycle();
					break;

				case UNSUBSCRIBE:
					checkArgument(cmd.hasUnsubscribe());
					handleUnsubscribe(cmd.Unsubscribe);
					cmd.Unsubscribe.recycle();
					break;

				case SEEK:
					checkArgument(cmd.hasSeek());
					handleSeek(cmd.Seek);
					cmd.Seek.recycle();
					break;

				case PING:
					checkArgument(cmd.hasPing());
					handlePing(cmd.Ping);
					cmd.Ping.recycle();
					break;

				case PONG:
					checkArgument(cmd.hasPong());
					handlePong(cmd.Pong);
					cmd.Pong.recycle();
					break;

				case REDELIVER_UNACKNOWLEDGED_MESSAGES:
					checkArgument(cmd.hasRedeliverUnacknowledgedMessages());
					handleRedeliverUnacknowledged(cmd.RedeliverUnacknowledgedMessages);
					cmd.RedeliverUnacknowledgedMessages.recycle();
					break;

				case CONSUMER_STATS:
					checkArgument(cmd.hasConsumerStats());
					handleConsumerStats(cmd.ConsumerStats);
					cmd.ConsumerStats.recycle();
					break;

				case CONSUMER_STATS_RESPONSE:
					checkArgument(cmd.hasConsumerStatsResponse());
					handleConsumerStatsResponse(cmd.ConsumerStatsResponse);
					cmd.ConsumerStatsResponse.recycle();
					break;

				case REACHED_END_OF_TOPIC:
					checkArgument(cmd.hasReachedEndOfTopic());
					handleReachedEndOfTopic(cmd.ReachedEndOfTopic);
					cmd.ReachedEndOfTopic.recycle();
					break;

				case GET_LAST_MESSAGE_ID:
					checkArgument(cmd.hasGetLastMessageId());
					handleGetLastMessageId(cmd.GetLastMessageId);
					cmd.GetLastMessageId.recycle();
					break;

				case GET_LAST_MESSAGE_ID_RESPONSE:
					checkArgument(cmd.hasGetLastMessageIdResponse());
					handleGetLastMessageIdSuccess(cmd.GetLastMessageIdResponse);
					cmd.GetLastMessageIdResponse.recycle();
					break;

				case ACTIVE_CONSUMER_CHANGE:
					handleActiveConsumerChange(cmd.ActiveConsumerChange);
					cmd.ActiveConsumerChange.recycle();
					break;

				case GET_TOPICS_OF_NAMESPACE:
					checkArgument(cmd.hasGetTopicsOfNamespace());
					handleGetTopicsOfNamespace(cmd.GetTopicsOfNamespace);
					cmd.GetTopicsOfNamespace.recycle();
					break;

				case GET_TOPICS_OF_NAMESPACE_RESPONSE:
					checkArgument(cmd.hasGetTopicsOfNamespaceResponse());
					handleGetTopicsOfNamespaceSuccess(cmd.GetTopicsOfNamespaceResponse);
					cmd.GetTopicsOfNamespaceResponse.recycle();
					break;

				case GET_SCHEMA:
					checkArgument(cmd.hasGetSchema());
					handleGetSchema(cmd.GetSchema);
					cmd.GetSchema.recycle();
					break;

				case GET_SCHEMA_RESPONSE:
					checkArgument(cmd.hasGetSchemaResponse());
					handleGetSchemaResponse(cmd.GetSchemaResponse);
					cmd.GetSchemaResponse.recycle();
					break;

				case GET_OR_CREATE_SCHEMA:
					checkArgument(cmd.hasGetOrCreateSchema());
					handleGetOrCreateSchema(cmd.GetOrCreateSchema);
					cmd.GetOrCreateSchema.recycle();
					break;

				case GET_OR_CREATE_SCHEMA_RESPONSE:
					checkArgument(cmd.hasGetOrCreateSchemaResponse());
					handleGetOrCreateSchemaResponse(cmd.GetOrCreateSchemaResponse);
					cmd.GetOrCreateSchemaResponse.recycle();
					break;

				case AUTH_CHALLENGE:
					checkArgument(cmd.hasAuthChallenge());
					handleAuthChallenge(cmd.AuthChallenge);
					cmd.AuthChallenge.recycle();
					break;

				case AUTH_RESPONSE:
					checkArgument(cmd.hasAuthResponse());
					handleAuthResponse(cmd.AuthResponse);
					cmd.AuthResponse.recycle();
					break;

				case NEW_TXN:
					checkArgument(cmd.hasNewTxn());
					handleNewTxn(cmd.NewTxn);
					cmd.NewTxn.recycle();
					break;

				case NEW_TXN_RESPONSE:
					checkArgument(cmd.hasNewTxnResponse());
					handleNewTxnResponse(cmd.NewTxnResponse);
					cmd.NewTxnResponse.recycle();
					break;

				case ADD_PARTITION_TO_TXN:
					checkArgument(cmd.hasAddPartitionToTxn());
					handleAddPartitionToTxn(cmd.AddPartitionToTxn);
					cmd.AddPartitionToTxn.recycle();
					break;

				case ADD_PARTITION_TO_TXN_RESPONSE:
					checkArgument(cmd.hasAddPartitionToTxnResponse());
					handleAddPartitionToTxnResponse(cmd.AddPartitionToTxnResponse);
					cmd.AddPartitionToTxnResponse.recycle();
					break;

				case ADD_SUBSCRIPTION_TO_TXN:
					checkArgument(cmd.hasAddSubscriptionToTxn());
					handleAddSubscriptionToTxn(cmd.AddSubscriptionToTxn);
					cmd.AddSubscriptionToTxn.recycle();
					break;

				case ADD_SUBSCRIPTION_TO_TXN_RESPONSE:
					checkArgument(cmd.hasAddSubscriptionToTxnResponse());
					handleAddSubscriptionToTxnResponse(cmd.AddSubscriptionToTxnResponse);
					cmd.AddSubscriptionToTxnResponse.recycle();
					break;

				case END_TXN:
					checkArgument(cmd.hasEndTxn());
					handleEndTxn(cmd.EndTxn);
					cmd.EndTxn.recycle();
					break;

				case END_TXN_RESPONSE:
					checkArgument(cmd.hasEndTxnResponse());
					handleEndTxnResponse(cmd.EndTxnResponse);
					cmd.EndTxnResponse.recycle();
					break;

				case END_TXN_ON_PARTITION:
					checkArgument(cmd.hasEndTxnOnPartition());
					handleEndTxnOnPartition(cmd.EndTxnOnPartition);
					cmd.EndTxnOnPartition.recycle();
					break;

				case END_TXN_ON_PARTITION_RESPONSE:
					checkArgument(cmd.hasEndTxnOnPartitionResponse());
					handleEndTxnOnPartitionResponse(cmd.EndTxnOnPartitionResponse);
					cmd.EndTxnOnPartitionResponse.recycle();
					break;

				case END_TXN_ON_SUBSCRIPTION:
					checkArgument(cmd.hasEndTxnOnSubscription());
					handleEndTxnOnSubscription(cmd.EndTxnOnSubscription);
					cmd.EndTxnOnSubscription.recycle();
					break;

				case END_TXN_ON_SUBSCRIPTION_RESPONSE:
					checkArgument(cmd.hasEndTxnOnSubscriptionResponse());
					handleEndTxnOnSubscriptionResponse(cmd.EndTxnOnSubscriptionResponse);
					cmd.EndTxnOnSubscriptionResponse.recycle();
					break;
				}
			}
			finally
			{
				if (cmdBuilder != null)
				{
					cmdBuilder.recycle();
				}

				if (cmd != null)
				{
					cmd.recycle();
				}

				buffer.release();
			}
		}

		protected internal abstract void messageReceived();

		protected internal virtual void handlePartitionMetadataRequest(PulsarApi.CommandPartitionedTopicMetadata response)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handlePartitionResponse(PulsarApi.CommandPartitionedTopicMetadataResponse response)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleLookup(PulsarApi.CommandLookupTopic lookup)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleLookupResponse(PulsarApi.CommandLookupTopicResponse connection)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleConnect(PulsarApi.CommandConnect connect)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleConnected(PulsarApi.CommandConnected connected)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleSubscribe(PulsarApi.CommandSubscribe subscribe)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleProducer(PulsarApi.CommandProducer producer)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleSend(PulsarApi.CommandSend send, ByteBuf headersAndPayload)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleSendReceipt(PulsarApi.CommandSendReceipt sendReceipt)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleSendError(PulsarApi.CommandSendError sendError)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleMessage(PulsarApi.CommandMessage cmdMessage, ByteBuf headersAndPayload)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAck(PulsarApi.CommandAck ack)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleFlow(PulsarApi.CommandFlow flow)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleRedeliverUnacknowledged(PulsarApi.CommandRedeliverUnacknowledgedMessages redeliver)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleUnsubscribe(PulsarApi.CommandUnsubscribe unsubscribe)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleSeek(PulsarApi.CommandSeek seek)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleActiveConsumerChange(PulsarApi.CommandActiveConsumerChange change)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleSuccess(PulsarApi.CommandSuccess success)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleProducerSuccess(PulsarApi.CommandProducerSuccess success)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleError(PulsarApi.CommandError error)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleCloseProducer(PulsarApi.CommandCloseProducer closeProducer)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleCloseConsumer(PulsarApi.CommandCloseConsumer closeConsumer)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handlePing(PulsarApi.CommandPing ping)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handlePong(PulsarApi.CommandPong pong)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleConsumerStats(PulsarApi.CommandConsumerStats commandConsumerStats)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleConsumerStatsResponse(PulsarApi.CommandConsumerStatsResponse commandConsumerStatsResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleReachedEndOfTopic(PulsarApi.CommandReachedEndOfTopic commandReachedEndOfTopic)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetLastMessageId(PulsarApi.CommandGetLastMessageId getLastMessageId)
		{
			throw new System.NotSupportedException();
		}
		protected internal virtual void handleGetLastMessageIdSuccess(PulsarApi.CommandGetLastMessageIdResponse success)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetTopicsOfNamespace(PulsarApi.CommandGetTopicsOfNamespace commandGetTopicsOfNamespace)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetTopicsOfNamespaceSuccess(PulsarApi.CommandGetTopicsOfNamespaceResponse response)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetSchema(PulsarApi.CommandGetSchema commandGetSchema)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetSchemaResponse(PulsarApi.CommandGetSchemaResponse commandGetSchemaResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetOrCreateSchema(PulsarApi.CommandGetOrCreateSchema commandGetOrCreateSchema)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleGetOrCreateSchemaResponse(PulsarApi.CommandGetOrCreateSchemaResponse commandGetOrCreateSchemaResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAuthResponse(PulsarApi.CommandAuthResponse commandAuthResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAuthChallenge(PulsarApi.CommandAuthChallenge commandAuthChallenge)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleNewTxn(PulsarApi.CommandNewTxn commandNewTxn)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleNewTxnResponse(PulsarApi.CommandNewTxnResponse commandNewTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAddPartitionToTxn(PulsarApi.CommandAddPartitionToTxn commandAddPartitionToTxn)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAddPartitionToTxnResponse(PulsarApi.CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAddSubscriptionToTxn(PulsarApi.CommandAddSubscriptionToTxn commandAddSubscriptionToTxn)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleAddSubscriptionToTxnResponse(PulsarApi.CommandAddSubscriptionToTxnResponse commandAddSubscriptionToTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleEndTxn(PulsarApi.CommandEndTxn commandEndTxn)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleEndTxnResponse(PulsarApi.CommandEndTxnResponse commandEndTxnResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleEndTxnOnPartition(PulsarApi.CommandEndTxnOnPartition commandEndTxnOnPartition)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleEndTxnOnPartitionResponse(PulsarApi.CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleEndTxnOnSubscription(PulsarApi.CommandEndTxnOnSubscription commandEndTxnOnSubscription)
		{
			throw new System.NotSupportedException();
		}

		protected internal virtual void handleEndTxnOnSubscriptionResponse(PulsarApi.CommandEndTxnOnSubscriptionResponse commandEndTxnOnSubscriptionResponse)
		{
			throw new System.NotSupportedException();
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarDecoder));
	}

}