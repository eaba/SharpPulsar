﻿using SharpPulsar.Schemas;
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
    [Collection("SchemaSpec")]
    public class BooleanSchemaTest
    {
        [Fact]
        public void TestSchemaEncode()
        {
            BooleanSchema schema = BooleanSchema.Of();
            byte[] expectedTrue = new byte[] { 1 };
            byte[] expectedFalse = new byte[] { 0 };
            Assert.Equal(expectedTrue, schema.Encode(true));
            Assert.Equal(expectedFalse, schema.Encode(false));
        }
        [Fact]
        public void TestSchemaEncodeDecodeFidelity()
        {
            BooleanSchema schema = BooleanSchema.Of();
            Assert.Equal(new bool?(true), schema.Decode(schema.Encode(true)));
            Assert.Equal(new bool?(false), schema.Decode(schema.Encode(false)));
        }
        [Fact]
        public void TestSchemaDecode()
        {
            byte[] trueBytes = new byte[] { 1 };
            byte[] falseBytes = new byte[] { 0 };
            BooleanSchema schema = BooleanSchema.Of();
            Assert.Equal(new bool?(true), schema.Decode(trueBytes));
            Assert.Equal(new bool?(false), schema.Decode(falseBytes));
        }
    }

}