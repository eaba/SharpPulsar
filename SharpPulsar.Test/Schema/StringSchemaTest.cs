﻿using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schemas;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SharpPulsar.Extension;
using SharpPulsar.Shared;

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
    /// <summary>
    /// Unit test <seealso cref="StringSchema"/>.
    /// </summary>
    public class StringSchemaTest
    {
        [Fact]
        public virtual void TestUtf8Charset()
        {
            StringSchema schema = new StringSchema();
            ISchemaInfo si = schema.SchemaInfo;
            Assert.False(si.Properties.ContainsKey(StringSchema.CHARSET_KEY));

            string myString = "my string for test";
            byte[] data = schema.Encode(myString);
            byte[] actualBytes = Encoding.UTF8.GetBytes(myString);
            for(var i = 0; i < actualBytes.Length; i++)
            {
                var expected = actualBytes[i];
                var actual = data[i];
                Assert.Equal(expected, actual);
            }
            string decodedString = schema.Decode(data);
            Assert.Equal(decodedString, myString);
        }
        [Fact]
        public virtual void TestAsciiCharset()
        {
            StringSchema schema = new StringSchema(Encoding.ASCII);
            ISchemaInfo si = schema.SchemaInfo;
            Assert.True(si.Properties.ContainsKey(StringSchema.CHARSET_KEY));
            Assert.Equal(si.Properties[StringSchema.CHARSET_KEY], Encoding.ASCII.WebName);

            string myString = "my string for test";
            byte[] data = schema.Encode(myString);
            byte[] actualBytes = Encoding.ASCII.GetBytes(myString);
            for (var i = 0; i < actualBytes.Length; i++)
            {
                var expected = actualBytes[i];
                var actual = data[i];
                Assert.Equal(expected, actual);
            }

            string decodedString = schema.Decode(data);
            Assert.Equal(decodedString, myString);
        }
        [Fact]
        public virtual void TestSchemaInfoWithoutCharset()
        {
            SchemaInfo si = new SchemaInfo
            {
                Name = "test-schema-info-without-charset",
                Type = SchemaType.STRING,
                Schema = new byte[0],
                Properties = new Dictionary<string,string>()
            };
            StringSchema schema = StringSchema.FromSchemaInfo(si);

            string myString = "my string for test";
            byte[] data = schema.Encode(myString);
            byte[] actualBytes = Encoding.UTF8.GetBytes(myString);
            for (var i = 0; i < actualBytes.Length; i++)
            {
                var expected = actualBytes[i];
                var actual = data[i];
                Assert.Equal(expected, actual);
            }

            string decodedString = schema.Decode(data);
            Assert.Equal(decodedString, myString);
        }

        [Fact]
        public virtual void TestSchemaInfoWithUtf8Charset()
        {
            var charset = Encoding.UTF8;
            IDictionary<string, string> properties = new Dictionary<string, string>();
            properties[StringSchema.CHARSET_KEY] = charset.WebName;
            SchemaInfo si = new SchemaInfo
            {
                Name = "test-schema-info-without-charset",
                Type = SchemaType.STRING,
                Schema = new byte[0],
                Properties = properties
            };
            StringSchema schema = StringSchema.FromSchemaInfo(si);

            string myString = "my string for test";
            byte[] data = schema.Encode(myString);
            byte[] actualBytes = charset.GetBytes(myString);
            for (var i = 0; i < actualBytes.Length; i++)
            {
                var expected = actualBytes[i];
                var actual = data[i];
                Assert.Equal(expected, actual);
            }

            string decodedString = schema.Decode(data);
            Assert.Equal(decodedString, myString);
        }
        
        [Fact]
        public virtual void TestSchemaInfoWithAsciiCharset()
        {
            var charset = Encoding.ASCII;
            IDictionary<string, string> properties = new Dictionary<string, string>();
            properties[StringSchema.CHARSET_KEY] = charset.WebName;
            SchemaInfo si = new SchemaInfo
            {
                Name = "test-schema-info-without-charset",
                Type = SchemaType.STRING,
                Schema = new byte[0],
                Properties = properties
            };
            StringSchema schema = StringSchema.FromSchemaInfo(si);

            string myString = "my string for test";
            byte[] data = schema.Encode(myString);
            byte[] actualBytes = charset.GetBytes(myString);
            for (var i = 0; i < actualBytes.Length; i++)
            {
                var expected = actualBytes[i];
                var actual = data[i];
                Assert.Equal(expected, actual);
            }

            string decodedString = schema.Decode(data);
            Assert.Equal(decodedString, myString);
        }

        [Fact]
        public virtual void TestStringSchema()
        {
            string testString = "hello world";
            byte[] testBytes = Encoding.UTF8.GetBytes(testString);
            StringSchema stringSchema = new StringSchema();
            Assert.Equal(testString, stringSchema.Decode(testBytes));
            var act = stringSchema.Encode(testString);
            for (var i = 0; i < testBytes.Length; i++)
            {
                var expected = testBytes[i];
                var actual = act[i];
                Assert.Equal(expected, actual);
            }

            byte[] bytes2 = Encoding.Unicode.GetBytes(testString);
            StringSchema stringSchemaUtf16 = new StringSchema(Encoding.Unicode);
            Assert.Equal(testString, stringSchemaUtf16.Decode(bytes2));
            var act2 = stringSchemaUtf16.Encode(testString);
            for (var i = 0; i < bytes2.Length; i++)
            {
                var expected = bytes2[i];
                var actual = act2[i];
                Assert.Equal(expected, actual);
            }
        }
    }

}