﻿using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

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
	using Field = Api.Schema.Field;

	/// <summary>
	/// Generic json record.
	/// </summary>
	public class GenericJsonRecord : VersionedGenericRecord
    {
        private readonly JsonDocument _jsonDocument;
		public GenericJsonRecord(sbyte[] schemaVersion, IList<Field> fields, JsonDocument jd) : base(schemaVersion, fields)
		{
			_jsonDocument = jd;
		}


		public override object GetField(string fieldName)
		{
			var fn = _jsonDocument.RootElement.EnumerateArray().Where(x => !string.IsNullOrWhiteSpace(x.GetProperty(fieldName).GetString()));
            var jsonElements = fn as JsonElement[] ?? fn.ToArray();
            return jsonElements;
        }

		public override object GetField(Field field)
		{
			throw new System.NotImplementedException();
		}
	}

}