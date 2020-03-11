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
namespace Avro
{
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A singleton class containing data used by codegen.
    /// </summary>
    public sealed class CodeGenUtil
    {
        /// <summary>
        /// Gets singleton instance of this class.
        /// </summary>
        public static CodeGenUtil Instance { get; } = new CodeGenUtil();

        /// <summary>
        /// Gets namespaces to import in generated code.
        /// </summary>
        public CodeNamespaceImport[] NamespaceImports { get; private set; }

        /// <summary>
        /// Gets comment included at the top of each generated code file.
        /// </summary>
        public CodeCommentStatement FileComment { get; private set; }

        /// <summary>
        /// Gets reserved keywords in the C# language.
        /// </summary>
        public HashSet<string> ReservedKeywords { get; private set; }

        private const char At = '@';
        private const char Dot = '.';

        /// <summary>
        /// Fully-qualified name of a <see cref="Object"/> type.
        /// </summary>
        public const string Object = "System.Object";

        private CodeGenUtil()
        {
            this.NamespaceImports = new CodeNamespaceImport[] {
                new CodeNamespaceImport("System"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.Text"),
                new CodeNamespaceImport("Avro"),
                new CodeNamespaceImport("Avro.Specific") };

            this.FileComment = new CodeCommentStatement(
@"------------------------------------------------------------------------------
 <auto-generated>
    Generated by " + System.AppDomain.CurrentDomain.FriendlyName + ", version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + @"
    Changes to this file may cause incorrect behavior and will be lost if code
    is regenerated
 </auto-generated>
 ------------------------------------------------------------------------------");

            // Visual Studio 2010 https://msdn.microsoft.com/en-us/library/x53a06bb.aspx
            this.ReservedKeywords = new HashSet<string>() {
                "abstract","as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
                "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
                "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
                "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
                "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
                "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
                "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
                "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while", "value", "partial" };
        }

        /// <summary>
        /// Append @ to all reserved keywords that appear on the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Mangle(string name)
        {
            var builder = new StringBuilder();
            string[] names = name.Split(Dot);
            for (int i = 0; i < names.Length; ++i)
            {
                if (this.ReservedKeywords.Contains(names[i]))
                {
                    builder.Append(At);
                }

                builder.Append(names[i]);
                builder.Append(Dot);
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        /// <summary>
        /// Remove all the @.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string UnMangle(string name)
        {
            var builder = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; ++i)
                if (name[i] != At)
                {
                    builder.Append(name[i]);
                }

            return builder.ToString();
        }
    }
}
