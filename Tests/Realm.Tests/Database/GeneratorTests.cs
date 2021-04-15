// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2021 Realm Inc.
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
using NUnit.Framework;
using Realm.Generator;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AAAAAAGeneratorTests : RealmInstanceTest
    {
        [Test]
        public void TestA()
        {
            var standalone = new SimpleObject
            {
                Id = 1,
                StringValue = "bla"
            };

            _realm.Write(() =>
            {
                _realm.Add(standalone, update: true);
            });

            Assert.That(standalone.IsManaged, Is.True);

            var queried = _realm.Find<SimpleObject>(1);
            Assert.That(queried, Is.EqualTo(standalone));
        }
    }

    public partial class SimpleObject : RealmObject
    {
        [PrimaryKey]
        public long Id { get; set; }

        public string StringValue { get; set; }
    }
}
