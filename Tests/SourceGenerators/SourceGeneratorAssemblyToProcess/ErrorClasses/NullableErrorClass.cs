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
    public partial class NullableErrorClass : IRealmObject
    {
        public IList<string>? NullableCollection { get; } = null!;

        public NullableErrorClass NonNullableObject { get; set; } = null!;

        public RealmValue? NullableRealmValue { get; set; }

        public NullableErrorClass? NullableObject { get; set; }

        [Realms.Backlink(nameof(NullableObject))]
        public IQueryable<NullableErrorClass>? NullableBacklink { get; }
    }
#nullable disable
}
