////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System.IO;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class AccessTests
    {
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [TestCase("NullableCharProperty", '0')]
        [TestCase("NullableByteProperty", (byte)100)]
        [TestCase("NullableInt16Property", (short)100)]
        [TestCase("NullableInt32Property", 100)]
        [TestCase("NullableInt64Property", 100L)]
        [TestCase("NullableSingleProperty", 123.123f)] 
        [TestCase("NullableDoubleProperty", 123.123)] 
        [TestCase("NullableBooleanProperty", true)]
        [TestCase("StringProperty", "foo")]
        public void SetValueAndReplaceWithNull(string propertyName, object propertyValue)
        {
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                TestHelpers.SetPropertyValue(ato, propertyName, propertyValue);
                transaction.Commit();
            }

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(propertyValue));

            using (var transaction = _realm.BeginWrite())
            {
                TestHelpers.SetPropertyValue(ato, propertyName, null);
                transaction.Commit();
            }

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(null));
        }
    }
}
