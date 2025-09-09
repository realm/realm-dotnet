# Lists - .NET SDK
## Overview
A Realm list implements
[IList<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=net-5.0),
and contains zero or more instances of a
Realm type. Like a C# `List`,
a Realm collection is homogenous (all objects in a collection are of
the same type).

## Lists
Realm objects can contain lists of any supported data type.
You create a collection by defining a getter-only property of type
[IList<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=net-5.0),
where `T` can be any data type (except other collections). A list of realm
objects represents a to-many relationship
between two Realm types, the containing class and the type in the list.

Lists are mutable: within a write transaction, you can add and remove elements
on a list.

### Lists and Nullability
Deleting an object from the database will remove it from any lists
where it existed. Therefore, a list of objects will never contain deleted objects.
However, lists of primitive types can contain null values. If you do not
want to allow null values in a list, then either use non-nullable types in
the list declaration (for example, use `IList<double>` instead of
`IList<double?>`). If you are using the older schema
type definition (your classes derive from the `RealmObject` base class),
or you do not have nullability enabled, use the
[Required] attribute if the list
contains nullable reference types, such as `string` or `byte[]`.

For more information, refer to Required and Optional Properties.

## Watching For Changes
You can use the [INotifyCollectionChanged.CollectionChanged](https://learn.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged.collectionchanged)
event on a list to watch for changes to the list, and the
[INotifyPropertyChanged.PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged)
event to watch for changes to specific properties in the list.

In the following code example, we have a class with an
`IList<string>` property named `StringList`. We set up event
handlers for both the `CollectionChanged` and `PropertyChanged` events:

```csharp
var list = container.StringList.AsRealmCollection();

list.CollectionChanged += (sender, e) =>
{
    Console.WriteLine($"List {sender} changed: {e.Action}");
};

list.PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"Property changed on {sender}: {e.PropertyName}");
};

```
