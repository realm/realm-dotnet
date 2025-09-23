# Update Data - .NET SDK
Updates are the same as creating a new document. When updating documents, all
writes must happen in a transaction.

## Modify an Object
The following example shows how to modify an existing object. In this example,
we are updating the `Name` and `Age` properties of a `Dog` object:

```csharp
var dog = realm.All<Dog>().First();
realm.WriteAsync(() =>
{
    dog.Name = "Wolfie";
    dog.Age += 1;
});

```

### Upserts
An upsert allows you to create or modify a document without knowing
if the document already exists. For more information, see Upsert a Realm Object.

## Update a Collection
The following code demonstrates how to update a
collection.

```csharp
realm.Write(() =>
{
    // Create someone to take care of some dogs.
    var ali = new Person { Id = id, Name = "Ali" };
    realm.Add(ali);
    // Find dogs younger than 2.
    var puppies = realm.All<Dog>().Where(dog => dog.Age < 2);
    // Loop through one by one to update.
    foreach (var puppy in puppies)
    {
        // Add Ali to the list of Owners for each puppy
        puppy.Owners.Add(ali);
    }
});

```

> **NOTE:**
> Because realm uses implicit inverse
relationships between the
Dog's `Owners` property and the Person's `Dogs`
property, Realm automatically updates Ali's
list of dogs at the same time we update each dog's `Owners` list.
>
