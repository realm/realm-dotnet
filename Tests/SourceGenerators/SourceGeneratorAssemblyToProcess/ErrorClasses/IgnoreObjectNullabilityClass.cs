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
    public partial class IgnoreObjectNullabilityClass : IRealmObject
    {
        public IgnoreObjectNullabilityClass? NullableObject { get; set; } = null!;

        public IgnoreObjectNullabilityClass NonNullableObject { get; set; } = null!;

        public IList<IgnoreObjectNullabilityClass> ListNonNullableObject { get; } = null!;

        public IList<IgnoreObjectNullabilityClass?> ListNullableObject { get; } = null!;

        public ISet<IgnoreObjectNullabilityClass> SetNonNullableObject { get; } = null!;

        public ISet<IgnoreObjectNullabilityClass?> SetNullableObject { get; } = null!;

        public IDictionary<string, IgnoreObjectNullabilityClass> DictionaryNonNullableObject { get; } = null!;

        public IDictionary<string, IgnoreObjectNullabilityClass> DictionaryNullableObject { get; } = null!;

        [Realms.Backlink(nameof(NullableObject))]
        public IQueryable<IgnoreObjectNullabilityClass?> BacklinkNullableObject { get; } = null!;

        [Realms.Backlink(nameof(NullableObject))]
        public IQueryable<IgnoreObjectNullabilityClass> BacklinkNonNullableObject { get; } = null!;
    }
#nullable disable
}
