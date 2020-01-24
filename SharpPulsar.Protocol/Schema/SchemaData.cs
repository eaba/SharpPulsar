﻿using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Builder;
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
namespace SharpPulsar.Protocol.Schema
{

	/// <summary>
	/// Schema data.
	/// </summary>
	public class SchemaData
	{
		public SchemaType Type { get; set; }
		public  bool IsDeleted { get; set; }
		public  long Timestamp { get; set; }
		public  string User { get; set; }
		public  sbyte[] Data { get; set; }
		public IDictionary<string, string> Properties = new Dictionary<string, string>();

		/// <summary>
		/// Convert a schema data to a schema info.
		/// </summary>
		/// <returns> the converted schema info. </returns>
		public virtual SchemaInfo ToSchemaInfo()
		{
			return new SchemaInfoBuilder().SetName("").SetType(Type).SetSchema(Data).SetProperties(Properties).Build();
		}

		/// <summary>
		/// Convert a schema info to a schema data.
		/// </summary>
		/// <param name="schemaInfo"> schema info </param>
		/// <returns> the converted schema schema data </returns>
		public static SchemaData FromSchemaInfo(SchemaInfo SchemaInfo)
		{
			return new SchemaDataBuilder().SetType(SchemaInfo.Type).SetData(SchemaInfo.Schema).SetProperties(SchemaInfo.Properties).Build();
		}

	}
}