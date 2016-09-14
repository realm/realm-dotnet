using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared.RealmResults
{
    [TestFixture]
    public class DisallowedPredicateParameters
    {
        [Test]
        public void DisallowedPredicateParametersShouldThrow()
        {
            var realm = Realm.GetInstance();

            var accessPublicField = realm.All<ClassWithUnqueriableMembers>().Where(c => c.PublicField == null);
            Assert.Throws<NotSupportedException>(() => accessPublicField.ToList());

            var accessPublicMethod = realm.All<ClassWithUnqueriableMembers>().Where(c => c.PublicMethod() == null);
            Assert.Throws<NotSupportedException>(() => accessPublicMethod.ToList());

            var accessIgnoredProperty = realm.All<ClassWithUnqueriableMembers>().Where(c => c.IgnoredProperty == null);
            Assert.Throws<NotSupportedException>(() => accessIgnoredProperty.ToList());

            var accessNonAutomaticProperty = realm.All<ClassWithUnqueriableMembers>().Where(c => c.NonAutomaticProperty == null);
            Assert.Throws<NotSupportedException>(() => accessNonAutomaticProperty.ToList());

            var indirectAccess =
                realm.All<ClassWithUnqueriableMembers>().Where(c => c.RealmObjectProperty.FirstName == null);
            Assert.Throws<NotSupportedException>(() => indirectAccess.ToList());
        }
    }
}
