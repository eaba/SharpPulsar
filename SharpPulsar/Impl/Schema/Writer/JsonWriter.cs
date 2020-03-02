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

using System;
using System.IO;
using System.Linq;
using Avro.IO;
using SharpPulsar.Api.Schema;

using Avro.Reflect;
using SharpPulsar.Impl.Conf;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;

namespace SharpPulsar.Impl.Schema.Writer
{

	public class JsonWriter : ISchemaWriter
	{

		private readonly ObjectMapper _objectMapper;

		public JsonWriter(ObjectMapper objectMapper)
		{
			this._objectMapper = objectMapper;
		}

		public sbyte[] Write(object message)
		{
			try
			{
				return (sbyte[])(Array)_objectMapper.WriteValueAsBytes(message);
			}
			catch (System.Exception e)
			{
				throw new SchemaSerializationException(e);
			}
		}
    }

}