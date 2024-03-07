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
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Schema;
#if TEST_WEAVER
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif

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

        public static readonly IndexType[] IndexTypes = new[] { IndexType.None, IndexType.General };

        public static readonly bool?[] NullableBoolValues = new[] { true, false, (bool?)null };

        [Test]
        public void Property_WhenRequired_ShouldBeNonNullable()
        {
            var schema = ObjectSchema.FromType(typeof(RequiredPropertyClass));

            Assert.That(schema.TryFindProperty(nameof(RequiredPropertyClass.FooRequired), out var prop), Is.True);
            Assert.That(prop.Type.HasFlag(PropertyType.Nullable), Is.False);
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
            [ValueSource(nameof(IndexTypes))] IndexType indexType,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            var expectedType = isNullable switch
            {
                true => typeInfo.InherentType | PropertyType.Nullable,
                false => typeInfo.InherentType & ~PropertyType.Nullable,
                _ => typeInfo.InherentType
            };

            var expectedIndexType = isPrimaryKey ? IndexType.General : indexType;
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType("foo", typeof(IList<>).MakeGenericType(typeInfo.Type), isPrimaryKey, indexType, isNullable),
                PropertyType.Set => Property.FromType("foo", typeof(ISet<>).MakeGenericType(typeInfo.Type), isPrimaryKey, indexType, isNullable),
                PropertyType.Dictionary => Property.FromType("foo", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeInfo.Type), isPrimaryKey, indexType, isNullable),
                _ => Property.FromType("foo", typeInfo.Type, isPrimaryKey, indexType, isNullable),
            };

            if (isPrimaryKey && (collectionModifier != default || !Property.PrimaryKeyTypes.Contains(typeInfo.InherentType & ~PropertyType.Nullable)))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (isPrimaryKey && indexType == IndexType.FullText)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("PrimaryKey properties cannot have a FullText index"));
            }
            else if (expectedIndexType == IndexType.General && (collectionModifier != default || !Property.IndexableTypes.Contains(typeInfo.InherentType & ~PropertyType.Nullable)))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else if (expectedIndexType == IndexType.FullText && typeInfo.Type != typeof(string))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot have a FullText index added to it"));
            }
            else
            {
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.EqualTo(isPrimaryKey), $"Expect property.IsPrimaryKey to be {isPrimaryKey}");
                Assert.That(property.IndexType, Is.EqualTo(expectedIndexType));
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
            [ValueSource(nameof(IndexTypes))] IndexType indexType,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType("foo", typeof(IList<>).MakeGenericType(typeInfo.Type), isPrimaryKey, indexType, isNullable),
                PropertyType.Set => Property.FromType("foo", typeof(ISet<>).MakeGenericType(typeInfo.Type), isPrimaryKey, indexType, isNullable),
                PropertyType.Dictionary => Property.FromType("foo", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeInfo.Type), isPrimaryKey, indexType, isNullable),
                _ => Property.FromType("foo", typeInfo.Type, isPrimaryKey, indexType, isNullable),
            };

            if (isPrimaryKey)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (indexType == IndexType.General)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else if (indexType == IndexType.FullText)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot have a FullText index"));
            }
            else if (isNullable == true && (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be nullable"));
            }
            else if (isNullable == false && (collectionModifier == PropertyType.Dictionary || collectionModifier == default))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be required"));
            }
            else
            {
                var nullableModifier = (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set) ? default : PropertyType.Nullable;
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.False);
                Assert.That(property.IndexType, Is.EqualTo(IndexType.None));
                Assert.That(property.Type, Is.EqualTo(PropertyType.Object | nullableModifier | collectionModifier));
                Assert.That(property.ObjectType, Is.EqualTo(typeInfo.ExpectedObjectName));
            }
        }

        [Test]
        public void Property_FromType_Generic_String(
            [ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(IndexTypes))] IndexType indexType,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            var inherentType = PropertyType.String | PropertyType.Nullable;
            var expectedType = isNullable switch
            {
                true => inherentType | PropertyType.Nullable,
                false => inherentType & ~PropertyType.Nullable,
                _ => inherentType
            };

            var expectedIndexType = isPrimaryKey ? IndexType.General : indexType;

            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType<IList<string>>("foo", isPrimaryKey, indexType, isNullable),
                PropertyType.Set => Property.FromType<ISet<string>>("foo", isPrimaryKey, indexType, isNullable),
                PropertyType.Dictionary => Property.FromType<IDictionary<string, string>>("foo", isPrimaryKey, indexType, isNullable),
                _ => Property.FromType<string>("foo", isPrimaryKey, indexType, isNullable),
            };

            if (isPrimaryKey && collectionModifier != default)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (indexType == IndexType.General && collectionModifier != default)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else
            {
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.EqualTo(isPrimaryKey), $"Expect property.IsPrimaryKey to be {isPrimaryKey}");
                Assert.That(property.IndexType, Is.EqualTo(expectedIndexType));
                Assert.That(property.Type, Is.EqualTo(expectedType | collectionModifier));
            }
        }

        [Test]
        public void Property_FromTypeGeneric_Object(
            [ValueSource(nameof(CollectionModifiers))] PropertyType collectionModifier,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(IndexTypes))] IndexType indexType,
            [ValueSource(nameof(NullableBoolValues))] bool? isNullable)
        {
            Property getProperty() => collectionModifier switch
            {
                PropertyType.Array => Property.FromType<IList<Person>>("foo", isPrimaryKey, indexType, isNullable),
                PropertyType.Set => Property.FromType<ISet<Person>>("foo", isPrimaryKey, indexType, isNullable),
                PropertyType.Dictionary => Property.FromType<IDictionary<string, Person>>("foo", isPrimaryKey, indexType, isNullable),
                _ => Property.FromType<Person>("foo", isPrimaryKey, indexType, isNullable),
            };

            if (isPrimaryKey)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (indexType == IndexType.General)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else if (indexType == IndexType.FullText)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot have a FullText index"));
            }
            else if (isNullable == true && (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be nullable"));
            }
            else if (isNullable == false && (collectionModifier == PropertyType.Dictionary || collectionModifier == default))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be required"));
            }
            else
            {
                var nullableModifier = (collectionModifier == PropertyType.Array || collectionModifier == PropertyType.Set) ? default : PropertyType.Nullable;
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.IsPrimaryKey, Is.False);
                Assert.That(property.IndexType, Is.EqualTo(IndexType.None));
                Assert.That(property.Type, Is.EqualTo(PropertyType.Object | nullableModifier | collectionModifier));
                Assert.That(property.ObjectType, Is.EqualTo(nameof(Person)));
            }
        }

        [Test]
        public void Property_Primitive_Tests(
            [ValueSource(typeof(TestHelpers), nameof(TestHelpers.PrimitiveRealmValueTypes))] RealmValueType type,
            [ValueSource(nameof(BoolValues))] bool isPrimaryKey,
            [ValueSource(nameof(IndexTypes))] IndexType indexType,
            [ValueSource(nameof(BoolValues))] bool isNullable)
        {
            Property getProperty() => Property.Primitive("foo", type, isPrimaryKey, indexType, isNullable);

            if (type == RealmValueType.Null || type == RealmValueType.Object)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain($"can't be {type}"));
                return;
            }

            var expectedIndexType = isPrimaryKey ? IndexType.General : indexType;
            var expectedType = type.ToPropertyType(isNullable);

            if (isPrimaryKey && !Property.PrimaryKeyTypes.Contains(expectedType & ~PropertyType.Nullable))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be primary key"));
            }
            else if (isPrimaryKey && indexType == IndexType.FullText)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("PrimaryKey properties cannot have a FullText index"));
            }
            else if (indexType == IndexType.General && !Property.IndexableTypes.Contains(expectedType & ~PropertyType.Nullable))
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot be indexed"));
            }
            else if (indexType == IndexType.FullText && (expectedType & ~PropertyType.Nullable) != PropertyType.String)
            {
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain("cannot have a FullText index"));
            }
            else
            {
                var property = getProperty();
                Assert.That(property.Name, Is.EqualTo("foo"));
                Assert.That(property.ObjectType, Is.Null);
                Assert.That(property.LinkOriginPropertyName, Is.Null);
                Assert.That(property.IsPrimaryKey, Is.EqualTo(isPrimaryKey), $"Expect property.IsPrimaryKey to be {isPrimaryKey}");
                Assert.That(property.IndexType, Is.EqualTo(expectedIndexType));
                Assert.That(property.Type, Is.EqualTo(expectedType));
            }
        }

        [Test]
        public void Property_PrimitiveCollection_Tests(
            [ValueSource(nameof(CollectionTypes))] PropertyType collectionType,
            [ValueSource(typeof(TestHelpers), nameof(TestHelpers.PrimitiveRealmValueTypes))] RealmValueType type,
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
                var ex = Assert.Throws<ArgumentException>(() => getProperty())!;
                Assert.That(ex.Message, Does.Contain($"can't be {type}"));
                return;
            }

            var expectedType = type.ToPropertyType(isNullable) | collectionType;

            var property = getProperty();
            Assert.That(property.Name, Is.EqualTo("foo"));
            Assert.That(property.IsPrimaryKey, Is.False);
            Assert.That(property.IndexType, Is.EqualTo(IndexType.None));
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
            Assert.That(property.IndexType, Is.EqualTo(IndexType.None));
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
            Assert.That(property.IndexType, Is.EqualTo(IndexType.None));
            Assert.That(property.ObjectType, Is.EqualTo("Bar"));
            Assert.That(property.LinkOriginPropertyName, Is.EqualTo("OriginProperty"));
            Assert.That(property.Type, Is.EqualTo(PropertyType.Array | PropertyType.LinkingObjects));
        }

        [Test]
        public void Property_FromType_InvalidArguments()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Property.FromType(null!, typeof(string)))!;
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => Property.FromType(string.Empty, typeof(string)))!;
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentNullException>(() => Property.FromType("foo", null!))!;
            Assert.That(ex3.ParamName, Is.EqualTo("type"));

            var ex4 = Assert.Throws<ArgumentException>(() => Property.FromType("foo", typeof(Exception)))!;
            Assert.That(ex4.ParamName, Is.EqualTo("type"));
        }

        [Test]
        public void Property_FromType_Generic_InvalidArguments()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Property.FromType<string>(null!))!;
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => Property.FromType<string>(string.Empty))!;
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentException>(() => Property.FromType<Exception>("foo"))!;
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

            var ex1 = Assert.Throws<ArgumentNullException>(() => getProperty(null!))!;
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => getProperty(string.Empty))!;
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

            var ex1 = Assert.Throws<ArgumentNullException>(() => getProperty(null!, "Bar"))!;
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => getProperty(string.Empty, "Bar"))!;
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentNullException>(() => getProperty("Foo", null!))!;
            Assert.That(ex3.ParamName, Is.EqualTo("objectType"));

            var ex4 = Assert.Throws<ArgumentException>(() => getProperty("Foo", string.Empty))!;
            Assert.That(ex4.ParamName, Is.EqualTo("objectType"));
        }

        [Test]
        public void Property_Backlinks_InvalidArguments()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Property.Backlinks(null!, "Bar", "Origin"))!;
            Assert.That(ex1.ParamName, Is.EqualTo("name"));

            var ex2 = Assert.Throws<ArgumentException>(() => Property.Backlinks(string.Empty, "Bar", "Origin"))!;
            Assert.That(ex2.ParamName, Is.EqualTo("name"));

            var ex3 = Assert.Throws<ArgumentNullException>(() => Property.Backlinks("Foo", null!, "Origin"))!;
            Assert.That(ex3.ParamName, Is.EqualTo("originObjectType"));

            var ex4 = Assert.Throws<ArgumentException>(() => Property.Backlinks("Foo", string.Empty, "Origin"))!;
            Assert.That(ex4.ParamName, Is.EqualTo("originObjectType"));

            var ex5 = Assert.Throws<ArgumentNullException>(() => Property.Backlinks("Foo", "Bar", null!))!;
            Assert.That(ex5.ParamName, Is.EqualTo("originPropertyName"));

            var ex6 = Assert.Throws<ArgumentException>(() => Property.Backlinks("Foo", "Bar", string.Empty))!;
            Assert.That(ex6.ParamName, Is.EqualTo("originPropertyName"));
        }

        [Test]
        public void ObjectSchemaBuilder_FromType_AddsCorrectProperties()
        {
            var builder = new ObjectSchema.Builder(typeof(ClassWithUnqueryableMembers));
            Assert.That(builder.Name, Is.EqualTo(nameof(ClassWithUnqueryableMembers)));
            Assert.That(builder.RealmSchemaType, Is.Not.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
            Assert.That(builder.RealmSchemaType, Is.Not.EqualTo(ObjectSchema.ObjectType.AsymmetricObject));

            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.PublicField)), Is.False);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.PublicMethod)), Is.False);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.IgnoredProperty)), Is.False);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.NonAutomaticProperty)), Is.False);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.PropertyWithOnlyGet)), Is.False);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.StaticProperty)), Is.False);

            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.RealPropertyToSatisfyWeaver)), Is.True);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.RealmObjectProperty)), Is.True);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.RealmListProperty)), Is.True);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.FirstName)), Is.True);
            Assert.That(builder.Contains(nameof(ClassWithUnqueryableMembers.BacklinkProperty)), Is.True);

            Assert.That(builder.Build().Type, Is.EqualTo(typeof(ClassWithUnqueryableMembers)));
        }

        [Test]
        public void ObjectSchemaBuilder_FromType_CanAddProperties()
        {
            var builder = new ObjectSchema.Builder(typeof(PrimaryKeyGuidObject));
            Assert.That(builder.Name, Is.EqualTo(nameof(PrimaryKeyGuidObject)));
            Assert.That(builder.RealmSchemaType, Is.Not.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
            Assert.That(builder.RealmSchemaType, Is.Not.EqualTo(ObjectSchema.ObjectType.AsymmetricObject));

            Assert.That(builder.Count, Is.EqualTo(1));

            builder.Add(Property.FromType<int>("Foo"));

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder["_id"].Type, Is.EqualTo(PropertyType.Guid));
            Assert.That(builder["Foo"].Type, Is.EqualTo(PropertyType.Int));
        }

        [Test]
        public void ObjectSchemaBuilder_FromType_InvalidCases()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectSchema.Builder(null!));
            Assert.Throws<ArgumentException>(() => new ObjectSchema.Builder(typeof(string)));
        }

        [Test]
        public void ObjectSchemaBuilder_FromType_Indexes()
        {
            var builder = new ObjectSchema.Builder(typeof(IndexesClass));
            Assert.That(builder.Name, Is.EqualTo(nameof(IndexesClass)));
            Assert.That(builder.Count, Is.EqualTo(8));

            Assert.That(builder[nameof(IndexesClass.Id)].IndexType, Is.EqualTo(IndexType.General));
            Assert.That(builder[nameof(IndexesClass.Id)].IsPrimaryKey, Is.True);

            Assert.That(builder[nameof(IndexesClass.StringFts)].IndexType, Is.EqualTo(IndexType.FullText));
            Assert.That(builder[nameof(IndexesClass.StringGeneral)].IndexType, Is.EqualTo(IndexType.General));
            Assert.That(builder[nameof(IndexesClass.StringDefault)].IndexType, Is.EqualTo(IndexType.General));
            Assert.That(builder[nameof(IndexesClass.StringNone)].IndexType, Is.EqualTo(IndexType.None));
            Assert.That(builder[nameof(IndexesClass.IntGeneral)].IndexType, Is.EqualTo(IndexType.General));
            Assert.That(builder[nameof(IndexesClass.IntDefault)].IndexType, Is.EqualTo(IndexType.General));
            Assert.That(builder[nameof(IndexesClass.IntNone)].IndexType, Is.EqualTo(IndexType.None));
        }

        [Test]
        public void ObjectSchemaBuilder_CanBuildEmptySchema()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.EmbeddedObject);
            var schema = builder.Build();

            Assert.That(schema.Count, Is.Zero);
            Assert.That(schema.Name, Is.EqualTo("MyClass"));
            Assert.That(builder.RealmSchemaType, Is.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
        }

        [Test]
        public void ObjectSchemaBuilder_InvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectSchema.Builder(null!, ObjectSchema.ObjectType.EmbeddedObject));
            Assert.Throws<ArgumentException>(() => new ObjectSchema.Builder(string.Empty, ObjectSchema.ObjectType.EmbeddedObject));
        }

        [Test]
        public void ObjectSchemaBuilder_Indexer_InvalidArguments()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject);

            // null is not a valid name
            Assert.Throws<ArgumentNullException>(() => _ = builder[null!]);
            Assert.Throws<ArgumentNullException>(() => builder[null!] = Property.Primitive("Foo", RealmValueType.Int));

            // Getting a non-existent item
            Assert.Throws<KeyNotFoundException>(() => _ = builder["non-existent"]);

            // Mismatch between provided name and property name
            var ex = Assert.Throws<ArgumentException>(() => builder["Bar"] = Property.Primitive("Foo", RealmValueType.Int))!;
            Assert.That(ex.Message, Does.Contain("Bar"));
            Assert.That(ex.Message, Does.Contain("Foo"));
        }

        [Test]
        public void ObjectSchemaBuilder_Indexer_GetSet()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject);

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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject);

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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject);

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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject);

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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject);

            var propertyFoo = Property.Primitive("Foo", RealmValueType.Int);

            var returnedBuilder = builder.Add(propertyFoo);

            Assert.AreSame(builder, returnedBuilder);
        }

        [Test]
        public void ObjectSchemaBuilder_ContainsItem_ReturnsTrueForMatches()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
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
        public void ObjectSchemaBuilder_ContainsString_ReturnsTrueForMatches()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Foo", RealmValueType.Int)
            };

            Assert.That(builder.Contains("Foo"), Is.True);
            Assert.That(builder.Contains("Bar"), Is.False);

            Assert.Throws<ArgumentNullException>(() => builder.Contains(null!));
        }

        [Test]
        public void ObjectSchemaBuilder_RemoveItem()
        {
            var propertyToRemain = Property.Primitive("Foo", RealmValueType.Decimal128);
            var propertyToRemove = Property.Object("Bar", "MyOtherClass");
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
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
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
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
        public void ObjectSchemaBuilder_Build_WhenMultiplePKProperties_Throws()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("PK1", RealmValueType.Int, isPrimaryKey: true),
                Property.Primitive("PK2", RealmValueType.Int, isPrimaryKey: true),
            };

            var ex = Assert.Throws<ArgumentException>(() => builder.Build())!;
            Assert.That(ex.Message, Does.Contain("PK1"));
            Assert.That(ex.Message, Does.Contain("PK2"));
        }

        [Test]
        public void ObjectSchemaBuilder_Build_ResolvesPrimaryKey()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Foo", RealmValueType.Date, isNullable: true),
                Property.Primitive("PK", RealmValueType.Int, isPrimaryKey: true),
                Property.Primitive("SomeOtherProp", RealmValueType.Int, indexType: IndexType.General),
            };

            var schema = builder.Build();
            Assert.That(schema.PrimaryKeyProperty, Is.Not.Null);
            Assert.That(schema.PrimaryKeyProperty!.Value, Is.EqualTo(builder["PK"]));
        }

        [Test]
        public void ObjectSchemaBuilder_Build_WhenNoPK()
        {
            var builder = new ObjectSchema.Builder("MyClass", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Foo", RealmValueType.Date, isNullable: true),
                Property.Primitive("SomeOtherProp", RealmValueType.Int, indexType: IndexType.General),
            };

            var schema = builder.Build();
            Assert.That(schema.PrimaryKeyProperty, Is.Null);
        }

        [Test]
        public void ObjectSchema_FromType_AssignsTypeInfo([Values(typeof(Person), typeof(EmbeddedAllTypesObject))] Type type)
        {
            var schema = ObjectSchema.FromType(type);

            Assert.That(schema.Name, Is.EqualTo(type.Name));
            Assert.That(schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject, Is.EqualTo(typeof(IEmbeddedObject).IsAssignableFrom(type)));
            Assert.That(schema.Type, Is.EqualTo(type));
        }

        [Test]
        public void ObjectSchema_FromType_RemappedType()
        {
            var schema = ObjectSchema.FromType(typeof(RemappedTypeObject));

            Assert.That(schema.Name, Is.EqualTo("__RemappedTypeObject"));
            Assert.That(schema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
            Assert.That(schema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.AsymmetricObject));
            Assert.That(schema.TryFindProperty("__mappedLink", out var remappedProp), Is.True);
            Assert.That(remappedProp.Type, Is.EqualTo(PropertyType.Object | PropertyType.Nullable));
            Assert.That(remappedProp.ObjectType, Is.EqualTo("__RemappedTypeObject"));
        }

        [Test]
        public void ObjectSchema_FromType_ResolvesPrimaryKey()
        {
            var schema = ObjectSchema.FromType(typeof(PrimaryKeyStringObject));

            Assert.That(schema.Name, Is.EqualTo(nameof(PrimaryKeyStringObject)));
            Assert.That(schema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
            Assert.That(schema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.AsymmetricObject));
            Assert.That(schema.PrimaryKeyProperty, Is.Not.Null);
            Assert.That(schema.PrimaryKeyProperty!.Value.IsPrimaryKey, Is.True);
            Assert.That(schema.PrimaryKeyProperty.Value.Type, Is.EqualTo(PropertyType.NullableString));
            Assert.That(schema.PrimaryKeyProperty.Value.Name, Is.EqualTo("_id"));
        }

        [Test]
        public void ObjectSchema_FromType_InvalidCases()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectSchema.FromType(null!));
            Assert.Throws<ArgumentException>(() => ObjectSchema.FromType(typeof(string)));
        }

        [Test]
        public void ObjectSchema_TryFindProperty_InvalidCases()
        {
            var schema = ObjectSchema.FromType(typeof(AllTypesObject));

            Assert.Throws<ArgumentNullException>(() => schema.TryFindProperty(null!, out _));
            Assert.Throws<ArgumentException>(() => schema.TryFindProperty(string.Empty, out _));
        }

        [Test]
        public void ObjectSchema_GetBuilder_Build_CreatesNewInstance()
        {
            var originalSchema = ObjectSchema.FromType(typeof(PrimaryKeyGuidObject));
            var newlyBuiltSchema = originalSchema.GetBuilder().Build();

            Assert.That(originalSchema, Is.Not.SameAs(newlyBuiltSchema));
        }

        [Test]
        public void ObjectSchema_GetBuilder_ContainsAllProperties()
        {
            var originalSchema = ObjectSchema.FromType(typeof(PrimaryKeyGuidObject));
            var builder = originalSchema.GetBuilder();

            Assert.That(originalSchema, Is.EquivalentTo(builder));
            Assert.That(originalSchema.Name, Is.EqualTo(builder.Name));
            Assert.That(originalSchema.BaseType, Is.EqualTo(builder.RealmSchemaType));
        }

        [Test]
        public void ObjectSchema_GetBuilder_PreservesTypeInfo()
        {
            var typedSchema = ObjectSchema.FromType(typeof(PrimaryKeyGuidObject));
            var typedBuilder = typedSchema.GetBuilder();

            Assert.That(typedSchema.Type, Is.EqualTo(typedBuilder.Type));
            Assert.That(typedBuilder.Type, Is.EqualTo(typeof(PrimaryKeyGuidObject)));

            var untypedSchema = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject)
            {
                Property.FromType<int>("Bar")
            }.Build();
            var untypedBuilder = untypedSchema.GetBuilder();
            Assert.That(untypedSchema.Type, Is.EqualTo(untypedBuilder.Type));
            Assert.That(untypedBuilder.Type, Is.Null);
        }

        [Test]
        public void ObjectSchema_GetBuilder_DoesntModifyOriginal()
        {
            var schema = ObjectSchema.FromType(typeof(PrimaryKeyGuidObject));
            var builder = schema.GetBuilder();
            builder.Add(Property.FromType<int>("Foo"));

            Assert.That(schema, Is.Not.EquivalentTo(builder));
            Assert.That(schema, Is.Not.EquivalentTo(builder.Build()));
        }

        [Test]
        public void RealmSchema_AddDefaultTypes_InvalidTestCases()
        {
            // Can't pass null for types
            Assert.Throws<ArgumentNullException>(() => RealmSchema.AddDefaultTypes(null!));

            // Can't pass a non-RealmObject inheritor
            Assert.Throws<ArgumentException>(() => RealmSchema.AddDefaultTypes(new[] { typeof(string) }));

            // Can't add a new type after defaultTypes has been evaluated
            _ = RealmSchema.Default;
            Assert.Throws<NotSupportedException>(() => RealmSchema.AddDefaultTypes(new[] { typeof(ExplicitClass) }));
        }

        [Test]
        public void RealmSchema_AddDefaultTypes_WhenTypeIsAlreadyPresent_IsNoOp()
        {
            // Evaluate default schema, then try to add a class already in there
            _ = RealmSchema.Default;
            Assert.DoesNotThrow(() => RealmSchema.AddDefaultTypes(new[] { typeof(Person) }));
        }

        [Test]
        public void RealmSchema_TryFindProperty_InvalidCases()
        {
            var schema = RealmSchema.Empty;

            Assert.Throws<ArgumentNullException>(() => schema.TryFindObjectSchema(null!, out _));
            Assert.Throws<ArgumentException>(() => schema.TryFindObjectSchema(string.Empty, out _));
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromObjectSchemas([Values(BclCollectionType.Array, BclCollectionType.List)] BclCollectionType collectionType)
        {
            var atoSchema = ObjectSchema.FromType(typeof(AllTypesObject));
            var personSchema = ObjectSchema.FromType(typeof(Person));
            RealmSchema schema = collectionType switch
            {
                BclCollectionType.Array => new[] { atoSchema, personSchema },
                BclCollectionType.List => new List<ObjectSchema> { atoSchema, personSchema },
                _ => throw new NotSupportedException(),
            };

            Assert.That(schema.Count, Is.EqualTo(2));
            Assert.That(schema, Is.EquivalentTo(new[] { atoSchema, personSchema }));

            Assert.That(schema.TryFindObjectSchema(nameof(AllTypesObject), out var foundAtoSchema), Is.True);
            Assert.That(foundAtoSchema, Is.EqualTo(atoSchema));
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromNullObjectSchemas([Values(BclCollectionType.Array, BclCollectionType.List)] BclCollectionType collectionType)
        {
            RealmSchema schema = collectionType switch
            {
                BclCollectionType.Array => (ObjectSchema[])null!,
                BclCollectionType.List => (List<ObjectSchema>)null!,
                _ => throw new NotSupportedException(),
            };

            Assert.That(schema, Is.Null);
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromObjectSchemas_WithDuplicates([Values(BclCollectionType.Array, BclCollectionType.List)] BclCollectionType collectionType)
        {
            var foo1 = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var foo2 = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                RealmSchema schema = collectionType switch
                {
                    BclCollectionType.Array => new[] { foo1, foo2 },
                    BclCollectionType.List => new List<ObjectSchema> { foo1, foo2 },
                    _ => throw new NotSupportedException(),
                };
            })!;

            Assert.That(ex.Message, Does.Contain("Foo"));
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromObjectSchemas_WithNulls([Values(BclCollectionType.Array, BclCollectionType.List)] BclCollectionType collectionType)
        {
            var foo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                RealmSchema schema = collectionType switch
                {
                    BclCollectionType.Array => new[] { foo, null! },
                    BclCollectionType.List => new List<ObjectSchema> { foo, null! },
                    _ => throw new NotSupportedException(),
                };
            });
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromTypes(
            [Values(BclCollectionType.Array, BclCollectionType.List, BclCollectionType.HashSet)] BclCollectionType collectionType)
        {
            var atoSchema = typeof(AllTypesObject);
            var personSchema = typeof(Person);
            RealmSchema schema = collectionType switch
            {
                BclCollectionType.Array => new[] { atoSchema, personSchema },
                BclCollectionType.List => new List<Type> { atoSchema, personSchema },
                BclCollectionType.HashSet => new HashSet<Type> { atoSchema, personSchema },
                _ => throw new NotSupportedException(),
            };

            Assert.That(schema.Count, Is.EqualTo(2));

            Assert.That(schema.TryFindObjectSchema(nameof(AllTypesObject), out var foundAtoSchema), Is.True);
            Assert.That(foundAtoSchema, Is.EquivalentTo(ObjectSchema.FromType(atoSchema)));
            Assert.That(foundAtoSchema!.Type, Is.EqualTo(atoSchema));

            Assert.That(schema.TryFindObjectSchema(nameof(Person), out var foundPersonSchema), Is.True);
            Assert.That(foundPersonSchema, Is.EquivalentTo(ObjectSchema.FromType(personSchema)));
            Assert.That(foundPersonSchema!.Type, Is.EqualTo(personSchema));
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromNullTypes(
            [Values(BclCollectionType.Array, BclCollectionType.List, BclCollectionType.HashSet)] BclCollectionType collectionType)
        {
            RealmSchema schema = collectionType switch
            {
                BclCollectionType.Array => (Type[])null!,
                BclCollectionType.List => (List<Type>)null!,
                BclCollectionType.HashSet => (HashSet<Type>)null!,
                _ => throw new NotSupportedException(),
            };

            Assert.That(schema, Is.Null);
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromTypes_WithDuplicates(
            [Values(BclCollectionType.Array, BclCollectionType.List, BclCollectionType.HashSet)] BclCollectionType collectionType)
        {
            var type1 = typeof(Person);
            var type2 = typeof(Person);
            RealmSchema schema = collectionType switch
            {
                BclCollectionType.Array => new[] { type1, type2 },
                BclCollectionType.List => new List<Type> { type1, type2 },
                BclCollectionType.HashSet => new HashSet<Type> { type1, type2 },
                _ => throw new NotSupportedException(),
            };

            Assert.That(schema.Count, Is.EqualTo(1));
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromTypes_WithDuplicatesFromDifferentNamespaces(
            [Values(BclCollectionType.Array, BclCollectionType.List, BclCollectionType.HashSet)] BclCollectionType collectionType)
        {
            var type1 = typeof(Foo.DuplicateClass);
            var type2 = typeof(Bar.DuplicateClass);
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                RealmSchema schema = collectionType switch
                {
                    BclCollectionType.Array => new[] { type1, type2 },
                    BclCollectionType.List => new List<Type> { type1, type2 },
                    BclCollectionType.HashSet => new HashSet<Type> { type1, type2 },
                    _ => throw new NotSupportedException(),
                };
            })!;

            Assert.That(ex.Message, Does.Contain("Foo"));
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromTypes_WithNulls(
            [Values(BclCollectionType.Array, BclCollectionType.List, BclCollectionType.HashSet)] BclCollectionType collectionType)
        {
            var type = typeof(AllTypesObject);
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                RealmSchema schema = collectionType switch
                {
                    BclCollectionType.Array => new[] { type, null! },
                    BclCollectionType.List => new List<Type> { type, null! },
                    BclCollectionType.HashSet => new HashSet<Type> { type, null! },
                    _ => throw new NotSupportedException(),
                };
            });
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromNullBuilder()
        {
            RealmSchema.Builder builder = null!;
            RealmSchema schema = builder;

            Assert.That(schema, Is.Null);

            RealmSchema.Builder? nullableBuilder = null;
            RealmSchema? nullableSchema = nullableBuilder;

            Assert.That(nullableSchema, Is.Null);
        }

        [Test]
        public void RealmSchema_ImplicitConversion_FromBuilder()
        {
            var builder = new RealmSchema.Builder(new[] { typeof(Person) });
            RealmSchema schema = builder;

            Assert.That(schema.Count, Is.EqualTo(1));
            Assert.That(schema.TryFindObjectSchema(nameof(Person), out var personSchema), Is.True);
            Assert.That(personSchema!.Type, Is.EqualTo(typeof(Person)));
        }

        [Test]
        public void RealmSchema_GetBuilder_Build_CreatesNewInstance()
        {
            RealmSchema originalSchema = new[] { typeof(PrimaryKeyGuidObject) };
            var newlyBuiltSchema = originalSchema.GetBuilder().Build();

            Assert.That(originalSchema, Is.Not.SameAs(newlyBuiltSchema));
        }

        [Test]
        public void RealmSchema_GetBuilder_ContainsAllSchemas()
        {
            var types = new[] { typeof(Person), typeof(AllTypesObject), typeof(EmbeddedIntPropertyObject) };
            RealmSchema originalSchema = types;
            var builder = originalSchema.GetBuilder();

            Assert.That(builder, Is.EquivalentTo(originalSchema));
            Assert.That(builder.Select(b => b.Name), Is.EquivalentTo(types.Select(t => t.Name)));
        }

        [Test]
        public void RealmSchema_GetBuilder_DoesntModifyOriginal()
        {
            RealmSchema schema = new[] { typeof(PrimaryKeyGuidObject) };
            var builder = schema.GetBuilder();
            builder.Add(typeof(AllTypesObject));

            Assert.That(schema.Select(t => t.Name), Is.Not.EquivalentTo(builder.Select(t => t.Name)));
            Assert.That(schema.Select(t => t.Name), Is.Not.EquivalentTo(builder.Build().Select(t => t.Name)));
        }

        [Test]
        public void RealmSchemaBuilder_CanBuildEmptySchema()
        {
            var builder = new RealmSchema.Builder();
            var schema = builder.Build();

            Assert.That(schema.Count, Is.Zero);
        }

        [Test]
        public void RealmSchemaBuilder_InvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new RealmSchema.Builder((IEnumerable<ObjectSchema>)null!));
            Assert.Throws<ArgumentNullException>(() => new RealmSchema.Builder((IEnumerable<Type>)null!));
        }

        [Test]
        public void RealmSchemaBuilder_Indexer_InvalidArguments()
        {
            var builder = new RealmSchema.Builder();

            // null is not a valid name
            Assert.Throws<ArgumentNullException>(() => _ = builder[null!]);
            Assert.Throws<ArgumentNullException>(() => builder[null!] = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build());

            // Getting a non-existent item
            Assert.Throws<KeyNotFoundException>(() => _ = builder["non-existent"]);

            // Mismatch between provided name and property name
            var ex = Assert.Throws<ArgumentException>(() => builder["Bar"] = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build())!;
            Assert.That(ex.Message, Does.Contain("Bar"));
            Assert.That(ex.Message, Does.Contain("Foo"));
        }

        [Test]
        public void RealmSchemaBuilder_Indexer_GetSet()
        {
            var builder = new RealmSchema.Builder();

            Assert.That(builder.Count, Is.Zero);

            var schemaToSet = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            builder["Foo"] = schemaToSet;

            Assert.That(builder.Count, Is.EqualTo(1));

            var schemaFromGet = builder["Foo"];
            Assert.That(schemaFromGet, Is.EqualTo(schemaToSet));
        }

        [Test]
        public void RealmSchemaBuilder_Indexer_ReplaceExisting()
        {
            var builder = new RealmSchema.Builder();

            var schemaToSet = ObjectSchema.FromType(typeof(AllTypesObject));
            builder[nameof(AllTypesObject)] = schemaToSet;

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder[nameof(AllTypesObject)].Type, Is.EqualTo(typeof(AllTypesObject)));

            var schemaToReplace = new ObjectSchema.Builder(nameof(AllTypesObject), ObjectSchema.ObjectType.RealmObject)
                .Add(Property.Primitive("Foo", RealmValueType.String))
                .Build();
            builder[nameof(AllTypesObject)] = schemaToReplace;

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder[nameof(AllTypesObject)], Is.EqualTo(schemaToReplace));
            Assert.That(builder[nameof(AllTypesObject)].Type, Is.Null);
        }

        [Test]
        public void RealmSchemaBuilder_AddObjectSchema()
        {
            var builder = new RealmSchema.Builder();

            var atoSchema = ObjectSchema.FromType(typeof(AllTypesObject));
            var personSchema = ObjectSchema.FromType(typeof(Person));

            builder.Add(atoSchema);
            builder.Add(personSchema);

            Assert.That(builder.Count, Is.EqualTo(2));
            Assert.That(builder[nameof(AllTypesObject)], Is.EqualTo(atoSchema));
            Assert.That(builder[nameof(Person)], Is.EqualTo(personSchema));
        }

        [Test]
        public void RealmSchemaBuilder_AddObjectSchema_WhenDuplicate_Throws()
        {
            var builder = new RealmSchema.Builder();

            var schemaFoo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var schemaFooAgain = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();

            builder.Add(schemaFoo);

            Assert.Throws<ArgumentException>(() => builder.Add(schemaFoo));
            Assert.Throws<ArgumentException>(() => builder.Add(schemaFooAgain));

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder["Foo"], Is.EqualTo(schemaFoo));
        }

        [Test]
        public void RealmSchemaBuilder_AddObjectSchema_ReturnsSameBuilderInstance()
        {
            var builder = new RealmSchema.Builder();

            var schemaFoo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();

            var returnedBuilder = builder.Add(schemaFoo);

            Assert.AreSame(builder, returnedBuilder);
        }

        [Test]
        public void RealmSchemaBuilder_AddObjectSchemaBuilder()
        {
            var builder = new RealmSchema.Builder();

            var fooBuilder = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Prop1", RealmValueType.String)
            };

            var barBuilder = new ObjectSchema.Builder("Bar", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Prop2", RealmValueType.Float)
            };

            builder.Add(fooBuilder);
            builder.Add(barBuilder);

            Assert.That(builder.Count, Is.EqualTo(2));
            Assert.That(builder["Foo"], Is.EquivalentTo(fooBuilder));
            Assert.That(builder["Bar"], Is.EquivalentTo(barBuilder));
        }

        [Test]
        public void RealmSchemaBuilder_AddObjectSchemaBuilder_WhenDuplicate_Throws()
        {
            var builder = new RealmSchema.Builder();

            var fooBuilder = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Prop", RealmValueType.String)
            };

            var fooBuilderAgain = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject);

            builder.Add(fooBuilder);

            Assert.Throws<ArgumentException>(() => builder.Add(fooBuilder));
            Assert.Throws<ArgumentException>(() => builder.Add(fooBuilderAgain));

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder["Foo"], Is.EquivalentTo(fooBuilder));
        }

        [Test]
        public void RealmSchemaBuilder_AddObjectSchemaBuilder_ReturnsSameBuilderInstance()
        {
            var builder = new RealmSchema.Builder();

            var schemaFoo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject);

            var returnedBuilder = builder.Add(schemaFoo);

            Assert.AreSame(builder, returnedBuilder);
        }

        [Test]
        public void RealmSchemaBuilder_AddType()
        {
            var builder = new RealmSchema.Builder();

            var atoType = typeof(AllTypesObject);
            var personType = typeof(Person);

            builder.Add(atoType);
            builder.Add(personType);

            Assert.That(builder.Count, Is.EqualTo(2));
            Assert.That(builder[nameof(AllTypesObject)].Type, Is.EqualTo(atoType));
            Assert.That(builder[nameof(Person)].Type, Is.EqualTo(personType));
        }

        [Test]
        public void RealmSchemaBuilder_AddType_WhenDuplicate_Throws()
        {
            var builder = new RealmSchema.Builder();

            var fooDuplicate = typeof(Foo.DuplicateClass);
            var barDuplicate = typeof(Bar.DuplicateClass);

            builder.Add(fooDuplicate);

            // Adding the same type should be ignored
            Assert.DoesNotThrow(() => builder.Add(fooDuplicate));
            var ex = Assert.Throws<ArgumentException>(() => builder.Add(barDuplicate))!;
            Assert.That(ex.Message, Does.Contain(fooDuplicate.FullName));
            Assert.That(ex.Message, Does.Contain(barDuplicate.FullName));

            Assert.That(builder.Count, Is.EqualTo(1));
            Assert.That(builder[nameof(Foo.DuplicateClass)].Type, Is.EqualTo(fooDuplicate));
        }

        [Test]
        public void RealmSchemaBuilder_AddType_ReturnsSameBuilderInstance()
        {
            var builder = new RealmSchema.Builder();

            var returnedBuilder = builder.Add(typeof(AllTypesObject));

            Assert.AreSame(builder, returnedBuilder);
        }

        [Test]
        public void RealmSchemaBuilder_Add_InvalidArguments()
        {
            var builder = new RealmSchema.Builder();

            Assert.Throws<ArgumentNullException>(() => builder.Add((ObjectSchema)null!));
            Assert.Throws<ArgumentNullException>(() => builder.Add((Type)null!));
            Assert.Throws<ArgumentNullException>(() => builder.Add((ObjectSchema.Builder)null!));
        }

        [Test]
        public void RealmSchemaBuilder_ContainsItem_ReturnsTrueForMatches()
        {
            var fooBuilder = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject)
            {
                Property.Primitive("Bar", RealmValueType.Double)
            };

            var builder = new RealmSchema.Builder
            {
                fooBuilder
            };

            // Contains should return true for both the added property and an equivalent one
            Assert.That(builder.Contains(builder["Foo"]), Is.True);

            // Should return false for different name or different instance
            Assert.That(builder.Contains(fooBuilder.Build()), Is.False);
            Assert.That(builder.Contains(new ObjectSchema.Builder("Bar", ObjectSchema.ObjectType.RealmObject).Build()), Is.False);

            Assert.Throws<ArgumentNullException>(() => builder.Contains((ObjectSchema)null!));
        }

        [Test]
        public void RealmSchemaBuilder_ContainsString_ReturnsTrueForMatches()
        {
            var builder = new RealmSchema.Builder
            {
                new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject)
            };

            Assert.That(builder.Contains("Foo"), Is.True);
            Assert.That(builder.Contains("Bar"), Is.False);

            Assert.Throws<ArgumentNullException>(() => builder.Contains((null as string)!));
        }

        [Test]
        public void RealmSchemaBuilder_RemoveItem()
        {
            var foo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var bar = new ObjectSchema.Builder("Bar", ObjectSchema.ObjectType.RealmObject).Build();
            var builder = new RealmSchema.Builder
            {
                foo,
                bar
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove(bar), Is.True);
            Assert.That(builder.Count, Is.EqualTo(1));

            Assert.That(builder["Foo"], Is.EqualTo(foo));
        }

        [Test]
        public void RealmSchemaBuilder_RemoveItem_WhenItemIsNotEqual_DoesntRemove()
        {
            var foo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var bar = new ObjectSchema.Builder("Bar", ObjectSchema.ObjectType.RealmObject).Build();
            var builder = new RealmSchema.Builder
            {
                foo,
                bar
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            var fooAgain = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            Assert.That(builder.Remove(fooAgain), Is.False);
            Assert.That(builder.Remove(new ObjectSchema.Builder("FooFoo", ObjectSchema.ObjectType.RealmObject).Build()), Is.False);

            Assert.That(builder.Count, Is.EqualTo(2));
        }

        [Test]
        public void RealmSchemaBuilder_RemoveString()
        {
            var foo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var bar = new ObjectSchema.Builder("Bar", ObjectSchema.ObjectType.RealmObject).Build();
            var builder = new RealmSchema.Builder
            {
                foo,
                bar
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove("Bar"), Is.True);
            Assert.That(builder.Count, Is.EqualTo(1));

            Assert.That(builder["Foo"], Is.EqualTo(foo));
        }

        [Test]
        public void RealmSchemaBuilder_RemoveString_WhenItemIsNotEquivalent_DoesntRemove()
        {
            var foo = new ObjectSchema.Builder("Foo", ObjectSchema.ObjectType.RealmObject).Build();
            var bar = new ObjectSchema.Builder("Bar", ObjectSchema.ObjectType.RealmObject).Build();
            var builder = new RealmSchema.Builder
            {
                foo,
                bar
            };

            Assert.That(builder.Count, Is.EqualTo(2));

            Assert.That(builder.Remove("FooFoo"), Is.False);

            Assert.That(builder.Count, Is.EqualTo(2));
            Assert.That(builder["Foo"], Is.EqualTo(foo));
            Assert.That(builder["Bar"], Is.EqualTo(bar));
        }

        private static void ValidateBuiltSchema(ObjectSchema.Builder builder, params Property[] expectedProperties)
        {
            var schema = builder.Build();
            Assert.That(schema.Name, Is.EqualTo("MyClass"));
            Assert.That(schema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
            Assert.That(schema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.AsymmetricObject));
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

            public override string? ToString() => Type.FullName;
        }

        public enum BclCollectionType
        {
            Array,
            List,
            HashSet
        }
    }

    public partial class RequiredPropertyClass : TestRealmObject
    {
#if TEST_WEAVER
        [Required]
#endif
        public string FooRequired { get; set; } = string.Empty;
    }

    [Explicit]
    public partial class ExplicitClass : TestRealmObject
    {
        public int Foo { get; set; }
    }

    public partial class IndexesClass : TestRealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set;  }

        [Indexed(IndexType.FullText)]
        public string? StringFts { get; set; }

        [Indexed(IndexType.General)]
        public string? StringGeneral { get; set; }

        [Indexed]
        public string? StringDefault { get; set; }

        public string? StringNone { get; set; }

        [Indexed(IndexType.General)]
        public int IntGeneral { get; set; }

        [Indexed]
        public int IntDefault { get; set; }

        public int IntNone { get; set; }
    }
}
