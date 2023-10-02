////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    internal class RealmValueWithCollections : RealmInstanceTest
    {
        private RealmValueObject PersistAndFind(RealmValue rv)
        {
            _realm.Write(() =>
            {
                _realm.Add(new RealmValueObject { RealmValueProperty = rv });
            });

            return _realm.All<RealmValueObject>().First();
        }

        #region List

        [Test]
        public void List_WhenRetrieved_WorksWithAllTypes([Values(true, false)] bool isManaged)
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var originalList = new List<RealmValue>
            {
                RealmValue.Null,
                1,
                true,
                "string",
                new byte[] { 0, 1, 2 },
                new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero),
                1f,
                2d,
                3m,
                new ObjectId("5f63e882536de46d71877979"),
                Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31"),
                new InternalObject { IntProperty = 10, StringProperty = "brown" },
                innerList,
                innerSet,
                innerDict,
            };

            RealmValue rv = originalList;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.List));
            Assert.That(rv != RealmValue.Null);

            Assert.That(rv.AsList(), Is.EqualTo(originalList));
            Assert.That(rv.AsAny(), Is.EqualTo(originalList));
            Assert.That(rv.As<IList<RealmValue>>(), Is.EqualTo(originalList));

            Assert.That(rv == originalList);
            Assert.That(rv.Equals(originalList));
        }

        [Test]
        public void List_WhenUsingIndexAccessWithCollections_WorkAsExpected([Values(true, false)] bool isManaged)
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var originalList = new List<RealmValue>
            {
                innerList,
                innerSet,
                innerDict,
            };

            RealmValue rv = originalList;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            var retrievedList = rv.AsList()[0];
            var retrivedSet = rv.AsList()[1];
            var retrievedDict = rv.AsList()[2];

            Assert.That(retrievedList.AsList(), Is.EqualTo(innerList));
            Assert.That(retrivedSet.AsSet(), Is.EquivalentTo(innerSet));
            Assert.That(retrievedDict.AsDictionary(), Is.EquivalentTo(innerDict));
        }

        [Test]
        public void List_CanBeCopiedFromManagedList([Values(true, false)] bool isManaged)
        {
            var originalList = new List<RealmValue>() { 1, "string", true };

            RealmValue rv = originalList;

            if (isManaged)
            {
                rv = PersistAndFind(originalList).RealmValueProperty;
            }

            var newObj = new RealmValueObject { RealmValueProperty = rv };

            RealmValue rv2;

            if (isManaged)
            {
                rv2 = PersistAndFind(rv).RealmValueProperty;
            }
            else
            {
                rv2 = newObj.RealmValueProperty;
            }

            Assert.That(rv.AsList(), Is.EqualTo(rv2.AsList()));
            Assert.That(rv.AsList(), Is.EqualTo(originalList));
            Assert.That(rv2.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_BuiltWithConstructorMethodOrOperatorOrCreate_WorksTheSame([Values(true, false)] bool isManaged)
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rvOperator = originalList;
            RealmValue rvConstructor = RealmValue.List(originalList);
            RealmValue rvCreate = RealmValue.Create(originalList, RealmValueType.List);

            if (isManaged)
            {
                rvOperator = PersistAndFind(rvOperator).RealmValueProperty;
                rvConstructor = PersistAndFind(rvConstructor).RealmValueProperty;
                rvCreate = PersistAndFind(rvCreate).RealmValueProperty;
            }

            Assert.That(rvOperator.AsList(), Is.EqualTo(originalList));
            Assert.That(rvConstructor.AsList(), Is.EqualTo(originalList));
            Assert.That(rvCreate.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WhenManaged_IsNotSameReferenceAsOriginalList()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rv = originalList;
            rv = PersistAndFind(rv).RealmValueProperty;
            var retrievedList = rv.AsList();

            Assert.That(ReferenceEquals(originalList, retrievedList), Is.False);
        }

        [Test]
        public void List_WhenUnmanaged_IsSameReferenceAsOriginalList()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rv = originalList;
            var retrievedList = rv.AsList();

            Assert.That(ReferenceEquals(originalList, retrievedList), Is.True);
        }

        [Test]
        public void List_AfterCreation_CanBeAssigned([Values(true, false)] bool isManaged)
        {
            var stringVal = "Mario";
            var rvo = new RealmValueObject { RealmValueProperty = stringVal };

            if (isManaged)
            {
                _realm.Write(() =>
                {
                    _realm.Add(rvo);
                });
            }

            Assert.That(rvo.RealmValueProperty == stringVal);

            var listVal = new List<RealmValue> { 1, "string", true };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = listVal;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            var newStringVal = "Luigi";

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = newStringVal;
            });

            Assert.That(rvo.RealmValueProperty == newStringVal);
        }

        [Test]
        public void List_WhenManaged_CanBeModified()
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = "Mario";
                listVal[1] = "Mario"; // To keep both list updated
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().RemoveAt(2);
                listVal.RemoveAt(2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add("newVal");
                listVal.Add("newVal");
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));
        }

        [Test]
        public void List_AddSetInsertMoveRemoveList_WorksAsIntended()
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            var innerList1 = new List<RealmValue> { "inner", 23, false };

            // Indexer
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = innerList1;
                listVal[1] = innerList1;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Insert
            var innerList2 = new List<RealmValue> { "inner2", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Insert(1, innerList2);
                listVal.Insert(1, innerList2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Add
            var innerList3 = new List<RealmValue> { "inner3", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add(innerList3);
                listVal.Add(innerList3);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Move
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Move(0, 1);
                listVal.Move(0, 1);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Remove
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().RemoveAt(2);
                listVal.RemoveAt(2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));
        }

        [Test]
        public void List_AddSetInsertMoveRemoveSet_WorksAsIntended()
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            var innerSet1 = new HashSet<RealmValue> { "inner", 23, false };

            // Indexer
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = innerSet1;
                listVal[1] = innerSet1;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Insert
            var innerSet2 = new HashSet<RealmValue> { "inner2", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Insert(1, innerSet2);
                listVal.Insert(1, innerSet2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Add
            var innerSet3 = new HashSet<RealmValue> { "inner3", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add(innerSet3);
                listVal.Add(innerSet3);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Move
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Move(0, 1);
                listVal.Move(0, 1);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Remove
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().RemoveAt(2);
                listVal.RemoveAt(2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));
        }

        [Test]
        public void List_AddSetInsertMoveRemoveDictionary_WorksAsIntended()
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            var innerDict1 = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            // Indexer
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = innerDict1;
                listVal[1] = innerDict1;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Insert
            var innerDict2 = new Dictionary<string, RealmValue>
            {
                { "s1", 2 },
                { "s2", "ah" },
                { "s3", false },
            };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Insert(1, innerDict2);
                listVal.Insert(1, innerDict2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Add
            var innerDict3 = new Dictionary<string, RealmValue>
            {
                { "s1", 3 },
                { "s2", "ahs" },
                { "s3", false },
            };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add(innerDict3);
                listVal.Add(innerDict3);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Move
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Move(0, 1);
                listVal.Move(0, 1);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Remove
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().RemoveAt(2);
                listVal.RemoveAt(2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));
        }

        public void List_RemoveWithCollectionArgument_ReturnsFalse()
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var listVal = new List<RealmValue> { innerList, innerSet, innerDict };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            _realm.Write(() =>
            {
                Assert.That(rvo.RealmValueProperty.AsList().Remove(innerList), Is.False);
                Assert.That(rvo.RealmValueProperty.AsList().Remove(innerSet), Is.False);
                Assert.That(rvo.RealmValueProperty.AsList().Remove(innerDict), Is.False);
            });
        }

        [Test]
        public void List_WhenManaged_WorksWithDynamic()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject());
            });

            _realm.Write(() =>
            {
                rvo.DynamicApi.Set(nameof(RealmValueObject.RealmValueProperty), originalList);
            });

            var rvp = rvo.DynamicApi.Get<RealmValue>(nameof(RealmValueObject.RealmValueProperty));

            Assert.That(rvp.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WhenManaged_WorksWithNotifications()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = originalList });
            });

            var callbacks = new List<ChangeSet>();
            using var token = rvo.RealmValueProperty.AsList().SubscribeForNotifications((collection, changes) =>
            {
                if (changes != null)
                {
                    callbacks.Add(changes);
                }
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[2] = "mario";
            });

            _realm.Refresh();

            Assert.That(callbacks.Count, Is.EqualTo(1));
        }

        #endregion

        #region Set

        [Test]
        public void Set_WhenRetrieved_WorksWithAllTypes([Values(true, false)] bool isManaged)
        {
            var originalSet = new HashSet<RealmValue>
            {
                RealmValue.Null,
                1,
                true,
                "string",
                new byte[] { 0, 1, 2 },
                new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero),
                1.5f,
                2d,
                3m,
                new ObjectId("5f63e882536de46d71877979"),
                Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31"),
                new InternalObject { IntProperty = 10, StringProperty = "brown" },
            };

            RealmValue rv = originalSet;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Set));
            Assert.That(rv != RealmValue.Null);

            Assert.That(rv.AsSet(), Is.EquivalentTo(originalSet));
            Assert.That(rv.AsAny(), Is.EquivalentTo(originalSet));
            Assert.That(rv.As<ISet<RealmValue>>(), Is.EquivalentTo(originalSet));

            Assert.That(rv == originalSet);
            Assert.That(rv.Equals(originalSet));
        }

        [Test]
        public void Set_CanBeCopiedFromManagedSet([Values(true, false)] bool isManaged)
        {
            var originalSet = new HashSet<RealmValue>() { 1, "string", true };

            RealmValue rv = originalSet;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            var newObj = new RealmValueObject { RealmValueProperty = rv };

            RealmValue rv2;

            if (isManaged)
            {
                rv2 = PersistAndFind(rv).RealmValueProperty;
            }
            else
            {
                rv2 = newObj.RealmValueProperty;
            }

            Assert.That(rv.AsSet(), Is.EquivalentTo(rv2.AsSet()));
            Assert.That(rv.AsSet(), Is.EquivalentTo(originalSet));
            Assert.That(rv2.AsSet(), Is.EquivalentTo(originalSet));
        }

        [Test]
        public void Set_WhenBeingAddedToRealmWithInternalCollections_Throws()
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var errorMessage = "Set cannot contain other collections";

            RealmValue rv = new HashSet<RealmValue>() { 1, "string", true, innerList };
            Assert.That(() => _realm.Write(() => _realm.Add(new RealmValueObject { RealmValueProperty = rv })),
                Throws.TypeOf<InvalidOperationException>().And.Message.Contains(errorMessage));

            rv = new HashSet<RealmValue>() { 1, "string", true, innerSet };
            Assert.That(() => _realm.Write(() => _realm.Add(new RealmValueObject { RealmValueProperty = rv })),
                Throws.TypeOf<InvalidOperationException>().And.Message.Contains(errorMessage));

            rv = new HashSet<RealmValue>() { 1, "string", true, innerDict };
            Assert.That(() => _realm.Write(() => _realm.Add(new RealmValueObject { RealmValueProperty = rv })),
                Throws.TypeOf<InvalidOperationException>().And.Message.Contains(errorMessage));
        }

        [Test]
        public void Set_WhenAddingCollections_Throws()
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var errorMessage = "Set cannot contain other collections";

            var originalSet = new HashSet<RealmValue>() { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = originalSet });
            });

            Assert.That(() => _realm.Write(() => rvo.RealmValueProperty.AsSet().Add(innerList)),
                Throws.TypeOf<InvalidOperationException>().And.Message.Contains(errorMessage));

            Assert.That(() => _realm.Write(() => rvo.RealmValueProperty.AsSet().Add(innerSet)),
                Throws.TypeOf<InvalidOperationException>().And.Message.Contains(errorMessage));

            Assert.That(() => _realm.Write(() => rvo.RealmValueProperty.AsSet().Add(innerDict)),
                Throws.TypeOf<InvalidOperationException>().And.Message.Contains(errorMessage));
        }

        [Test]
        public void Set_BuiltWithConstructorMethodOrOperatorOrCreate_WorksTheSame([Values(true, false)] bool isManaged)
        {
            var originalSet = new HashSet<RealmValue>() { 1, "string", true };

            RealmValue rvOperator = originalSet;
            RealmValue rvConstructor = RealmValue.Set(originalSet);
            RealmValue rvCreate = RealmValue.Create(originalSet, RealmValueType.Set);

            if (isManaged)
            {
                rvOperator = PersistAndFind(rvOperator).RealmValueProperty;
                rvConstructor = PersistAndFind(rvConstructor).RealmValueProperty;
                rvCreate = PersistAndFind(rvCreate).RealmValueProperty;
            }

            Assert.That(rvOperator.AsSet(), Is.EquivalentTo(originalSet));
            Assert.That(rvConstructor.AsSet(), Is.EquivalentTo(originalSet));
            Assert.That(rvCreate.AsSet(), Is.EquivalentTo(originalSet));
        }

        [Test]
        public void Set_WhenManaged_IsNotSameReferenceAsOriginalList()
        {
            var originalSet = new HashSet<RealmValue>() { 1, "string", true };

            RealmValue rv = originalSet;
            rv = PersistAndFind(rv).RealmValueProperty;
            var retrievedSet = rv.AsSet();

            Assert.That(ReferenceEquals(originalSet, retrievedSet), Is.False);
        }

        [Test]
        public void Set_WhenUnmanaged_IsSameReferenceAsOriginalList()
        {
            var originalSet = new HashSet<RealmValue>() { 1, "string", true };

            RealmValue rv = originalSet;
            var retrievedSet = rv.AsSet();

            Assert.That(ReferenceEquals(originalSet, retrievedSet), Is.True);
        }

        [Test]
        public void Set_AfterCreation_CanBeAssigned([Values(true, false)] bool isManaged)
        {
            var stringVal = "Mario";
            var rvo = new RealmValueObject { RealmValueProperty = stringVal };

            if (isManaged)
            {
                _realm.Write(() =>
                {
                    _realm.Add(rvo);
                });
            }

            Assert.That(rvo.RealmValueProperty == stringVal);

            var setVal = new HashSet<RealmValue>() { 1, "string", true };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = setVal;
            });

            Assert.That(rvo.RealmValueProperty.AsSet(), Is.EquivalentTo(setVal));

            var newStringVal = "Luigi";

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = newStringVal;
            });

            Assert.That(rvo.RealmValueProperty == newStringVal);
        }

        [Test]
        public void Set_WhenManaged_WorksWithDynamic()
        {
            var originalSet = new HashSet<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject());
            });

            _realm.Write(() =>
            {
                rvo.DynamicApi.Set(nameof(RealmValueObject.RealmValueProperty), originalSet);
            });

            var rvp = rvo.DynamicApi.Get<RealmValue>(nameof(RealmValueObject.RealmValueProperty));

            Assert.That(rvp.AsSet(), Is.EquivalentTo(originalSet));
        }

        [Test]
        public void Set_WhenManaged_WorksWithNotifications()
        {
            var originalSet = new HashSet<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = originalSet });
            });

            var callbacks = new List<ChangeSet>();
            using var token = rvo.RealmValueProperty.AsSet().SubscribeForNotifications((collection, changes) =>
            {
                if (changes != null)
                {
                    callbacks.Add(changes);
                }
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsSet().Add("mario");
            });

            _realm.Refresh();

            Assert.That(callbacks.Count, Is.EqualTo(1));
        }

        #endregion

        #region Dictionary

        [Test]
        public void Dictionary_WhenRetrieved_WorksWithAllTypes([Values(true, false)] bool isManaged)
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };

            var originalDict = new Dictionary<string, RealmValue>
            {
                { "1", RealmValue.Null },
                { "2", 1 },
                { "3", true },
                { "4", "string" },
                { "5", new byte[] { 0, 1, 2 } },
                { "6", new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero) },
                { "7", 1f },
                { "8", 2d },
                { "9", 3m },
                { "a", new ObjectId("5f63e882536de46d71877979") },
                { "b", Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31") },
                { "c", new InternalObject { IntProperty = 10, StringProperty = "brown" } },
                { "d", innerList },
                { "e", innerDict },
                { "f", innerSet },
            };

            RealmValue rv = originalDict;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Dictionary));
            Assert.That(rv != RealmValue.Null);

            Assert.That(rv.AsDictionary(), Is.EquivalentTo(originalDict));
            Assert.That(rv.AsAny(), Is.EquivalentTo(originalDict));
            Assert.That(rv.As<IDictionary<string, RealmValue>>(), Is.EquivalentTo(originalDict));

            Assert.That(rv == originalDict);
            Assert.That(rv.Equals(originalDict));
        }

        [Test]
        public void Dictionary_WhenUsingIndexAccessWithCollections_WorkAsExpected([Values(true, false)] bool isManaged)
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerSet = new HashSet<RealmValue> { 1, "str", true };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var originalDict = new Dictionary<string, RealmValue>
            {
                { "0", innerList },
                { "1", innerSet },
                { "2", innerDict },
            };

            RealmValue rv = originalDict;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            var retrievedList = rv.AsDictionary().ElementAt(0).Value;
            var retrivedSet = rv.AsDictionary().ElementAt(1).Value;
            var retrievedDict = rv.AsDictionary().ElementAt(2).Value;

            Assert.That(retrievedList.AsList(), Is.EqualTo(innerList));
            Assert.That(retrivedSet.AsSet(), Is.EquivalentTo(innerSet));
            Assert.That(retrievedDict.AsDictionary(), Is.EquivalentTo(innerDict));

            var retrievedList2 = rv.AsDictionary()["0"];
            var retrivedSet2 = rv.AsDictionary()["1"];
            var retrievedDict2 = rv.AsDictionary()["2"];

            Assert.That(retrievedList2.AsList(), Is.EqualTo(innerList));
            Assert.That(retrivedSet2.AsSet(), Is.EquivalentTo(innerSet));
            Assert.That(retrievedDict2.AsDictionary(), Is.EquivalentTo(innerDict));
        }

        [Test]
        public void Dictionary_CanBeCopiedFromManagedDictionary([Values(true, false)] bool isManaged)
        {
            var originalDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            RealmValue rv = originalDict;

            if (isManaged)
            {
                rv = PersistAndFind(originalDict).RealmValueProperty;
            }

            var newObj = new RealmValueObject { RealmValueProperty = rv };

            RealmValue rv2;

            if (isManaged)
            {
                rv2 = PersistAndFind(rv).RealmValueProperty;
            }
            else
            {
                rv2 = newObj.RealmValueProperty;
            }

            Assert.That(rv.AsDictionary(), Is.EqualTo(rv2.AsDictionary()));
            Assert.That(rv.AsDictionary(), Is.EqualTo(originalDict));
            Assert.That(rv2.AsDictionary(), Is.EqualTo(originalDict));
        }

        [Test]
        public void Dictionary_BuiltWithConstructorMethodOrOperatorOrCreate_WorksTheSame([Values(true, false)] bool isManaged)
        {
            var originalDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            RealmValue rvOperator = originalDict;
            RealmValue rvConstructor = RealmValue.Dictionary(originalDict);
            RealmValue rvCreate = RealmValue.Create(originalDict, RealmValueType.Dictionary);

            if (isManaged)
            {
                rvOperator = PersistAndFind(rvOperator).RealmValueProperty;
                rvConstructor = PersistAndFind(rvConstructor).RealmValueProperty;
                rvCreate = PersistAndFind(rvCreate).RealmValueProperty;
            }

            Assert.That(rvOperator.AsDictionary(), Is.EqualTo(originalDict));
            Assert.That(rvConstructor.AsDictionary(), Is.EqualTo(originalDict));
            Assert.That(rvCreate.AsDictionary(), Is.EqualTo(originalDict));
        }

        [Test]
        public void Dictionary_WhenManaged_IsNotSameReferenceAsOriginalList()
        {
            var originalDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            RealmValue rv = originalDict;
            rv = PersistAndFind(rv).RealmValueProperty;
            var retrievedDict = rv.AsDictionary();

            Assert.That(ReferenceEquals(originalDict, retrievedDict), Is.False);
        }

        [Test]
        public void Dictionary_WhenUnmanaged_IsSameReferenceAsOriginalList()
        {
            var originalDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            RealmValue rv = originalDict;
            var retrievedDict = rv.AsDictionary();

            Assert.That(ReferenceEquals(originalDict, retrievedDict), Is.True);
        }

        #endregion
    }
}
