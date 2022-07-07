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
using Realms;

namespace Realm.SourceGeneratorTestingPlayground
{
    public partial class TestClass : IRealmObject
    {
        //public int IntProp { get; set; }

        //public string StringPropClassic { get; set; }

#nullable enable
        //public string StringPropNew { get; set; }

        //public byte[]? NullableByte { get; set; }

        //public int? NullableInt { get; set; }

        public string? StringPropNullable { get; set; }

        public IList<string?>? TestDarling;
    }
}
