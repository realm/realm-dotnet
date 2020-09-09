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
using System.Reflection;
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
            obj.RecursiveObject = new RecursiveEmbeddedObject
            {
                String = "first",
                Child = new RecursiveEmbeddedObject
                {
                    String = "second",
                    Child = new RecursiveEmbeddedObject
                    {
                        String = "third"
                    }
                },
                Children =
                {
                    new RecursiveEmbeddedObject
                    {
                        String = "I'm in a list",
                        Child = new RecursiveEmbeddedObject
                        {
                            String = "child in a list"
                        },
                        Children =
                        {
                            new RecursiveEmbeddedObject
                            {
                                String = "children in a list"
                            }
                        }
                    }
                }
            };

            Assert.That(obj.AllTypesObject.NullableSingleProperty, Is.EqualTo(1.4f));
            Assert.That(obj.ListOfAllTypesObjects.Count, Is.EqualTo(1));
            Assert.That(obj.RecursiveObject.String, Is.EqualTo("first"));
            Assert.That(obj.RecursiveObject.Child.String, Is.EqualTo("second"));
            Assert.That(obj.RecursiveObject.Child.Child.String, Is.EqualTo("third"));
            Assert.That(obj.RecursiveObject.Children[0].Child.String, Is.EqualTo("child in a list"));
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

            var properties = typeof(EmbeddedAllTypesObject).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var prop in properties)
            {
                Assert.That(prop.GetValue(parent.AllTypesObject), Is.EqualTo(prop.GetValue(copy)));
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
        public void RecursiveEmbedded_WhenLinkingToItself_Fails()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new RecursiveEmbeddedObject
                {
                    String = "a"
                }
            };

            // Set up a recursive relationship linking to itself
            parent.RecursiveObject.Child = parent.RecursiveObject;

            var ex = Assert.Throws<RealmException>(() => _realm.Write(() => _realm.Add(parent)));
            Assert.That(ex.Message, Is.EqualTo("Can't link to an embedded object that is already managed."));
        }

        [Test]
        public void RecursiveEmbeddedList_WhenLinkingToItself_Fails()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new RecursiveEmbeddedObject
                {
                    String = "a"
                }
            };

            // Set up a recursive relationship linking to itself
            parent.RecursiveObject.Children.Add(parent.RecursiveObject);

            var ex = Assert.Throws<RealmException>(() => _realm.Write(() => _realm.Add(parent)));
            Assert.That(ex.Message, Is.EqualTo("Can't add, set, or insert an embedded object that is already managed."));
        }

        [Test]
        public void RecursiveEmbedded_AddingALinkToItself_Fails()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new RecursiveEmbeddedObject { String = "a" }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var ex = Assert.Throws<RealmException>(() =>
            {
                _realm.Write(() =>
                {
                    parent.RecursiveObject.Child = parent.RecursiveObject;
                });
            });
            Assert.That(ex.Message, Is.EqualTo("Can't link to an embedded object that is already managed."));
        }

        [Test]
        public void RecursiveEmbeddedList_AddingALinkToItself_Fails()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new RecursiveEmbeddedObject { String = "a" }
            };

            _realm.Write(() =>
            {
                _realm.Add(parent);
            });

            var ex = Assert.Throws<RealmException>(() =>
            {
                _realm.Write(() =>
                {
                    parent.RecursiveObject.Children.Add(parent.RecursiveObject);
                });
            });
            Assert.That(ex.Message, Is.EqualTo("Can't add, set, or insert an embedded object that is already managed."));
        }

        [Test]
        public void RecursiveEmbedded_WhenDifferentReferences_Succeeds()
        {
            var parent = new ObjectWithEmbeddedProperties
            {
                RecursiveObject = new RecursiveEmbeddedObject
                {
                    String = "a",
                    Child = new RecursiveEmbeddedObject
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
                parent.RecursiveObject.Child.Child = new RecursiveEmbeddedObject
                {
                    String = "c"
                };
            });

            Assert.That(parent.RecursiveObject.Child.Child.IsManaged);
            Assert.That(parent.RecursiveObject.Child.Child.String, Is.EqualTo("c"));
            Assert.That(parent.RecursiveObject.Child.Child.Child, Is.Null);
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