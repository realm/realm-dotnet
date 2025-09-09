# React to Changes - .NET SDK
All Realm objects are **live objects**, which means they
automatically update whenever they're modified. Realm emits a
notification event whenever any property
changes.

Realm's notification system allows you to watch for and react to changes in your
data, independent of the writes that caused the changes. To observe changes, you
create a notification handler for a Realm, a
managed collection, or a Realm object
that you want to watch. You can then add your specific app logic related to the
change.

> **NOTE:**
> For information on binding data changes to the UI in your project, see Data Binding.
>

Realm emits three kinds of notifications:

- Realm notifications whenever a specific Realm commits a write transaction.
- Collection notifications whenever any a managed collection changes,
  such as inserts, updates, and deletes of objects in the collection.
- Object notifications whenever a specific Realm object changes.

> **NOTE:**
> Notifications only work when your realm regularly refreshes.
> In the Main or UI thread of your application, realm refreshes happen
> automatically. On background threads, you need to handle this yourself by either calling `Realm.Refresh()` or installing a
> [SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext)
> on the thread before opening the realm. The third-party library
> [Nito.AsyncEx.Context](https://www.nuget.org/packages/Nito.AsyncEx.Context/1.1.0)
> provides a `SynchronizationContext` implementation and a convenient
> API to install it.
>

## Register a Realm Change Listener
You can register a notification handler on an entire realm.
Realm invokes the notification handler whenever any write
transaction on that realm is committed.

The handler receives no specific information about the change. This is useful
when you want to know that there has been a change but do not need to know
specifically what change has occurred.

Suppose you are building a real-time collaborative app and you want to have a
counter that increases every time a change is made. In this scenario, you could
subscribe to the realm notification handler and add the code that controls
the indicator.

```csharp
// Observe realm notifications.
realm.RealmChanged += (sender, eventArgs) =>
{
    // The "sender" object is the realm that has changed.
    // "eventArgs" is reserved for future use.
    // ... update UI ...
};

```

## Watch for Collection Changes
You can watch for changes on a collection of realm objects and realm
collection properties on an object.

There are two ways to be notified about changes to a collection: register a
notification handler on the collection or handle the [CollectionChanged](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged.collectionchanged?view=net-6.0)
event.

You can register a notification handler on a specific
collection within a realm. The collection can be of realm objects
(like `realm.All<Person>()`) or a collection property on a
realm object (like `house.Owners`, where "Owners" is of type `IList`).

The handler receives a description of changes made to the collection since the
last notification. Unlike realm-wide notifications, collection notifications
contain detailed information about the change and provide the information you need
to manage a list or other view that represents the collection in the UI.

Realm emits an initial notification when a subscription is added. After the
initial notification, Realm delivers notifications asynchronously whenever a
write transaction adds, modifies, or removes objects in the collection.

### Notification ChangeSets
The notification contains a `ChangeSet` with 6 properties:

- `DeletedIndices` is an `int[]` that contains the indices of the
  objects that were deleted.
- `InsertedIndices` is an `int[]` that contains the indices of the
  objects that were inserted.
- `ModifiedIndices` is an `int[]` that contains the *old* indices of
  the objects that were modified. These indices indicate the position
  of the modified objects in the original collection before any
  deletions or insertions occurred.
- `NewModifiedIndices` is an `int[]` that represents the same entries
  as the `ModifiedIndices` property, but the indices represent the new
  locations in the collection after all changes have been accounted
  for.
- `IsCleared` is a boolean set to `true` when a collection has been
  cleared by calling the `Clear()` method.
- `Moved` is an array of `ChangeSet.Move` structs that contain the
  previous and new index of an object moved within the collection.

> **IMPORTANT:**
> In collection notification handlers, always apply changes
> in the following order:
>
> 1. deletions
> 2. insertions
> 3. modifications
>
> Handling insertions before deletions may result in unexpected behavior.
>

### Get Notified of All Collection Changes
To subscribe to collection notifications, call the `SubscribeForNotifications`
method. `SubscribeForNotifications` returns a subscription token which can be
disposed at any time to stop receiving notifications on the collection.

The following code shows how to observe a collection for changes.

```csharp
// Watch for collection notifications.
var subscriptionToken = realm.All<Dog>()
    .SubscribeForNotifications((sender, changes) =>
{
    if (changes == null)
    {
        // This is the case when the notification is called
        // for the first time.
        // Populate tableview/listview with all the items
        // from `collection`
        return;
    }

    // Handle individual changes
    foreach (var i in changes.DeletedIndices)
    {
        // ... handle deletions ...
    }

    foreach (var i in changes.InsertedIndices)
    {
        // ... handle insertions ...
    }

    foreach (var i in changes.NewModifiedIndices)
    {
        // ... handle modifications ...
    }

    if (changes.IsCleared)
    {
        // A special case if the collection has been cleared:
        // i.e., all items have been deleted by calling
        // the Clear() method.
    }
});

```

### Limit Notifications
The SDK provides also provides a `KeyPathsCollection`, which
provides a way to filter the fields that will trigger a notification. You pass
the `KeyPathsCollection` to the `SubscribeForNotifications` method.

The following code shows how to observe specific fields:

```csharp
var query = realm.All<Person>();
KeyPathsCollection kpc;

// Use one of these equivalent declarations to
// specify the fields you want to monitor for changes:

kpc = KeyPathsCollection.Of("Email", "Name");
kpc = new List<KeyPath> {"Email", "Name"};

// To get all notifications for top-level properties
// and 4 nested levels of properties, use the `Full`
// static value:

kpc = KeyPathsCollection.Full;

// To receive notifications for changes to the
// collection only and none of the properties,
// use the `Shallow` static value:

kpc = KeyPathsCollection.Shallow;

query.SubscribeForNotifications(notificationCallback, kpc);

```

## Unregister a Change Listener
To unregister a change listener, call `Dispose` on the token. The following code
shows how to do this:

```csharp
// Watch for collection notifications.
// Call Dispose() when you are done observing the
// collection.
var token = realm.All<Dog>()
    .SubscribeForNotifications((sender, changes) =>
    {
        // etc.
    });
// When you no longer want to receive notifications:
token.Dispose();

```

### Handle the CollectionChanged Event
Every Realm collection implements `INotifyCollectionChanged`, which allows you
to use a collection directly in data-binding scenarios. Because collections implement
`INotifyCollectionChanged`, another approach to monitoring collection changes
is to handle the [CollectionChanged](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged.collectionchanged)
event and check for the type of [NotifyCollectionChangedAction](https://learn.microsoft.com/en-us/dotnet/api/system.collections.specialized.notifycollectionchangedaction).

> **IMPORTANT:**
> The `CollectionChanged` event handler does not provide the same level of
> detail about changes as `SubscribeForNotifications` does.
>

The following code shows you how to implement the `CollectionChanged` event
handler:

```csharp
{
    // Subscribe to a query
    realm.All<Dog>().AsRealmCollection().CollectionChanged +=
        HandleCollectionChanged;

    // Subscribe to a property collection
    gracie.Owners.AsRealmCollection().CollectionChanged +=
        HandleCollectionChanged;
   ...
}

private void HandleCollectionChanged(object? sender,
    NotifyCollectionChangedEventArgs e)
{
    // Use e.Action to get the
    // NotifyCollectionChangedAction type.
    if (e.Action == NotifyCollectionChangedAction.Add)
    {
        // etc.
    }
}

```

## Register an Object Change Listener
You can register a notification handler on a specific object
within a realm so that the SDK notifies you when any of the object's
properties change. The handler receives information about which field
has changed. With the field name, you can get the new value.

The following code shows how to observe an object for
changes.

```csharp
    var artist = realm.All<Person>()
        .FirstOrDefault(p => p.Name == "Elvis Presley");

    artist.PropertyChanged += (sender, eventArgs) =>
    {
        var changedProperty = eventArgs.PropertyName!;

        Debug.WriteLine(
            $@"New value set for 'artist':
            '{changedProperty}' is now {artist.GetType()
            .GetProperty(changedProperty).GetValue(artist)}");
    };

    realm.Write(() =>
    {
        artist.Name = "Elvis Costello";
    });

    realm.Refresh();
}

```

## Unregister a Change Listener
When you no longer want to receive notifications on a change listener, you
unregister the handler. The code is the same for both a collection of realm
objects and a collection property. The following code shows how to unregister a
change listener on both:

```csharp
// Unsubscribe from notifications on a
// realm listener
realm.RealmChanged -= OnRealmChanged;

// Unsubscribe from notifications on a
// collection of realm objects
realm.All<Item>().AsRealmCollection()
    .CollectionChanged -= OnItemsChangedHandler;

// Unsubscribe from notifications on a
// collection property
items.AsRealmCollection().CollectionChanged -= OnItemsChangedHandler;

```

## Change Notification Limits
Changes in nested documents deeper than four levels down do not trigger
change notifications.

If you have a data structure where you need to listen for changes five
levels down or deeper, workarounds include:

- Refactor the schema to reduce nesting.
- Add something like "push-to-refresh" to enable users to manually refresh data.
