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
            mgc.PKey = 1;
            mgc.Name = "Mario";
            mgc.Integer = 24;
            mgc.IntegerList.Add(10);
            mgc.IntegerList.Add(20);

            Assert.That(mgc.PKey, Is.EqualTo(1));
            Assert.That(mgc.Name, Is.EqualTo("Mario"));
            Assert.That(mgc.Integer, Is.EqualTo(24));
            Assert.That(mgc.IntegerList[0], Is.EqualTo(10));
            Assert.That(mgc.IntegerList[1], Is.EqualTo(20));
        }

        [Test]
        public void TestManaged()
        {
            var mgc = new ManualllyGeneratedClass();
            mgc.PKey = 1;
            mgc.Name = "Mario";
            mgc.Integer = 24;
            mgc.IntegerList.Add(10);
            mgc.IntegerList.Add(20);

            using var realm = GetRealm();

            realm.Write(() =>
            {
                realm.Add(mgc);
            });

            var retrieved = realm.Find<ManualllyGeneratedClass>(1);
            retrieved = realm.All<ManualllyGeneratedClass>().First();

            Assert.That(retrieved.PKey, Is.EqualTo(1));
            Assert.That(retrieved.Name, Is.EqualTo("Mario"));
            Assert.That(retrieved.Integer, Is.EqualTo(24));
            Assert.That(retrieved.IntegerList.Count, Is.EqualTo(2));
            Assert.That(retrieved.IntegerList[0], Is.EqualTo(10));
            Assert.That(retrieved.IntegerList[1], Is.EqualTo(20));

            realm.Write(() =>
            {
                mgc.Name = "Luigi";
                mgc.Integer = 15;
                mgc.IntegerList.Add(30);
            });

            Assert.That(retrieved.Name, Is.EqualTo("Luigi"));
            Assert.That(retrieved.Integer, Is.EqualTo(15));
            Assert.That(retrieved.IntegerList.Count, Is.EqualTo(3));
            Assert.That(retrieved.IntegerList[0], Is.EqualTo(10));
            Assert.That(retrieved.IntegerList[1], Is.EqualTo(20));
            Assert.That(retrieved.IntegerList[2], Is.EqualTo(30));

            realm.Write(() =>
            {
                retrieved.Name = "Peach";
                retrieved.Integer = 65;
                retrieved.IntegerList.Add(40);
            });

            Assert.That(mgc.Name, Is.EqualTo("Peach"));
            Assert.That(mgc.Integer, Is.EqualTo(65));
            Assert.That(mgc.IntegerList.Count, Is.EqualTo(4));
            Assert.That(mgc.IntegerList[0], Is.EqualTo(10));
            Assert.That(mgc.IntegerList[1], Is.EqualTo(20));
            Assert.That(mgc.IntegerList[2], Is.EqualTo(30));
            Assert.That(mgc.IntegerList[3], Is.EqualTo(40));
        }
    }
}
