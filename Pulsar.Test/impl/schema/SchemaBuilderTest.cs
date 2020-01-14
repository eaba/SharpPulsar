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
namespace org.apache.pulsar.client.impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	using Data = lombok.Data;
	using EqualsAndHashCode = lombok.EqualsAndHashCode;
	using ToString = lombok.ToString;
	using Nullable = org.apache.avro.reflect.Nullable;
	using Schema = org.apache.pulsar.client.api.Schema;
	using org.apache.pulsar.client.api.schema;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Schema Builder Test.
	/// </summary>
	public class SchemaBuilderTest
	{

		private class AllOptionalFields
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<int> intField;
			internal int? intField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<long> longField;
			internal long? longField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private String stringField;
			internal string stringField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<bool> boolField;
			internal bool? boolField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<float> floatField;
			internal float? floatField;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private System.Nullable<double> doubleField;
			internal double? doubleField;
		}

		private class AllPrimitiveFields
		{
			internal int intField;
			internal long longField;
			internal bool boolField;
			internal float floatField;
			internal double doubleField;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @ToString @EqualsAndHashCode private static class People
		private class People
		{
			internal People1 people1;
			internal People2 people2;
			internal string name;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class People1
		private class People1
		{
			internal int age;
			internal int height;
			internal string name;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class People2
		private class People2
		{
			internal int age;
			internal int height;
			internal string name;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllOptionalFieldsSchema()
		public virtual void testAllOptionalFieldsSchema()
		{
			RecordSchemaBuilder recordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllOptionalFields");
			recordSchemaBuilder.field("intField").type(SchemaType.INT32).optional();
			recordSchemaBuilder.field("longField").type(SchemaType.INT64).optional();
			recordSchemaBuilder.field("stringField").type(SchemaType.STRING).optional();
			recordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN).optional();
			recordSchemaBuilder.field("floatField").type(SchemaType.FLOAT).optional();
			recordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE).optional();
			SchemaInfo schemaInfo = recordSchemaBuilder.build(SchemaType.AVRO);

			Schema<AllOptionalFields> pojoSchema = Schema.AVRO(typeof(AllOptionalFields));
			SchemaInfo pojoSchemaInfo = pojoSchema.SchemaInfo;

			org.apache.avro.Schema avroSchema = (new org.apache.avro.Schema.Parser()).parse(new string(schemaInfo.Schema, UTF_8)
		   );
			org.apache.avro.Schema avroPojoSchema = (new org.apache.avro.Schema.Parser()).parse(new string(pojoSchemaInfo.Schema, UTF_8)
		   );

			assertEquals(avroPojoSchema, avroSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllPrimitiveFieldsSchema()
		public virtual void testAllPrimitiveFieldsSchema()
		{
			RecordSchemaBuilder recordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllPrimitiveFields");
			recordSchemaBuilder.field("intField").type(SchemaType.INT32);
			recordSchemaBuilder.field("longField").type(SchemaType.INT64);
			recordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN);
			recordSchemaBuilder.field("floatField").type(SchemaType.FLOAT);
			recordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE);
			SchemaInfo schemaInfo = recordSchemaBuilder.build(SchemaType.AVRO);

			Schema<AllPrimitiveFields> pojoSchema = Schema.AVRO(typeof(AllPrimitiveFields));
			SchemaInfo pojoSchemaInfo = pojoSchema.SchemaInfo;

			org.apache.avro.Schema avroSchema = (new org.apache.avro.Schema.Parser()).parse(new string(schemaInfo.Schema, UTF_8)
		   );
			org.apache.avro.Schema avroPojoSchema = (new org.apache.avro.Schema.Parser()).parse(new string(pojoSchemaInfo.Schema, UTF_8)
		   );

			assertEquals(avroPojoSchema, avroSchema);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderByFieldName()
		public virtual void testGenericRecordBuilderByFieldName()
		{
			RecordSchemaBuilder recordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllPrimitiveFields");
			recordSchemaBuilder.field("intField").type(SchemaType.INT32);
			recordSchemaBuilder.field("longField").type(SchemaType.INT64);
			recordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN);
			recordSchemaBuilder.field("floatField").type(SchemaType.FLOAT);
			recordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE);
			SchemaInfo schemaInfo = recordSchemaBuilder.build(SchemaType.AVRO);
			GenericSchema schema = Schema.generic(schemaInfo);
			GenericRecord record = schema.newRecordBuilder().set("intField", 32).set("longField", 1234L).set("boolField", true).set("floatField", 0.7f).set("doubleField", 1.34d).build();

			sbyte[] serializedData = schema.encode(record);

			// create a POJO schema to deserialize the serialized data
			Schema<AllPrimitiveFields> pojoSchema = Schema.AVRO(typeof(AllPrimitiveFields));
			AllPrimitiveFields fields = pojoSchema.decode(serializedData);

			assertEquals(32, fields.intField);
			assertEquals(1234L, fields.longField);
			assertEquals(true, fields.boolField);
			assertEquals(0.7f, fields.floatField);
			assertEquals(1.34d, fields.doubleField);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderByIndex()
		public virtual void testGenericRecordBuilderByIndex()
		{
			RecordSchemaBuilder recordSchemaBuilder = SchemaBuilder.record("org.apache.pulsar.client.impl.schema.SchemaBuilderTest.AllPrimitiveFields");
			recordSchemaBuilder.field("intField").type(SchemaType.INT32);
			recordSchemaBuilder.field("longField").type(SchemaType.INT64);
			recordSchemaBuilder.field("boolField").type(SchemaType.BOOLEAN);
			recordSchemaBuilder.field("floatField").type(SchemaType.FLOAT);
			recordSchemaBuilder.field("doubleField").type(SchemaType.DOUBLE);
			SchemaInfo schemaInfo = recordSchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> schema = Schema.generic(schemaInfo);
			GenericRecord record = schema.newRecordBuilder().set(schema.Fields.get(0), 32).set(schema.Fields.get(1), 1234L).set(schema.Fields.get(2), true).set(schema.Fields.get(3), 0.7f).set(schema.Fields.get(4), 1.34d).build();

			sbyte[] serializedData = schema.encode(record);

			// create a POJO schema to deserialize the serialized data
			Schema<AllPrimitiveFields> pojoSchema = Schema.AVRO(typeof(AllPrimitiveFields));
			AllPrimitiveFields fields = pojoSchema.decode(serializedData);

			assertEquals(32, fields.intField);
			assertEquals(1234L, fields.longField);
			assertEquals(true, fields.boolField);
			assertEquals(0.7f, fields.floatField);
			assertEquals(1.34d, fields.doubleField);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldname()
		public virtual void testGenericRecordBuilderAvroByFieldname()
		{
			RecordSchemaBuilder people1SchemaBuilder = SchemaBuilder.record("People1");
			people1SchemaBuilder.field("age").type(SchemaType.INT32);
			people1SchemaBuilder.field("height").type(SchemaType.INT32);
			people1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo people1SchemaInfo = people1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema people1Schema = Schema.generic(people1SchemaInfo);


			GenericRecordBuilder people1RecordBuilder = people1Schema.newRecordBuilder();
			people1RecordBuilder.set("age", 20);
			people1RecordBuilder.set("height", 180);
			people1RecordBuilder.set("name", "people1");
			GenericRecord people1GenericRecord = people1RecordBuilder.build();

			RecordSchemaBuilder people2SchemaBuilder = SchemaBuilder.record("People2");
			people2SchemaBuilder.field("age").type(SchemaType.INT32);
			people2SchemaBuilder.field("height").type(SchemaType.INT32);
			people2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo people2SchemaInfo = people2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema people2Schema = Schema.generic(people2SchemaInfo);

			GenericRecordBuilder people2RecordBuilder = people2Schema.newRecordBuilder();
			people2RecordBuilder.set("age", 20);
			people2RecordBuilder.set("height", 180);
			people2RecordBuilder.set("name", "people2");
			GenericRecord people2GenericRecord = people2RecordBuilder.build();

			RecordSchemaBuilder peopleSchemaBuilder = SchemaBuilder.record("People");
			peopleSchemaBuilder.field("people1", people1Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("people2", people2Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo schemaInfo = peopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema peopleSchema = Schema.generic(schemaInfo);
			GenericRecordBuilder peopleRecordBuilder = peopleSchema.newRecordBuilder();
			peopleRecordBuilder.set("people1", people1GenericRecord);
			peopleRecordBuilder.set("people2", people2GenericRecord);
			peopleRecordBuilder.set("name", "people");
			GenericRecord peopleRecord = peopleRecordBuilder.build();

			sbyte[] peopleEncode = peopleSchema.encode(peopleRecord);

			GenericRecord people = (GenericRecord) peopleSchema.decode(peopleEncode);

			assertEquals(people.Fields, peopleRecord.Fields);
			assertEquals((people.getField("name")), peopleRecord.getField("name"));
			assertEquals(((GenericRecord)people.getField("people1")).getField("age"), people1GenericRecord.getField("age"));
			assertEquals(((GenericRecord)people.getField("people1")).getField("heigth"), people1GenericRecord.getField("heigth"));
			assertEquals(((GenericRecord)people.getField("people1")).getField("name"), people1GenericRecord.getField("name"));
			assertEquals(((GenericRecord)people.getField("people2")).getField("age"), people2GenericRecord.getField("age"));
			assertEquals(((GenericRecord)people.getField("people2")).getField("height"), people2GenericRecord.getField("height"));
			assertEquals(((GenericRecord)people.getField("people2")).getField("name"), people2GenericRecord.getField("name"));

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldnamePojo()
		public virtual void testGenericRecordBuilderAvroByFieldnamePojo()
		{
			RecordSchemaBuilder people1SchemaBuilder = SchemaBuilder.record("People1");
			people1SchemaBuilder.field("age").type(SchemaType.INT32);
			people1SchemaBuilder.field("height").type(SchemaType.INT32);
			people1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo people1SchemaInfo = people1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema people1Schema = Schema.generic(people1SchemaInfo);


			GenericRecordBuilder people1RecordBuilder = people1Schema.newRecordBuilder();
			people1RecordBuilder.set("age", 20);
			people1RecordBuilder.set("height", 180);
			people1RecordBuilder.set("name", "people1");
			GenericRecord people1GenericRecord = people1RecordBuilder.build();

			RecordSchemaBuilder people2SchemaBuilder = SchemaBuilder.record("People2");
			people2SchemaBuilder.field("age").type(SchemaType.INT32);
			people2SchemaBuilder.field("height").type(SchemaType.INT32);
			people2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo people2SchemaInfo = people2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema people2Schema = Schema.generic(people2SchemaInfo);

			GenericRecordBuilder people2RecordBuilder = people2Schema.newRecordBuilder();
			people2RecordBuilder.set("age", 20);
			people2RecordBuilder.set("height", 180);
			people2RecordBuilder.set("name", "people2");
			GenericRecord people2GenericRecord = people2RecordBuilder.build();

			RecordSchemaBuilder peopleSchemaBuilder = SchemaBuilder.record("People");
			peopleSchemaBuilder.field("people1", people1Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("people2", people2Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo schemaInfo = peopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema peopleSchema = Schema.generic(schemaInfo);
			GenericRecordBuilder peopleRecordBuilder = peopleSchema.newRecordBuilder();
			peopleRecordBuilder.set("people1", people1GenericRecord);
			peopleRecordBuilder.set("people2", people2GenericRecord);
			peopleRecordBuilder.set("name", "people");
			GenericRecord peopleRecord = peopleRecordBuilder.build();

			sbyte[] peopleEncode = peopleSchema.encode(peopleRecord);

			Schema<People> peopleDecodeSchema = Schema.AVRO(SchemaDefinition.builder<People>().withPojo(typeof(People)).withAlwaysAllowNull(false).build());
			People people = peopleDecodeSchema.decode(peopleEncode);

			assertEquals(people.name, peopleRecord.getField("name"));
			assertEquals(people.People1.age, people1GenericRecord.getField("age"));
			assertEquals(people.People1.height, people1GenericRecord.getField("height"));
			assertEquals(people.People1.name, people1GenericRecord.getField("name"));
			assertEquals(people.People2.age, people2GenericRecord.getField("age"));
			assertEquals(people.People2.height, people2GenericRecord.getField("height"));
			assertEquals(people.People2.name, people2GenericRecord.getField("name"));

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldIndex()
		public virtual void testGenericRecordBuilderAvroByFieldIndex()
		{
			RecordSchemaBuilder people1SchemaBuilder = SchemaBuilder.record("People1");
			people1SchemaBuilder.field("age").type(SchemaType.INT32);
			people1SchemaBuilder.field("height").type(SchemaType.INT32);
			people1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo people1SchemaInfo = people1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> people1Schema = Schema.generic(people1SchemaInfo);


			GenericRecordBuilder people1RecordBuilder = people1Schema.newRecordBuilder();
			people1RecordBuilder.set(people1Schema.Fields.get(0), 20);
			people1RecordBuilder.set(people1Schema.Fields.get(1), 180);
			people1RecordBuilder.set(people1Schema.Fields.get(2), "people1");
			GenericRecord people1GenericRecord = people1RecordBuilder.build();

			RecordSchemaBuilder people2SchemaBuilder = SchemaBuilder.record("People2");
			people2SchemaBuilder.field("age").type(SchemaType.INT32);
			people2SchemaBuilder.field("height").type(SchemaType.INT32);
			people2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo people2SchemaInfo = people2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> people2Schema = Schema.generic(people2SchemaInfo);

			GenericRecordBuilder people2RecordBuilder = people2Schema.newRecordBuilder();
			people2RecordBuilder.set(people2Schema.Fields.get(0), 20);
			people2RecordBuilder.set(people2Schema.Fields.get(1), 180);
			people2RecordBuilder.set(people2Schema.Fields.get(2), "people2");
			GenericRecord people2GenericRecord = people2RecordBuilder.build();

			RecordSchemaBuilder peopleSchemaBuilder = SchemaBuilder.record("People");
			peopleSchemaBuilder.field("people1", people1Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("people2", people2Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo schemaInfo = peopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema<GenericRecord> peopleSchema = Schema.generic(schemaInfo);
			GenericRecordBuilder peopleRecordBuilder = peopleSchema.newRecordBuilder();
			peopleRecordBuilder.set(peopleSchema.Fields.get(0), people1GenericRecord);
			peopleRecordBuilder.set(peopleSchema.Fields.get(1), people2GenericRecord);
			peopleRecordBuilder.set(peopleSchema.Fields.get(2), "people");
			GenericRecord peopleRecord = peopleRecordBuilder.build();

			sbyte[] peopleEncode = peopleSchema.encode(peopleRecord);

			GenericRecord people = (GenericRecord) peopleSchema.decode(peopleEncode);

			assertEquals(people.Fields, peopleRecord.Fields);
			assertEquals((people.getField("name")), peopleRecord.getField("name"));
			assertEquals(((GenericRecord)people.getField("people1")).getField("age"), people1GenericRecord.getField("age"));
			assertEquals(((GenericRecord)people.getField("people1")).getField("heigth"), people1GenericRecord.getField("heigth"));
			assertEquals(((GenericRecord)people.getField("people1")).getField("name"), people1GenericRecord.getField("name"));
			assertEquals(((GenericRecord)people.getField("people2")).getField("age"), people2GenericRecord.getField("age"));
			assertEquals(((GenericRecord)people.getField("people2")).getField("height"), people2GenericRecord.getField("height"));
			assertEquals(((GenericRecord)people.getField("people2")).getField("name"), people2GenericRecord.getField("name"));

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGenericRecordBuilderAvroByFieldIndexPojo()
		public virtual void testGenericRecordBuilderAvroByFieldIndexPojo()
		{
			RecordSchemaBuilder people1SchemaBuilder = SchemaBuilder.record("People1");
			people1SchemaBuilder.field("age").type(SchemaType.INT32);
			people1SchemaBuilder.field("height").type(SchemaType.INT32);
			people1SchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo people1SchemaInfo = people1SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> people1Schema = Schema.generic(people1SchemaInfo);


			GenericRecordBuilder people1RecordBuilder = people1Schema.newRecordBuilder();
			people1RecordBuilder.set(people1Schema.Fields.get(0), 20);
			people1RecordBuilder.set(people1Schema.Fields.get(1), 180);
			people1RecordBuilder.set(people1Schema.Fields.get(2), "people1");
			GenericRecord people1GenericRecord = people1RecordBuilder.build();

			RecordSchemaBuilder people2SchemaBuilder = SchemaBuilder.record("People2");
			people2SchemaBuilder.field("age").type(SchemaType.INT32);
			people2SchemaBuilder.field("height").type(SchemaType.INT32);
			people2SchemaBuilder.field("name").type(SchemaType.STRING);

			SchemaInfo people2SchemaInfo = people2SchemaBuilder.build(SchemaType.AVRO);
			GenericSchema<GenericRecord> people2Schema = Schema.generic(people2SchemaInfo);

			GenericRecordBuilder people2RecordBuilder = people2Schema.newRecordBuilder();
			people2RecordBuilder.set(people2Schema.Fields.get(0), 20);
			people2RecordBuilder.set(people2Schema.Fields.get(1), 180);
			people2RecordBuilder.set(people2Schema.Fields.get(2), "people2");
			GenericRecord people2GenericRecord = people2RecordBuilder.build();

			RecordSchemaBuilder peopleSchemaBuilder = SchemaBuilder.record("People");
			peopleSchemaBuilder.field("people1", people1Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("people2", people2Schema).type(SchemaType.AVRO);
			peopleSchemaBuilder.field("name").type(SchemaType.STRING);


			SchemaInfo schemaInfo = peopleSchemaBuilder.build(SchemaType.AVRO);

			GenericSchema<GenericRecord> peopleSchema = Schema.generic(schemaInfo);
			GenericRecordBuilder peopleRecordBuilder = peopleSchema.newRecordBuilder();
			peopleRecordBuilder.set(peopleSchema.Fields.get(0), people1GenericRecord);
			peopleRecordBuilder.set(peopleSchema.Fields.get(1), people2GenericRecord);
			peopleRecordBuilder.set(peopleSchema.Fields.get(2), "people");
			GenericRecord peopleRecord = peopleRecordBuilder.build();

			sbyte[] peopleEncode = peopleSchema.encode(peopleRecord);

			Schema<People> peopleDecodeSchema = Schema.AVRO(SchemaDefinition.builder<People>().withPojo(typeof(People)).withAlwaysAllowNull(false).build());
			People people = peopleDecodeSchema.decode(peopleEncode);

			assertEquals(people.name, peopleRecord.getField("name"));
			assertEquals(people.People1.age, people1GenericRecord.getField("age"));
			assertEquals(people.People1.height, people1GenericRecord.getField("height"));
			assertEquals(people.People1.name, people1GenericRecord.getField("name"));
			assertEquals(people.People2.age, people2GenericRecord.getField("age"));
			assertEquals(people.People2.height, people2GenericRecord.getField("height"));
			assertEquals(people.People2.name, people2GenericRecord.getField("name"));
		}
	}

}