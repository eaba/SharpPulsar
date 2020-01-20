﻿using System.Collections.Generic;

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
namespace org.apache.pulsar.common.util
{
	using SslContext = io.netty.handler.ssl.SslContext;

	/// <summary>
	/// SSL context builder for Netty.
	/// </summary>
	public class NettySslContextBuilder : SslContextAutoRefreshBuilder<SslContext>
	{
		private volatile SslContext sslNettyContext;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public NettySslContextBuilder(boolean allowInsecure, String trustCertsFilePath, String certificateFilePath, String keyFilePath, java.util.Set<String> ciphers, java.util.Set<String> protocols, boolean requireTrustedClientCertOnConnect, long delayInSeconds) throws javax.net.ssl.SSLException, java.io.FileNotFoundException, java.security.GeneralSecurityException, java.io.IOException
		public NettySslContextBuilder(bool allowInsecure, string trustCertsFilePath, string certificateFilePath, string keyFilePath, ISet<string> ciphers, ISet<string> protocols, bool requireTrustedClientCertOnConnect, long delayInSeconds) : base(allowInsecure, trustCertsFilePath, certificateFilePath, keyFilePath, ciphers, protocols, requireTrustedClientCertOnConnect, delayInSeconds)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public synchronized io.netty.handler.ssl.SslContext update() throws javax.net.ssl.SSLException, java.io.FileNotFoundException, java.security.GeneralSecurityException, java.io.IOException
		public override SslContext update()
		{
			lock (this)
			{
				this.sslNettyContext = SecurityUtility.createNettySslContextForServer(tlsAllowInsecureConnection, tlsTrustCertsFilePath.FileName, tlsCertificateFilePath.FileName, tlsKeyFilePath.FileName, tlsCiphers, tlsProtocols, tlsRequireTrustedClientCertOnConnect);
				return this.sslNettyContext;
			}
		}

		public override SslContext SslContext
		{
			get
			{
				return this.sslNettyContext;
			}
		}

	}

}