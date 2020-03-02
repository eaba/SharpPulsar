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
using System.Linq;
using Avro;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;

namespace SharpPulsar.Impl.Schema.Generic
{
	using Field = Api.Schema.Field;
	using IGenericRecord = Api.Schema.IGenericRecord;
	using IGenericRecordBuilder = Api.Schema.IGenericRecordBuilder;
	using SharpPulsar.Api.Schema;

	/// <summary>
	/// A generic json schema.
	/// </summary>
	public class GenericJsonSchema : GenericSchemaImpl
	{
		private static readonly ILogger _log = Utility.Log.Logger.CreateLogger(typeof(GenericJsonSchema));

        public GenericJsonSchema(SchemaInfo schemaInfo) : this(schemaInfo, true)
		{
		}

        public GenericJsonSchema(SchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema) : base(schemaInfo, useProvidedSchemaAsReaderSchema)
		{
		}

		public override GenericAvroReader LoadReader(BytesSchemaVersion schemaVersion)
		{
            _log.LogWarning("No schema found for version({}), use latest schema : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.Get()), SchemaInfo.SchemaDefinition);
            return Reader;
        }

		public override IGenericRecordBuilder NewRecordBuilder()
		{
			throw new System.NotSupportedException("Json Schema doesn't support record builder yet");
		}

        public override ISchema Auto()
        {
            throw new System.NotImplementedException();
        }

        public override ISchema Json(Type pojo)
        {
            throw new System.NotImplementedException();
        }

        public override bool RequireFetchingSchemaInfo()
        {
            throw new System.NotImplementedException();
        }

        public override bool SupportSchemaVersioning()
        {
            throw new System.NotImplementedException();
        }

        public override void Validate(sbyte[] message)
        {
            throw new System.NotImplementedException();
        }

        
        public override ISchema Json(ISchemaDefinition schemaDefinition)
        {
            throw new System.NotImplementedException();
        }

        public override ISchema Json(object pojo)
        {
            throw new System.NotImplementedException();
        }

        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
        {
            throw new System.NotImplementedException();
        }
    }

}