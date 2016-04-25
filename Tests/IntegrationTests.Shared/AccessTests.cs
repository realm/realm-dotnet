/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
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

        [TestCase("StringProperty", "hello")]
        [TestCase("CharProperty", '0')]
        [TestCase("ByteProperty", (byte)100)]
        [TestCase("Int16Property", (short)100)]
        [TestCase("Int32Property", 100)]
        [TestCase("Int64Property", 100L)]
        [TestCase("SingleProperty", 123.123f)] 
        [TestCase("DoubleProperty", 123.123)] 
        [TestCase("BooleanProperty", true)]
        public void SetAndGetValue(string propertyName, object propertyValue)
        {
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                TestHelpers.SetPropertyValue(ato, propertyName, propertyValue);
                transaction.Commit();
            }

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(propertyValue));
        }

        [Test]
        public void SetAndGetGuidValue()
        {
            var guid = Guid.NewGuid();
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                ato.GuidProperty = guid;
                transaction.Commit();
            }

            Assert.That(ato.GuidProperty, Is.EqualTo(guid));
        }

        [TestCase("NullableCharProperty", '0')]
        [TestCase("NullableByteProperty", (byte)100)]
        [TestCase("NullableInt16Property", (short)100)]
        [TestCase("NullableInt32Property", 100)]
        [TestCase("NullableInt64Property", 100L)]
        [TestCase("NullableSingleProperty", 123.123f)] 
        [TestCase("NullableDoubleProperty", 123.123)] 
        [TestCase("NullableBooleanProperty", true)]
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

            Assert.That(ato.NullableBooleanProperty, Is.EqualTo(null));
        }

        [Test]
        public void SetAndGetNullableGuidValue()
        {
            var guid = Guid.NewGuid();
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                ato.NullableGuidProperty = guid;
                transaction.Commit();
            }

            Assert.That(ato.NullableGuidProperty, Is.EqualTo(guid));

            using (var transaction = _realm.BeginWrite())
            {
                ato.NullableGuidProperty = null;
                transaction.Commit();
            }

            Assert.That(ato.NullableGuidProperty, Is.EqualTo(null));
        }
    }
}
