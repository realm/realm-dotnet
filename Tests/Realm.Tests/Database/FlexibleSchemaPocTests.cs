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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database;

[TestFixture, Preserve(AllMembers = true)]
public partial class FlexibleSchemaPocTests : RealmInstanceTest
{
    [Test]
    public void ConvertDictionary_ToMappedType()
    {
        // TODO: NI to get this to work
        AddData();

        var dogContainer = _realm.All<FlexibleSchemaPocContainer>().First(c => c.ContainedObjectType == nameof(Dog));

        var dog = dogContainer.MixedProperty.As<Dog>();
        // var dogFromDict = dogContainer.MixedDict.As<Dog>();
    }

    [Test]
    public void AccessMappedTypeProperties_ReadsValuesFromBackingStorage()
    {
        // TODO: LJ to get this to work - this is an example test that should start passing once we wire up the properties.
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
        public string Name { get; set; }

        public int BarkCount { get; set; }
    }

    // Generated
    public partial class Dog
    {
        private IDictionary<string, RealmValue> _backingStorage = null!;

        public void SetBackingStorage(IDictionary<string, RealmValue> dictionary)
        {
            _backingStorage = dictionary;
        }
    }

    // User-defined
    public partial class Bird : IMappedObject
    {
        public string Name { get; set; }

        public bool CanFly { get; set; }
    }

    // Generated
    public partial class Bird
    {
        private IDictionary<string, RealmValue> _backingStorage = null!;

        public void SetBackingStorage(IDictionary<string, RealmValue> dictionary)
        {
            _backingStorage = dictionary;
        }
    }
}
