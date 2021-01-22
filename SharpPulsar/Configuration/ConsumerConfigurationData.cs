﻿using SharpPulsar.Api;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;
using SharpPulsar.Utils;
using SharpPulsar.Interfaces;
using SharpPulsar.Common;
using SharpPulsar.Impl;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using BAMCIS.Util.Concurrent;

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
namespace SharpPulsar.Configuration
{
	public sealed class ConsumerConfigurationData<T>
	{
		public IMessageCrypto MessageCrypto { get; set; }
		public IMessageId StartMessageId { get; set; }
        public ConsumptionType ConsumptionType { get; set; } = ConsumptionType.Listener;
		public ISet<string> TopicNames { get; set; } = new SortedSet<string>();
		public List<IConsumerInterceptor<T>> Interceptors { get; set; }
		public SubType SubscriptionType { get; set; } = SubType.Exclusive;
		public IMessageListener<T> MessageListener { get; set; }
        public bool ForceTopicCreation { get; set; } = false;
		public IConsumerEventListener ConsumerEventListener { get; set; }
        public bool UseTls { get; set; } = false;
		public int ReceiverQueueSize { get; set; } = 1_000;

		public long AcknowledgementsGroupTimeMicros { get; set; } = TimeUnit.MILLISECONDS.ToMicroseconds(100);

		public long NegativeAckRedeliveryDelayMs { get; set; } = 30000;

		public int MaxTotalReceiverQueueSizeAcrossPartitions { get; set; } = 50000;

		public long AckTimeoutMillis { get; set; } = 0;

		public long TickDurationMillis { get; set; } = 1000;

		public int PriorityLevel { get; set; } = 0;

        public int MaxPendingChuckedMessage { get; set; }
        public long ExpireTimeOfIncompleteChunkedMessageMillis { get; set; }
        public bool AutoAckOldestChunkedMessageOnQueueFull { get; set; }

        public bool BatchConsume { get; set; } = false;
        public bool BatchIndexAckEnabled { get; set; } = false;
		public long BatchConsumeTimeout { get; set; } = 30_000; //30 seconds

		public ICryptoKeyReader CryptoKeyReader { get; set; }

		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		public int PatternAutoDiscoveryPeriod { get; set; } = 5;

	    public SubscriptionMode SubscriptionMode = SubscriptionMode.Durable;

		public RegexSubscriptionMode RegexSubscriptionMode { get; set; } = RegexSubscriptionMode.PersistentOnly;

		
        public BatchReceivePolicy BatchReceivePolicy { get; set; }

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool ReplicateSubscriptionState { get; set; } = false;
		public bool RetryEnable { get; set; } = false;

		public bool ResetIncludeHead { get; set; } = false;

        public KeySharedPolicy KeySharedPolicy { get; set; }


		public bool ReadCompacted { get; set; }

		public DeadLetterPolicy DeadLetterPolicy { get; set; }

        public SubscriptionInitialPosition SubscriptionInitialPosition { get; set; } =
            SubscriptionInitialPosition.Earliest;
		public Regex TopicsPattern { get; set; }

		public SortedDictionary<string, string> Properties { get; set; }

		public string ConsumerName { get; set; }

		public string SubscriptionName { get; set; }
		public string SingleTopic
		{
			get => TopicNames.Count == 1 ? TopicNames.First() : string.Empty;
            set => TopicNames = new HashSet<string> {value};
        }

    }

    public enum ConsumptionType
    {
		Queue,
		Listener
    }
}