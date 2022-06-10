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

using System.Linq;
using NUnit.Framework;
using Realms.Tests.SourceGeneration.TestClasses;

namespace Realms.Tests.SourceGeneration
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ManualGenerationTests : RealmInstanceTest
    {
        [Test]
        public void TestUnmanaged()
        {
            var mgc = new ManualllyGeneratedClass();
            mgc.PrimaryKeyValue = 1;
            mgc.StringValue = "Mario";
            mgc.IntValue = 24;
            mgc.ListValue.Add(10);
            mgc.ListValue.Add(20);

            Assert.That(mgc.PrimaryKeyValue, Is.EqualTo(1));
            Assert.That(mgc.StringValue, Is.EqualTo("Mario"));
            Assert.That(mgc.IntValue, Is.EqualTo(24));
            Assert.That(mgc.ListValue[0], Is.EqualTo(10));
            Assert.That(mgc.ListValue[1], Is.EqualTo(20));
        }

        [Test]
        public void TestManaged()
        {
            var mgc = new ManualllyGeneratedClass();
            mgc.PrimaryKeyValue = 1;
            mgc.StringValue = "Mario";
            mgc.IntValue = 24;
            mgc.ListValue.Add(10);
            mgc.ListValue.Add(20);

            using var realm = GetRealm();

            realm.Write(() =>
            {
                realm.Add(mgc);
            });

            var retrieved = realm.Find<ManualllyGeneratedClass>(1);
            retrieved = realm.All<ManualllyGeneratedClass>().First();

            Assert.That(retrieved.PrimaryKeyValue, Is.EqualTo(1));
            Assert.That(retrieved.StringValue, Is.EqualTo("Mario"));
            Assert.That(retrieved.IntValue, Is.EqualTo(24));
            Assert.That(retrieved.ListValue.Count, Is.EqualTo(2));
            Assert.That(retrieved.ListValue[0], Is.EqualTo(10));
            Assert.That(retrieved.ListValue[1], Is.EqualTo(20));

            realm.Write(() =>
            {
                mgc.StringValue = "Luigi";
                mgc.IntValue = 15;
                mgc.ListValue.Add(30);
            });

            Assert.That(retrieved.StringValue, Is.EqualTo("Luigi"));
            Assert.That(retrieved.IntValue, Is.EqualTo(15));
            Assert.That(retrieved.ListValue.Count, Is.EqualTo(3));
            Assert.That(retrieved.ListValue[0], Is.EqualTo(10));
            Assert.That(retrieved.ListValue[1], Is.EqualTo(20));
            Assert.That(retrieved.ListValue[2], Is.EqualTo(30));

            realm.Write(() =>
            {
                retrieved.StringValue = "Peach";
                retrieved.IntValue = 65;
                retrieved.ListValue.Add(40);
            });

            Assert.That(mgc.StringValue, Is.EqualTo("Peach"));
            Assert.That(mgc.IntValue, Is.EqualTo(65));
            Assert.That(mgc.ListValue.Count, Is.EqualTo(4));
            Assert.That(mgc.ListValue[0], Is.EqualTo(10));
            Assert.That(mgc.ListValue[1], Is.EqualTo(20));
            Assert.That(mgc.ListValue[2], Is.EqualTo(30));
            Assert.That(mgc.ListValue[3], Is.EqualTo(40));
        }
    }
}
