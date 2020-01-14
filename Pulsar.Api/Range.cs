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
namespace org.apache.pulsar.client.api
{
	/// <summary>
	/// Int range.
	/// </summary>
	public class Range
	{

		private readonly int start;
		private readonly int end;


		public Range(int start, int end)
		{
			if (end < start)
			{
				throw new System.ArgumentException("Range end must >= range start.");
			}
			this.start = start;
			this.end = end;
		}

		public static Range of(int start, int end)
		{
			return new Range(start, end);
		}

		public virtual int Start
		{
			get
			{
				return start;
			}
		}

		public virtual int End
		{
			get
			{
				return end;
			}
		}

		public virtual Range intersect(Range range)
		{
			int start = range.Start > this.Start ? range.Start : this.Start;
			int end = range.End < this.End ? range.End : this.End;
			if (end >= start)
			{
				return Range.of(start, end);
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			return "[" + start + ", " + end + "]";
		}
	}

}