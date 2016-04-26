/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

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
