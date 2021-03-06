﻿using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

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
    /// ZLib Compression.
    /// </summary>
    public class CompressionCodecZLib : CompressionCodec
	{
        public byte[] Encode(byte[] raw)
        {
            //ComponentAce.Compression.Libs.zlib
            throw new NotImplementedException();
        }

        public byte[] Decode(byte[] encoded, int uncompressedSize)
        {
            var outputStream = new MemoryStream();
            using var compressedStream = new MemoryStream(encoded);
            using var inputStream = new InflaterInputStream(compressedStream);
            inputStream.CopyTo(outputStream);
            outputStream.Position = 0;
            return outputStream.ToArray();
        }
    }

}