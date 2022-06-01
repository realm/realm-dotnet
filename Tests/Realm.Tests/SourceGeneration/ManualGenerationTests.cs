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
using NUnit.Framework;
using Realms.Schema;
using Realms.Tests.SourceGeneration.TestClasses;

namespace Realms.Tests.SourceGeneration
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ManulGenerationTests
    {
        [Test]
        public void TestUnmanaged()
        {
            var mgc = new ManualllyGeneratedClass();
            mgc.Integer = 24;
            mgc.IntegerList.Add(10);
            mgc.IntegerList.Add(20);

            Assert.That(mgc.Integer, Is.EqualTo(24));
            Assert.That(mgc.IntegerList[0], Is.EqualTo(10));
            Assert.That(mgc.IntegerList[1], Is.EqualTo(20));
        }

        [Test]
        public void TestManaged()
        {
            var mgc = new ManualllyGeneratedClass();
            mgc.Integer = 24;
            mgc.IntegerList.Add(10);
            mgc.IntegerList.Add(20);

            var schemaBuilder = new RealmSchema.Builder();
            schemaBuilder.Add(ObjectSchema.FromType(typeof(ManualllyGeneratedClass)));

            var config = new RealmConfiguration();
            config.Schema = schemaBuilder.Build();

            var realm = Realm.GetInstance();

            realm.Write(() =>
            {
                realm.RemoveAll<ManualllyGeneratedClass>();
            });

            realm.Write(() =>
            {
                realm.Add(mgc);
            });

            var retrieved = realm.All<ManualllyGeneratedClass>().First();

            Assert.That(retrieved.Integer, Is.EqualTo(24));
            Assert.That(retrieved.IntegerList.Count, Is.EqualTo(2));
            Assert.That(retrieved.IntegerList[0], Is.EqualTo(10));
            Assert.That(retrieved.IntegerList[1], Is.EqualTo(20));

            realm.Write(() =>
            {
                mgc.Integer = 15;
                mgc.IntegerList.Add(30);
            });

            Assert.That(retrieved.Integer, Is.EqualTo(15));
            Assert.That(retrieved.IntegerList.Count, Is.EqualTo(3));
            Assert.That(retrieved.IntegerList[0], Is.EqualTo(10));
            Assert.That(retrieved.IntegerList[1], Is.EqualTo(20));
            Assert.That(retrieved.IntegerList[2], Is.EqualTo(30));

            realm.Write(() =>
            {
                mgc.Integer = 65;
                mgc.IntegerList.Add(40);
            });

            Assert.That(mgc.Integer, Is.EqualTo(65));
            Assert.That(mgc.IntegerList.Count, Is.EqualTo(4));
            Assert.That(mgc.IntegerList[0], Is.EqualTo(10));
            Assert.That(mgc.IntegerList[1], Is.EqualTo(20));
            Assert.That(mgc.IntegerList[2], Is.EqualTo(30));
            Assert.That(mgc.IntegerList[3], Is.EqualTo(40));
        }
    }
}
