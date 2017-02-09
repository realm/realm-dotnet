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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DisallowedPredicateParameters
    {
        [Test]
        public void DisallowedPredicateParametersShouldThrow()
        {
            var realm = Realm.GetInstance();

            var accessPublicField = realm.All<ClassWithUnqueryableMembers>().Where(c => c.PublicField == null);
            Assert.Throws<NotSupportedException>(() => accessPublicField.ToList());

            var accessPublicMethod = realm.All<ClassWithUnqueryableMembers>().Where(c => c.PublicMethod() == null);
            Assert.Throws<NotSupportedException>(() => accessPublicMethod.ToList());

            var accessIgnoredProperty = realm.All<ClassWithUnqueryableMembers>().Where(c => c.IgnoredProperty == null);
            Assert.Throws<NotSupportedException>(() => accessIgnoredProperty.ToList());

            var accessNonAutomaticProperty = realm.All<ClassWithUnqueryableMembers>().Where(c => c.NonAutomaticProperty == null);
            Assert.Throws<NotSupportedException>(() => accessNonAutomaticProperty.ToList());

            var accessPropertyWithOnlyGet = realm.All<ClassWithUnqueryableMembers>().Where(c => c.PropertyWithOnlyGet == null);
            Assert.Throws<NotSupportedException>(() => accessPropertyWithOnlyGet.ToList());

            var indirectAccess =
                realm.All<ClassWithUnqueryableMembers>().Where(c => c.RealmObjectProperty.FirstName == null);
            Assert.Throws<NotSupportedException>(() => indirectAccess.ToList());
        }
    }
}
