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

using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;

namespace SharpPulsar.Test.Impl.auth
{

    public class AuthenticationTokenTest
	{

		public void TestAuthToken()
		{
			AuthenticationToken authToken = new AuthenticationToken("token-xyz");
			assertEquals(authToken.AuthMethodName, "token");

			IAuthenticationDataProvider authData = authToken.AuthData;
			assertTrue(authData.hasDataFromCommand());
			assertEquals(authData.CommandData, "token-xyz");

			assertFalse(authData.hasDataForTls());
			assertNull(authData.TlsCertificates);
			assertNull(authData.TlsPrivateKey);

			assertTrue(authData.hasDataForHttp());
			assertEquals(authData.HttpHeaders, Collections.singletonMap("Authorization", "Bearer token-xyz").entrySet());

			authToken.DisposeAsync();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenClientConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenClientConfig()
		{
			ClientConfigurationData clientConfig = new ClientConfigurationData();
			clientConfig.ServiceUrl = "pulsar://service-url";
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			clientConfig.AuthPluginClassName = typeof(AuthenticationToken).FullName;
			clientConfig.AuthParams = "token-xyz";

			PulsarClientImpl pulsarClient = new PulsarClientImpl(clientConfig);

			Authentication authToken = pulsarClient.Configuration.Authentication;
			assertEquals(authToken.AuthMethodName, "token");

			AuthenticationDataProvider authData = authToken.AuthData;
			assertTrue(authData.hasDataFromCommand());
			assertEquals(authData.CommandData, "token-xyz");

			assertFalse(authData.hasDataForTls());
			assertNull(authData.TlsCertificates);
			assertNull(authData.TlsPrivateKey);

			assertTrue(authData.hasDataForHttp());
			assertEquals(authData.HttpHeaders, Collections.singletonMap("Authorization", "Bearer token-xyz").entrySet());

			authToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfig()
		{
			AuthenticationToken authToken = new AuthenticationToken();
			authToken.configure("token:my-test-token-string");
			assertEquals(authToken.AuthMethodName, "token");

			AuthenticationDataProvider authData = authToken.AuthData;
			assertTrue(authData.hasDataFromCommand());
			assertEquals(authData.CommandData, "my-test-token-string");
			authToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfigFromFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfigFromFile()
		{
			File tokenFile = File.createTempFile("pular-test-token", ".key");
			tokenFile.deleteOnExit();
			FileUtils.write(tokenFile, "my-test-token-string", Charsets.UTF_8);

			AuthenticationToken authToken = new AuthenticationToken();
			authToken.configure("file://" + tokenFile);
			assertEquals(authToken.AuthMethodName, "token");

			AuthenticationDataProvider authData = authToken.AuthData;
			assertTrue(authData.hasDataFromCommand());
			assertEquals(authData.CommandData, "my-test-token-string");

			// Ensure if the file content changes, the token will get refreshed as well
			FileUtils.write(tokenFile, "other-token", Charsets.UTF_8);

			AuthenticationDataProvider authData2 = authToken.AuthData;
			assertTrue(authData2.hasDataFromCommand());
			assertEquals(authData2.CommandData, "other-token");

			authToken.Dispose();
		}

		/// <summary>
		/// File can have spaces and newlines before or after the token. We should be able to read
		/// the token correctly anyway.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfigFromFileWithNewline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfigFromFileWithNewline()
		{
			File tokenFile = File.createTempFile("pular-test-token", ".key");
			tokenFile.deleteOnExit();
			FileUtils.write(tokenFile, "  my-test-token-string  \r\n", Charsets.UTF_8);

			AuthenticationToken authToken = new AuthenticationToken();
			authToken.configure("file://" + tokenFile);
			assertEquals(authToken.AuthMethodName, "token");

			AuthenticationDataProvider authData = authToken.AuthData;
			assertTrue(authData.hasDataFromCommand());
			assertEquals(authData.CommandData, "my-test-token-string");

			// Ensure if the file content changes, the token will get refreshed as well
			FileUtils.write(tokenFile, "other-token", Charsets.UTF_8);

			AuthenticationDataProvider authData2 = authToken.AuthData;
			assertTrue(authData2.hasDataFromCommand());
			assertEquals(authData2.CommandData, "other-token");

			authToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfigNoPrefix() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfigNoPrefix()
		{
			AuthenticationToken authToken = new AuthenticationToken();
			authToken.configure("my-test-token-string");
			assertEquals(authToken.AuthMethodName, "token");

			AuthenticationDataProvider authData = authToken.AuthData;
			assertTrue(authData.hasDataFromCommand());
			assertEquals(authData.CommandData, "my-test-token-string");
			authToken.Dispose();
		}
	}

}