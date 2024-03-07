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
using Realms;

namespace SourceGeneratorPlayground
{
    public partial class UnsupportedIndexableTypes : IRealmObject
    {
        [Indexed]
        public RealmInteger<int>? NullableRealmIntegerProp { get; set; }

        [Indexed]
        public byte[] ByteArrayProp { get; set; }

        [Indexed]
        public float FloatProp { get; set; }

        [Indexed]
        public double DoubleProp { get; set; }

        [Indexed]
        public RealmObj? ObjectProp { get; set; }

        [Indexed]
        public decimal DecimalProp { get; set; }

        [Indexed]
        public int[] UnsupportedProp { get; set; } = null!;

        [Indexed(IndexType.FullText)]
        public int FtsIntProp { get; set; }

        [Indexed(IndexType.FullText)]
        public bool FtsBoolProp { get; set; }

        [Indexed(IndexType.FullText)]
        public RealmValue FtsRealmValueProp { get; set; }

        [Indexed(IndexType.FullText)]
        public RealmObj? FtsObjectProp { get; set; }

        [Indexed(IndexType.FullText)]
        public double FtsDoubleProp { get; set; }

        [Indexed(IndexType.None)]
        public int NoneIndexedInt { get; set; }

        [PrimaryKey]
        [Indexed]
        public int IndexedPrimaryKeyProp { get; set; }
    }
}
