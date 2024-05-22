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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class EmbeddedObjectsTests : RealmInstanceTest
    {
        [Test]
        public void EmbeddedObject_WhenUnmanaged_CanGetAndSetProperties()
        {
            var obj = new ObjectWithEmbeddedProperties
            {
                AllTypesObject = CreateEmbeddedAllTypesObject()
            };

            obj.ListOfAllTypesObjects.Add(new EmbeddedAllTypesObject());
            obj.RecursiveObject = new EmbeddedLevel1
            {
                String = "first",
                Child = new EmbeddedLevel2
                {
                    String = "second",
                    Child = new EmbeddedLevel3
                    {
                        String = "third"
                    }
                },
                Children =
                {
                    new EmbeddedLevel2
                    {
                        String = "I'm in a list",
                        Child = new EmbeddedLevel3
                        {
                            String = "child in a list"
                        },
                        Children =
                        {
                            new EmbeddedLevel3
                            {
                                String = "children in a list"
                            }
                        }
                    }
                }
            };
            obj.DictionaryOfAllTypesObjects.Add("foo", new EmbeddedAllTypesObject());

            Assert.That(obj.AllTypesObject.NullableSingleProperty, Is.EqualTo(1.4f));
            Assert.That(obj.ListOfAllTypesObjects.Count, Is.EqualTo(1));
            Assert.That(obj.DictionaryOfAllTypesObjects.Count, Is.EqualTo(1));
            Assert.That(obj.DictionaryOfAllTypesObjects.ContainsKey("foo"));
            Assert.That(obj.RecursiveObject.String, Is.EqualTo("first"));
            Assert.That(obj.RecursiveObject.Child.String, Is.EqualTo("second"));
            Assert.That(obj.RecursiveObject.Child.Child.String, Is.EqualTo("third"));
            Assert.That(obj.RecursiveObject.Children[0].Child!.String, Is.EqualTo("child in a list"));
            Assert.That(obj.RecursiveObject.Children[0].Children[0].String, Is.EqualTo("children in a list"));
        }

        [Test]
        public void EmbeddedParent_CanBeAddedToRealm()
        {
            var embedded = CreateEmbeddedAllTypesObject();
            var parent = new ObjectWithEmbeddedProperties
            {
                AllTypesObject = embedded
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.IsManaged);
            Assert.That(parent.AllTypesObject.IsManaged);
            Assert.That(embedded.IsManaged);

            var copy = CreateEmbeddedAllTypesObject();

#if TEST_WEAVER
            var properties = typeof(EmbeddedAllTypesObject).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => !p.HasCustomAttribute<BacklinkAttribute>());
#else
            var properties = typeof(EmbeddedAllTypesObject).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => !p.HasCustomAttribute<BacklinkAttribute>())
                .Intersect(typeof(EmbeddedAllTypesObject.IEmbeddedAllTypesObjectAccessor)
                .GetProperties());
#endif

            foreach (var prop in properties)
            {
                Assert.That(prop.GetValue(parent.AllTypesObject), Is.EqualTo(prop.GetValue(copy)), $"Expected {prop.Name} to have value {prop.GetValue(copy)} but was {prop.GetValue(parent.AllTypesObject)}");
            }
        }

        [Test]
        public void EmbeddedParent_CanBeAddedWhenPropertyIsNull()
        {
            var parent = new ObjectWithEmbeddedProperties();
            Assert.That(parent.AllTypesObject, Is.Null);

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.IsManaged);
            Assert.That(parent.AllTypesObject, Is.Null);
            Assert.That(parent.ListOfAllTypesObjects, Is.Empty);
        }

        [Test]
        public void EmbeddedParent_CanOverwriteEmbeddedProperty()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                AllTypesObject = new EmbeddedAllTypesObject
                {
                    DoubleProperty = 123.456
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var firstEmbedded = parent.AllTypesObject;
            Assert.True(firstEmbedded.IsManaged);
            Assert.True(firstEmbedded.IsValid);

            _realm.Write(() =>
            {
                parent.AllTypesObject = new EmbeddedAllTypesObject
                {
                    DoubleProperty = -987.654
                };
            });

            Assert.That(parent.AllTypesObject.DoubleProperty, Is.EqualTo(-987.654));

            Assert.True(firstEmbedded.IsManaged);
            Assert.False(firstEmbedded.IsValid);
        }

        [Test]
        public void EmbeddedParent_WithList_CanBeAddedToRealm()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                ListOfAllTypesObjects =
                {
                    new EmbeddedAllTypesObject
                    {
                        Int32Property = 1
                    },
                    new EmbeddedAllTypesObject
                    {
                        Int32Property = 2
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.IsManaged);
            Assert.That(parent.ListOfAllTypesObjects.AsRealmCollection().IsValid);
            Assert.That(parent.ListOfAllTypesObjects.Count, Is.EqualTo(2));
            Assert.That(parent.ListOfAllTypesObjects[0].Int32Property, Is.EqualTo(1));
            Assert.That(parent.ListOfAllTypesObjects[1].Int32Property, Is.EqualTo(2));
        }

        [Test]
        public void EmbeddedParent_WithDictionary_CanBeAddedToRealm()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                DictionaryOfAllTypesObjects =
                {
                    ["first"] = new EmbeddedAllTypesObject
                    {
                        Int32Property = 1
                    },
                    ["second"] = new EmbeddedAllTypesObject
                    {
                        Int32Property = 2
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.IsManaged);
            Assert.That(parent.DictionaryOfAllTypesObjects.AsRealmCollection().IsValid);
            Assert.That(parent.DictionaryOfAllTypesObjects.Count, Is.EqualTo(2));
            Assert.That(parent.DictionaryOfAllTypesObjects.ContainsKey("first"));
            Assert.That(parent.DictionaryOfAllTypesObjects.ContainsKey("second"));
            Assert.That(parent.DictionaryOfAllTypesObjects["first"]!.Int32Property, Is.EqualTo(1));
            Assert.That(parent.DictionaryOfAllTypesObjects["second"]!.Int32Property, Is.EqualTo(2));
        }

        [Test]
        public void ListOfEmbeddedObjects_CanAddItems()
        {
            var parent = new ObjectWithEmbeddedProperties();
            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var list = parent.ListOfAllTypesObjects;
            _realm.Write(() =>
            {
                parent.ListOfAllTypesObjects.Add(new EmbeddedAllTypesObject
                {
                    DecimalProperty = 123.456M
                });
            });

            Assert.That(parent.ListOfAllTypesObjects, Is.SameAs(list));
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].DecimalProperty, Is.EqualTo(123.456M));
        }

        [Test]
        public void DictionaryOfEmbeddedObjects_CanAddItems()
        {
            var parent = new ObjectWithEmbeddedProperties();
            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var dict = parent.DictionaryOfAllTypesObjects;
            _realm.Write(() =>
            {
                parent.DictionaryOfAllTypesObjects.Add("first", new EmbeddedAllTypesObject
                {
                    DecimalProperty = 123.456M
                });
            });

            Assert.That(parent.DictionaryOfAllTypesObjects, Is.SameAs(dict));
            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict.ContainsKey("first"));
            Assert.That(dict["first"]!.DecimalProperty, Is.EqualTo(123.456M));
        }

        [Test]
        public void ListOfEmbeddedObjects_CanInsertItems()
        {
            var parent = new ObjectWithEmbeddedProperties();
            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var list = parent.ListOfAllTypesObjects;
            _realm.Write(() =>
            {
                parent.ListOfAllTypesObjects.Insert(0, new EmbeddedAllTypesObject
                {
                    DecimalProperty = 123.456M
                });
            });

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].DecimalProperty, Is.EqualTo(123.456M));

            _realm.Write(() =>
            {
                list.Insert(0, new EmbeddedAllTypesObject
                {
                    DecimalProperty = 456.789M
                });
            });

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0].DecimalProperty, Is.EqualTo(456.789M));
            Assert.That(list[1].DecimalProperty, Is.EqualTo(123.456M));

            _realm.Write(() =>
            {
                list.Insert(1, new EmbeddedAllTypesObject
                {
                    DecimalProperty = 0
                });
            });

            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0].DecimalProperty, Is.EqualTo(456.789M));
            Assert.That(list[1].DecimalProperty, Is.EqualTo(0));
            Assert.That(list[2].DecimalProperty, Is.EqualTo(123.456M));
        }

        [Test]
        public void ListOfEmbeddedObjects_CanSetItems()
        {
            var parent = new ObjectWithEmbeddedProperties();
            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var list = parent.ListOfAllTypesObjects;
            _realm.Write(() =>
            {
                parent.ListOfAllTypesObjects.Add(new EmbeddedAllTypesObject
                {
                    StringProperty = "first"
                });
            });

            Assert.That(list.Count, Is.EqualTo(1));

            var firstItem = list[0];
            Assert.That(firstItem.StringProperty, Is.EqualTo("first"));

            _realm.Write(() =>
            {
                list[0] = new EmbeddedAllTypesObject
                {
                    StringProperty = "updated"
                };
            });

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(firstItem.IsValid, Is.False);
            Assert.That(list[0].StringProperty, Is.EqualTo("updated"));
        }

        [Test]
        public void DictionaryOfEmbeddedObjects_CanSetItems()
        {
            var parent = new ObjectWithEmbeddedProperties();
            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var dict = parent.DictionaryOfAllTypesObjects;
            _realm.Write(() =>
            {
                parent.DictionaryOfAllTypesObjects.Add("a", new EmbeddedAllTypesObject
                {
                    StringProperty = "first"
                });
            });

            Assert.That(dict.Count, Is.EqualTo(1));

            var firstItem = dict["a"]!;
            Assert.That(firstItem.StringProperty, Is.EqualTo("first"));

            _realm.Write(() =>
            {
                dict["a"] = new EmbeddedAllTypesObject
                {
                    StringProperty = "updated"
                };
            });

            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(firstItem.IsValid, Is.False);
            Assert.That(dict["a"]!.StringProperty, Is.EqualTo("updated"));
        }

        [Test]
        public void ListOfEmbeddedObjects_WhenItemIsRemoved_GetsDeleted()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                ListOfAllTypesObjects = { new EmbeddedAllTypesObject() }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.ListOfAllTypesObjects.Count, Is.EqualTo(1));

            var firstItem = parent.ListOfAllTypesObjects[0];

            _realm.Write(() =>
            {
                parent.ListOfAllTypesObjects.Remove(firstItem);
            });

            Assert.That(parent.ListOfAllTypesObjects.Count, Is.EqualTo(0));
            Assert.That(firstItem.IsValid, Is.False);

            _realm.Write(() =>
            {
                parent.ListOfAllTypesObjects.Add(new EmbeddedAllTypesObject());
            });

            Assert.That(parent.ListOfAllTypesObjects.Count, Is.EqualTo(1));

            var secondItem = parent.ListOfAllTypesObjects[0];

            _realm.Write(() =>
            {
                parent.ListOfAllTypesObjects.RemoveAt(0);
            });

            Assert.That(parent.ListOfAllTypesObjects.Count, Is.EqualTo(0));
            Assert.That(secondItem.IsValid, Is.False);
        }

        [Test]
        public void DictionaryOfEmbeddedObjects_WhenItemIsRemoved_GetsDeleted()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                DictionaryOfAllTypesObjects = { ["abc"] = new EmbeddedAllTypesObject() }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.DictionaryOfAllTypesObjects.Count, Is.EqualTo(1));

            var firstItem = parent.DictionaryOfAllTypesObjects["abc"];

            _realm.Write(() =>
            {
                parent.DictionaryOfAllTypesObjects.Remove("abc");
            });

            Assert.That(parent.DictionaryOfAllTypesObjects.Count, Is.EqualTo(0));
            Assert.That(firstItem!.IsValid, Is.False);

            _realm.Write(() =>
            {
                parent.DictionaryOfAllTypesObjects.Add("boo", new EmbeddedAllTypesObject());
            });

            Assert.That(parent.DictionaryOfAllTypesObjects.Count, Is.EqualTo(1));

            var secondItem = parent.DictionaryOfAllTypesObjects["boo"];

            _realm.Write(() =>
            {
                parent.DictionaryOfAllTypesObjects.Remove(new KeyValuePair<string, EmbeddedAllTypesObject?>("boo", secondItem));
            });

            Assert.That(parent.DictionaryOfAllTypesObjects.Count, Is.EqualTo(0));
            Assert.That(secondItem!.IsValid, Is.False);
        }

        [Test]
        public void Embedded_AddingALinkToManaged_Fails()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1 { String = "a" }
            };

            var parent2 = new ObjectWithEmbeddedProperties();

            _realm.Write(() =>
            {
                _realm.Add(parent);
                _realm.Add(parent2);
            });

            var ex = Assert.Throws<RealmException>(() =>
            {
                _realm.Write(() =>
                {
                    parent2.RecursiveObject = parent.RecursiveObject;
                });
            })!;
            Assert.That(ex.Message, Does.Contain("Can't link to an embedded object that is already managed."));
        }

        [Test]
        public void RecursiveEmbeddedList_AddingALinkToItself_Fails()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    String = "a",
                    Child = new EmbeddedLevel2()
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var ex = Assert.Throws<RealmException>(() =>
            {
                _realm.Write(() =>
                {
                    parent.RecursiveObject.Children.Add(parent.RecursiveObject.Child);
                });
            })!;
            Assert.That(ex.Message, Is.EqualTo("Can't add to the collection an embedded object that is already managed."));
        }

        [Test]
        public void RecursiveEmbedded_WhenDifferentReferences_Succeeds()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    String = "a",
                    Child = new EmbeddedLevel2
                    {
                        String = "b"
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.RecursiveObject.IsManaged);
            Assert.That(parent.RecursiveObject.String, Is.EqualTo("a"));
            Assert.That(parent.RecursiveObject.Child.IsManaged);
            Assert.That(parent.RecursiveObject.Child.String, Is.EqualTo("b"));
            Assert.That(parent.RecursiveObject.Child.Child, Is.Null);

            _realm.Write(() =>
            {
                parent.RecursiveObject.Child.Child = new EmbeddedLevel3
                {
                    String = "c"
                };
            });

            Assert.That(parent.RecursiveObject.Child.Child!.IsManaged);
            Assert.That(parent.RecursiveObject.Child.Child.String, Is.EqualTo("c"));
        }

        [Test]
        public void EmbeddedParent_AddOrUpdate_DeletesOldChild()
        {
            var primaryKey = ObjectId.GenerateNewId();

            var parent = new ObjectWithEmbeddedProperties
            {
                PrimaryKey = primaryKey,
                AllTypesObject = new EmbeddedAllTypesObject { StringProperty = "A" }
            };

            var firstChild = parent.AllTypesObject;

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(firstChild.IsValid);
            Assert.That(firstChild.IsManaged);

            var updatingParent = new ObjectWithEmbeddedProperties
            {
                PrimaryKey = primaryKey,
                AllTypesObject = new EmbeddedAllTypesObject { StringProperty = "B" }
            };

            _realm.Write(() =>
            {
                _realm.Add(updatingParent, update: true);
            });

            Assert.That(firstChild.IsValid, Is.False);
            Assert.That(firstChild.IsManaged);

            Assert.That(updatingParent.AllTypesObject.StringProperty, Is.EqualTo("B"));
            Assert.That(parent.AllTypesObject.StringProperty, Is.EqualTo("B"));
        }

        [Test]
        public void EmbeddedObject_WhenDeleted_IsRemovedFromParent()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                AllTypesObject = new EmbeddedAllTypesObject(),
                ListOfAllTypesObjects =
                {
                    new EmbeddedAllTypesObject(),
                    new EmbeddedAllTypesObject { StringProperty = "this will survive" }
                }
            };

            _realm.Write(() =>
                    {
                        _realm.Add(parent);
                    });

            var directLink = parent.AllTypesObject;
            var listLink = parent.ListOfAllTypesObjects[0];

            _realm.Write(() =>
            {
                _realm.Remove(directLink);
                _realm.Remove(listLink);
            });

            Assert.That(parent.AllTypesObject, Is.Null);
            Assert.That(parent.ListOfAllTypesObjects.Count, Is.EqualTo(1));
            Assert.That(parent.ListOfAllTypesObjects[0].StringProperty, Is.EqualTo("this will survive"));
        }

        [Test]
        public void EmbeddedParent_AddOrUpdate_DeletesOldChildren()
        {
            var primaryKey = ObjectId.GenerateNewId();

            var parent = new ObjectWithEmbeddedProperties
            {
                PrimaryKey = primaryKey,
                ListOfAllTypesObjects =
                {
                    new EmbeddedAllTypesObject { StringProperty = "A" }
                }
            };

            var firstChild = parent.ListOfAllTypesObjects[0];

            _realm.Write(() =>
            {
                _realm.Add(parent, update: true);
            });

            Assert.That(firstChild.IsValid);
            Assert.That(firstChild.IsManaged);

            var updatingParent = new ObjectWithEmbeddedProperties
            {
                PrimaryKey = primaryKey,
                ListOfAllTypesObjects =
                {
                    new EmbeddedAllTypesObject { StringProperty = "B" }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(updatingParent, update: true);
            });

            Assert.That(firstChild.IsValid, Is.False);
            Assert.That(firstChild.IsManaged);

            Assert.That(parent.ListOfAllTypesObjects.Single().StringProperty, Is.EqualTo("B"));
            Assert.That(updatingParent.ListOfAllTypesObjects.Single().StringProperty, Is.EqualTo("B"));
        }

        [Test]
        public void EmbeddedObject_WhenDeleted_DeletesDependentTree()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    Child = new EmbeddedLevel2
                    {
                        Children =
                        {
                            new EmbeddedLevel3()
                        }
                    },
                    Children =
                    {
                        new EmbeddedLevel2
                        {
                            Child = new EmbeddedLevel3()
                        }
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var previousEmbedded1 = _realm.AllEmbedded<EmbeddedLevel1>().ToArray();
            Assert.That(previousEmbedded1.Length, Is.EqualTo(1));
            var previousEmbedded2 = _realm.AllEmbedded<EmbeddedLevel2>().ToArray();
            Assert.That(previousEmbedded2.Length, Is.EqualTo(2));
            var previousEmbedded3 = _realm.AllEmbedded<EmbeddedLevel3>().ToArray();
            Assert.That(previousEmbedded3.Length, Is.EqualTo(2));

            _realm.Write(() =>
            {
                parent.RecursiveObject = new EmbeddedLevel1();
            });

            foreach (var previous in previousEmbedded3)
            {
                Assert.That(previous.IsValid, Is.False);
            }

            Assert.That(_realm.AllEmbedded<EmbeddedLevel1>().Count(), Is.EqualTo(1));
            Assert.That(_realm.AllEmbedded<EmbeddedLevel2>().Count(), Is.EqualTo(0));
            Assert.That(_realm.AllEmbedded<EmbeddedLevel3>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void EmbeddedObject_WhenParentAccessed_ReturnsParent()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    Child = new EmbeddedLevel2
                    {
                        Child = new EmbeddedLevel3()
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent, Is.EqualTo(parent.RecursiveObject.Parent));

            var firstChild = parent.RecursiveObject;
            Assert.That(firstChild, Is.EqualTo(firstChild.Child.Parent));

            var secondChild = firstChild.Child;
            Assert.That(secondChild, Is.EqualTo(secondChild.Child.Parent));
        }

        [Test]
        public void EmbeddedObject_WhenParentAccessedInList_ReturnsParent()
        {
            var parent = new ObjectWithEmbeddedProperties();
            parent.ListOfAllTypesObjects.Add(new EmbeddedAllTypesObject());

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent, Is.EqualTo(parent.ListOfAllTypesObjects.Single().Parent));
        }

        [Test]
        public void EmbeddedObject_WhenParentAccessedInDictionary_ReturnsParent()
        {
            var parent = new ObjectWithEmbeddedProperties();
            parent.DictionaryOfAllTypesObjects.Add("child", new EmbeddedAllTypesObject());

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent, Is.EqualTo(parent.DictionaryOfAllTypesObjects["child"]!.Parent));
        }

        [Test]
        public void EmbeddedObjectUnmanaged_WhenParentAccessed_ReturnsNull()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    Child = new EmbeddedLevel2
                    {
                        Child = new EmbeddedLevel3()
                    }
                }
            };

            Assert.That(parent.RecursiveObject.Parent, Is.Null);

            var firstChild = parent.RecursiveObject;
            Assert.That(firstChild.Child.Parent, Is.Null);

            var secondChild = firstChild.Child;
            Assert.That(secondChild.Child.Parent, Is.Null);
        }

        [Test]
        public void NonEmbeddedObject_WhenParentAccessed_Throws()
        {
            var topLevel = new IntPropertyObject
            {
                Int = 1
            };

            _realm.Write(() =>
            {
                _realm.Add(topLevel);
            });

            // Objects not implementing IEmbeddedObject will not have the "Parent" field,
            // but the "GetParent" method is still accessible on its accessor. It should
            // throw as it should not be used for such objects.
            Assert.Throws<InvalidOperationException>(() => ((IRealmObjectBase)topLevel).Accessor.GetParent());
        }

        [Test]
        public void StaticBacklinks()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                AllTypesObject = CreateEmbeddedAllTypesObject()
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            Assert.That(parent.AllTypesObject.ContainersObjects.Count(), Is.EqualTo(1));
            Assert.That(parent.AllTypesObject.ContainersObjects.Single(), Is.EqualTo(parent));
        }

        [Test]
        public void DynamicBacklinks()
        {
            TestHelpers.IgnoreOnUnity();

            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    String = "level 1",
                    Child = new EmbeddedLevel2
                    {
                        String = "level 2"
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var topLevelBacklinks = parent.RecursiveObject.DynamicApi.GetBacklinksFromType(nameof(ObjectWithEmbeddedProperties), nameof(ObjectWithEmbeddedProperties.RecursiveObject));
            Assert.That(topLevelBacklinks.Count(), Is.EqualTo(1));
            Assert.That(topLevelBacklinks.Single(), Is.EqualTo(parent));

            var secondLevelBacklinks = parent.RecursiveObject.Child.DynamicApi.GetBacklinksFromType(nameof(EmbeddedLevel1), nameof(EmbeddedLevel1.Child));
            Assert.That(secondLevelBacklinks.Count(), Is.EqualTo(1));
            Assert.That(secondLevelBacklinks.Single(), Is.EqualTo(parent.RecursiveObject));

            // This should be empty because no objects link to it via .Children
            var secondLevelChildrenBacklinks = parent.RecursiveObject.Child.DynamicApi.GetBacklinksFromType(nameof(EmbeddedLevel1), nameof(EmbeddedLevel1.Children));
            Assert.That(secondLevelChildrenBacklinks.Count(), Is.EqualTo(0));
        }

        [Test]
        public void DynamicBacklinks_NewAPI()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    String = "level 1",
                    Child = new EmbeddedLevel2
                    {
                        String = "level 2"
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var topLevelBacklinks = parent.RecursiveObject.DynamicApi.GetBacklinksFromType(nameof(ObjectWithEmbeddedProperties), nameof(ObjectWithEmbeddedProperties.RecursiveObject));
            Assert.That(topLevelBacklinks.Count(), Is.EqualTo(1));

            var parentViaBacklinks = topLevelBacklinks.Single();
            Assert.That(parentViaBacklinks, Is.EqualTo(parent));

            var recursiveObjViaBacklinks = parentViaBacklinks.DynamicApi.Get<IRealmObjectBase>(nameof(ObjectWithEmbeddedProperties.RecursiveObject));
            Assert.That(recursiveObjViaBacklinks, Is.EqualTo(parent.RecursiveObject));
            Assert.That(recursiveObjViaBacklinks.DynamicApi.Get<string>(nameof(EmbeddedLevel1.String)), Is.EqualTo("level 1"));

#if !UNITY
            dynamic dynamicParentViaBacklinks = topLevelBacklinks.Single();
            Assert.That(dynamicParentViaBacklinks, Is.EqualTo(parent));

            var dynamicRecursiveObjViaBacklinks = dynamicParentViaBacklinks.RecursiveObject;

            Assert.That(dynamicRecursiveObjViaBacklinks, Is.EqualTo(parent.RecursiveObject));
            Assert.That(dynamicRecursiveObjViaBacklinks.String, Is.EqualTo("level 1"));
#endif

            var secondLevelBacklinks = parent.RecursiveObject.Child.DynamicApi.GetBacklinksFromType(nameof(EmbeddedLevel1), nameof(EmbeddedLevel1.Child));
            Assert.That(secondLevelBacklinks.Count(), Is.EqualTo(1));
            Assert.That(secondLevelBacklinks.Single(), Is.EqualTo(parent.RecursiveObject));

            // This should be empty because no objects link to it via .Children
            var secondLevelChildrenBacklinks = parent.RecursiveObject.Child.DynamicApi.GetBacklinksFromType(nameof(EmbeddedLevel1), nameof(EmbeddedLevel1.Children));
            Assert.That(secondLevelChildrenBacklinks.Count(), Is.EqualTo(0));
        }

        [Test]
        public void EmbeddedObject_WhenReassignedToSameValue_IsNoOp()
        {
            var parent = _realm.Write(() =>
            {
                return _realm.Add(new ObjectWithEmbeddedProperties
                {
                    RecursiveObject = new EmbeddedLevel1
                    {
                        String = "abc"
                    }
                });
            });

            var child = parent.RecursiveObject;
            Assert.DoesNotThrow(() => _realm.Write(() =>
            {
                parent.RecursiveObject = child;
            }));

            Assert.That(parent.RecursiveObject!.String, Is.EqualTo("abc"));
        }

        private static EmbeddedAllTypesObject CreateEmbeddedAllTypesObject()
        {
            return new EmbeddedAllTypesObject
            {
                BooleanProperty = true,
                ByteCounterProperty = 2,
                ByteProperty = 5,
                CharProperty = 'b',
                DateTimeOffsetProperty = new DateTimeOffset(2020, 1, 2, 5, 6, 3, 2, TimeSpan.Zero),
                Decimal128Property = 2432546.52435893468943943643M,
                DecimalProperty = 234324.2139123912041M,
                DoubleProperty = 432.321,
                Int16CounterProperty = 123,
                Int16Property = 546,
                Int32CounterProperty = 324,
                Int32Property = 549,
                Int64CounterProperty = 943829483486934,
                Int64Property = 90439604934069,
                NullableBooleanProperty = null,
                NullableByteCounterProperty = 1,
                NullableByteProperty = null,
                NullableCharProperty = 'b',
                NullableDateTimeOffsetProperty = null,
                NullableDecimal128Property = 123.567M,
                NullableDecimalProperty = null,
                NullableDoubleProperty = 24.6,
                NullableInt16CounterProperty = null,
                NullableInt16Property = 2,
                NullableInt32CounterProperty = 98,
                NullableInt32Property = null,
                NullableInt64CounterProperty = 4,
                NullableInt64Property = null,
                NullableSingleProperty = 1.4f,
                SingleProperty = 1.6f,
                StringProperty = "abcd"
            };
        }
    }
}
