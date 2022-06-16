// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Realms.Schema;

namespace Realm.SourceGenerator
{
    internal record ClassInfo
    {
        public string Name { get; set; }

        public bool IsEmbedded { get; set; }

        public string MapTo { get; set; }

        public string Namespace { get; set; }

        public Accessibility Accessibility { get; set; }

        public IEnumerable<PropertyInfo> Properties { get; set; }

    }

    internal record PropertyInfo
    {
        public string Name { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsRequired { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsNullable { get; set; }

        public string MapTo { get; set; }

        public string Backlink { get; set; }

        public TypeInfo TypeInfo { get; set; }

        public Accessibility Accessibility { get; set; }  //TODO At the end check if this is needed
    }

    internal record TypeInfo
    {
        public PropertyType Type { get; set; }

        public string TypeString { get; set; }

    }
}
