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
        protected string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
        }

        [Test]
        public void SetValueAndReplaceWithNull()
        {
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                TestHelpers.SetPropertyValue(ato, "NullableBooleanProperty", true);
                transaction.Commit();
            }

            Assert.That(ato.NullableBooleanProperty, Is.EqualTo(true));

            using (var transaction = _realm.BeginWrite())
            {
                TestHelpers.SetPropertyValue(ato, "NullableBooleanProperty", null);
                transaction.Commit();
            }

            Assert.That(ato.NullableBooleanProperty, Is.EqualTo(null));
        }
    }
}
