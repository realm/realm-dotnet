////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AARealmValueTests : RealmInstanceTest //TODO for testing
    {
        public AARealmValueTests()
        {
        }

        //TODO FP Most probably we can make the next function generic by using TestCases

        [Test]
        public void RealmValue_SetIntProperty()
        {
            var integerValue = 10;

            var rvo = new RealmValueObject
            {
                Id = 1,
                RealmValue = integerValue
            };

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            Assert.That(_realm.Find<RealmValueObject>(1).RealmValue.AsInt16(), Is.EqualTo(integerValue));
        }

        /*What to test?
         * - A realmValue can be set and retrieved with the same type from the db
         * - A realm value can change type and it can be retrieved
         * - Int, bool, char and others are memorised as 64bit, so we can see if we can cast it to narrower types
         * - Need to add RealmValue to the test classes with all Types
         * 
         * 
         * 
         * Later:
         * - Queries
         * - List of realm values work as expected
         * 
         */


        private class RealmValueObject : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public RealmValue RealmValue { get; set; }
        }
    }
}
