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