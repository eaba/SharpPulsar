﻿using System.Collections.Generic;
using System.IO;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using Xunit;

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
namespace SharpPulsar.Test.Impl.conf
{
//	import static org.testng.Assert.fail;
/// <summary>
	/// Unit test <seealso cref="ConfigurationDataUtils"/>.
	/// </summary>
	public class ConfigurationDataUtilsTest
	{
		[Fact]
		public void TestLoadClientConfigurationData()
		{
			ClientConfigurationData confData = new ClientConfigurationData();
			confData.ServiceUrl = "pulsar://unknown:6650";
			confData.MaxLookupRequest = 600;
			confData.NumIoThreads = 33;
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["serviceUrl"] = "pulsar://localhost:6650";
			config["maxLookupRequest"] = 70000;
			confData = ConfigurationDataUtils.LoadData(config, confData, typeof(ClientConfigurationData));
			Assert.Equal("pulsar://localhost:6650", confData.ServiceUrl);
            Assert.Equal(70000, confData.MaxLookupRequest);
            Assert.Equal(33, confData.NumIoThreads);
		}

		[Fact]
		public void TestLoadProducerConfigurationData()
		{
			ProducerConfigurationData confData = new ProducerConfigurationData();
			confData.ProducerName = "unset";
			confData.BatchingEnabled = true;
			confData.BatchingMaxMessages = 1234;
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["producerName"] = "test-producer";
			config["batchingEnabled"] = false;
			confData.BatcherBuilder = DefaultImplementation.NewDefaultBatcherBuilder();
			confData = ConfigurationDataUtils.LoadData(config, confData, typeof(ProducerConfigurationData));
            Assert.Equal("test-producer", confData.ProducerName);
			Assert.False(confData.BatchingEnabled);
            Assert.Equal(1234, confData.BatchingMaxMessages);
		}
		[Fact]
		public void TestLoadConsumerConfigurationData()
		{
			ConsumerConfigurationData<object> confData = new ConsumerConfigurationData<object>();
			confData.SubscriptionName = "unknown-subscription";
			confData.PriorityLevel = 10000;
			confData.ConsumerName = "unknown-consumer";
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["subscriptionName"] = "test-subscription";
			config["priorityLevel"] = 100;
			confData = ConfigurationDataUtils.LoadData(config, confData, typeof(ConsumerConfigurationData<object>));
            Assert.Equal("test-subscription", confData.SubscriptionName);
            Assert.Equal(100, confData.PriorityLevel);
            Assert.Equal("unknown-consumer", confData.ConsumerName);
		}

		[Fact]
		public void TestLoadReaderConfigurationData()
		{
			ReaderConfigurationData<object> confData = new ReaderConfigurationData<object>();
			confData.TopicName = "unknown";
			confData.ReceiverQueueSize = 1000000;
			confData.ReaderName = "unknown-reader";
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["topicName"] = "test-topic";
			config["receiverQueueSize"] = 100;
			confData = ConfigurationDataUtils.LoadData(config, confData, typeof(ReaderConfigurationData<object>));
            Assert.Equal("test-topic", confData.TopicName);
            Assert.Equal(100, confData.ReceiverQueueSize);
            Assert.Equal("unknown-reader", confData.ReaderName);
		}

		[Fact]
		public void TestLoadConfigurationDataWithUnknownFields()
		{
			ReaderConfigurationData<object> confData = new ReaderConfigurationData<object>();
			confData.TopicName = "unknown";
			confData.ReceiverQueueSize = 1000000;
			confData.ReaderName = "unknown-reader";
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["unknown"] = "test-topic";
			config["receiverQueueSize"] = 100;
			try
			{
				ConfigurationDataUtils.LoadData(config, confData, typeof(ReaderConfigurationData<object>));
				Assert.False(false, "Should fail loading configuration data with unknown fields");
			}
			catch (System.Exception re)
			{
				Assert.True(re.InnerException is IOException);
			}
		}

		[Fact]
		public void TestConfigBuilder()
		{
			ClientConfigurationData clientConfig = new ClientConfigurationData();
			clientConfig.ServiceUrl = "pulsar://unknown:6650";
			clientConfig.StatsIntervalSeconds = 80;

			PulsarClientImpl pulsarClient = new PulsarClientImpl(clientConfig);
			Assert.NotNull(pulsarClient);

            Assert.Equal("pulsar://unknown:6650", pulsarClient.Configuration.ServiceUrl);
            Assert.Equal(1, pulsarClient.Configuration.NumListenerThreads);
            Assert.Equal(80, pulsarClient.Configuration.StatsIntervalSeconds);
		}
	}

}