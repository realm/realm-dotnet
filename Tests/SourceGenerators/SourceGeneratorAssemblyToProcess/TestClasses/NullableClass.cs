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

using System.Collections.Generic;
using System.Linq;
using Realms;

namespace SourceGeneratorAssemblyToProcess
{
#nullable enable
    public partial class NullableClass : IRealmObject
    {
        public int NonNullableInt { get; set; }

        public int? NullableInt { get; set; }

        public string NonNullableString { get; set; } = null!;

        public string? NullableString { get; set; }

        public byte[] NonNullableData { get; set; } = null!;

        public byte[]? NullableData { get; set; }

        public IList<int?> CollectionOfNullableInt { get; } = null!;

        public IList<int> CollectionOfNonNullableInt { get; } = null!;

        public IList<string?> CollectionOfNullableString { get; } = null!;

        public IList<string> CollectionOfNonNullableString { get; } = null!;

        public RealmInteger<int> NonNullableRealmInt { get; set; }

        public RealmInteger<int>?  NullableRealmInt { get; set; }

        public NullableClass? NullableObject { get; set; }

        public IList<NullableClass> ListNonNullabeObject { get; } = null!;

        public ISet<NullableClass> SetNonNullableObject { get; } = null!;

        public IDictionary<string, NullableClass?> DictionaryNullableObject { get; } = null!;

        public RealmValue NonNullableRealmValue { get; set; }

        [Realms.Backlink(nameof(NullableObject))]
        public IQueryable<NullableClass> Backlink { get; } = null!;
    }
#nullable disable
}
