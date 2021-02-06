﻿using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Batch.Api;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Reader;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Queues;
using SharpPulsar.Utility;
using System;
using System.Linq;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
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
    public class ReaderActor<T>: ReceiveActor
	{
		private static readonly BatchReceivePolicy _disabledBatchReceivePolicy = new BatchReceivePolicy.Builder().Timeout((int)TimeUnit.MILLISECONDS.ToMilliseconds(0)).MaxNumMessages(1).Build();
		private readonly IActorRef _consumer;

		public ReaderActor(IActorRef client, ReaderConfigurationData<T> readerConfiguration, IAdvancedScheduler listenerExecutor, ISchema<T> schema, ClientConfigurationData clientConfigurationData, ConsumerQueueCollections<T> consumerQueue)
		{
			var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
			if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
			{
				subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
			}

			ConsumerConfigurationData<T> consumerConfiguration = new ConsumerConfigurationData<T>();
			consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
			consumerConfiguration.SubscriptionName = subscription;
			consumerConfiguration.SubscriptionType = SubType.Exclusive;
			consumerConfiguration.SubscriptionMode = SubscriptionMode.NonDurable;
			consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
			consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;

			// Reader doesn't need any batch receiving behaviours
			// disable the batch receive timer for the ConsumerImpl instance wrapped by the ReaderImpl
			consumerConfiguration.BatchReceivePolicy = _disabledBatchReceivePolicy;

			if(readerConfiguration.ReaderName != null)
			{
				consumerConfiguration.ConsumerName = readerConfiguration.ReaderName;
			}

			if(readerConfiguration.ResetIncludeHead)
			{
				consumerConfiguration.ResetIncludeHead = true;
			}

			if(readerConfiguration.ReaderListener != null)
			{
				var readerListener = readerConfiguration.ReaderListener;
				consumerConfiguration.MessageListener = new MessageListenerAnonymousInnerClass(Self, readerListener);
			}

			consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
			if(readerConfiguration.CryptoKeyReader != null)
			{
				consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
			}

			if(readerConfiguration.KeyHashRanges != null)
			{
				consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().GetRanges(readerConfiguration.KeyHashRanges.ToArray());
			}

			int partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
			_consumer = Context.ActorOf(ConsumerActor<T>.NewConsumer(client, readerConfiguration.TopicName, consumerConfiguration, listenerExecutor, partitionIdx, false, readerConfiguration.StartMessageId, schema, null, true, readerConfiguration.StartMessageFromRollbackDurationInSec, clientConfigurationData, consumerQueue));
			Receive<ReadNext>(_ => {
				_consumer.Tell(Messages.Consumer.Receive.Instance);
			
			});
			Receive<ReadNextTimeout>(m => {
				_consumer.Tell(new ReceiveWithTimeout(m.Timeout, m.Unit));

			});
			Receive<HasReachedEndOfTopic>(m => {
				_consumer.Tell(m);

			});
			Receive<AcknowledgeCumulativeMessage<T>> (m => {
				_consumer.Tell(m);
			});
			Receive<HasMessageAvailable> (m => {
				_consumer.Tell(m);
			});
			Receive<GetTopic> (m => {
				_consumer.Tell(m);
			});
			Receive<IsConnected> (m => {
				_consumer.Tell(m);
			});
			Receive<SeekMessageId> (m => {
				_consumer.Tell(m);
			});
			Receive<SeekTimestamp> (m => {
				_consumer.Tell(m);
			});
		}

		private class MessageListenerAnonymousInnerClass : IMessageListener<T>
		{
			private readonly IActorRef _outerInstance;

			private IReaderListener<T> _readerListener;

			public MessageListenerAnonymousInnerClass(IActorRef outerInstance, IReaderListener<T> readerListener)
			{
				_outerInstance = outerInstance;
				_readerListener = readerListener;
			}


			public void Received(IActorRef consumer, IMessage<T> msg)
			{
				_readerListener.Received(_outerInstance, msg);
				consumer.Tell(new AcknowledgeCumulativeMessage<T>(msg));
			}

			public void ReachedEndOfTopic(IActorRef consumer)
			{
				_readerListener.ReachedEndOfTopic(_outerInstance);
			}
		}

		public virtual IActorRef Consumer
		{
			get
			{
				return _consumer;
			}
		}


        protected override void PostStop()
        {
			_consumer.GracefulStop(TimeSpan.FromSeconds(1));
            base.PostStop();
        }

	}

}