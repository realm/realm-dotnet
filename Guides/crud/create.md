# Create Data - .NET SDK
## Create a Realm Object
When creating or updating documents, all writes must happen in a
transaction.

The following code shows two methods for creating a new Realm object. In the
first example, we create the object first, and then add it to the realm within
`WriteAsync()`
method. In the second example, we create the document within the `WriteAsync`
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
});

// Or 

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

## Upsert a Realm Object
Upserting a document is the same as creating a new one, except you set the
optional `update` parameter to `true`. In this example, we create a new
`Item` object with a unique `Id`. We then insert an item with the
same id but a different `Name` value. Because we have set the `update`
parameter to `true`, the existing record is updated with the new name.

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
