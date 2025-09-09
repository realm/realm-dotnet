# Sets - .NET SDK
> Version added: 10.2.0

## Overview
A Realm set, like the C#
[HashSet<>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-5.0),
is an implementation of
[ICollection<>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=net-5.0)
and
[IEnumerable<>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=net-5.0).
It supports values of any
Realm type except collections.
To define a set, use a getter-only `ISet<TValue>` property, where `TValue`
is any of the supported types.

Deleting an object from the database will remove it from any sets
in which it existed. Therefore, a set of objects will never contain null objects.
However, sets of primitive types can contain null values. If you do not
want to allow null values in a set, then either use non-nullable types in
the set declaration (for example, use `ISet<double>` instead of
`ISet<double?>`). If you are using the older schema
type definition (your classes derive from the `RealmObject` base class),
or you do not have nullability enabled, you will need to use the
[Required] attribute if the set
contains nullable reference types, such as `string` or `byte[]`.

## Set Types
The following code shows examples of set types:

```csharp
public partial class Inventory : IRealmObject
{
    // A Set can contain any Realm-supported type, including
    // objects that inherit from RealmObject
    public ISet<Plant> PlantSet { get; }

    public ISet<double> DoubleSet { get; }

    public ISet<int?> NullableIntsSet { get; }

    public ISet<string> RequiredStrings { get; }
}

```

## Usage Example
The following code shows how to create, write to, and read from Sets.

```csharp
var inventory = new Inventory();
inventory.PlantSet.Add(new Plant() { Name = "Prickly Pear" });
inventory.DoubleSet.Add(123.45);

realm.Write(() =>
{
    realm.Add<Inventory>(inventory);
});

// convert the Plant Set to an IQueryable and apply a filter
var pricklyPear = inventory.PlantSet.AsRealmQueryable()
    .Where(p => p.Name == "Prickly Pear");
// Alternatively, apply a filter directly on the Plant Set
var pricklyPearPlants = inventory.PlantSet
    .Filter("Name == 'Prickly Pear'");

// Find all Inventory items that have at least one value in their
// DoubleSet that is larger than 5
var moreThan100 = realm.All<Inventory>()
    .Filter("DoubleSet.@values > 100");

```

## Watching For Changes
You can use the [INotifyCollectionChanged.CollectionChanged](https://learn.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged.collectionchanged)
event on a set to watch for changes to the set, and the
[INotifyPropertyChanged.PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged)
event to watch for changes to specific properties in the set.

In the following code example, we have a class with an
`ISet<string>` property named `StringSet`. We set up event
handlers for both the `CollectionChanged` and `PropertyChanged` events:

```csharp
var stringSet = container.StringSet.AsRealmCollection();

stringSet.CollectionChanged += (sender, e) =>
{
    Console.WriteLine($"Set {sender} changed: {e.Action}");
};

stringSet.PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"Property changed on {sender}: {e.PropertyName}");
};

```
