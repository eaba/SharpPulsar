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
namespace org.apache.pulsar.common.schema
{
	/// <summary>
	/// Types of supported schema for Pulsar messages.
	/// 
	/// <para>Ideally we should have just one single set of enum definitions
	/// for schema type. but we have 3 locations of defining schema types.
	/// 
	/// </para>
	/// <para>when you are adding a new schema type that whose
	/// schema info is required to be recorded in schema registry,
	/// add corresponding schema type into `pulsar-common/src/main/proto/PulsarApi.proto`
	/// and `pulsar-broker/src/main/proto/SchemaRegistryFormat.proto`.
	/// </para>
	/// </summary>
	public sealed class SchemaType
	{
		/// <summary>
		/// No schema defined.
		/// </summary>
		public static readonly SchemaType NONE = new SchemaType("NONE", InnerEnum.NONE, 0);

		/// <summary>
		/// Simple String encoding with UTF-8.
		/// </summary>
		public static readonly SchemaType STRING = new SchemaType("STRING", InnerEnum.STRING, 1);

		/// <summary>
		/// JSON object encoding and validation.
		/// </summary>
		public static readonly SchemaType JSON = new SchemaType("JSON", InnerEnum.JSON, 2);

		/// <summary>
		/// Protobuf message encoding and decoding.
		/// </summary>
		public static readonly SchemaType PROTOBUF = new SchemaType("PROTOBUF", InnerEnum.PROTOBUF, 3);

		/// <summary>
		/// Serialize and deserialize via avro.
		/// </summary>
		public static readonly SchemaType AVRO = new SchemaType("AVRO", InnerEnum.AVRO, 4);

		/// <summary>
		/// boolean schema defined.
		/// @since 2.3.0
		/// </summary>
		public static readonly SchemaType BOOLEAN = new SchemaType("BOOLEAN", InnerEnum.BOOLEAN, 5);

		/// <summary>
		/// A 8-byte integer.
		/// </summary>
		public static readonly SchemaType INT8 = new SchemaType("INT8", InnerEnum.INT8, 6);

		/// <summary>
		/// A 16-byte integer.
		/// </summary>
		public static readonly SchemaType INT16 = new SchemaType("INT16", InnerEnum.INT16, 7);

		/// <summary>
		/// A 32-byte integer.
		/// </summary>
		public static readonly SchemaType INT32 = new SchemaType("INT32", InnerEnum.INT32, 8);

		/// <summary>
		/// A 64-byte integer.
		/// </summary>
		public static readonly SchemaType INT64 = new SchemaType("INT64", InnerEnum.INT64, 9);

		/// <summary>
		/// A float number.
		/// </summary>
		public static readonly SchemaType FLOAT = new SchemaType("FLOAT", InnerEnum.FLOAT, 10);

		/// <summary>
		/// A double number.
		/// </summary>
		public static readonly SchemaType DOUBLE = new SchemaType("DOUBLE", InnerEnum.DOUBLE, 11);

		/// <summary>
		/// Date.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType DATE = new SchemaType("DATE", InnerEnum.DATE, 12);

		/// <summary>
		/// Time.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType TIME = new SchemaType("TIME", InnerEnum.TIME, 13);

		/// <summary>
		/// Timestamp.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType TIMESTAMP = new SchemaType("TIMESTAMP", InnerEnum.TIMESTAMP, 14);

		/// <summary>
		/// A Schema that contains Key Schema and Value Schema.
		/// </summary>
		public static readonly SchemaType KEY_VALUE = new SchemaType("KEY_VALUE", InnerEnum.KEY_VALUE, 15);

		//
		// Schemas that don't have schema info. the value should be negative.
		//

		/// <summary>
		/// A bytes array.
		/// </summary>
		public static readonly SchemaType BYTES = new SchemaType("BYTES", InnerEnum.BYTES, -1);

		/// <summary>
		/// Auto Detect Schema Type.
		/// </summary>
		[System.Obsolete]
		public static readonly SchemaType AUTO = new SchemaType("AUTO", InnerEnum.AUTO, -2);

		/// <summary>
		/// Auto Consume Type.
		/// </summary>
		public static readonly SchemaType AUTO_CONSUME = new SchemaType("AUTO_CONSUME", InnerEnum.AUTO_CONSUME, -3);

		/// <summary>
		/// Auto Publish Type.
		/// </summary>
		public static readonly SchemaType AUTO_PUBLISH = new SchemaType("AUTO_PUBLISH", InnerEnum.AUTO_PUBLISH, -4);

		private static readonly IList<SchemaType> valueList = new List<SchemaType>();

		static SchemaType()
		{
			valueList.Add(NONE);
			valueList.Add(STRING);
			valueList.Add(JSON);
			valueList.Add(PROTOBUF);
			valueList.Add(AVRO);
			valueList.Add(BOOLEAN);
			valueList.Add(INT8);
			valueList.Add(INT16);
			valueList.Add(INT32);
			valueList.Add(INT64);
			valueList.Add(FLOAT);
			valueList.Add(DOUBLE);
			valueList.Add(DATE);
			valueList.Add(TIME);
			valueList.Add(TIMESTAMP);
			valueList.Add(KEY_VALUE);
			valueList.Add(BYTES);
			valueList.Add(AUTO);
			valueList.Add(AUTO_CONSUME);
			valueList.Add(AUTO_PUBLISH);
		}

		public enum InnerEnum
		{
			NONE,
			STRING,
			JSON,
			PROTOBUF,
			AVRO,
			BOOLEAN,
			INT8,
			INT16,
			INT32,
			INT64,
			FLOAT,
			DOUBLE,
			DATE,
			TIME,
			TIMESTAMP,
			KEY_VALUE,
			BYTES,
			AUTO,
			AUTO_CONSUME,
			AUTO_PUBLISH
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		internal int value;

		internal SchemaType(string name, InnerEnum innerEnum, int value)
		{
			this.value = value;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public int Value
		{
			get
			{
				return this.value;
			}
		}

		public static SchemaType valueOf(int value)
		{
			switch (value)
			{
			  case 0:
				  return NONE;
			  case 1:
				  return STRING;
			  case 2:
				  return JSON;
			  case 3:
				  return PROTOBUF;
			  case 4:
				  return AVRO;
			  case 5:
				  return BOOLEAN;
			  case 6:
				  return INT8;
			  case 7:
				  return INT16;
			  case 8:
				  return INT32;
			  case 9:
				  return INT64;
			  case 10:
				  return FLOAT;
			  case 11:
				  return DOUBLE;
			  case 12:
				  return DATE;
			  case 13:
				  return TIME;
			  case 14:
				  return TIMESTAMP;
			  case 15:
				  return KEY_VALUE;
			  case -1:
				  return BYTES;
			  case -2:
				  return AUTO;
			  case -3:
				  return AUTO_CONSUME;
			  case -4:
				  return AUTO_PUBLISH;
			  default:
				  return NONE;
			}
		}


		public bool Primitive
		{
			get
			{
				return isPrimitiveType(this);
			}
		}

		public bool Struct
		{
			get
			{
				return isStructType(this);
			}
		}

		public static bool isPrimitiveType(SchemaType type)
		{
			switch (type.innerEnumValue)
			{
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.STRING:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.BOOLEAN:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.INT8:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.INT16:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.INT32:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.INT64:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.FLOAT:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.DOUBLE:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.DATE:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.TIME:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.TIMESTAMP:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.BYTES:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.NONE:
					return true;
				default:
					return false;
			}

		}

		public static bool isStructType(SchemaType type)
		{
			switch (type.innerEnumValue)
			{
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.AVRO:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.JSON:
				case org.apache.pulsar.common.schema.SchemaType.InnerEnum.PROTOBUF:
					return true;
				default:
					return false;
			}
		}

		public static IList<SchemaType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}
	}

}