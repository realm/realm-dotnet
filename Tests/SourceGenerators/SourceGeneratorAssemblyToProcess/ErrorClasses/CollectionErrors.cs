////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Collections.Generic;
using Realms;

namespace SourceGeneratorPlayground
{
    public partial class CollectionErrors : IRealmObject
    {
        public IDictionary<int, string> UnsupportetDictionaryKeyProp { get; }

        public ISet<EmbeddedObj> SetOfEmbeddedObj { get; }

        public IList<int> CollectionWithSetter { get; set; }

        public IList<RealmInteger<int>> CollectionOfRealmInteger { get; }

        public IList<DateTime> CollectionOfUnsupportedType { get; }

        public List<int> ListInsteadOfIList { get; }

        public IList<int> CollectionWithInitializer { get; } = new List<int>
        {
            1,
            2,
            3
        };

        public IList<string> CollectionWithCtorInitializer { get; }

        // This should not generate error as it's initialized to null
        public IList<string> ValidCollectionInitializer { get; } = null!;

        public IList<string> ValidCollectionInitializerInCtor { get; }

        public CollectionErrors()
        {
            CollectionWithCtorInitializer = new List<string>();

            // This should not generate error as it's initialized to null
            ValidCollectionInitializerInCtor = null!;
        }
    }
}
