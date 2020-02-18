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
namespace SharpPulsar.Test.Util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
public class ObjectCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCache()
		public virtual void TestCache()
		{

			AtomicLong CurrentTime = new AtomicLong(0);

			Clock Clock = mock(typeof(Clock));
			when(Clock.millis()).then(invocation => CurrentTime.longValue());

			AtomicInteger CurrentValue = new AtomicInteger(0);

			System.Func<int> Cache = new ObjectCache<int>(() => CurrentValue.AndIncrement, 10, TimeUnit.MILLISECONDS, Clock);

			cache();
			assertEquals(cache().intValue(), 0);
			assertEquals(cache().intValue(), 0);

			CurrentTime.set(1);
			// Still the value has not expired
			assertEquals(cache().intValue(), 0);

			CurrentTime.set(10);
			assertEquals(cache().intValue(), 1);


			CurrentTime.set(15);
			assertEquals(cache().intValue(), 1);

			CurrentTime.set(22);
			assertEquals(cache().intValue(), 2);
		}
	}

}