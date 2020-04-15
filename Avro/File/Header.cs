/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Avro.Schemas;

namespace Avro.File
{
    using System.Collections.Generic;

    /// <summary>
    /// Header on an Avro data file.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Gets metadata in this header.
        /// </summary>
        public IDictionary<string, byte[]> MetaData { get; }

        /// <summary>
        /// Gets sync token.
        /// </summary>
        public byte[] SyncData { get; }

        /// <summary>
        /// Gets or sets avro schema.
        /// </summary>
        public Schema Schema { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Header"/> class.
        /// </summary>
        public Header()
        {
            this.MetaData = new Dictionary<string, byte[]>();
            this.SyncData = new byte[16];
        }
    }
}
