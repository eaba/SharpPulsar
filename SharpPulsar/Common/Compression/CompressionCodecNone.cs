﻿

using System;
using System.Buffers;
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
namespace SharpPulsar.Common.Compression
{

	/// <summary>
	/// No compression.
	/// </summary>
	public class CompressionCodecNone : CompressionCodec
	{

		public byte[] Encode(byte[] raw, ArrayPool<byte> pool)
		{
            var rented = pool.Rent(raw.Length);
            Array.Copy(raw, rented, raw.Length);
            pool.Return(rented);
            return rented;
		}

		public byte[] Decode(byte[] encoded, int uncompressedSize)
		{
			return encoded;
		}
	}

}