# Quick Start - .NET SDK

This Quick Start demonstrates how to use Realm with the .NET MAUI SDK.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.107 or later)
- .NET MAUI workload: `dotnet workload install maui`
- Platform tooling for your target (Xcode for iOS/macOS, Android SDK for Android)

## Install Realm

Add the Realm NuGet package to your MAUI project:

```
dotnet add package Realm
```

Or via the NuGet Package Manager in Visual Studio: search for **Realm** and click **Install**.

> **NOTE:**
> With .NET MAUI's single-project model you only need to install Realm in the one shared project — it covers all target platforms automatically.

### Add the Realm Weaver to FodyWeavers.xml

> **NOTE:**
> You can skip this step if you were *not* already using [Fody](https://github.com/Fody/Fody) in your project. Visual Studio will generate a properly-configured `FodyWeavers.xml` file for you when you first build.

If your project was already using [Fody](https://github.com/Fody/Fody), add the Realm weaver manually:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Realm />
</Weavers>
```

```

## Import the SDK
Add the following line to the top of your source files to use the SDK:

```csharp
using Realms;
```


## Define Your Object Model
Your application's object model defines
the data that you can store within Realm.

> **IMPORTANT:**
> All Realm objects inherit from the
> `IRealmObject`, `IEmbeddedObject`, or `IAsymmetricObject`
> interface and must be declared `partial` classes.
>
> In versions of the .NET SDK older than 10.18.0, objects derive from
> `RealmObject`, `EmbeddedObject`, or `AsymmetricObject` base classes.
> This approach to Realm model definition is still supported, but does
> not include new features such as the nullability annotations. In a
> future SDK release, the base classes will become deprecated. You
> should use the interfaces for any new classes that you write and
> should consider migrating your existing classes.
>

The following code shows how to define an object model for an `Item` object. In
this example, we have marked the `Id` field as the Primary Key and
marked the `Status` property as optional. We've also chosen to use the
`MapTo` attribute.

```csharp
public partial class Item : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("assignee")]
    public string Assignee { get; set; }

    [MapTo("name")]
    public string? Name { get; set; }

    [MapTo("status")]
    public string? Status { get; set; }
}

```

## Open a Local Realm
In a local realm, you open a realm with either the
`Realm.GetInstance()` or `Realm.GetInstanceAsync()` method. Which
method you use depends entirely on if and how you are using
[asynchronous patterns](https://docs.microsoft.com/en-us/dotnet/csharp/async) in your app.

The following code shows how to use `GetInstance()`:

```csharp
var realm = Realm.GetInstance();

```

For more information, see: Open a Realm.

## Create, Read, Update, and Delete Objects
When creating or updating documents, all writes must happen in a
transaction.

The following code shows two methods for creating a new Realm object. In the
first example, we create the object first, and then add it to the realm within
`WriteAsync()`
method.

In the second example, we create the document within the `WriteAsync`
block, which returns a realm object we can further work with.

```csharp
var testItem = new Item
{
    Name = "Do this thing",
    Status = ItemStatus.Open.ToString(),
    Assignee = "Aimee"
};

await realm.WriteAsync(() =>
{
    realm.Add(testItem);

var testItem2 =
    await realm.WriteAsync(() =>
    {
        return realm.Add<Item>(new Item
        {
            Name = "Do this thing, too",
            Status = ItemStatus.InProgress.ToString(),
            Assignee = "Satya"
        });
    }
);

```

Upserting a document is the same as creating a new one, except you set the
optional `update` parameter to `true`.

In this example, we create a new `Item` object with a unique `Id`. We
then insert an item with the same id but a different `Name` value.
Because we have set the `update` parameter to `true`, the existing
record is updated with the new name.

```csharp
var id = ObjectId.GenerateNewId();

var item1 = new Item
{
    Id = id,
    Name = "Defibrillate the Master Oscillator",
    Assignee = "Aimee"
};

// Add a new person to the realm. Since nobody with the existing Id
// has been added yet, this person is added.
await realm.WriteAsync(() =>
{
    realm.Add(item1, update: true);
});

var item2 = new Item
{
    Id = id,
    Name = "Fluxify the Turbo Encabulator",
    Assignee = "Aimee"
};

// Based on the unique Id field, we have an existing person,
// but with a different name. When `update` is true, you overwrite
// the original entry.
await realm.WriteAsync(() =>
{
    realm.Add(item2, update: true);
});
// item1 now has a Name of "Fluxify the Turbo Encabulator"
// and item2 was not added as a new Item in the collection.

```

You also delete items within a `WriteAsync()` method. The following code shows
how to delete a single `Item` from the collection and how to delete an entire
collection:

```csharp
realm.Write(() =>
{
    realm.Remove(myItem);
});

realm.Write(() =>
{
    realm.RemoveAll<Item>();
});

```

The following pages cover each of these topics in more detail:

- [Create](crud/create.md)
- [Update](crud/update.md)
- [Delete](crud/delete.md)

## Finding, Filtering, and Sorting Documents
You search for documents with the Realm query engine, using either LINQ or
the Realm Query Language (RQL).

The following example finds all objects of type "Item":

```csharp
var allItems = realm.All<Item>();

```

You filter results using either LINQ or RQL.

This example uses LINQ to find all Items that have a status of "Open":

```csharp
var openItems = realm.All<Item>()
    .Where(i => i.Status == "Open");

```

You can also sort results using LINQ or RQL:

```csharp
var sortedItems = realm.All<Item>()
    .OrderBy(i => i.Status);

```

For details on querying, filtering, and sorting documents, see Filter and Sort Data.

## Watch for Changes
As document collections change, it is often important to make updates to the data
on a client app. You can watch a realm, collection, or object for changes with the
`SubscribeForNotifications()` method.

The following example shows how to add a notification handler on an entire realm
collection:

```csharp
// Observe realm notifications.
realm.RealmChanged += (sender, eventArgs) =>
{
    // The "sender" object is the realm that has changed.
    // "eventArgs" is reserved for future use.
    // ... update UI ...
};

```

You can also add notification handlers on collections and individual objects. For
more information, refer to React to Changes.
