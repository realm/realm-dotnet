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

using System.Reflection;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
#if WINDOWS
    [Ignore("ReflectableType is not respected by WPF.")]
#elif NETCOREAPP1_1
    [Ignore("ReflectableType is not needed/supported on .NET Core")]
#endif
    public class ReflectableTypeTests : RealmInstanceTest
    {
        private const string DogName = "Sharo";
        private const string OwnerName = "Peter";

        [Test]
        public void ReflectableGetRelatedObject_WhenObjectIsValid_ShouldReturnObject()
        {
            var owner = AddDogAndOwner();

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty(nameof(Owner.TopDog));
            var getter = topDogProperty.GetMethod;

            var topDog = getter.Invoke(owner, null) as Dog;
            Assert.That(topDog, Is.Not.Null);
            Assert.That(topDog.Name, Is.EqualTo(DogName));
        }

        [Test]
        public void ReflectablePropertyGetValue_WhenObjectIsValid_ShouldReturnObject()
        {
            var owner = AddDogAndOwner();

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
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
        public void ReflectableGetRelatedObject_WhenObjectIsRemoved_ShouldReturnNull()
        {
            var owner = AddDogAndOwner();

            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty(nameof(Owner.TopDog));
            var getter = topDogProperty.GetMethod;

            var topDog = getter.Invoke(owner, null);
            Assert.That(topDog, Is.Null);
        }

        [Test]
        public void ReflectablePropertyGetValue_WhenObjectIsRemoved_ShouldReturnNull()
        {
            var owner = AddDogAndOwner();

            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
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
        public void ReflectableGetTopLevelProperty_WhenObjectIsValid_ShouldReturnValue()
        {
            var owner = AddDogAndOwner();

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
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
        public void ReflectableGetTopLevelProperty_WhenObjectIsRemoved_ShouldReturnDefault()
        {
            var owner = AddDogAndOwner();
            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
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
        public void ReflectableSetter_WhenObjectIsValid_ShouldSetValue()
        {
            var owner = AddDogAndOwner();
            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var nameSetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).SetMethod;

            _realm.Write(() =>
            {
                nameSetter.Invoke(owner, new[] { "John" });
            });

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void ReflectableSetter_WhenNotInTransaction_ShouldCreateTransactionAndCommit()
        {
            var owner = AddDogAndOwner();
            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var nameSetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).SetMethod;

            nameSetter.Invoke(owner, new[] { "John" });

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void ReflectableSetValue_WhenObjectIsValid_ShouldSetValue()
        {
            var owner = AddDogAndOwner();
            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var pi = typeInfo.GetDeclaredProperty(nameof(Owner.Name));

            _realm.Write(() =>
            {
                pi.SetValue(owner, "John");
            });

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void ReflectableSetValue_WhenNotInTransaction_ShouldCreateTransactionAndCommit()
        {
            var owner = AddDogAndOwner();
            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var pi = typeInfo.GetDeclaredProperty(nameof(Owner.Name));

            pi.SetValue(owner, "John");

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void Setter_WhenNotInTransaction_ShouldThrow()
        {
            var owner = AddDogAndOwner();
            var setter = owner.GetType().GetProperty(nameof(Owner.Name)).SetMethod;

            Assert.That(() =>
            {
                setter.Invoke(owner, new[] { "John" });
            }, Throws.InnerException.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void Setter_WhenInTransaction_ShouldSetValue()
        {
            var owner = AddDogAndOwner();
            var setter = owner.GetType().GetProperty(nameof(Owner.Name)).SetMethod;

            _realm.Write(() =>
            {
                setter.Invoke(owner, new[] { "John" });
            });

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void SetValue_WhenNotInTransaction_ShouldThrow()
        {
            var owner = AddDogAndOwner();
            var pi = owner.GetType().GetProperty(nameof(Owner.Name));

            Assert.That(() =>
            {
                pi.SetValue(owner, "John");
            }, Throws.InnerException.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void SetValue_WhenInTransaction_ShouldSetValue()
        {
            var owner = AddDogAndOwner();
            var pi = owner.GetType().GetProperty(nameof(Owner.Name));

            _realm.Write(() =>
            {
                pi.SetValue(owner, "John");
            });

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void ReflectableSetter_WhenObjectIsInvalid_ShouldThrow()
        {
            var owner = AddDogAndOwner();
            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var nameSetter = typeInfo.GetDeclaredProperty(nameof(Owner.Name)).SetMethod;

            Assert.That(() =>
            {
                _realm.Write(() =>
                {
                    nameSetter.Invoke(owner, new[] { "John" });
                });
            }, Throws.InnerException.TypeOf<RealmInvalidObjectException>());
        }

        [Test]
        public void ReflectableSetter_WhenObjectIsStandalone_ShouldSetValue()
        {
            var owner = AddDogAndOwner(add: false);

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var pi = typeInfo.GetDeclaredProperty(nameof(Owner.Name));

            pi.SetValue(owner, "John");

            Assert.That(owner.Name, Is.EqualTo("John"));
        }

        [Test]
        public void ReflectableGetter_WhenObjectIsStandalone_ShouldGetValue()
        {
            var owner = AddDogAndOwner(add: false);

            var typeInfo = ((IReflectableType)owner).GetTypeInfo();
            var pi = typeInfo.GetDeclaredProperty(nameof(Owner.Name));

            var name = pi.GetValue(owner);

            Assert.That(name, Is.EqualTo(OwnerName));
        }

        private Owner AddDogAndOwner(bool add = true)
        {
            var owner = new Owner
            {
                TopDog = new Dog
                {
                    Name = DogName
                },
                Name = OwnerName
            };

            if (add)
            {
                _realm.Write(() =>
                {
                    _realm.Add(owner);
                });
            }

            return owner;
        }
    }
}