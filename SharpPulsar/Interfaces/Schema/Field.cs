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
namespace SharpPulsar.Interfaces.Schema
{
    /// <summary>
    /// A field in a record, consisting of a field name, index, and
    /// <seealso cref="Schema"/> for the field value.
    /// </summary>
    public class Field
    {

        /// <summary>
        /// The field name.
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// The index of the field within the record.
        /// </summary>
        private readonly int _index;
        public Field(string name, int index)
        {
            _name = name;
            _index = index;            
        }
        public string Name => _name;
        public int Index => _index;
    }

}