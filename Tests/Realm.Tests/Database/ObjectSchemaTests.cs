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
using System.Reflection;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Schema;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ObjectSchemaTests
    {
        private class RequiredPropertyClass : RealmObject
        {
            [Required]
            public string FooRequired { get; set; }
        }

        public static readonly PropertyType[] CollectionModifiers = new[]
        {
            PropertyType.Int,
            PropertyType.Array,
            PropertyType.Dictionary,
            PropertyType.Set
        };

        public static readonly PropertyType[] CollectionTypes = new[]
        {
            PropertyType.Array,
            PropertyType.Dictionary,
            PropertyType.Set
        };

        public static readonly bool[] BoolValues = new[] { true, false };

        public static readonly bool?[] NullableBoolValues = new[] { true, false, (bool?)null };

        [Test]
        public void Property_WhenRequired_ShouldBeNonNullable()
        {
            var schema = ObjectSchema.FromType(typeof(RequiredPropertyClass).GetTypeInfo());

            if (!schema.TryFindProperty(nameof(RequiredPropertyClass.FooRequired), out var prop))
            {
                Assert.Fail("Could not find property");
            }

            Assert.That(prop.Type.HasFlag(PropertyType.Nullable), Is.False);
        }

        [Realms.Explicit]
        private class ExplicitClass : RealmObject
        {
            public int Foo { get; set; }
        }

        [Test]
        [Obsolete("Remove when we remove RealmSchema.Find")]
        public void Class_WhenExplicit_ShouldNotBeInDefaultSchema_Legacy()
        {
            Assert.That(RealmSchema.Default.Find(nameof(ExplicitClass)), Is.Null);
        }

        [Test]
        public void Class_WhenExplicit_ShouldNotBeInDefaultSchema()
        {
            var isInSchema = RealmSchema.Default.TryFindObjectSchema(nameof(ExplicitClass), out var schema);
            Assert.That(isInSchema, Is.False);
            Assert.That(schema, Is.Null);
        }

        public class FromTypeTestData
        {
            public Type Type { get; }

            public PropertyType InherentType { get; }

            public FromTypeTestData(Type type, PropertyType propertyType)
            {
                Type = type;
                InherentType = propertyType;
            }

            public override string ToString() => Type.FullName;
        }

        public static FromTypeTestData[] FromTypeTestCases =
        {
            new FromTypeTestData(typeof(bool), PropertyType.Bool),
            new FromTypeTestData(typeof(bool?), PropertyType.NullableBool),
            new FromTypeTestData(typeof(char), PropertyType.Int),
            new FromTypeTestData(typeof(char?), PropertyType.NullableInt),
            new FromTypeTestData(typeof(byte), PropertyType.Int),
            new FromTypeTestData(typeof(byte?), PropertyType.NullableInt),
            new FromTypeTestData(typeof(short), PropertyType.Int),
            new FromTypeTestData(typeof(short?), PropertyType.NullableInt),
            new FromTypeTestData(typeof(int), PropertyType.Int),
            new FromTypeTestData(typeof(int?), PropertyType.NullableInt),
            new FromTypeTestData(typeof(long), PropertyType.Int),
            new FromTypeTestData(typeof(long?), PropertyType.NullableInt),
            new FromTypeTestData(typeof(string), PropertyType.NullableString),
            new FromTypeTestData(typeof(byte[]), PropertyType.NullableData),
            new FromTypeTestData(typeof(float), PropertyType.Float),
            new FromTypeTestData(typeof(float?), PropertyType.NullableFloat),
            new FromTypeTestData(typeof(double), PropertyType.Double),
            new FromTypeTestData(typeof(double?), PropertyType.NullableDouble),
            new FromTypeTestData(typeof(decimal), PropertyType.Decimal),
            new FromTypeTestData(typeof(decimal?), PropertyType.NullableDecimal),
            new FromTypeTestData(typeof(Decimal128), PropertyType.Decimal),
            new FromTypeTestData(typeof(Decimal128?), PropertyType.NullableDecimal),
            new FromTypeTestData(typeof(DateTimeOffset), PropertyType.Date),
            new FromTypeTestData(typeof(DateTimeOffset?), PropertyType.NullableDate),
            new FromTypeTestData(typeof(ObjectId), PropertyType.ObjectId),
            new FromTypeTestData(typeof(ObjectId?), PropertyType.NullableObjectId),
            new FromTypeTestData(typeof(Guid), PropertyType.Guid),
            new FromTypeTestData(typeof(Guid?), PropertyType.NullableGuid),
        };

        [Test]
        public void Property_FromType_Tests(
            [ValueSource(nameof(FromTypeTestCases))] FromTypeTestData typeInfo,
            [ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(BoolValues))] bool isIndexed,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            var expectedType = isNullable switch
            {
                true => typeInfo.InherentType | PropertyType.Nullable,
                false => typeInfo.InherentType & ~PropertyType.Nullable,
                _ => typeInfo.InherentType
            };

            var expectedIsIndexed = isIndexed || isPrimaryKey;
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType("foo", typeof(IList<>).MakeGenericType(typeInfo.Type), isPrimaryKey, isIndexed, isNullable),
                PropertyType.Set => Property.FromType("foo", typeof(ISet<>).MakeGenericType(typeInfo.Type), isPrimaryKey, isIndexed, isNullable),
                PropertyType.Dictionary => Property.FromType("foo", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeInfo.Type), isPrimaryKey, isIndexed, isNullable),
                _ => Property.FromType("foo", typeInfo.Type, isPrimaryKey, isIndexed, isNullable),
            };

            if (isPrimaryKey && (collectionModifier != default || !Property.PrimaryKeyTypes.Contains(typeInfo.InherentType & ~PropertyType.Nullable)))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (expectedIsIndexed && (collectionModifier != default || !Property.IndexableTypes.Contains(typeInfo.InherentType & ~PropertyType.Nullable)))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else
            {
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.EqualTo(isPrimaryKey), $"Expect property.IsPrimaryKey to be {isPrimaryKey}");
                Assert.That(property.IsIndexed, Is.EqualTo(expectedIsIndexed), $"Expect property.IsIndexed to be {isIndexed} || {isPrimaryKey}");
                Assert.That(property.Type, Is.EqualTo(expectedType | collectionModifier));
            }
        }

        public class TypeInfoTestCase
        {
            public Type Type { get; }

            public string ExpectedObjectName { get; }

            public TypeInfoTestCase(Type type, string expectedObjectName)
            {
                Type = type;
                ExpectedObjectName = expectedObjectName;
            }

            public override string ToString() => ExpectedObjectName;
        }

        public static TypeInfoTestCase[] ObjectTypeTestCases =
        {
            new TypeInfoTestCase(typeof(Person), nameof(Person)),
            new TypeInfoTestCase(typeof(RemappedTypeObject), "__RemappedTypeObject"),
        };

        [Test]
        public void Property_FromType_Object(
            [ValueSource(nameof(ObjectTypeTestCases))] TypeInfoTestCase typeInfo,
            [ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(BoolValues))] bool isIndexed,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType("foo", typeof(IList<>).MakeGenericType(typeInfo.Type), isPrimaryKey, isIndexed, isNullable),
                PropertyType.Set => Property.FromType("foo", typeof(ISet<>).MakeGenericType(typeInfo.Type), isPrimaryKey, isIndexed, isNullable),
                PropertyType.Dictionary => Property.FromType("foo", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeInfo.Type), isPrimaryKey, isIndexed, isNullable),
                _ => Property.FromType("foo", typeInfo.Type, isPrimaryKey, isIndexed, isNullable),
            };

            if (isPrimaryKey)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (isIndexed)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else if (isNullable == true && (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be nullable"));
            }
            else if (isNullable == false && (collectionModifier == PropertyType.Dictionary || collectionModifier == default))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be required"));
            }
            else
            {
                var nullableModifier = (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set) ? default : PropertyType.Nullable;
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.False);
                Assert.That(property.IsIndexed, Is.False);
                Assert.That(property.Type, Is.EqualTo(PropertyType.Object | nullableModifier | collectionModifier));
                Assert.That(property.ObjectType, Is.EqualTo(typeInfo.ExpectedObjectName));
            }
        }

        [Test]
        public void Property_FromType_Generic_String(
            [ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(BoolValues))] bool isIndexed,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            var inherentType = PropertyType.String | PropertyType.Nullable;
            var expectedType = isNullable switch
            {
                true => inherentType | PropertyType.Nullable,
                false => inherentType & ~PropertyType.Nullable,
                _ => inherentType
            };

            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType<IList<string>>("foo", isPrimaryKey, isIndexed, isNullable),
                PropertyType.Set => Property.FromType<ISet<string>>("foo", isPrimaryKey, isIndexed, isNullable),
                PropertyType.Dictionary => Property.FromType<IDictionary<string, string>>("foo", isPrimaryKey, isIndexed, isNullable),
                _ => Property.FromType<string>("foo", isPrimaryKey, isIndexed, isNullable),
            };

            if (isPrimaryKey && collectionModifier != default)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (isIndexed && collectionModifier != default)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else
            {
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.EqualTo(isPrimaryKey), $"Expect property.IsPrimaryKey to be {isPrimaryKey}");
                Assert.That(property.IsIndexed, Is.EqualTo(isIndexed || isPrimaryKey), $"Expect property.IsIndexed to be {isIndexed} || {isPrimaryKey}");
                Assert.That(property.Type, Is.EqualTo(expectedType | collectionModifier));
            }
        }

        [Test]
        public void Property_FromTypeGeneric_Object(
            [ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(BoolValues))] bool isIndexed,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType<IList<Person>>("foo", isPrimaryKey, isIndexed, isNullable),
                PropertyType.Set => Property.FromType<ISet<Person>>("foo", isPrimaryKey, isIndexed, isNullable),
                PropertyType.Dictionary => Property.FromType<IDictionary<string, Person>>("foo", isPrimaryKey, isIndexed, isNullable),
                _ => Property.FromType<Person>("foo", isPrimaryKey, isIndexed, isNullable),
            };

            if (isPrimaryKey)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (isIndexed)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else if (isNullable == true && (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be nullable"));
            }
            else if (isNullable == false && (collectionModifier == PropertyType.Dictionary || collectionModifier == default))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be required"));
            }
            else
            {
                var nullableModifier = (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set) ? default : PropertyType.Nullable;
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.False);
                Assert.That(property.IsIndexed, Is.False);
                Assert.That(property.Type, Is.EqualTo(PropertyType.Object | nullableModifier | collectionModifier));
                Assert.That(property.ObjectType, Is.EqualTo(nameof(Person)));
            }
        }

        public static readonly RealmValueType[] PrimitiveTypes = ReflectionExtensions.GetEnumValues<RealmValueType>();

        [Test]
        public void Property_Primitive_Tests(
            [ValueSource(nameof(PrimitiveTypes))] RealmValueType type,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(BoolValues))] bool isIndexed,
            [ValueSource(nameof(BoolValues))] bool isNullable)
        {
            Property getProperty() => Property.Primitive("foo", type, isPrimaryKey, isIndexed, isNullable);

            if (type == RealmValueType.Null || type == RealmValueType.Object)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain($"can't be {type}"));
                return;
            }

            var expectedType = type.ToPropertyType(isNullable);

            if (isPrimaryKey && !Property.PrimaryKeyTypes.Contains(expectedType & ~PropertyType.Nullable))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (isIndexed && !Property.IndexableTypes.Contains(expectedType & ~PropertyType.Nullable))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else
            {
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.ObjectType, Is.Null);
                Assert.That(property.LinkOriginPropertyName, Is.Null);
                Assert.That(property.IsPrimaryKey, Is.EqualTo(isPrimaryKey), $"Expect property.IsPrimaryKey to be {isPrimaryKey}");
                Assert.That(property.IsIndexed, Is.EqualTo(isPrimaryKey || isIndexed), $"Expect property.IsIndexed to be {isIndexed} || {isPrimaryKey}");
                Assert.That(property.Type, Is.EqualTo(expectedType));
            }
        }

        [Test]
        public void Property_PrimitiveCollection_Tests(
            [ValueSource(nameof(CollectionTypes))] PropertyType collectionType,
            [ValueSource(nameof(PrimitiveTypes))] RealmValueType type,
            [ValueSource(nameof(BoolValues))] bool isNullable)
        {
            Property getProperty() => collectionType switch
            {
                PropertyType.Array => Property.PrimitiveList("foo", type, isNullable),
                PropertyType.Set => Property.PrimitiveSet("foo", type, isNullable),
                PropertyType.Dictionary => Property.PrimitiveDictionary("foo", type, isNullable),
                _ => throw new Exception($"Unexpected modifier: {CollectionTypes}"),
            };

            if (type == RealmValueType.Null || type == RealmValueType.Object)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty());
                Assert.That(ex.Message, Does.Contain($"can't be {type}"));
                return;
            }

            var expectedType = type.ToPropertyType(isNullable) | collectionType;

            var property = getProperty();
            Assert.That(property.Name, Is.EqualTo("foo"));
            Assert.That(property.IsPrimaryKey, Is.False);
            Assert.That(property.IsIndexed, Is.False);
            Assert.That(property.ObjectType, Is.Null);
            Assert.That(property.LinkOriginPropertyName, Is.Null);
            Assert.That(property.Type, Is.EqualTo(expectedType));
        }
    }
}
