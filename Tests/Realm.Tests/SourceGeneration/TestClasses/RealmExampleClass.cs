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

namespace Realms.Tests.SourceGeneration.TestClasses
{
    public partial class RealmExampleClass: IRealmObject
    {
        [PrimaryKey]
        public int PrimaryKey { get; set; }

        //public int Age { get; set; }

        //public int? NullableAge { get; set; }

        //[Ignored]
        //public int IgnoredString { get; set; }

        //public IList<int> IntList { get; }

        //public byte[] ByteArray { get; set; }

        //public EmbeddedClass Embedded { get; set; }

        //public IList<EmbeddedClass> ListOfEmbedded { get;  }

        [PrimaryKey]
        public IDictionary<string, int> DictionaryInt { get; }
    }

    public partial class EmbeddedClass : IEmbeddedObject
    {

    }
}
