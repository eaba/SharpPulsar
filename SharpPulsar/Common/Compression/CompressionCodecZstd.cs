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

using System;
using System.Buffers;
using ZstdNet;

namespace SharpPulsar.Common.Compression
{
    /// <summary>
    /// Zstandard Compression.
    /// </summary>
    public class CompressionCodecZstd : CompressionCodec
	{
		public byte[] Encode(byte[] source, ArrayPool<byte> pool)
		{
            using var compressor = new Compressor();
            var compressed = compressor.Wrap(source);
            var rented = pool.Rent(compressed.Length);
            Array.Copy(compressed, rented, compressed.Length);
            pool.Return(source);
            return rented;
        }

		public byte[] Decode(byte[] encoded, int uncompressedLength)
		{
			using var decompressor = new Decompressor();
            return decompressor.Unwrap(encoded, uncompressedLength);
		}
	}

}