﻿
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

using System.IO;

namespace SharpPulsar.Common.Compression
{
	/// <summary>
	/// Generic compression codec interface.
	/// </summary>
	public interface CompressionCodec
	{

		/// <summary>
		/// Compress a buffer.
		/// </summary>
		/// <param name="raw">
		///            a buffer with the uncompressed content. The reader/writer indexes will not be modified </param>
		/// <returns> a new buffer with the compressed content. The buffer needs to be released by the receiver </returns>
		byte[] Encode(byte[] raw);

		/// <summary>
		/// Decompress a buffer.
		/// 
		/// <para>The buffer needs to have been compressed with the matching Encoder.
		/// 
		/// </para>
		/// </summary>
		/// <param name="encoded">
		///            the compressed content </param>
		/// <param name="uncompressedSize">
		///            the Size of the original content </param>
		/// <returns> a ByteBuf with the compressed content. The buffer needs to be released by the receiver </returns>
		/// <exception cref="IOException">
		///             if the decompression fails </exception>
		///             
		byte[] Decode(byte[] encoded, int uncompressedSize);

	}

}