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
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class CollectionTests : RealmInstanceTest
    {
        [Test]
        public void Insert_WhenIndexIsNegative_ShouldThrow()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            Assert.That(() =>
            {
                _realm.Write(() => container.Items.Insert(-1, new IntPropertyObject()));
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Insert_WhenIndexIsMoreThanCount_ShouldThrow()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            Assert.That(() =>
            {
                _realm.Write(() => container.Items.Insert(1, new IntPropertyObject()));
            }, Throws.TypeOf<ArgumentOutOfRangeException>());

            _realm.Write(() => container.Items.Add(new IntPropertyObject()));
            Assert.That(() =>
            {
                _realm.Write(() => container.Items.Insert(2, new IntPropertyObject()));
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Insert_WhenIndexIsEqualToCount_ShouldWork()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            var toInsert1 = new IntPropertyObject();
            _realm.Write(() => container.Items.Insert(0, toInsert1));
            Assert.That(container.Items.Count, Is.EqualTo(1));
            Assert.That(container.Items[0], Is.EqualTo(toInsert1));

            var toInsert2 = new IntPropertyObject();
            _realm.Write(() => container.Items.Insert(1, toInsert2));
            Assert.That(container.Items.Count, Is.EqualTo(2));
            Assert.That(container.Items[1], Is.EqualTo(toInsert2));
        }

        [TestCase(0, 3, "12304")]
        [TestCase(0, 4, "12340")]
        [TestCase(4, 0, "40123")]
        [TestCase(2, 4, "01342")]
        [TestCase(3, 1, "03124")]
        [TestCase(4, 4, "01234")]
        [TestCase(1, 1, "01234")]
        public void Move_WhenUnmanaged_ShouldMoveTheItem(int from, int to, string expected)
        {
            var list = new List<IntPropertyObject>();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new IntPropertyObject { Int = i });
            }

            var item = list[from];
            list.Move(item, to);
            Assert.That(string.Join(string.Empty, list.Select(i => i.Int)), Is.EqualTo(expected));
        }

        [TestCase(0, -1)]
        [TestCase(4, 5)]
        [TestCase(1, 5)]
        [TestCase(3, -3)]
        public void Move_WhenUnmanagedAndDestinationIndexIsInvalid_ShouldThrow(int from, int to)
        {
            var list = new List<IntPropertyObject>();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new IntPropertyObject { Int = i });
            }

            var item = list[from];
            Assert.That(() => list.Move(item, to), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(0, 3, "12304")]
        [TestCase(0, 4, "12340")]
        [TestCase(4, 0, "40123")]
        [TestCase(2, 4, "01342")]
        [TestCase(3, 1, "03124")]
        [TestCase(4, 4, "01234")]
        [TestCase(1, 1, "01234")]
        public void Move_WhenManaged_ShouldMoveTheItem(int from, int to, string expected)
        {
            var container = GetPopulatedManagedContainerObject();

            _realm.Write(() =>
            {
                var item = container.Items[from];
                container.Items.Move(item, to);
            });

            Assert.That(string.Join(string.Empty, container.Items.Select(i => i.Int)), Is.EqualTo(expected));
        }

        [TestCase(0, -1)]
        [TestCase(4, 5)]
        [TestCase(1, 5)]
        [TestCase(3, -3)]
        public void Move_WhenManagedAndDestinationIndexIsInvalid_ShouldThrow(int from, int to)
        {
            var container = GetPopulatedManagedContainerObject();

            _realm.Write(() =>
            {
                var item = container.Items[from];
                Assert.That(() => container.Items.Move(item, to), Throws.TypeOf<ArgumentOutOfRangeException>());
            });
        }

        [Test]
        public void IList_IsReadOnly_WhenRealmIsReadOnly_ShouldBeTrue()
        {
            var writeableContainer = GetPopulatedManagedContainerObject();
            Assert.That(writeableContainer.Items.IsReadOnly, Is.False);

            _realm.Dispose();

            var config = _configuration.ConfigWithPath(null);
            config.IsReadOnly = true;

            _realm = Realm.GetInstance(config);

            var readonlyContainer = _realm.All<ContainerObject>().Single();
            Assert.That(readonlyContainer.Items.IsReadOnly);
        }

        [Test]
        public void Results_GetAtIndex_WhenIndexIsOutOfRange_ShouldThrow()
        {
            Assert.That(() => _realm.All<IntPropertyObject>().AsRealmCollection()[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _realm.All<IntPropertyObject>().AsRealmCollection()[0], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void List_GetAtIndex_WhenIndexIsOutOfRange_ShouldThrow()
        {
            var owner = new Owner();
            _realm.Write(() => _realm.Add(owner));

            Assert.That(() => owner.Dogs.AsRealmCollection()[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => owner.Dogs.AsRealmCollection()[0], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void List_WhenRealmIsClosed_ShouldBeInvalid()
        {
            var container = GetPopulatedManagedContainerObject();

            Assert.That(container.Items.AsRealmCollection().IsValid);

            _realm.Dispose();

            Assert.That(container.Items.AsRealmCollection().IsValid, Is.False);
        }

        [Test]
        public void List_WhenParentIsDeleted_ShouldBeInvalid()
        {
            var container = GetPopulatedManagedContainerObject();

            Assert.That(container.Items.AsRealmCollection().IsValid);

            _realm.Write(() => _realm.Remove(container));

            Assert.That(container.Items.AsRealmCollection().IsValid, Is.False);
        }

        [Test]
        public void Results_WhenRealmIsClosed_ShouldBeInvalid()
        {
            var items = _realm.All<IntPropertyObject>();

            Assert.That(items.AsRealmCollection().IsValid);

            _realm.Dispose();

            Assert.That(items.AsRealmCollection().IsValid, Is.False);
        }

        private ContainerObject GetPopulatedManagedContainerObject()
        {
            var container = new ContainerObject();

            for (var i = 0; i < 5; i++)
            {
                container.Items.Add(new IntPropertyObject { Int = i });
            }

            _realm.Write(() => _realm.Add(container));

            return container;
        }
    }
}
