﻿using System.IO;
using Avro.IO;
using Avro.Reflect;

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
namespace SharpPulsar.Impl.Schema.Generic
{

    public class GenericAvroWriter
	{
        private Avro.Schemas.Schema _schema;

		public GenericAvroWriter(Avro.Schemas.Schema schema)
		{
            _schema = schema;
        }

		public byte[] Write(object message)
		{
            var writer = new ReflectDefaultWriter(message.GetType(), _schema, new ClassCache());
            using var stream = new MemoryStream(256);
            writer.Write(message, new BinaryEncoder(stream));
            return stream.ToArray();
        }
	}

}