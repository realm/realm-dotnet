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
        public static Func<int, List<RealmValue>> ListGenerator = i => { return new List<RealmValue> { $"inner{i}", i }; };
        public static Func<int, Dictionary<string, RealmValue>> DictGenerator = i => { return new Dictionary<string, RealmValue> { { "s1", i }, { "s2", $"ah{i}" } }; };

        public static Func<int, RealmValue>[] CollectionGenerators = new Func<int, RealmValue>[]
        {
            i => (RealmValue)ListGenerator(i),
            i => (RealmValue)DictGenerator(i),
        };

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
            var innerList = ListGenerator(1);
            var innerDict = DictGenerator(1);

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
            Assert.That(rv.AsList().SequenceEqual(originalList));
        }

        [Test]
        public void List_WhenUsingIndexAccessWithCollections_WorkAsExpected([Values(true, false)] bool isManaged)
        {
            var innerList = ListGenerator(1);
            var innerDict = DictGenerator(1);

            var originalList = new List<RealmValue>
            {
                innerList,
                innerDict,
            };

            RealmValue rv = originalList;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            var retrievedList = rv.AsList()[0];
            var retrievedDict = rv.AsList()[1];

            Assert.That(retrievedList.AsList(), Is.EqualTo(innerList));
            Assert.That(retrievedDict.AsDictionary(), Is.EquivalentTo(innerDict));
        }

        [Test]
        public void List_CanBeCopiedFromManagedList([Values(true, false)] bool isManaged)
        {
            var originalList = ListGenerator(1);

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
        public void List_BuiltWithConstructorMethodOrOperatorOrCreateOrArray_WorksTheSame([Values(true, false)] bool isManaged)
        {
            var originalList = ListGenerator(1);

            RealmValue rvOperator = originalList;
            RealmValue rvConstructor = RealmValue.List(originalList);
            RealmValue rvCreate = RealmValue.Create(originalList, RealmValueType.List);
            RealmValue rvArray = originalList.ToArray();

            if (isManaged)
            {
                rvOperator = PersistAndFind(rvOperator).RealmValueProperty;
                rvConstructor = PersistAndFind(rvConstructor).RealmValueProperty;
                rvCreate = PersistAndFind(rvCreate).RealmValueProperty;
                rvArray = PersistAndFind(rvCreate).RealmValueProperty;
            }

            Assert.That(rvOperator.AsList(), Is.EqualTo(originalList));
            Assert.That(rvConstructor.AsList(), Is.EqualTo(originalList));
            Assert.That(rvCreate.AsList(), Is.EqualTo(originalList));
            Assert.That(rvArray.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WhenManaged_IsNotSameReferenceAsOriginalList()
        {
            var originalList = ListGenerator(1);

            RealmValue rv = originalList;
            rv = PersistAndFind(rv).RealmValueProperty;
            var retrievedList = rv.AsList();

            Assert.That(ReferenceEquals(originalList, retrievedList), Is.False);
        }

        [Test]
        public void List_WhenUnmanaged_IsSameReferenceAsOriginalList()
        {
            var originalList = ListGenerator(1);

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

            var listVal = ListGenerator(1);

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

        [TestCaseSource(nameof(CollectionGenerators))]
        public void List_AddSetInsertMoveRemoveCollection_WorksAsIntended(Func<int, RealmValue> collectionGenerator)
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            // Indexer
            var c1 = collectionGenerator(1);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = c1;
                listVal[1] = c1;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Insert
            var c2 = collectionGenerator(2);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Insert(1, c2);
                listVal.Insert(1, c2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            // Add
            var c3 = collectionGenerator(3);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add(c3);
                listVal.Add(c3);
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
        public void List_RemoveWithCollectionArgument_ReturnsFalse()
        {
            var innerList = new List<RealmValue> { "inner2", true, 2.0 };
            var innerDict = new Dictionary<string, RealmValue>
            {
                { "s1", 1 },
                { "s2", "ah" },
                { "s3", true },
            };

            var listVal = new List<RealmValue> { innerList, innerDict };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            _realm.Write(() =>
            {
                Assert.That(rvo.RealmValueProperty.AsList().Remove(innerList), Is.False);
                Assert.That(rvo.RealmValueProperty.AsList().Remove(innerDict), Is.False);
            });
        }

        [Test]
        public void List_WhenManaged_WorksWithDynamicLikeApi()
        {
            var originalList = ListGenerator(1);

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

#if !UNITY
        [Test]
#endif
        public void List_WhenManaged_WorksWithDynamic()
        {
            var originalList = ListGenerator(1);

            dynamic rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject());
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = originalList;
            });

            var rvp = rvo.RealmValueProperty;

            Assert.That(rvp.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WhenManaged_WorksWithNotifications()
        {
            var originalList = ListGenerator(1);

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
                rvo.RealmValueProperty.AsList()[1] = "mario";
            });

            _realm.Refresh();

            Assert.That(callbacks.Count, Is.EqualTo(1));
        }

        #endregion

        #region Dictionary

        [Test]
        public void Dictionary_WhenRetrieved_WorksWithAllTypes([Values(true, false)] bool isManaged)
        {
            var innerList = ListGenerator(1);
            var innerDict = DictGenerator(1);

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
            };

            RealmValue rv = originalDict;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Dictionary));
            Assert.That(rv != RealmValue.Null, "Different than null");

            Assert.That(rv.AsDictionary(), Is.EquivalentTo(originalDict));
            Assert.That(rv.AsAny(), Is.EquivalentTo(originalDict));
            Assert.That(rv.As<IDictionary<string, RealmValue>>(), Is.EquivalentTo(originalDict));

            Assert.That(rv == originalDict, "Equal to original dict");
            Assert.That(rv.Equals(originalDict), "Equal to original dict with Equals");
        }

        [Test]
        public void Dictionary_WhenUsingIndexAccessWithCollections_WorkAsExpected([Values(true, false)] bool isManaged)
        {
            var innerList = ListGenerator(1);
            var innerDict = DictGenerator(1);

            var originalDict = new Dictionary<string, RealmValue>
            {
                { "0", innerList },
                { "1", innerDict },
            };

            RealmValue rv = originalDict;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            var retrievedList = rv.AsDictionary().ElementAt(0).Value;
            var retrievedDict = rv.AsDictionary().ElementAt(1).Value;

            Assert.That(retrievedList.AsList(), Is.EqualTo(innerList));
            Assert.That(retrievedDict.AsDictionary(), Is.EquivalentTo(innerDict));

            var retrievedList2 = rv.AsDictionary()["0"];
            var retrievedDict2 = rv.AsDictionary()["1"];

            Assert.That(retrievedList2.AsList(), Is.EqualTo(innerList));
            Assert.That(retrievedDict2.AsDictionary(), Is.EquivalentTo(innerDict));
        }

        [Test]
        public void Dictionary_CanBeCopiedFromManagedDictionary([Values(true, false)] bool isManaged)
        {
            var originalDict = DictGenerator(1);

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
            var originalDict = DictGenerator(1);

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
            var originalDict = DictGenerator(1);

            RealmValue rv = originalDict;
            rv = PersistAndFind(rv).RealmValueProperty;
            var retrievedDict = rv.AsDictionary();

            Assert.That(ReferenceEquals(originalDict, retrievedDict), Is.False);
        }

        [Test]
        public void Dictionary_WhenUnmanaged_IsSameReferenceAsOriginalList()
        {
            var originalDict = DictGenerator(1);

            RealmValue rv = originalDict;
            var retrievedDict = rv.AsDictionary();

            Assert.That(ReferenceEquals(originalDict, retrievedDict), Is.True);
        }

        [Test]
        public void Dictionary_AfterCreation_CanBeAssigned([Values(true, false)] bool isManaged)
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

            var dictVal = DictGenerator(1);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = dictVal;
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));

            var newStringVal = "Luigi";

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = newStringVal;
            });

            Assert.That(rvo.RealmValueProperty == newStringVal);
        }

        [Test]
        public void Dictionary_WhenManaged_CanBeModified()
        {
            var dictVal = DictGenerator(1);

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = dictVal });
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary()["s1"] = "Mario";
                dictVal["s1"] = "Mario"; // To keep both list updated
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary().Remove("s2");
                dictVal.Remove("s2");
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary().Add("s4", "newVal");
                dictVal.Add("s4", "newVal");
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));
        }

        [TestCaseSource(nameof(CollectionGenerators))]
        public void Dictionary_AddSetInsertMoveRemoveCollection_WorksAsIntended(Func<int, RealmValue> collectionGenerator)
        {
            var dictVal = DictGenerator(1);

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = dictVal });
            });

            // Indexer
            var c1 = collectionGenerator(1);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary()["s1"] = c1;
                dictVal["s1"] = c1;
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));

            // Add
            var c3 = collectionGenerator(3);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary().Add("s4", c3);
                dictVal.Add("s4", c3);
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));

            // Remove
            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary().Remove("s4");
                dictVal.Remove("s4");
            });

            Assert.That(rvo.RealmValueProperty.AsDictionary(), Is.EquivalentTo(dictVal));
        }

        [Test]
        public void Dictionary_RemoveWithCollectionArgument_ReturnsFalse()
        {
            var innerList = ListGenerator(1);
            var innerDict = DictGenerator(1);

            var dictVal = new Dictionary<string, RealmValue>
            {
                { "s1", innerList },
                { "s3", innerDict },
            };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = dictVal });
            });

            _realm.Write(() =>
            {
                Assert.That(rvo.RealmValueProperty.AsDictionary().Remove(new KeyValuePair<string, RealmValue>("s1", innerList)), Is.False);
                Assert.That(rvo.RealmValueProperty.AsDictionary().Remove(new KeyValuePair<string, RealmValue>("s3", innerDict)), Is.False);
            });
        }

        [Test]
        public void Dictionary_WhenManaged_WorksWithDynamicLikeApi()
        {
            var dictVal = DictGenerator(1);

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject());
            });

            _realm.Write(() =>
            {
                rvo.DynamicApi.Set(nameof(RealmValueObject.RealmValueProperty), dictVal);
            });

            var rvp = rvo.DynamicApi.Get<RealmValue>(nameof(RealmValueObject.RealmValueProperty));

            Assert.That(rvp.AsDictionary(), Is.EqualTo(dictVal));
        }

#if !UNITY
        [Test]
#endif
        public void Dictionary_WhenManaged_WorksWithDynamic()
        {
            var dictVal = DictGenerator(1);

            dynamic rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject());
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = dictVal;
            });

            var rvp = rvo.RealmValueProperty;

            Assert.That(rvp.AsDictionary(), Is.EqualTo(dictVal));
        }

        [Test]
        public void Dictionary_WhenManaged_WorksWithNotifications()
        {
            var dictVal = DictGenerator(1);

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = dictVal });
            });

            var callbacks = new List<ChangeSet>();
            using var token = rvo.RealmValueProperty.AsDictionary().SubscribeForNotifications((collection, changes) =>
            {
                if (changes != null)
                {
                    callbacks.Add(changes);
                }
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsDictionary()["s2"] = "mario";
            });

            _realm.Refresh();

            Assert.That(callbacks.Count, Is.EqualTo(1));
        }

        #endregion
    }
}
