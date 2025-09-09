# Configure & Open a Realm - .NET SDK
A **realm** is a set of related objects that conform to a pre-defined
schema. Realms may contain more than one type of data as long as a
schema exists for each type.

Every realm stores data in a separate realm file that contains a binary
encoding of each object in the realm.

## Realm Files
Realm stores a binary encoded version of every object and type in a
realm in a single `.realm` file. The file is located at the path that
you define in the
`RealmConfiguration`
object when you open the realm, or in the default path if you do not
specify a path.

The default realm file is named `default.realm` and is located where the OS
stores app-specific data.

> **TIP:**
> Every production application should implement `ShouldCompactOnLaunch`
to periodically reduce the realm file size. For more information
about compacting a realm, see: Reduce Realm File Size - .NET SDK.
>

### Auxiliary Realm Files
Realm creates additional files for each realm:

- **realm files**, suffixed with "realm", e.g. default.realm:
contain object data.
- **lock files**, suffixed with "lock", e.g. default.realm.lock:
keep track of which versions of data in a realm are
actively in use. This prevents realm from reclaiming storage space
that is still used by a client application.
- **note files**, suffixed with "note", e.g. default.realm.note:
enable inter-thread and inter-process notifications.
- **management files**, suffixed with "management", e.g. default.realm.management:
internal state management.

Deleting these files has important implications.
For more information about deleting `.realm` or auxiliary files, see:
Delete a Realm.

## Open a Realm
The following sections describe how to open a realm file for local
use.

### Local Realms
When opening a local realm, pass a
`RealmConfiguration`
object to either `GetInstanceAsync()` or `GetInstance()`. The following example
creates a `RealmConfiguration` object with a local file path, sets the
`IsReadOnly` property to `true`, and then opens a local realm  with that
configuration information:

```csharp
var config = new RealmConfiguration(pathToDb + "my.realm")
{
    IsReadOnly = true,
};
Realm localRealm;
try
{
    localRealm = Realm.GetInstance(config);
}
catch (RealmFileAccessErrorException ex)
{
    Console.WriteLine($@"Error creating or opening the
        realm file. {ex.Message}");
}

```

### In-Memory Realms
With an
`InMemoryConfiguration`
object, you can create a realm that runs entirely in memory (that is, without
the data written to disk.) The following example shows how to do this:

```csharp
var config = new InMemoryConfiguration("some-identifier");
var realm = Realm.GetInstance(config);

```

In-memory realms might still use disk space if memory is running low, but all
files created by an in-memory realm will be deleted when the realm is
closed. When creating an in-memory realm, the identifier must be unique to all
realms, including both in-memory and persisted realms.

> **IMPORTANT:**
> When an in-memory realm is disposed or garbage-collected, the data is lost.
To keep an in-memory realm "alive" throughout your app's execution, be sure
to hold a reference to realm.
>

## Scoping the Realm
The realm instance implements `IDisposable` to ensure native resources are
freed up. You should dispose of a realm object immediately after use, especially
on background threads. The simplest way to do this is by declaring the realm
object with a `using` statement, or wrapping the code that interacts with a
realm in a `using (...)` statement:

```csharp
config = new RealmConfiguration(pathToDb);
using (var realm = Realm.GetInstance(config))
{
    var allItems = realm.All<Item>();
}

```

If you require a realm object to be shared outside of a single method, be sure
to manage its state by calling the
`Dispose()` method:

```csharp
realm.Dispose();

```

> **NOTE:**
> As a general rule, you should dispose of the realm only on background threads,
because disposing of a realm invalidates all objects associated with that
instance. If you are data binding the realm objects on the main thread,
for example, you should not call `Dispose()`.
>

## Class Subsets
By default, all `RealmObject` classes are stored in a realm. In some
scenarios, you may want to limit the classes that get stored, which you can do
with the
`Schema`
property of the `RealmConfiguration` object. The following code demonstrates
how you specify two classes you want stored in the realm:

```csharp
var config = new RealmConfiguration()
{
    Schema = new Type[]
    {
        typeof(AClassWorthStoring),
        typeof(AnotherClassWorthStoring)
    }
};

```
