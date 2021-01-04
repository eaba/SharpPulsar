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
namespace SharpPulsar.Test.Schema
{

	public class TimeSchemaTest
	{

		public virtual void TestSchemaEncode()
		{
			TimeSchema Schema = TimeSchema.of();
			Time Data = new Time(DateTimeHelper.CurrentUnixTimeMillis());
			sbyte[] Expected = new sbyte[] {(sbyte)((int)((uint)Data.Time >> 56)), (sbyte)((int)((uint)Data.Time >> 48)), (sbyte)((int)((uint)Data.Time >> 40)), (sbyte)((int)((uint)Data.Time >> 32)), (sbyte)((int)((uint)Data.Time >> 24)), (sbyte)((int)((uint)Data.Time >> 16)), (sbyte)((int)((uint)Data.Time >> 8)), ((long?)Data.Time).Value};
			Assert.assertEquals(Expected, Schema.encode(Data));
		}


		public virtual void TestSchemaEncodeDecodeFidelity()
		{
			TimeSchema Schema = TimeSchema.of();
			Time Time = new Time(DateTimeHelper.CurrentUnixTimeMillis());
			Assert.assertEquals(Time, Schema.decode(Schema.encode(Time)));
		}

		public virtual void TestSchemaDecode()
		{
			sbyte[] ByteData = new sbyte[] {0, 0, 0, 0, 0, 10, 24, 42};
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.buffer(ByteData.Length);
			ByteBuf.writeBytes(ByteData);
			long Expected = 10 * 65536 + 24 * 256 + 42;
			TimeSchema Schema = TimeSchema.of();
			Assert.assertEquals(Expected, Schema.decode(ByteData).Time);
			Assert.assertEquals(Expected, Schema.decode(ByteBuf).Time);
		}


		public virtual void TestNullEncodeDecode()
		{
			ByteBuf ByteBuf = null;
			sbyte[] Bytes = null;
			Assert.assertNull(TimeSchema.of().encode(null));
			Assert.assertNull(TimeSchema.of().decode(Bytes));
			Assert.assertNull(TimeSchema.of().decode(ByteBuf));
		}

	}

}