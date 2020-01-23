﻿using BAMCIS.Util.Concurrent;
using SharpPulsar.Exception;
using SharpPulsar.Interface.Auth;
using System;
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
namespace SharpPulsar.Interface
{
	/// <summary>
	/// Builder interface that is used to configure and construct a <seealso cref="IPulsarClient"/> instance.
	/// 
	/// @since 2.0.0
	/// </summary>
	using UnsupportedAuthenticationException = PulsarClientException.UnsupportedAuthenticationException;

	/// <summary>
	/// Builder interface that is used to configure and construct a <seealso cref="PulsarClient"/> instance.
	/// 
	/// @since 2.0.0
	/// </summary>
	public interface IClientBuilder : ICloneable
	{

		/// <summary>
		/// Construct the final <seealso cref="PulsarClient"/> instance.
		/// </summary>
		/// <returns> the new <seealso cref="PulsarClient"/> instance </returns>
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: PulsarClient build() throws PulsarClientException;
		IPulsarClient Build();

		/// <summary>
		/// Load the configuration from provided <tt>config</tt> map.
		/// 
		/// <para>Example:
		/// 
		/// <pre>
		/// {@code
		/// Map<String, Object> config = new HashMap<>();
		/// config.put("serviceUrl", "pulsar://localhost:6650");
		/// config.put("numIoThreads", 20);
		/// 
		/// IClientBuilder builder = ...;
		/// builder = builder.loadConf(config);
		/// 
		/// PulsarClient client = builder.build();
		/// }
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="config">
		///            configuration to load </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder LoadConf(IDictionary<string, object> Config);

		/// <summary>
		/// Create a copy of the current client builder.
		/// 
		/// <para>Cloning the builder can be used to share an incomplete configuration and specialize it multiple times. For
		/// example:
		/// 
		/// <pre>{@code
		/// IClientBuilder builder = PulsarClient.builder()
		///               .ioThreads(8)
		///               .listenerThreads(4);
		/// 
		/// PulsarClient client1 = builder.clone()
		///                  .serviceUrl("pulsar://localhost:6650").build();
		/// PulsarClient client2 = builder.clone()
		///                  .serviceUrl("pulsar://other-host:6650").build();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a clone of the client builder instance </returns>
		IClientBuilder Clone();

		/// <summary>
		/// Configure the service URL for the Pulsar service.
		/// 
		/// <para>This parameter is required.
		/// 
		/// </para>
		/// <para>Examples:
		/// <ul>
		/// <li>{@code pulsar://my-broker:6650} for regular endpoint</li>
		/// <li>{@code pulsar+ssl://my-broker:6651} for TLS encrypted endpoint</li>
		/// </ul>
		/// 
		/// </para>
		/// </summary>
		/// <param name="serviceUrl">
		///            the URL of the Pulsar service that the client should connect to </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder ServiceUrl(string ServiceUrl);

		/// <summary>
		/// Configure the service URL provider for Pulsar service.
		/// 
		/// <para>Instead of specifying a static service URL string (with <seealso cref="serviceUrl(string)"/>), an application
		/// can pass a <seealso cref="ServiceUrlProvider"/> instance that dynamically provide a service URL.
		/// 
		/// </para>
		/// </summary>
		/// <param name="serviceUrlProvider">
		///            the provider instance </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder ServiceUrlProvider(IServiceUrlProvider ServiceUrlProvider);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar client instance.
		/// 
		/// <para>Example:
		/// <pre>{@code
		/// PulsarClient client = PulsarClient.builder()
		///         .serviceUrl("pulsar+ssl://broker.example.com:6651/")
		///         .authentication(
		///               AuthenticationFactory.TLS("/my/cert/file", "/my/key/file")
		///         .build();
		/// }</pre>
		/// 
		/// </para>
		/// <para>For token based authentication, this will look like:
		/// <pre>{@code
		/// AuthenticationFactory
		///      .token("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJKb2UifQ.ipevRNuRP6HflG8cFKnmUPtypruRC4fb1DWtoLL62SY")
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="authentication">
		///            an instance of the <seealso cref="Authentication"/> provider already constructed </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder Authentication(IAuthentication Authentication);

		/// <summary>
		/// Configure the authentication provider to use in the Pulsar client instance.
		/// 
		/// <para>Example:
		/// <pre>
		/// <code>
		/// PulsarClient client = PulsarClient.builder()
		///          .serviceUrl("pulsar+ssl://broker.example.com:6651/)
		///          .authentication(
		///              "org.apache.pulsar.client.impl.auth.AuthenticationTls",
		///              "tlsCertFile:/my/cert/file,tlsKeyFile:/my/key/file")
		///          .build();
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParamsString">
		///            string which represents parameters for the Authentication-Plugin, e.g., "key1:val1,key2:val2" </param>
		/// <returns> the client builder instance </returns>
		/// <exception cref="UnsupportedAuthenticationException">
		///             failed to instantiate specified Authentication-Plugin </exception>
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: IClientBuilder authentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
		IClientBuilder Authentication(string AuthPluginClassName, string AuthParamsString);

		/// <summary>
		/// Configure the authentication provider to use in the Pulsar client instance
		/// using a config map.
		/// 
		/// <para>Example:
		/// <pre>{@code
		/// Map<String, String> conf = new TreeMap<>();
		/// conf.put("tlsCertFile", "/my/cert/file");
		/// conf.put("tlsKeyFile", "/my/key/file");
		/// 
		/// PulsarClient client = PulsarClient.builder()
		///          .serviceUrl("pulsar+ssl://broker.example.com:6651/)
		///          .authentication(
		///              "org.apache.pulsar.client.impl.auth.AuthenticationTls", conf)
		///          .build();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParams">
		///            map which represents parameters for the Authentication-Plugin </param>
		/// <returns> the client builder instance </returns>
		/// <exception cref="UnsupportedAuthenticationException">
		///             failed to instantiate specified Authentication-Plugin </exception>
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: IClientBuilder authentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
		IClientBuilder Authentication(string AuthPluginClassName, IDictionary<string, string> AuthParams);

		/// <summary>
		/// Set the operation timeout <i>(default: 30 seconds)</i>.
		/// 
		/// <para>Producer-create, subscribe and unsubscribe operations will be retried until this interval, after which the
		/// operation will be marked as failed
		/// 
		/// </para>
		/// </summary>
		/// <param name="operationTimeout">
		///            operation timeout </param>
		/// <param name="unit">
		///            time unit for {@code operationTimeout} </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder OperationTimeout(int OperationTimeout, TimeUnit Unit);

		/// <summary>
		/// Set the number of threads to be used for handling connections to brokers <i>(default: 1 thread)</i>.
		/// </summary>
		/// <param name="numIoThreads"> the number of IO threads </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder IoThreads(int NumIoThreads);

		/// <summary>
		/// Set the number of threads to be used for message listeners <i>(default: 1 thread)</i>.
		/// 
		/// <para>The listener thread pool is shared across all the consumers and readers that are
		/// using a "listener" model to get messages. For a given consumer, the listener will be
		/// always invoked from the same thread, to ensure ordering.
		/// 
		/// </para>
		/// </summary>
		/// <param name="numListenerThreads"> the number of listener threads </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder ListenerThreads(int NumListenerThreads);

		/// <summary>
		/// Sets the max number of connection that the client library will open to a single broker.
		/// 
		/// <para>By default, the connection pool will use a single connection for all the producers and consumers.
		/// Increasing this parameter may improve throughput when using many producers over a high latency connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="connectionsPerBroker">
		///            max number of connections per broker (needs to be greater than 0) </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder ConnectionsPerBroker(int ConnectionsPerBroker);

		/// <summary>
		/// Configure whether to use TCP no-delay flag on the connection, to disable Nagle algorithm.
		/// 
		/// <para>No-delay features make sure packets are sent out on the network as soon as possible, and it's critical
		/// to achieve low latency publishes. On the other hand, sending out a huge number of small packets
		/// might limit the overall throughput, so if latency is not a concern,
		/// it's advisable to set the <code>useTcpNoDelay</code> flag to false.
		/// 
		/// </para>
		/// <para>Default value is true.
		/// 
		/// </para>
		/// </summary>
		/// <param name="enableTcpNoDelay"> whether to enable TCP no-delay feature </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder EnableTcpNoDelay(bool EnableTcpNoDelay);

		/// <summary>
		/// Configure whether to use TLS encryption on the connection
		/// <i>(default: true if serviceUrl starts with "pulsar+ssl://", false otherwise)</i>.
		/// </summary>
		/// <param name="enableTls"> </param>
		/// @deprecated use "pulsar+ssl://" in serviceUrl to enable 
		/// <returns> the client builder instance </returns>
		[Obsolete(@"use ""pulsar+ssl://"" in serviceUrl to enable")]
		IClientBuilder EnableTls(bool EnableTls);

		/// <summary>
		/// Set the path to the trusted TLS certificate file.
		/// </summary>
		/// <param name="tlsTrustCertsFilePath"> </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder TlsTrustCertsFilePath(string TlsTrustCertsFilePath);

		/// <summary>
		/// Configure whether the Pulsar client accept untrusted TLS certificate from broker <i>(default: false)</i>.
		/// </summary>
		/// <param name="allowTlsInsecureConnection"> whether to accept a untrusted TLS certificate </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder AllowTlsInsecureConnection(bool AllowTlsInsecureConnection);

		/// <summary>
		/// It allows to validate hostname verification when client connects to broker over tls. It validates incoming x509
		/// certificate and matches provided hostname(CN/SAN) with expected broker's host name. It follows RFC 2818, 3.1.
		/// Server Identity hostname verification.
		/// </summary>
		/// <seealso cref= <a href="https://tools.ietf.org/html/rfc2818">RFC 818</a>
		/// </seealso>
		/// <param name="enableTlsHostnameVerification"> whether to enable TLS hostname verification </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder EnableTlsHostnameVerification(bool EnableTlsHostnameVerification);

		/// <summary>
		/// Set the interval between each stat info <i>(default: 60 seconds)</i> Stats will be activated with positive
		/// statsInterval It should be set to at least 1 second.
		/// </summary>
		/// <param name="statsInterval">
		///            the interval between each stat info </param>
		/// <param name="unit">
		///            time unit for {@code statsInterval} </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder StatsInterval(long StatsInterval, TimeUnit Unit);

		/// <summary>
		/// Number of concurrent lookup-requests allowed to send on each broker-connection to prevent overload on broker.
		/// <i>(default: 5000)</i> It should be configured with higher value only in case of it requires to produce/subscribe
		/// on thousands of topic using created <seealso cref="PulsarClient"/>.
		/// </summary>
		/// <param name="maxConcurrentLookupRequests"> </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder MaxConcurrentLookupRequests(int MaxConcurrentLookupRequests);

		/// <summary>
		/// Number of max lookup-requests allowed on each broker-connection to prevent overload on broker.
		/// <i>(default: 50000)</i> It should be bigger than maxConcurrentLookupRequests.
		/// Requests that inside maxConcurrentLookupRequests already send to broker, and requests beyond
		/// maxConcurrentLookupRequests and under maxLookupRequests will wait in each client cnx.
		/// </summary>
		/// <param name="maxLookupRequests"> </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder MaxLookupRequests(int MaxLookupRequests);

		/// <summary>
		/// Set max number of broker-rejected requests in a certain time-frame (30 seconds) after which current connection
		/// will be closed and client creates a new connection that give chance to connect a different broker <i>(default:
		/// 50)</i>.
		/// </summary>
		/// <param name="maxNumberOfRejectedRequestPerConnection"> </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder MaxNumberOfRejectedRequestPerConnection(int MaxNumberOfRejectedRequestPerConnection);

		/// <summary>
		/// Set keep alive interval for each client-broker-connection. <i>(default: 30 seconds)</i>.
		/// </summary>
		/// <param name="keepAliveInterval"> </param>
		/// <param name="unit"> the time unit in which the keepAliveInterval is defined </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder KeepAliveInterval(int KeepAliveInterval, TimeUnit Unit);

		/// <summary>
		/// Set the duration of time to wait for a connection to a broker to be established. If the duration passes without a
		/// response from the broker, the connection attempt is dropped.
		/// 
		/// @since 2.3.0 </summary>
		/// <param name="duration">
		///            the duration to wait </param>
		/// <param name="unit">
		///            the time unit in which the duration is defined </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder ConnectionTimeout(int Duration, TimeUnit Unit);

		/// <summary>
		/// Set the duration of time for a backoff interval.
		/// </summary>
		/// <param name="duration"> the duration of the interval </param>
		/// <param name="unit"> the time unit in which the duration is defined </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder StartingBackoffInterval(long Duration, TimeUnit Unit);

		/// <summary>
		/// Set the maximum duration of time for a backoff interval.
		/// </summary>
		/// <param name="duration"> the duration of the interval </param>
		/// <param name="unit"> the time unit in which the duration is defined </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder MaxBackoffInterval(long Duration, TimeUnit Unit);

		/// <summary>
		/// The clock used by the pulsar client.
		/// 
		/// <para>The clock is currently used by producer for setting publish timestamps.
		/// <seealso cref="Clock.millis()"/> is called to retrieve current timestamp as the publish
		/// timestamp when producers produce messages. The default clock is a system default zone
		/// clock. So the publish timestamp is same as calling <seealso cref="System.currentTimeMillis()"/>.
		/// 
		/// </para>
		/// <para>Warning: the clock is used for TTL enforcement and timestamp based seeks.
		/// so be aware of the impacts if you are going to use a different clock.
		/// 
		/// </para>
		/// </summary>
		/// <param name="clock"> the clock used by the pulsar client to retrieve time information </param>
		/// <returns> the client builder instance </returns>
		IClientBuilder Clock(DateTime Clock);
	}

}
