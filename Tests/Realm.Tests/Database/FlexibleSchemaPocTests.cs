////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Realms.Helpers;

namespace Realms.Tests.Database;

[TestFixture, Preserve(AllMembers = true)]
public partial class FlexibleSchemaPocTests : RealmInstanceTest
{
    [Test]
    public void RealmValue_AsMappedType_ReturnsCorrectObject()
    {
        AddData();

        var dogContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Dog));

        var dog = dogContainer.MixedProperty.As<Dog>();

        Assert.That(dog, Is.TypeOf<Dog>());
        Assert.That(dog.Name, Is.EqualTo("Fido"));
        Assert.That(dog.BarkCount, Is.EqualTo(5));

        var dog2 = dogContainer.MixedProperty.AsMappedObject<Dog>();

        Assert.That(dog2, Is.TypeOf<Dog>());
        Assert.That(dog2.Name, Is.EqualTo("Fido"));
        Assert.That(dog2.BarkCount, Is.EqualTo(5));

        var nullContainer = _realm.Write(() => _realm.Add(new FlexibleSchemaPocContainer("null")
        {
            MixedProperty = RealmValue.Null
        }));

        var nullDog = nullContainer.MixedProperty.As<Dog?>();
        Assert.That(nullDog, Is.Null);

        var nullDog2 = nullContainer.MixedProperty.AsNullableMappedObject<Dog>();
        Assert.That(nullDog2, Is.Null);
    }

    [Test]
    public void RealmValue_AsMappedType_WhenTypeIsIncorrect_Throws()
    {
        var intContainer = _realm.Write(() => _realm.Add(new FlexibleSchemaPocContainer("int")
        {
            MixedProperty = 5
        }));

        Assert.Throws<InvalidCastException>(() => intContainer.MixedProperty.As<Dog>());
        Assert.Throws<InvalidCastException>(() => intContainer.MixedProperty.AsMappedObject<Dog>());
        Assert.Throws<InvalidCastException>(() => intContainer.MixedProperty.AsNullableMappedObject<Dog>());
    }

    [Test]
    public void AccessMappedTypeProperties_ReadsValuesFromBackingStorage()
    {
        AddData();
        var dogContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Dog));

        var dog = new Dog();
        dog.SetBackingStorage(dogContainer.MixedDict);

        Assert.That(dog.Name, Is.EqualTo("Fido"));
        Assert.That(dog.BarkCount, Is.EqualTo(5));

        var birdContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Bird));

        var bird = new Bird();
        bird.SetBackingStorage(birdContainer.MixedDict);

        Assert.That(bird.Name, Is.EqualTo("Tweety"));
        Assert.That(bird.CanFly, Is.True);
    }

    [Test]
    public void AccessMappedTypeProperties_WhenNonExistent_Throws()
    {
        var dog = new Dog();
        dog.SetBackingStorage(new Dictionary<string, RealmValue>());

        Assert.That(() => dog.Name, Throws.TypeOf<ArgumentException>().And.Message.Contains($"A property with name '{nameof(dog.Name)}' does not exist on '{nameof(Dog)}'."));
        Assert.That(() => dog.BarkCount, Throws.TypeOf<ArgumentException>().And.Message.Contains($"A property with name '{nameof(dog.BarkCount)}' does not exist on '{nameof(Dog)}'."));

        var bird = new Bird();
        bird.SetBackingStorage(new Dictionary<string, RealmValue>());

        Assert.That(() => bird.Name, Throws.TypeOf<ArgumentException>().And.Message.Contains($"A property with name '{nameof(bird.Name)}' does not exist on '{nameof(Bird)}'."));
        Assert.That(() => bird.CanFly, Throws.TypeOf<ArgumentException>().And.Message.Contains($"A property with name '{nameof(bird.CanFly)}' does not exist on '{nameof(Bird)}'."));
    }

    [Test]
    public void UpdateMappedTypeProperties_WritesValuesToBackingStorage()
    {
        AddData();
        var dogContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Dog));

        var dog = new Dog();
        dog.SetBackingStorage(dogContainer.MixedDict);

        _realm.Write(() =>
        {
            dog.Name = "Updated Fido";
            dog.BarkCount++;
        });
        Assert.That(dog.Name, Is.EqualTo("Updated Fido"));
        Assert.That(dog.BarkCount, Is.EqualTo(6));

        var birdContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Bird));

        var bird = new Bird();
        bird.SetBackingStorage(birdContainer.MixedDict);

        _realm.Write(() =>
        {
            bird.Name = "Updated Tweety";
            bird.CanFly = false;
        });
        Assert.That(bird.Name, Is.EqualTo("Updated Tweety"));
        Assert.That(bird.CanFly, Is.False);
    }

    [Test]
    public void NotifyPropertyChanged_NotifiesForModifications()
    {
        AddData();

        var dogContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Dog));

        var dog = dogContainer.MixedProperty.As<Dog>();
        var changes = new List<PropertyChangedEventArgs>();
        dog.PropertyChanged += (s, e) =>
        {
            Assert.That(s, Is.EqualTo(dog));
            changes.Add(e);
        };

        _realm.Write(() =>
        {
            dogContainer.MixedProperty.AsDictionary()[nameof(Dog.BarkCount)] = 10;
        });

        _realm.Refresh();

        Assert.That(changes.Count, Is.EqualTo(1));
        Assert.That(changes[0].PropertyName, Is.EqualTo(nameof(Dog.BarkCount)));

        _realm.Write(() =>
        {
            dogContainer.MixedProperty.AsDictionary()[nameof(Dog.BarkCount)] = 15;
            dogContainer.MixedProperty.AsDictionary()[nameof(Dog.Name)] = "Fido III";
        });
        _realm.Refresh();

        Assert.That(changes.Count, Is.EqualTo(3));
        Assert.That(changes[1].PropertyName, Is.EqualTo(nameof(Dog.BarkCount)));
        Assert.That(changes[2].PropertyName, Is.EqualTo(nameof(Dog.Name)));
    }

    private void AddData()
    {
        _realm.Write(() =>
        {
            var dogContainer = new FlexibleSchemaPocContainer(nameof(Dog))
            {
                MixedDict =
                {
                    [nameof(Dog.Name)] = "Fido", [nameof(Dog.BarkCount)] = 5
                },
                MixedProperty = new Dictionary<string, RealmValue>
                {
                    [nameof(Dog.Name)] = "Fido", [nameof(Dog.BarkCount)] = 5
                }
            };

            var birdContainer = new FlexibleSchemaPocContainer(nameof(Bird))
            {
                MixedDict =
                {
                    [nameof(Bird.Name)] = "Tweety", [nameof(Bird.CanFly)] = true
                },
                MixedProperty = new Dictionary<string, RealmValue>
                {
                    [nameof(Bird.Name)] = "Tweety", [nameof(Bird.CanFly)] = true
                }
            };

            _realm.Add(dogContainer);
            _realm.Add(birdContainer);
        });
    }

    public partial class FlexibleSchemaPocContainer : IRealmObject
    {
        public string ContainedObjectType { get; set; }

        public RealmValue MixedProperty { get; set; }

        public IDictionary<string, RealmValue> MixedDict { get; } = null!;

        public FlexibleSchemaPocContainer(string containedObjectType)
        {
            ContainedObjectType = containedObjectType;
        }
    }

    // User-defined
    public partial class Dog : IMappedObject
    {
        // public string Name { get; set; }

        // public int BarkCount { get; set; }
    }

    // Generated
    public partial class Dog : IMappedObject
    {
        public string Name
        {
            get => Get<string>(nameof(Name));
            set => Set(nameof(Name), value);
        }

        public int BarkCount
        {
            get => Get<int>(nameof(BarkCount));
            set => Set(nameof(BarkCount), value);
        }

        private T Get<T>(string propertyName)
        {
            Argument.Ensure(_backingStorage.TryGetValue(propertyName, out var value), $"A property with name '{propertyName}' does not exist on '{nameof(Dog)}'.", nameof(propertyName));

            return value.As<T>();
        }

        private void Set(string propertyName, RealmValue value)
        {
            _backingStorage[propertyName] = value;
        }
    }

    // User-defined
    public partial class Bird : IMappedObject
    {
        // public string Name { get; set; }

        // public bool CanFly { get; set; }
    }

    // Generated
    public partial class Bird : IMappedObject
    {
        public string Name
        {
            get => Get<string>(nameof(Name));
            set => Set(nameof(Name), value);
        }

        public bool CanFly
        {
            get => Get<bool>(nameof(CanFly));
            set => Set(nameof(CanFly), value);
        }

        private T Get<T>(string propertyName)
        {
            Argument.Ensure(_backingStorage.TryGetValue(propertyName, out var value), $"A property with name '{propertyName}' does not exist on '{nameof(Bird)}'.", nameof(propertyName));

            return value.As<T>();
        }

        private void Set(string propertyName, RealmValue value)
        {
            _backingStorage[propertyName] = value;
        }
    }
}
