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
    [RealmClass]
    public partial class SimpleObject : RealmObject
    {
        private int intValue;
        private string stringValue;
    }

    [TestFixture, Preserve(AllMembers = true)]
    public class AAAAAAGeneratorTests : RealmInstanceTest
    {
        [Test]
        public void TestA()
        {
            var intValue = 1;
            var stringValue = "bla";

            var standalone = new SimpleObject
            {
                IntValue = intValue,
                StringValue = stringValue,
            };

            Assert.That(standalone.IntValue, Is.EqualTo(intValue));
            Assert.That(standalone.StringValue, Is.EqualTo(stringValue));

            _realm.Write(() =>
            {
                _realm.Add(standalone, update: true);
            });

            Assert.That(standalone.IsManaged, Is.True);

            Assert.That(standalone.IntValue, Is.EqualTo(intValue));
            Assert.That(standalone.StringValue, Is.EqualTo(stringValue));

            var queried = _realm.All<SimpleObject>().First();
            Assert.That(queried.IntValue, Is.EqualTo(standalone.IntValue));
            Assert.That(queried.StringValue, Is.EqualTo(standalone.StringValue));

            intValue = 5;
            stringValue = "abracadabra";

            _realm.Write(() =>
            {
                standalone.IntValue = intValue;
                standalone.StringValue = stringValue;
            });

            Assert.That(standalone.IntValue, Is.EqualTo(intValue));
            Assert.That(standalone.StringValue, Is.EqualTo(stringValue));

            queried = _realm.All<SimpleObject>().First();
            Assert.That(queried.IntValue, Is.EqualTo(standalone.IntValue));
            Assert.That(queried.StringValue, Is.EqualTo(standalone.StringValue));
        }
    }
}
