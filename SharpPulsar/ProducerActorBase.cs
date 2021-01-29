﻿using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Queues;
using System;
using System.Collections.Concurrent;

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
	public abstract class ProducerActorBase<T> : ReceiveActor
	{
		public abstract long LastDisconnectedTimestamp {get;}
		public abstract bool Connected {get;}
		public abstract IProducerStats Stats {get;}
		public abstract long LastSequenceId {get;}
		public abstract IMessageId Send(sbyte[] message);
		public abstract string ProducerName {get;}

		protected internal readonly ProducerConfigurationData Conf;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ProducerInterceptors<T> Interceptors;
		protected internal readonly ConcurrentDictionary<SchemaHash, sbyte[]> SchemaCache;
		protected internal MultiSchemaMode _multiSchemaMode = MultiSchemaMode.Auto;
		protected internal IActorRef Client;
		protected internal readonly ClientConfigurationData ClientConfiguration;
		protected internal readonly ProducerQueueCollection ProducerQueue;
		protected internal HandlerState State;
		private string _topic;

		public ProducerActorBase(IActorRef client, string topic, ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData configurationData, ProducerQueueCollection queue)
		{
			ClientConfiguration = configurationData;
			ProducerQueue = queue;
			Client = client;
			_topic = topic;
			Conf = conf;
			Schema = schema;
			Interceptors = interceptors;
			SchemaCache = new ConcurrentDictionary<SchemaHash, sbyte[]>();
			if(!conf.MultiSchema)
			{
				_multiSchemaMode = MultiSchemaMode.Disabled;
			}
			State = new HandlerState(client, topic, Context.System, ProducerName);

		}

		public virtual IMessageId Send(T message)
		{
			return NewMessage().Value(message).Send();
		}

		public virtual ITypedMessageBuilder<T> NewMessage()
		{
			return new TypedMessageBuilder<T>(Self, Schema);
		}

		public virtual ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
		{
			Condition.CheckArgument(schema != null);
			return new TypedMessageBuilder<V>(Self, schema);
		}

		public virtual TypedMessageBuilder<T> NewMessage(IActorRef txn)
		{
			// check the producer has proper settings to send transactional messages
			if(Conf.SendTimeoutMs > 0)
			{
				throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
			}

			return new TypedMessageBuilder<T>(Self, Schema, txn);
		}

		public virtual string Topic
		{
			get
			{
				return Topic;
			}
		}

		public virtual ProducerConfigurationData Configuration
		{
			get
			{
				return Conf;
			}
		}


		protected internal virtual IMessage<T> BeforeSend(IMessage<T> message)
		{
			if(Interceptors != null)
			{
				return Interceptors.BeforeSend(Self, message);
			}
			else
			{
				return message;
			}
		}

		protected internal virtual void OnSendAcknowledgement(IMessage<T> message, IMessageId msgId, Exception exception)
		{
			if(Interceptors != null)
			{
				Interceptors.OnSendAcknowledgement(Self, message, msgId, exception);
			}
		}

		public override string ToString()
		{
			return "ProducerBase{" + "topic='" + Topic + '\'' + '}';
		}

		public enum MultiSchemaMode
		{
			Auto,
			Enabled,
			Disabled
		}
	}

}