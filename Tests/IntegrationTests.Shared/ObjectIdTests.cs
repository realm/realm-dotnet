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
using System.Text;
using System.Linq;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    // using classes from TestObjects.cs
    [TestFixture, Preserve(AllMembers = true)]
    public class ObjectIdTests
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

        [Test]
        public void FindByStringId()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdStringObject>();
                obj.StringProperty = "Zaphod";
            });

            var foundObj = _realm.ById<ObjectIdStringObject>("Zaphod");
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.StringProperty, Is.EqualTo("Zaphod"));
        }


        [Test]
        public void ExceptionIfNoIdDeclared()
        {
            Assert.Throws<RealmClassLacksObjectIdException>( () => {
                var foundObj = _realm.ById<Person>("Zaphod");
            });
        }
    }
}

