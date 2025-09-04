# Dictionaries - .NET SDK
> Version added: 10.2.0

## Overview
A Realm dictionary is an implementation of
[IDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2?view=net-5.0)
that has keys of type `String` and supports values of any
Realm type except
collections.
To define a dictionary, use
a getter-only `IDictionary<string, TValue>` property, where `TValue` is any
of the supported types.

## Dictionary Types
A dictionary of objects can contain null objects. Likewise, dictionaries
of primitive types can also contain null values. If you do not
want to allow null values in a dictionary, then either use non-nullable types in
the dictionary declaration (for example, use `IDictionary<string, double>`
instead of `IDictionary<string, double?>`). If you are using the older schema
type definition (your classes derive from the `RealmObject` base class),
or you do not have nullability enabled, use the
[Required] attribute if the dictionary
contains nullable reference types, such as `string` or `byte[]`.

Realm disallows the use of `.` or `$` characters in map keys.
You can use percent encoding and decoding to store a map key that contains
one of these disallowed characters.

The following code shows examples of dictionary types:

```csharp

public partial class Inventory : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public string Id { get; set; }
    // The key must be of type string; the value can be
    // of any Realm-supported type, including objects
    // that inherit from RealmObject or EmbeddedObject
    public IDictionary<string, Plant?> Plants { get; }

    public IDictionary<string, bool> BooleansDictionary { get; }

    public IDictionary<string, int?> NullableIntDictionary { get; }

    public IDictionary<string, string> RequiredStringsDictionary { get; }
}

```

## Usage Example
The following code shows how to create, write to, and read from Dictionaries
using either a string query (RQL) or LINQ.

```csharp
var storeInventory = new Inventory()
{
    Id = ObjectId.GenerateNewId().ToString()
};

storeInventory.Plants.Add("Petunia", new Plant());
storeInventory.NullableIntDictionary.Add("random things", 7);
storeInventory.RequiredStringsDictionary.Add("foo", "bar");

var storeInventory2 = new Inventory()
{
    Id = ObjectId.GenerateNewId().ToString()
};

storeInventory2.RequiredStringsDictionary.Add("foo", "Bar");

realm.Write(() =>
{
    realm.Add(storeInventory);
    realm.Add(storeInventory2);
});

// Find all Inventory items that have "Petunia"
// as a key in their Plants dictionary.
var petunias = realm.All<Inventory>()
    .Filter("Plants.@keys == 'Petunia'");

// Find all Inventory items that have at least one value in their
// IntDictionary that is larger than 5 using RQL
var matchesMoreThanFive = realm.All<Inventory>()
    .Filter("NullableIntDictionary.@values > 5");

// Find all Inventory items where the RequiredStringsDictionary has a key
// "Foo", and the value of that key contains the phrase "bar"
// (case insensitive)
var matches = realm.All<Inventory>()
    .Filter("RequiredStringsDictionary['foo'] CONTAINS[c] 'bar'");
// matches.Count() == 2

// Query the Plants dictionary of an Inventory object
// for a specific plant
var myStoreInventory = realm
    .All<Inventory>().FirstOrDefault();

var petunia = myStoreInventory.Plants.AsRealmQueryable()
    .Where(p => p.Name == "Petunia");

```

## Watching For Changes
You can use the [INotifyCollectionChanged.CollectionChanged](https://learn.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged.collectionchanged)
event on a dictionary to watch for changes to the collection, and the
[INotifyPropertyChanged.PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged)
event to watch for changes to specific properties in the dictionary.

In the following code example, we have a class with an `IDictionary<string, int>`
property named `IntDictionary`. We set up event
handlers for both the `CollectionChanged` and `PropertyChanged` events:

```csharp
var dictionary = container.IntDictionary.AsRealmCollection();

dictionary.CollectionChanged += (sender, e) =>
{
    Console.WriteLine($"Collection {sender} changed: {e.Action}");
};

dictionary.PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"Property changed on {sender}: {e.PropertyName}");
};

```
