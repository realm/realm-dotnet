# Field Types - .NET SDK
## Overview
The .NET SDK supports three categories of data types:

- .NET types
- Bson types
- Realm-Specific types

## .NET Types
Realm supports the following .NET data types and their
nullable counterparts:

- `bool`
- `byte`
- `short`
- `int`
- `long`
- `float`
- `double`
- `decimal`
- `char`
- `string`
- `byte[]`
- `DateTimeOffset` (note: Realm converts `DateTimeOffset` values to UTC before storing in the database and does not store the timezone information. See [Issue #1835](https://github.com//realm/realm-dotnet/issues/1835) for more information.)
- `Guid`
- `IList<T>`, where T is any of the supported data types
(see Lists)

> **NOTE:**
> The `byte`, `char`, `short`, `int`, and `long` types  are all stored
as 64 bit integer values within Realm.
>

## Bson Types
- `ObjectId`
- `Decimal128`

### Guid and ObjectId Properties
The `ObjectId` Bson type is a 12-byte unique value, while the
built-in .NET type `Guid` is a 16-byte universally-unique value. Both types are
indexable, and either can be used as a
Primary Key.

### Using Decimal Values
Realm supports 128-bit decimal values with the `Decimal128` Bson type. When
defining a decimal type, you can use the `Decimal128` Bson type or the .NET
`decimal` type, even though it is only a 96-bit decimal. The SDK automatically
converts between the two, although you risk losing precision or range. The
following example shows how to use both the `Decimal128` Bson type and the .NET
`decimal` type:

```csharp
public class MyClassWithDecimals
{
    [PrimaryKey]
    public ObjectId _id { get; } = ObjectId.GenerateNewId();

    // Standard (96-bit) decimal value type
    public decimal VeryPreciseNumber { get; set; }

    // 128-bit Decimal128
    public Decimal128 EvenMorePreciseNumber { get; set; }
    public Decimal128 AnotherEvenMorePreciseNumber { get; set; }

    // Nullable decimal or Decimal128 are supported, too
    public decimal? MaybeDecimal { get; set; }
    public Decimal128? MaybeDecimal128 { get; set; }

    public void DoDecimalStuff()
    {
        var myInstance = new MyClassWithDecimals();
        // To store decimal values:
        realm.Write(() =>
        {
            myInstance.VeryPreciseNumber = 1.234567890123456789M;
            myInstance.EvenMorePreciseNumber = Decimal128.Parse("987654321.123456789");

            // Decimal128 has explicit constructors that take a float or a double
            myInstance.EvenMorePreciseNumber = new Decimal128(9.99999);
        });

    }
}

```

## Realm-Specific Types
Any class that implements `RealmObject` or `EmbeddedObject` can contain a
`RealmObject` subclass and/or an Embedded Objects
subclass. In addition, Realm supports the following custom types:

- Collections
- Dictionaries
- Sets
- RealmInteger
- RealmValue
