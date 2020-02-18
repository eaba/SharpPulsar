﻿using System;
using System.Collections.Generic;
using SharpPulsar.Api;
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
namespace SharpPulsar.Test.Impl
{

    public class BuildersTest
	{
		public virtual void ClientBuilderTest()
		{
			var clientBuilder = (PulsarClientImpl)new PulsarClientBuilderImpl().IoThreads(10).MaxNumberOfRejectedRequestPerConnection(200).ServiceUrl("pulsar://service:6650").Build();

			Assert.False(clientBuilder.Configuration.UseTls);
			Assert.Equal("pulsar://service:6650", clientBuilder.Configuration.ServiceUrl);

			ClientBuilderImpl b2 = (ClientBuilderImpl) clientBuilder.Clone();
			assertNotSame(b2, clientBuilder);

			b2.serviceUrl("pulsar://other-broker:6650");

			assertEquals(clientBuilder.Conf.ServiceUrl, "pulsar://service:6650");
			assertEquals(b2.Conf.ServiceUrl, "pulsar://other-broker:6650");
		}

		public virtual void EnableTlsTest()
		{
			ClientBuilderImpl builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar://service:6650");
			assertFalse(builder.Conf.UseTls);
			assertEquals(builder.Conf.ServiceUrl, "pulsar://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("http://service:6650");
			assertFalse(builder.Conf.UseTls);
			assertEquals(builder.Conf.ServiceUrl, "http://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar+ssl://service:6650");
			assertTrue(builder.Conf.UseTls);
			assertEquals(builder.Conf.ServiceUrl, "pulsar+ssl://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("https://service:6650");
			assertTrue(builder.Conf.UseTls);
			assertEquals(builder.Conf.ServiceUrl, "https://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar://service:6650").enableTls(true);
			assertTrue(builder.Conf.UseTls);
			assertEquals(builder.Conf.ServiceUrl, "pulsar://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar+ssl://service:6650").enableTls(false);
			assertTrue(builder.Conf.UseTls);
			assertEquals(builder.Conf.ServiceUrl, "pulsar+ssl://service:6650");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readerBuilderLoadConfTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void ReaderBuilderLoadConfTest()
		{
			PulsarClient client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
			string topicName = "test_src";
			MessageId messageId = new MessageIdImpl(1, 2, 3);
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["topicName"] = topicName;
			config["receiverQueueSize"] = 2000;
			ReaderBuilderImpl<sbyte[]> builder = (ReaderBuilderImpl<sbyte[]>) client.newReader().startMessageId(messageId).loadConf(config);

			Type clazz = builder.GetType();
			System.Reflection.FieldInfo conf = clazz.getDeclaredField("conf");
			conf.Accessible = true;
			object obj = conf.get(builder);
			assertTrue(obj is ReaderConfigurationData<>);
			assertEquals(((ReaderConfigurationData) obj).TopicName, topicName);
			assertEquals(((ReaderConfigurationData) obj).StartMessageId, messageId);
		}
	}

}