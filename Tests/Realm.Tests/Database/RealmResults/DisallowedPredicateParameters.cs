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

using System;
using System.Linq;
using NUnit.Framework;
using Realms;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DisallowedPredicateParameters : RealmTest
    {
        [Test]
        public void DisallowedPredicateParametersShouldThrow()
        {
            using var realm = GetRealm();
            var accessPublicField = realm.All<ClassWithUnqueryableMembers>().Where(c => c.PublicField == null);
            Assert.That(() => accessPublicField.ToList(), Throws.TypeOf<NotSupportedException>());

            var accessPublicMethod = realm.All<ClassWithUnqueryableMembers>().Where(c => c.PublicMethod() == null);
            Assert.That(() => accessPublicMethod.ToList(), Throws.TypeOf<NotSupportedException>());

            var accessIgnoredProperty = realm.All<ClassWithUnqueryableMembers>().Where(c => c.IgnoredProperty == null);
            Assert.That(() => accessIgnoredProperty.ToList(), Throws.TypeOf<NotSupportedException>());

            var accessNonAutomaticProperty = realm.All<ClassWithUnqueryableMembers>().Where(c => c.NonAutomaticProperty == null);
            Assert.That(() => accessNonAutomaticProperty.ToList(), Throws.TypeOf<NotSupportedException>());

            var accessPropertyWithOnlyGet = realm.All<ClassWithUnqueryableMembers>().Where(c => c.PropertyWithOnlyGet == null);
            Assert.That(() => accessPropertyWithOnlyGet.ToList(), Throws.TypeOf<NotSupportedException>());

            var indirectAccess = realm.All<ClassWithUnqueryableMembers>().Where(c => c.RealmObjectProperty.FirstName == null);
            Assert.That(() => indirectAccess.ToList(), Throws.TypeOf<NotSupportedException>());

            var listAccess = realm.All<ClassWithUnqueryableMembers>().Where(c => c.RealmListProperty != null);
            Assert.That(() => listAccess.ToArray(), Throws.TypeOf<NotSupportedException>());

            var person = new Person();
            var listContains = realm.All<ClassWithUnqueryableMembers>().Where(c => c.RealmListProperty.Contains(person));
            Assert.That(() => listContains.ToArray(), Throws.TypeOf<NotSupportedException>());

            var backlinkAccess = realm.All<ClassWithUnqueryableMembers>().Where(c => c.BacklinkProperty != null);
            Assert.That(() => backlinkAccess.ToArray(), Throws.TypeOf<NotSupportedException>());

            var backlinkItem = new UnqueryableBacklinks();
            var backlinkContains = realm.All<ClassWithUnqueryableMembers>().Where(c => c.BacklinkProperty.Contains(backlinkItem));
            Assert.That(() => backlinkContains.ToArray(), Throws.TypeOf<NotSupportedException>());
        }
    }
}
