﻿using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api.Schema;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;

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
namespace SharpPulsar.Impl.Schema.Reader
{

	public class JsonReader<T> : ISchemaReader<T>
	{
		private readonly ObjectMapper _objectMapper;

		public JsonReader(ObjectMapper objectMapper)
		{
			this._objectMapper = objectMapper;
		}

		public  T Read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				return (T)_objectMapper.ReadValue((byte[])(Array)bytes, offset, length);
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
		}

		public T Read(Stream inputStream)
		{
			try
			{
				return (T)_objectMapper.ReadValue(inputStream);
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
			finally
			{
				try
				{
					inputStream.Close();
				}
				catch (IOException e)
				{
					Log.LogError("JsonReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(JsonReader<T>));
	}

}