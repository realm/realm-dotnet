////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using Realms;

namespace SourceGeneratorAssemblyToProcess
{
    internal partial class IndexedClass : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        [Indexed(IndexType.FullText)]
        public string FullTextProp { get; set; } = "";

        [Indexed(IndexType.FullText)]
        public string? NullableFullTextProp { get; set; }

        [Indexed(IndexType.General)]
        public int IntProp { get; set; }

        [Indexed]
        public Guid GuidProp { get; set; }

        [Indexed(IndexType.General)]
        public Guid GeneralGuidProp { get; set; }
    }
}
