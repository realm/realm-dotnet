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

using NUnit.Framework;
using Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class DateTimeTests
    {
        //TODO: this is ripe for refactoring across test fixture classes

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

        [Test]
        public void SetAndGetPropertyTest()
        {
            var turingsBirthday = new DateTimeOffset(1912, 6, 23, 0, 0, 0, TimeSpan.Zero);

            Person turing;
            using (var transaction = _realm.BeginWrite())
            {
                turing = _realm.CreateObject<Person>();
                turing.FirstName = "Alan";
                turing.LastName = "Turing";
                turing.Birthday = turingsBirthday;
                transaction.Commit();
            }

            // perform a db fetch
            var turingAgain = _realm.All<Person>().First();

            Assert.That(turingAgain.Birthday, Is.EqualTo(turingsBirthday));
        }
    }
}
