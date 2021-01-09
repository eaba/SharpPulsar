﻿using SharpPulsar.Schema;
using Xunit;
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
namespace SharpPulsar.Test.Schema
{
	
	public class DoubleSchemaTest
	{
		[Fact]
		public void TestSchemaEncode()
		{
			DoubleSchema schema = DoubleSchema.Of();
			double? data = new double?(12345678.1234);
			long LongData = System.BitConverter.DoubleToInt64Bits(data.Value);
			sbyte[] Expected = new sbyte[] {(sbyte)((long)((ulong)LongData >> 56)), (sbyte)((long)((ulong)LongData >> 48)), (sbyte)((long)((ulong)LongData >> 40)), (sbyte)((long)((ulong)LongData >> 32)), (sbyte)((long)((ulong)LongData >> 24)), (sbyte)((long)((ulong)LongData >> 16)), (sbyte)((long)((ulong)LongData >> 8)), (sbyte)((long?)LongData).Value};
			Assert.Equal(Expected, schema.Encode(data.Value));
		}

		[Fact]
		public void TestSchemaEncodeDecodeFidelity()
		{
			DoubleSchema schema = DoubleSchema.Of();
			double? dbl = new double?(1234578.8754321);
			sbyte[] bytes = schema.Encode(dbl.Value);
			Assert.Equal(dbl, schema.Decode(bytes));
		}
	}

}