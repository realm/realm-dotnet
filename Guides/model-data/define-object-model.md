# Object Models - .NET SDK
## Create an Object Model
Realm classes are regular C# classes that define the Realm schema.

> **IMPORTANT:**
> All Realm objects inherit from the
`IRealmObject`,
`IEmbeddedObject`, or
`IAsymmetricObject`
interface and must be declared `partial` classes.
>
> In versions of the .NET SDK older than 10.18.0, objects derive from
`RealmObject`,
`EmbeddedObject`, or
`AsymmetricObject`
base classes. This approach to Realm model definition is still supported, but
does not include new features such as the nullability annotations. In a future SDK release, the
base classes will become deprecated. You should use the interfaces for any
new classes that you write and should consider migrating your existing
classes.
>

> **NOTE:**
> Class names are limited to a maximum of 57 UTF-8 characters.
>

### Object Schema
An **object schema** is a configuration object that defines the properties and
relationships of a Realm object. Realm client
applications define object schemas with the native class implementation in their
respective language using the Object Schema.

Object schemas specify constraints on object properties such as the data
type of each property and whether or not a property is required. Schemas can
also define relationships between object
types in a realm.

Realm guarantees that all
objects in a realm conform to the
schema for their object type and validates objects whenever they're
created, modified, or deleted.

## Property Annotations
Schema properties are standard C# properties on a `RealmObject`. There are
several property annotations that you can use to more finely define how a Realm
handles a specific property.

### Primary Key
A **primary key** is a property that uniquely identifies an object. You can
create a primary key with any of the following types (or their nullable counterparts):

- `ObjectId`
- `UUID`
- `string`
- `char`
- `byte`
- `short`
- `int`
- `long`

You may define a primary key on a **single property** for an
object type as part of the object schema.
Realm automatically indexes primary key properties, which
allows you to efficiently read and modify objects based on their primary
key.

If an object type has a primary key, then all objects of that type must
include the primary key property with a value that is unique among
objects of the same type in a realm.

> **NOTE:**
> Once you assign a property as a primary key, you cannot change it.
>

The following example demonstrates how to designate a primary key in an object schema:

```csharp
public partial class Dog : IRealmObject
{
    [PrimaryKey]
    public string Name { get; set; }
    public int Age { get; set; }
    public Person? Owner { get; set; }
}

```

### Indexes
**Indexes** significantly improve query times in a Realm. Without
indexes, Realm scans every document in a collection to select the documents
that match the given query. However, if an applicable index exists for a query,
Realm uses the index to limit the number of documents that it must inspect.

You can index properties with the following types:

- `bool`
- `byte`
- `short`
- `int`
- `long`
- `DateTimeOffset`
- `char`
- `string`
- `ObjectId`
- `UUID`

> **NOTE:**
> Adding an index speeds up queries at the cost of slightly slower write
times and additional storage and memory overhead. Indexes require space in your
realm file, so adding an index to a property increases disk space consumed
by your realm file. Each index entry is a minimum of 12 bytes.
>

To index a property, use the `Indexed`
attribute. With the `Indexed` attribute, you can specify the type of index
on the property by using the `IndexType`
enum. In the following example, we have a default ("General") index on the `Name`
property:

```csharp
public partial class Person : IRealmObject
{
    [Indexed(IndexType.General)]
    public string Name { get; set; }

    [Indexed(IndexType.FullText)]
    public string Biography { get; set; }
}

```

### Full-Text Search Indexes
In addition to standard indexes, Realm also supports Full-Text Search (FTS) indexes
on `string` properties. While you can query a string field with or without a
standard index, an FTS index enables searching for multiple words and phrases and
excluding others.

For more information on querying full-text indexes, see Full Text Search
(LINQ) and Full Text Search (RQL).

To index an FTS property, use the `Indexed`
attribute with the `IndexType.FullText`
enum. In the following example, we have a `FullText` index on the
`Biography` property:

```csharp
public partial class Person : IRealmObject
{
    [Indexed(IndexType.General)]
    public string Name { get; set; }

    [Indexed(IndexType.FullText)]
    public string Biography { get; set; }
}

```

### Default Field Values
You can use the built-in language features to assign a default value to a property.
In C#, you can assign a default value on primitives in the property declaration.
You cannot set a default value on a collection, except to set it to `null!`.
Even if you set a collection to `null!`, collections are always initialized on
first access, so will never be null.

```csharp
public partial class Person : IRealmObject
{
    public string Name { get; set; } = "foo";

    public IList<PhoneNumber> PhoneNumbers { get; } = null!;
}

```

> **NOTE:**
> While default values ensure that a newly created object cannot contain
a value of `null` (unless you specify a default value of `null`),
they do not impact the nullability of a property. To make a property
non-nullable, see Required Properties.
>

### Ignore a Property
If you don't want to save a property in your model to a realm, you can
ignore that property. A property is ignored by default if it is not autoimplemented or
does not have a setter.

Ignore a property from a Realm object model with the
`Ignored` attribute:

```csharp
// Rather than store an Image in Realm,
// store the path to the Image...
public string ThumbnailPath { get; set; }

// ...and the Image itself can be
// in-memory when the app is running:
[Ignored]
public Image? Thumbnail { get; set; }

```

### Rename a Property
By default, Realm uses the name defined in the model class
to represent properties internally. In some cases you might want to change
this behavior:

- To make it easier to work across platforms, since naming conventions differ.
- To change a property name in .NET without forcing a migration.

Choosing an internal name that differs from the name used in model classes
has the following implications:

- Migrations must use the internal name when creating classes and properties.
- Schema errors reported will use the internal name.

Use the `MapTo`
attribute to rename a property:

```csharp
public partial class Person : IRealmObject
{
    [MapTo("moniker")]
    public string Name { get; set; }
}

```

### Rename a Class
By default, Realm uses the name defined in the model class
to represent classes internally. In some cases you might want to change
this behavior:

- To support multiple model classes with the same simple name in different namespaces.
- To make it easier to work across platforms, since naming conventions differ.
- To use a class name that is longer than the 57 character limit enforced by Realm.
- To change a class name in .NET without forcing a migration.

Use the `MapTo`
attribute to rename a class:

```csharp
[MapTo("Human")]
public partial class Person : IRealmObject
{
    public string Name { get; set; }
}

```

### Custom Setters
Realm will not store a property with a custom setter. To use a custom setter,
store the property value in a private property and then
map that value to a public property with the custom setter. Realm will store the
private property, while you modify its value via the public property.
In the following code, the private `email` property is stored in the realm,
but the public `Email` property, which provides validation, is not persisted:

```csharp
// This property will be stored in the Realm
private string email { get; set; }

// Custom validation of the email property.
// This property is *not* stored in Realm.
public string Email
{
    get { return email; }
    set
    {
        if (!value.Contains("@")) throw new Exception("Invalid email address");
        email = value;
    }
}

```

## Define Unstructured Data
> Version added: 12.2.0

Starting in SDK version 12.2.0, you can store
collections of mixed data
within a `RealmValue` property. You can use this feature to model complex data
structures, such as JSON, without having to define a
strict data model.

**Unstructured data** is data that doesn't easily conform to an expected
schema, making it difficult or impractical to model to individual
data classes. For example, your app might have highly variable data or dynamic
data whose structure is unknown at runtime.

Storing collections in a mixed property offers flexibility without sacrificing
functionality. And
you can work with them the same way you would a non-mixed
collection:

- You can nest mixed collections up to 100 levels.
- You can query on and react to changes on mixed collections.
- You can find and update individual mixed collection elements.

However, storing data in mixed collections is less performant than using a structured
schema or serializing JSON blobs into a single string property.

To model unstructured data in your app, define the appropriate properties in
your schema as RealmValue types. You can then
set these `RealmValue` properties as a list or a
dictionary of `RealmValue` elements.
Note that `RealmValue` *cannot* represent a set or an embedded
object.

> **TIP:**
> - Use a map of mixed data types when the type is unknown but each value will have a unique identifier.
> - Use a list of mixed data types when the type is unknown but the order of objects is meaningful.
>

## Omit Classes from your Realm Schema
By default, your application's Realm Schema includes all
classes that implement `IRealmObject` or `IEmbeddedObject`.
If you only want to include a subset of these classes in your Realm
Schema, you can update your configuration to include the specific classes you want:

```csharp
// Declare your schema
partial class LoneClass : IRealmObject
{
    public string Name { get; set; }
}

class AnotherClass
{
    private void SetUpMyRealmConfig()
    {
        // Define your config with a single class
        var config = new RealmConfiguration("RealmWithOneClass.realm");
        config.Schema = new[] { typeof(LoneClass) };

        // Or, specify multiple classes to use in the Realm
        config.Schema = new[] { typeof(Dog), typeof(Cat) };
    }
}

```

## Required and Optional Properties
In C#, value types, such as `int` and `bool`, are implicitly non-nullable.
However, they can be made optional by using the question mark (`?`) [notation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types).

Beginning with C# 8.0, nullable reference types were introduced. If your project
is using C# 8.0 or later, you can also declare reference types, such as `string`
and `byte[]`, as nullable with `?`.

> **NOTE:**
> Beginning with .NET 6.0, the nullable context is enabled by default for new
projects. For older projects, you can manually enable it. For more information,
refer to [https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types#setting-the-nullable-context](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types#setting-the-nullable-context).
>

The Realm .NET SDK fully supports the nullable-aware context and uses nullability
to determine whether a property is required or optional. The SDK has the
following rules:

- Realm assumes that both value- and reference-type properties are required if
you do not designate them as nullable. If you designate them as nullable
by using `?`, Realm considers them optional.
- You must declare properties that are Realm object types as nullable.
- You cannot declare collections (list, sets, backlinks, and dictionaries) as
nullable, but their parameters may be nullable according to the following rules: For all types of collections, if the parameters are primitives
(value- or reference-types), they can be required or nullable.For lists, sets, and backlinks, if the parameters are Realm objects, they
**cannot** be nullable.For dictionaries with a value type of Realm object, you **must** declare
the value type parameter as nullable.

The following code snippet demonstrates these rules:

```csharp
#nullable enable
public partial class Person : IRealmObject
{
    /* Reference Types */

    public string NonNullableName { get; set; }
    public string? NullableName { get; set; }
    public byte[] NonNullableArray { get; set; }
    public byte[]? NullableArray { get; set; }

    /* Value Types */
    public int NonNullableInt { get; set; }
    public int? NullableInt { get; set; }

    /* Realm Objects */

    public Dog? NullableDog { get; set; }
    // public Dog NonNullableDog { get; set; } // Compile-time error

    /* Collections of Primitives */

    public IList<int> IntListWithNonNullableValues { get; }
    public IList<int?> IntListWithNullableValues { get; }
    // public IList<int>? NullableListOfInts { get; } // Compile-time error

    /* Collections of Realm Objects */

    public IList<Dog> ListOfNonNullableObjects { get; }
    // public IList<Dog>? NullableListOfObjects { get; }  // Compile-time error
    // public IList<Dog?> ListOfNullableObjects { get; } // Compile-time error

    public ISet<Dog> SetOfNonNullableObjects { get; }
    // public ISet<Dog>? NullableSetOfObjects { get; }  // Compile-time error
    // public ISet<Dog?> SetOfNullableObjects { get; } // Compile-time error

    public IDictionary<string, Dog?> DictionaryOfNullableObjects { get; }
    // public IDictionary<string, Dog> DictionaryOfNonNullableObjects { get; } // Compile-time error
    // public IDictionary<string, Dog>? NullableDictionaryOfObjects { get; } // Compile-time error

    [Backlink(nameof(Dog.Person))]
    public IQueryable<Dog> MyDogs { get; }

    // [Backlink(nameof(Dog.People))]
    // public IQueryable<Dog?> MyDogs { get; } // Compile-time error
}

```

> **NOTE:**
> If you are using the older schema type definition (your classes derive from
the `RealmObject` base class), or you do not have nullability enabled, you
will need to use the
`Required` attribute
for any required `string` and `byte[]` property.
>

### Ignoring Nullability
You may prefer to have more flexibility in defining the nullability of properties
in your Realm objects. You can do so by setting `realm.ignore_objects_nullability = true`
in a [global configuration file](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files).

If you enable `realm.ignore_objects_nullability`, nullability annotations
will be ignored on Realm object properties, including collections of Realm
objects.
