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
using MongoDB.Bson;
using Realms;

namespace SourceGeneratorPlayground
{
    public partial class UnsupportedRequiredTypes : IRealmObject
    {
        [Required]
        public RealmInteger<int>? NullableRealmIntegerProp { get; set; }

        [Required]
        public byte[] ByteArrayProp { get; set; }

        [Required]
        public bool BoolProp { get; set; }

        [Required]
        public DateTimeOffset DateProp { get; set; }

        [Required]
        public float FloatProp { get; set; }

        [Required]
        public double DoubleProp { get; set; }

        [Required]
        public RealmObj ObjectProp { get; set; }

        [Required]
        public RealmValue RealmvalueProp { get; set; }

        [Required]
        public decimal DecimalProp { get; set; }

        [Required]
        public ObjectId ObjectIdProp { get; set; }

        [Required]
        public Guid GuidProp { get; set; }

        [Required]
        public int[] UnsupportedProp { get; set; }
    }
}
