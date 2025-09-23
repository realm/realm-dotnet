# Read Data - .NET SDK
You can read back the data that you have stored in Realm by finding,
filtering, and sorting objects.

## Read from Realm
You read from a realm with LINQ queries.

> **NOTE:**
> The examples on this page use the data model of a project
management app that has two Realm object types: `Project`
and `Item`. A `Project` has zero or more `Items`:
>
> ```csharp
> public partial class Items : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
>     public string Name { get; set; }
>     public string Assignee { get; set; }
>     public bool IsComplete { get; set; }
>     public int Priority { get; set; }
>     public int ProgressMinutes { get; set; }
> }
>
> public partial class Project : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
>     public string Name { get; set; }
>     public IList<Items> Items { get; }
> }
>
> ```
>

## Query All Objects of a Given Type
To read all objects of a certain type in a realm, call `realm.All<T>`, where `T`
is the realm object type. You can then use the returned results collection to further
filter and sort the results.

> **EXAMPLE:**
> In order to access all Projects and Items, use
the following syntax:
>
> ```csharp
> var projects = realm.All<Project>();
> var items = realm.All<Items>();
>
> ```
>

## Find a Specific Object by Primary Key
You can find a specific item by its primary key using the
`Find`
method. The following example finds a single Project:

```csharp
var myProject = realm.Find<Project>(projectId);

```
