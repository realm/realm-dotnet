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

            Assert.That(schema.TryFindProperty(nameof(RequiredPropertyClass.FooRequired), out var prop), Is.True);
            Assert.That(prop.Type.HasFlag(PropertyType.Nullable), Is.False);
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

        [Test]
        public void Property_Object_Tests([ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier)
        {
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.ObjectList("foo", "Bar"),
                PropertyType.Set => Property.ObjectSet("foo", "Bar"),
                PropertyType.Dictionary => Property.ObjectDictionary("foo", "Bar"),
                _ => Property.Object("foo", "Bar"),
            };

            var nullabilityModifier = collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set ? default : PropertyType.Nullable;
            var expectedType = PropertyType.Object | collectionModifier | nullabilityModifier;

            var property = getProperty();
            Assert.That(property.Name, Is.EqualTo("foo"));
            Assert.That(property.IsPrimaryKey, Is.False);
            Assert.That(property.IsIndexed, Is.False);
            Assert.That(property.ObjectType, Is.EqualTo("Bar"));
            Assert.That(property.LinkOriginPropertyName, Is.Null);
            Assert.That(property.Type, Is.EqualTo(expectedType));
        }

        [Test]
        public void Property_Backlinks()
        {
            var property = Property.Backlinks("foo", "Bar", "OriginProperty");
            Assert.That(property.Name, Is.EqualTo("foo"));
            Assert.That(property.IsPrimaryKey, Is.False);
            Assert.That(property.IsIndexed, Is.False);
            Assert.That(property.ObjectType, Is.EqualTo("Bar"));
            Assert.That(property.LinkOriginPropertyName, Is.EqualTo("OriginProperty"));
            Assert.That(property.Type, Is.EqualTo(PropertyType.Array | PropertyType.LinkingObjects));
        }

        [Test]
        public void Property_FromType_InvalidArguments()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Property.FromType(null, typeof(string)));
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => Property.FromType(string.Empty, typeof(string)));
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentNullException>(() => Property.FromType("foo", null));
            Assert.That(ex3.ParamName, Is.EqualTo("type"));

            var ex4 = Assert.Throws<ArgumentException>(() => Property.FromType("foo", typeof(Exception)));
            Assert.That(ex4.ParamName, Is.EqualTo("type"));
        }

        [Test]
        public void Property_FromType_Generic_InvalidArguments()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Property.FromType<string>(null));
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => Property.FromType<string>(string.Empty));
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentException>(() => Property.FromType<Exception>("foo"));
            Assert.That(ex3.ParamName, Is.EqualTo("type"));
        }

        [Test]
        public void Property_Primitive_InvalidArguments([ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier)
        {
            Property getProperty(string name) => collectionModifier switch
            {
                PropertyType.Array => Property.PrimitiveList(name, RealmValueType.Int),
                PropertyType.Set => Property.PrimitiveSet(name, RealmValueType.Int),
                PropertyType.Dictionary => Property.PrimitiveDictionary(name, RealmValueType.Int),
                _ => Property.Primitive(name, RealmValueType.Int),
            };

            var ex1 = Assert.Throws<ArgumentNullException>(() => getProperty(null));
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => getProperty(string.Empty));
            Assert.That(ex2.ParamName, Is.EqualTo("name"));
        }

        [Test]
        public void Property_Object_InvalidArguments([ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier)
        {
            Property getProperty(string name, string objectType) => collectionModifier switch
            {
                PropertyType.Array => Property.ObjectList(name, objectType),
                PropertyType.Set => Property.ObjectSet(name, objectType),
                PropertyType.Dictionary => Property.ObjectDictionary(name, objectType),
                _ => Property.Object(name, objectType),
            };

            var ex1 = Assert.Throws<ArgumentNullException>(() => getProperty(null, "Bar"));
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => getProperty(string.Empty, "Bar"));
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentNullException>(() => getProperty("Foo", null));
            Assert.That(ex3.ParamName, Is.EqualTo("objectType"));

            var ex4 = Assert.Throws<ArgumentException>(() => getProperty("Foo", string.Empty));
            Assert.That(ex4.ParamName, Is.EqualTo("objectType"));
        }

        [Test]
        public void Property_Backlinks_InvalidArguments()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Property.Backlinks(null, "Bar", "Origin"));
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => Property.Backlinks(string.Empty, "Bar", "Origin"));
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentNullException>(() => Property.Backlinks("Foo", null, "Origin"));
            Assert.That(ex3.ParamName, Is.EqualTo("originObjectType"));

            var ex4 = Assert.Throws<ArgumentException>(() => Property.Backlinks("Foo", string.Empty, "Origin"));
            Assert.That(ex4.ParamName, Is.EqualTo("originObjectType"));

            var ex5 = Assert.Throws<ArgumentNullException>(() => Property.Backlinks("Foo", "Bar", null));
            Assert.That(ex5.ParamName, Is.EqualTo("originPropertyName"));

            var ex6 = Assert.Throws<ArgumentException>(() => Property.Backlinks("Foo", "Bar", string.Empty));
            Assert.That(ex6.ParamName, Is.EqualTo("originPropertyName"));
        }

        [Test]
        public void ObjectSchemaBuilder_CanBuildEmptySchema()
        {
            var builder = new ObjectSchema.Builder("MyClass", isEmbedded: true);
            var schema = builder.Build();

            Assert.That(schema.Count, Is.Zero);
            Assert.That(schema.Name, Is.EqualTo("MyClass"));
            Assert.That(schema.IsEmbedded, Is.True);
        }

        [Test]
        public void ObjectSchemaBuilder_InvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectSchema.Builder(null, isEmbedded: true));
            Assert.Throws<ArgumentException>(() => new ObjectSchema.Builder(string.Empty, isEmbedded: true));
        }

        [Test]
        public void ObjectSchemaBuilder_Indexer_InvalidArguments()
        {
            var builder = new ObjectSchema.Builder("MyClass");

            // null is not a valid name
            Assert.Throws<ArgumentNullException>(() => _ = builder[null]);
            Assert.Throws<ArgumentNullException>(() => builder[null] = Property.Primitive("Foo", RealmValueType.Int));

            // Getting a non-existent item
            Assert.Throws<KeyNotFoundException>(() => _ = builder["non-existent"]);

            // Mismatch between provided name and property name
            var ex = Assert.Throws<ArgumentException>(() => builder["Bar"] = Property.Primitive("Foo", RealmValueType.Int));
            Assert.That(ex.Message, Does.Contain("Bar"));
            Assert.That(ex.Message, Does.Contain("Foo"));
        }

        [Test]
        public void ObjectSchemaBuilder_Indexer_GetSet()
        {
            var builder = new ObjectSchema.Builder("MyClass");

            Assert.That(builder.Count, Is.Zero);

            var propertyToSet = Property.Primitive("Foo", RealmValueType.Int);
            builder["Foo"] = propertyToSet;

            Assert.That(builder.Count, Is.EqualTo(1));

            var propertyFromGet = builder["Foo"];
            Assert.That(propertyFromGet, Is.EqualTo(propertyToSet));

            ValidateBuiltSchema(builder, propertyToSet);
        }

        [Test]
        public void ObjectSchemaBuilder_Indexer_ReplaceExisting()
        {
            var builder = new ObjectSchema.Builder("MyClass");

            var propertyToSet = Property.Primitive("Foo", RealmValueType.Int);
            builder["Foo"] = propertyToSet;

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder["Foo"].Type, Is.EqualTo(PropertyType.Int));

            var propertyToReplace = Property.Primitive("Foo", RealmValueType.String, isNullable: true);
            builder["Foo"] = propertyToReplace;

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder["Foo"].Type, Is.EqualTo(PropertyType.NullableString));
            Assert.That(builder["Foo"], Is.EqualTo(propertyToReplace));

            ValidateBuiltSchema(builder, propertyToReplace);
        }

        [Test]
        public void ObjectSchemaBuilder_Add_AddsProperty()
        {
            var builder = new ObjectSchema.Builder("MyClass");

            var propertyFoo = Property.Primitive("Foo", RealmValueType.Int);
            var propertyBar = Property.Primitive("Bar", RealmValueType.Int);

            builder.Add(propertyFoo);
            builder.Add(propertyBar);

            Assert.That(builder.Count, Is.EqualTo(2));
            Assert.That(builder["Foo"], Is.EqualTo(propertyFoo));
            Assert.That(builder["Bar"], Is.EqualTo(propertyBar));

            ValidateBuiltSchema(builder, propertyFoo, propertyBar);
        }

        [Test]
        public void ObjectSchemaBuilder_Add_WhenDuplicate_Throws()
        {
            var builder = new ObjectSchema.Builder("MyClass");

            var propertyFoo = Property.Primitive("Foo", RealmValueType.Int);
            var propertyFooAgain = Property.Primitive("Foo", RealmValueType.String);

            builder.Add(propertyFoo);

            Assert.Throws<ArgumentException>(() => builder.Add(propertyFoo));
            Assert.Throws<ArgumentException>(() => builder.Add(propertyFooAgain));

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder["Foo"], Is.EqualTo(propertyFoo));

            ValidateBuiltSchema(builder, propertyFoo);
        }

        [Test]
        public void ObjectSchemaBuilder_Add_ReturnsSameBuilderInstance()
        {
            var builder = new ObjectSchema.Builder("MyClass");

            var propertyFoo = Property.Primitive("Foo", RealmValueType.Int);

            var returnedBuilder = builder.Add(propertyFoo);

            Assert.AreSame(builder, returnedBuilder);
        }

        [Test]
        public void ObjectSchemaBuilder_ContainsItem_ReturnsTrueForMatches()
        {
            var builder = new ObjectSchema.Builder("MyClass")
            {
                Property.Primitive("Foo", RealmValueType.Int)
            };

            // Contains should return true for both the added property and an equivalent one
            Assert.That(builder.Contains(builder["Foo"]), Is.True);
            Assert.That(builder.Contains(Property.Primitive("Foo", RealmValueType.Int)), Is.True);

            // Should return false for wrong type or different name
            Assert.That(builder.Contains(Property.Primitive("Bar", RealmValueType.Int)), Is.False);
            Assert.That(builder.Contains(Property.Primitive("Foo", RealmValueType.String)), Is.False);
        }

        [Test]
        public void ObjectSchemaBuilder_RemoveItem()
        {
            var propertyToRemain = Property.Primitive("Foo", RealmValueType.Decimal128);
            var propertyToRemove = Property.Object("Bar", "MyOtherClass");
            var builder = new ObjectSchema.Builder("MyClass")
            {
                propertyToRemain,
                propertyToRemove
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove(propertyToRemove), Is.True);
            Assert.That(builder.Count, Is.EqualTo(1));

            ValidateBuiltSchema(builder, propertyToRemain);
        }

        [Test]
        public void ObjectSchemaBuilder_RemoveItem_WhenItemIsNotEquivalent_DoesntRemove()
        {
            var foo = Property.Primitive("Foo", RealmValueType.Decimal128);
            var bar = Property.Object("Bar", "MyOtherClass");
            var builder = new ObjectSchema.Builder("MyClass")
            {
                foo,
                bar
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove(Property.Primitive("Foo", RealmValueType.Int)), Is.False);
            Assert.That(builder.Remove(Property.Primitive("FooFoo", RealmValueType.Decimal128)), Is.False);

            Assert.That(builder.Count, Is.EqualTo(2));

            ValidateBuiltSchema(builder, foo, bar);
        }

        [Test]
        public void ObjectSchemaBuilder_RemoveString()
        {
            var propertyToRemain = Property.Primitive("Foo", RealmValueType.Decimal128);
            var propertyToRemove = Property.Object("Bar", "MyOtherClass");
            var builder = new ObjectSchema.Builder("MyClass")
            {
                propertyToRemain,
                propertyToRemove
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove("Bar"), Is.True);
            Assert.That(builder.Count, Is.EqualTo(1));

            ValidateBuiltSchema(builder, propertyToRemain);
        }

        [Test]
        public void ObjectSchemaBuilder_RemoveString_WhenItemIsNotEquivalent_DoesntRemove()
        {
            var foo = Property.Primitive("Foo", RealmValueType.Decimal128);
            var bar = Property.Object("Bar", "MyOtherClass");
            var builder = new ObjectSchema.Builder("MyClass")
            {
                foo,
                bar
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove("FooFoo"), Is.False);

            Assert.That(builder.Count, Is.EqualTo(2));

            ValidateBuiltSchema(builder, foo, bar);
        }

        [Test]
        public void ObjectSchemaBuilder_ContainsString_ReturnsTrueForMatches()
        {
            var builder = new ObjectSchema.Builder("MyClass")
            {
                Property.Primitive("Foo", RealmValueType.Int)
            };

            Assert.That(builder.Contains("Foo"), Is.True);
            Assert.That(builder.Contains("Bar"), Is.False);

            Assert.Throws<ArgumentNullException>(() => builder.Contains(null));
        }

        [Test]
        public void ObjectSchemaBuilder_Build_WhenMultiplePKProperties_Throws()
        {
            var builder = new ObjectSchema.Builder("MyClass")
            {
                Property.Primitive("PK1", RealmValueType.Int, isPrimaryKey: true),
                Property.Primitive("PK2", RealmValueType.Int, isPrimaryKey: true),
            };

            var ex = Assert.Throws<ArgumentException>(() => builder.Build());
            Assert.That(ex.Message, Does.Contain("PK1"));
            Assert.That(ex.Message, Does.Contain("PK2"));
        }

        [Test]
        public void ObjectSchemaBuilder_Build_ResolvesPrimaryKey()
        {
            var builder = new ObjectSchema.Builder("MyClass")
            {
                Property.Primitive("Foo", RealmValueType.Date, isNullable: true),
                Property.Primitive("PK", RealmValueType.Int, isPrimaryKey: true),
                Property.Primitive("SomeOtherProp", RealmValueType.Int, isIndexed: true),
            };

            var schema = builder.Build();
            Assert.That(schema.PrimaryKeyProperty, Is.Not.Null);
            Assert.That(schema.PrimaryKeyProperty.Value, Is.EqualTo(builder["PK"]));
        }

        [Test]
        public void ObjectSchemaBuilder_Build_WhenNoPK()
        {
            var builder = new ObjectSchema.Builder("MyClass")
            {
                Property.Primitive("Foo", RealmValueType.Date, isNullable: true),
                Property.Primitive("SomeOtherProp", RealmValueType.Int, isIndexed: true),
            };

            var schema = builder.Build();
            Assert.That(schema.PrimaryKeyProperty, Is.Null);
        }

        [Test]
        public void ObjectSchema_FromType_AssignsTypeInfo([Values(typeof(Person), typeof(EmbeddedAllTypesObject))] Type type)
        {
            var schema = ObjectSchema.FromType(type.GetTypeInfo());

            Assert.That(schema.Name, Is.EqualTo(type.Name));
            Assert.That(schema.IsEmbedded, Is.EqualTo(type.BaseType == typeof(EmbeddedObject)));
            Assert.That(schema.Type, Is.EqualTo(type.GetTypeInfo()));
        }

        [Test]
        public void ObjectSchema_FromType_RemappedType()
        {
            var schema = ObjectSchema.FromType(typeof(RemappedTypeObject).GetTypeInfo());

            Assert.That(schema.Name, Is.EqualTo("__RemappedTypeObject"));
            Assert.That(schema.IsEmbedded, Is.False);
            Assert.That(schema.TryFindProperty("__mappedLink", out var remappedProp), Is.True);
            Assert.That(remappedProp.Type, Is.EqualTo(PropertyType.Object | PropertyType.Nullable));
            Assert.That(remappedProp.ObjectType, Is.EqualTo("__RemappedTypeObject"));
        }

        [Test]
        public void ObjectSchema_FromType_ResolvesPrimaryKey()
        {
            var schema = ObjectSchema.FromType(typeof(PrimaryKeyStringObject).GetTypeInfo());

            Assert.That(schema.Name, Is.EqualTo(nameof(PrimaryKeyStringObject)));
            Assert.That(schema.IsEmbedded, Is.False);
            Assert.That(schema.PrimaryKeyProperty, Is.Not.Null);
            Assert.That(schema.PrimaryKeyProperty.Value.IsPrimaryKey, Is.True);
            Assert.That(schema.PrimaryKeyProperty.Value.Type, Is.EqualTo(PropertyType.NullableString));
            Assert.That(schema.PrimaryKeyProperty.Value.Name, Is.EqualTo("_id"));
        }

        [Test]
        public void ObjectSchema_FromType_InvalidCases()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectSchema.FromType(null));
            Assert.Throws<ArgumentException>(() => ObjectSchema.FromType(typeof(string).GetTypeInfo()));
        }

        private static void ValidateBuiltSchema(ObjectSchema.Builder builder, params Property[] expectedProperties)
        {
            var schema = builder.Build();
            Assert.That(schema.Name, Is.EqualTo("MyClass"));
            Assert.That(schema.IsEmbedded, Is.False);
            Assert.That(schema.Count, Is.EqualTo(expectedProperties.Length));

            foreach (var prop in expectedProperties)
            {
                Assert.That(schema.TryFindProperty(prop.Name, out var schemaProperty), Is.True);
                Assert.That(schemaProperty, Is.EqualTo(prop));
            }

            Assert.That(schema, Is.EquivalentTo(expectedProperties));
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

        private class RequiredPropertyClass : RealmObject
        {
            [Required]
            public string FooRequired { get; set; }
        }

        [Explicit]
        private class ExplicitClass : RealmObject
        {
            public int Foo { get; set; }
        }
    }
}
