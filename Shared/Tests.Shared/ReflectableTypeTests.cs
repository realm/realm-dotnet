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
using System.Reflection;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ReflectableTypeTests
    {
        private const string DogName = "Sharo";
        private const string OwnerName = "Peter";

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
        public void ReflectedGetRelatedObject_WhenObjectIsValid_ShouldReturnObject()
        {
            var owner = AddDogAndOwner();

            var typeInfo = owner.GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty(nameof(Owner.TopDog));
            var getter = topDogProperty.GetMethod;

            var topDog = getter.Invoke(owner, null) as Dog;
            Assert.That(topDog, Is.Not.Null);
            Assert.That(topDog.Name, Is.EqualTo(DogName));
        }

        [Test]
        public void ReflectedPropertyGetValue_WhenObjectIsValid_ShouldReturnObject()
        {
            var owner = AddDogAndOwner();

            var typeInfo = owner.GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty(nameof(Owner.TopDog));

            var topDog = topDogProperty.GetValue(owner, null) as Dog;
            Assert.That(topDog, Is.Not.Null);
            Assert.That(topDog.Name, Is.EqualTo(DogName));
        }

        [Test]
        public void RegularGetRelatedObject_WhenObjectIsValid_ShouldReturnObject()
        {
            var owner = AddDogAndOwner();
            var topDog = owner.TopDog;
            Assert.That(topDog, Is.Not.Null);
            Assert.That(topDog.Name, Is.EqualTo(DogName));
        }

        [Test]
        public void ReflectedGetRelatedObject_WhenObjectIsRemoved_ShouldReturnNull()
        {
            var owner = AddDogAndOwner();

            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = owner.GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty(nameof(Owner.TopDog));
            var getter = topDogProperty.GetMethod;

            var topDog = getter.Invoke(owner, null);
            Assert.That(topDog, Is.Null);
        }

        [Test]
        public void ReflectedPropertyGetValue_WhenObjectIsRemoved_ShouldReturnNull()
        {
            var owner = AddDogAndOwner();

            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = owner.GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty(nameof(Owner.TopDog));

            var topDog = topDogProperty.GetValue(owner, null);
            Assert.That(topDog, Is.Null);
        }

        [Test]
        public void RegularGetRelatedObject_WhenObjectIsRemoved_ShouldThrow()
        {
            var owner = AddDogAndOwner();
            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            Assert.Throws<RealmInvalidObjectException>(() =>
            {
                var topDog = owner.TopDog;
            });
        }

        [Test]
        public void ReflectedGetTopLevelProperty_WhenObjectIsValid_ShouldReturnValue()
        {
            var owner = AddDogAndOwner();

            var typeInfo = owner.GetTypeInfo();
            var nameGetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).GetMethod;

            var name = nameGetter.Invoke(owner, null);
            Assert.That(name, Is.EqualTo(OwnerName));
        }

        [Test]
        public void RegularGetTopLevelProperty_WhenObjectIsValid_ShouldReturnValue()
        {
            var owner = AddDogAndOwner();
            Assert.That(owner.Name, Is.EqualTo(OwnerName));
        }

        [Test]
        public void ReflectedGetTopLevelProperty_WhenObjectIsRemoved_ShouldReturnDefault()
        {
            var owner = AddDogAndOwner();
            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = owner.GetTypeInfo();
            var nameGetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).GetMethod;

            var name = nameGetter.Invoke(owner, null);
            Assert.That(name, Is.EqualTo(default(string)));
        }

        [Test]
        public void RegularGetTopLevelProperty_WhenObjectIsRemoved_ShouldReturnDefault()
        {
            var owner = AddDogAndOwner();
            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            Assert.Throws<RealmInvalidObjectException>(() =>
            {
                var name = owner.Name;
            });
        }

        [Test]
        public void ReflectedSetter_WhenObjectIsValid_ShouldSetValue()
        {
            var owner = AddDogAndOwner();
            var typeInfo = owner.GetTypeInfo();
            var nameSetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).SetMethod;

            _realm.Write(() =>
            {
                nameSetter.Invoke(owner, new[] { "John" });
            });

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void ReflectedSetter_WhenObjectIsInvalid_ShouldThrow()
        {
            var owner = AddDogAndOwner();
            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = owner.GetTypeInfo();
            var nameSetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).SetMethod;

            Assert.Throws<TargetInvocationException>(() =>
            {
                _realm.Write(() =>
                {
                    nameSetter.Invoke(owner, new[] { "John" });
                });
            });
        }

        private Owner AddDogAndOwner()
        {
            var owner = new Owner
            {
                TopDog = new Dog
                {
                    Name = DogName
                },
                Name = OwnerName
            };

            _realm.Write(() =>
            {
                _realm.Manage(owner);
            });

            return owner;
        }
    }
}