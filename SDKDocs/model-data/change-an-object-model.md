# Change an Object Model - .NET SDK
## Overview
A **migration** transforms an existing realm and its objects from its
current Realm Schema version to a newer one.
Application data models typically change over time to accommodate new
requirements and features. Migrations give you the flexibility to
automatically update your existing application data whenever a client
application upgrades to a newer version.

Consider the following example, in which we have a `RealmObject`
called "Person":

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string FullName { get; set; }
    public int Age { get; set; }
}

```

Suppose we now want to split up the `FullName` property into two separate
properties, `FirstName` and `LastName`:

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

```

At this point, there is a mismatch between the model and any saved data, and
an exception will be thrown when you try to open the realm.

## Schema Version
The **schema version** identifies the state of a Realm Schema at some point in time. Realm tracks the schema
version of each realm and uses it to map the objects in each realm
to the correct schema.

Schema versions are integers that you may include
in the realm configuration when you open a realm. If a client
application does not specify a version number when it opens a realm then
the realm defaults to version `0`.

> **IMPORTANT:**
> Migrations must update a realm to a
higher schema version. Realm will throw an error if a client
application opens a realm with a schema version that is lower than
the realm's current version or if the specified schema version is the
same as the realm's current version but includes different
object schemas.
>

## Migrate a Schema
The following examples demonstrate how to add, delete, and modify
properties in a schema. First, make the required schema change. Then,
create a corresponding migration function to move data from the original schema
to the updated schema.

> **NOTE:**
> Assume that each schema change in this example occurs after
an application has used each version for some amount of time. New
schema version numbers only apply once you open the realm
with an updated schema and explicitly specify the new version
number, so in order to get to version 3, you would first need to
open the app with versions 1 and 2.
>

A realm using schema version `1` has a `Person` object type:

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string FirstName { get; set; }
    public int Age { get; set; }
}

```

## Add a Property
The following example adds a `LastName` property to the
original Person schema:

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

```

## Delete a Property
The following example uses a combined
`FullName` property instead of the separate `FirstName` and
`LastName` property in the original Person schema:

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string FullName { get; set; }
    public int Age { get; set; }
}

```

## Modify a Property Type or Rename a Property
The following example modifies the `Age` property in the
original Person schema by
renaming it to `Birthday` and changing the type to `DateTimeOffset`:

```csharp
public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string FullName { get; set; }
    public DateTimeOffset Birthday { get; set; }
}

```

## Migration Functions
> **TIP:**
> When developing or debugging your application, you may prefer to delete
the realm instead of migrating it. Use the
`ShouldDeleteIfMigrationNeeded`
flag to delete the database automatically when a schema mismatch would
require a migration.
>
> Never release an app to production with this flag set to `true`.
>

To migrate the realm to conform to the updated `Person` schema,
set the realm's schema version to
`4` and define a migration function to set the value of
`FullName` based on the existing `FirstName` and `LastName`
properties and the value of `Birthday` based on `Age`:

```csharp
var config = new RealmConfiguration
{
    SchemaVersion = 4,
    MigrationCallback = (migration, oldSchemaVersion) =>
    {
        var oldVersionPeople = migration.OldRealm.DynamicApi.All("Person");
        var newVersionPeople = migration.NewRealm.All<Person>();

        // Migrate Person objects
        for (var i = 0; i < newVersionPeople.Count(); i++)
        {
            var oldVersionPerson = oldVersionPeople.ElementAt(i);
            var newVersionPerson = newVersionPeople.ElementAt(i);

            // Changes from version 1 to 2 (adding LastName) will
            // occur automatically when Realm detects the change

            // Migrate Person from version 2 to 3:
            // Replace FirstName and LastName with FullName
            // LastName doesn't exist in version 1
            var firstName = oldVersionPerson.DynamicApi.Get<string>("FirstName");
            var lastName = oldVersionPerson.DynamicApi.Get<string>("LastName");

            if (oldSchemaVersion < 2)
            {
                newVersionPerson.FullName = firstName;
            }
            else if (oldSchemaVersion < 3)
            {
                newVersionPerson.FullName = $"{firstName} {lastName}";
            }

            // Migrate Person from version 3 to 4: replace Age with Birthday
            if (oldSchemaVersion < 4)
            {
                var birthYear =
                    DateTimeOffset.UtcNow.Year - oldVersionPerson.DynamicApi.Get<int>("Age");
                newVersionPerson.Birthday =
                    new DateTimeOffset(birthYear, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }
        }
    }
};
var realm = Realm.GetInstance(config);

```
