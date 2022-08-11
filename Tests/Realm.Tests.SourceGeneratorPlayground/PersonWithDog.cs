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

using Realms;

namespace SourceGeneratorPlayground
{
    public partial class Person : IRealmObject
    {
        [PrimaryKey]
        public int Id { set; get; }

        public string Name { get; set; }

        [Backlink("Owner")]
        public IQueryable<Dog> Dogs { get; }
    }

    public partial class Dog : IRealmObject
    {
        public string Name { get; set; }

        public Person Owner { get; set; }
    }
}
