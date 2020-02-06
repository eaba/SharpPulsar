﻿using System;
using System.Collections.Generic;

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
namespace SharpPulsar.Impl
{
    using Microsoft.Extensions.Logging;
    using SharpPulsar.Api;
    using SharpPulsar.Api.Interceptor;
    using IMessageId = Api.IMessageId;


	/// <summary>
	/// A container that holds the list<seealso cref="ProducerInterceptor"/>
	/// and wraps calls to the chain of custom interceptors.
	/// </summary>
	public class ProducerInterceptors : IDisposable
	{
		private static readonly ILogger log = new LoggerFactory().CreateLogger<ProducerInterceptors>();

		private readonly IList<IProducerInterceptor> interceptors;

		public ProducerInterceptors(IList<IProducerInterceptor> Interceptors)
		{
			this.interceptors = Interceptors;
		}

		/// <summary>
		/// This is called when client sends message to pulsar broker, before key and value gets serialized.
		/// The method calls <seealso cref="ProducerInterceptor.beforeSend(Producer,Message)"/> method. Message returned from
		/// first interceptor's beforeSend() is passed to the second interceptor beforeSend(), and so on in the
		/// interceptor chain. The message returned from the last interceptor is returned from this method.
		/// 
		/// This method does not throw exceptions. Exceptions thrown by any interceptor methods are caught and ignored.
		/// If a interceptor in the middle of the chain, that normally modifies the message, throws an exception,
		/// the next interceptor in the chain will be called with a message returned by the previous interceptor that did
		/// not throw an exception.
		/// </summary>
		/// <param name="producer"> the producer which contains the interceptor. </param>
		/// <param name="message"> the message from client </param>
		/// <returns> the message to send to topic/partition </returns>
		public virtual Message<T> BeforeSend<T>(IProducer<T> producer, Message<T> message)
		{
			var interceptorMessage = message;
			foreach (IProducerInterceptor interceptor in interceptors)
			{
				if (!interceptor.Eligible(message))
				{
					continue;
				}
				try
				{
					interceptorMessage = interceptor.BeforeSend(producer, interceptorMessage);
				}
				catch (System.Exception E)
				{
					if (producer != null)
					{
						log.LogWarning("Error executing interceptor beforeSend callback for topicName:{} ", producer.Topic, E);
					}
					else
					{
						log.LogWarning("Error Error executing interceptor beforeSend callback ", E);
					}
				}
			}
			return interceptorMessage;
		}

		/// <summary>
		/// This method is called when the message send to the broker has been acknowledged, or when sending the record fails
		/// before it gets send to the broker.
		/// This method calls <seealso cref="ProducerInterceptor.onSendAcknowledgement(Producer, Message, IMessageId, System.Exception)"/> method for
		/// each interceptor.
		/// 
		/// This method does not throw exceptions. Exceptions thrown by any of interceptor methods are caught and ignored.
		/// </summary>
		/// <param name="producer"> the producer which contains the interceptor. </param>
		/// <param name="message"> The message returned from the last interceptor is returned from <seealso cref="ProducerInterceptor.beforeSend(Producer, Message)"/> </param>
		/// <param name="msgId"> The message id that broker returned. Null if has error occurred. </param>
		/// <param name="exception"> The exception thrown during processing of this message. Null if no error occurred. </param>
		public virtual void OnSendAcknowledgement<T>(IProducer<T> producer, Message<T> message, IMessageId msgId, System.Exception exception)
		{
			foreach (IProducerInterceptor interceptor in interceptors)
			{
				if (!interceptor.Eligible(message))
				{
					continue;
				}
				try
				{
					interceptor.OnSendAcknowledgement(producer, message, msgId, exception);
				}
				catch (System.Exception e)
				{
					log.LogWarning("Error executing interceptor onSendAcknowledgement callback ", e);
				}
			}
		}

		public void Close()
		{
			foreach (IProducerInterceptor interceptor in interceptors)
			{
				try
				{
					interceptor.Close();
				}
				catch (System.Exception e)
				{
					log.LogError("Fail to close producer interceptor ", e);
				}
			}
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}

}