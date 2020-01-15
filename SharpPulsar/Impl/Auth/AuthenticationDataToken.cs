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

namespace Pulsar.Client.Impl.Auth
{

	using AuthenticationDataProvider = Api.AuthenticationDataProvider;

	public class AuthenticationDataToken : AuthenticationDataProvider
	{
		public const string HTTP_HEADER_NAME = "Authorization";

		private readonly System.Func<string> tokenSupplier;

		public AuthenticationDataToken(System.Func<string> tokenSupplier)
		{
			this.tokenSupplier = tokenSupplier;
		}

		public bool HasDataForHttp()
		{
			return true;
		}

		public ISet<KeyValuePair<string, string>> HttpHeaders
		{
			get
			{
				return Collections.singletonMap(HTTP_HEADER_NAME, "Bearer " + Token).entrySet();
			}
		}

		public bool HasDataFromCommand()
		{
			return true;
		}

		public string CommandData
		{
			get
			{
				return Token;
			}
		}

		private string Token
		{
			get
			{
				try
				{
					return tokenSupplier.get();
				}
				catch (Exception t)
				{
					throw new Exception("failed to get client token", t);
				}
			}
		}
	}

}