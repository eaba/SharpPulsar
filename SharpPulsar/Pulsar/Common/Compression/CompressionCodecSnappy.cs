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
/// 

namespace SharpPulsar.Common.Compression
{
    using Microsoft.Extensions.Logging;
    using Snappy;
    using System.IO;

    //using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;

    /// <summary>
    /// Snappy Compression.
    /// </summary>
    public class CompressionCodecSnappy : CompressionCodec
	{
		public byte[] Encode(byte[] source)
        {
            return SnappyCodec.Compress(source);
        }

		public byte[] Decode(byte[] encoded, int uncompressedLength)
        {
            var target = new byte[uncompressedLength];
            SnappyCodec.Uncompress(encoded, 0, encoded.Length, target, 0);
            return target;
        }
	}
	
}