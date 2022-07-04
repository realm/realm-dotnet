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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms;

namespace Realm.SourceGeneratorTestingPlayground
{
    public partial class MultiplePrimaryKeys : IRealmObject
    {
        [PrimaryKey]
        public int PrimaryKey1 { get; set; }

        [PrimaryKey]
        public int PrimaryKey2 { get; set; }
    }

    public partial class EmbeddedWithPrimaryKey : IEmbeddedObject
    {
        [PrimaryKey]
        public int PrimaryKey1 { get; set; }
    }

    public partial class UnsupportedPrimaryKeyTypes : IRealmObject
    {
        [PrimaryKey]
        public RealmInteger<int> RealmIntegerProp { get; set; }

        [PrimaryKey]
        public bool BoolProp { get; set; }

        [PrimaryKey]
        public byte[] ByteArrayProp { get; set; }

        [PrimaryKey]
        public DateTimeOffset DateProp { get; set; }

        [PrimaryKey]
        public float FloatProp { get; set; }

        [PrimaryKey]
        public double DoubleProp { get; set; }

        [PrimaryKey]
        public MultiplePrimaryKeys ObjectProp { get; set; }

        [PrimaryKey]
        public RealmValue RealmvalueProp { get; set; }

        [PrimaryKey]
        public decimal DecimalProp { get; set; }

        [PrimaryKey]
        public int[] UnsupportedProp { get; set; }
    }

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
        public MultiplePrimaryKeys ObjectProp { get; set; }

        [Indexed]
        public RealmValue RealmvalueProp { get; set; }

        [Indexed]
        public decimal DecimalProp { get; set; }

        [Indexed]
        public int[] UnsupportedProp { get; set; }
    }

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
        public MultiplePrimaryKeys ObjectProp { get; set; }

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

    public partial class BacklinkClass : IRealmObject
    {
        public UnsupportedBacklink InverseLink { get; set; }
    }

    public partial class UnsupportedBacklink : IRealmObject
    {
        [Backlink("WrongPropertyName")]
        public IQueryable<BacklinkClass> WrongBacklinkProp { get; }

        [Backlink(nameof(BacklinkClass.InverseLink))]
        public IQueryable<BacklinkClass> CorrectBacklinkProp { get; }
    }
}
