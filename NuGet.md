<picture>
    <source srcset="./media/logo-dark.svg" media="(prefers-color-scheme: dark)" alt="realm by MongoDB">
    <img src="./media/logo.svg" alt="realm by MongoDB">
</picture>

Realm is a mobile database that runs directly on phones, tablets or wearables. It supports all major mobile and desktop operating systems, such as iOS, Android, UWP, macOS, Linux, and Windows. For a full list of supported platforms and their versions, check out the [Supported Platforms](https://docs.mongodb.com/realm/dotnet/#supported-platforms) sub-section in the documentation.

## Features

* **Mobile-first:** Realm is the first database built from the ground up to run directly inside phones, tablets, and wearables.
* **Simple:** Data is directly [exposed as objects](https://docs.mongodb.com/realm/dotnet/objects/) and [queryable by code](https://docs.mongodb.com/realm/dotnet/query-engine/), removing the need for ORM's riddled with performance & maintenance issues. Plus, we've worked hard to [keep our API down to just a few common classes](https://docs.mongodb.com/realm-sdks/dotnet/latest/): most of our users pick it up intuitively, getting simple apps up & running in minutes.
* **Modern:** Realm supports relationships, generics, vectorization and modern C# idioms.
* **Fast:** Realm is faster than even raw SQLite on common operations while maintaining an extremely rich feature set.
* **[Device Sync](https://www.mongodb.com/atlas/app-services/device-sync)**: Makes it simple to keep data in sync across users, devices, and your backend in real-time. Get started for free with [a template application](https://github.com/mongodb/template-app-maui-todo) and [create the cloud backend](http://mongodb.com/realm/register?utm_medium=github_atlas_CTA&utm_source=realm_dotnet_github).

## Getting Started

### Model definition

Define a persistable model by inheriting from `IRealmObject`. The Realm source generator will generate an implementation for most of the functionality, so you only need to specify the properties you want to persist:

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTimeOffset Birthday { get; set; }

    // You can define constructors as usual
    public Person(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
```

### Open a Realm file

Open a Realm instance by calling `Realm.GetInstance`:

```csharp
// You can provide a relative or an absolute path to the Realm file or let
// Realm use the default one.
var realm = Realm.GetInstance("people.realm");
```

### CRUD operations

Add, read, update, and remove objects by calling the corresponding API on the `Realm` instance:

```csharp
// Always mutate the Realm instance in a write transaction
realm.Write(() =>
{
    realm.Add(new Person("John", "Smith"));
});

var peopleWithJ = realm.All<Person>().Where(p => p.FirstName.StartsWith("J"));

// All Realm collections and objects are reactive and implement INotifyCollectionChanged/INotifyPropertyChanged

peopleWithJ.AsRealmCollection().CollectionChanged += (s, e) =>
{
    // React to notifications
};
```

For more examples, see the detailed instructions in our [User Guide](https://docs.mongodb.com/realm/dotnet/install/) to add Realm to your solution.

## Documentation

The documentation can be found at [docs.mongodb.com/realm/dotnet/](https://docs.mongodb.com/realm/dotnet/).
The API reference is located at [docs.mongodb.com/realm-sdks/dotnet/latest/](https://docs.mongodb.com/realm-sdks/dotnet/latest/).

## Getting Help

- **Need help with your code?**: Look for previous questions on the  [#realm tag](https://stackoverflow.com/questions/tagged/realm?sort=newest) — or [ask a new question](https://stackoverflow.com/questions/ask?tags=realm). You can also check out our [Community Forum](https://developer.mongodb.com/community/forums/tags/c/realm/9/realm-sdk) where general questions about how to do something can be discussed.
- **Have a bug to report?** [Open an issue](https://github.com/realm/realm-dotnet/issues/new). If possible, include the version of Realm, a full log, the Realm file, and a project that shows the issue.
- **Have a feature request?** [Open an issue](https://github.com/realm/realm-dotnet/issues/new). Tell us what the feature should do, and why you want the feature.
