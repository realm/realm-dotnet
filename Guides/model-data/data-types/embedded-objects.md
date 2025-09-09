# Embedded Objects - .NET SDK
## Overview
An embedded object is a special type of Realm object that models complex data about a specific object.
Embedded objects are similar to relationships, but they provide additional constraints and
map more naturally to the denormalized document model.

Realm enforces unique ownership constraints that treat each embedded
object as nested data inside of a single, specific parent object. An
embedded object inherits the lifecycle of its parent object and cannot
exist as an independent Realm object. Realm automatically deletes
embedded objects if their parent object is deleted or when overwritten
by a new embedded object instance.

> **NOTE:**
> When you delete a Realm object, Realm automatically deletes any
embedded objects referenced by that object. Any objects that your
application must persist after the deletion of their parent object
should use relationships
instead.
>

## Embedded Object Data Models
You can define embedded object types using either Realm object models or a
server-side document schema. Embedded object types are reusable and
[composable](https://en.wikipedia.org/wiki/Composability) . You can use the same embedded object
type in multiple parent object types and you can embed objects inside of other
embedded objects.

> **IMPORTANT:**
> Embedded objects cannot have a primary key.
>

### Realm Object Models
To define an embedded object, implement the `IEmbeddedObject` interface.
You can reference an embedded object type from parent object types in
the same way as you would define a relationship.

Consider the following example where the `Address` is an Embedded Object. Both
the `Contact` and the `Business` classes reference the `Address` as an
embedded object:

```csharp
public partial class Address : IEmbeddedObject
{
    [MapTo("street")]
    public string Street { get; set; }

    [MapTo("city")]
    public string City { get; set; }

    [MapTo("country")]
    public string Country { get; set; }

    [MapTo("postalCode")]
    public string PostalCode { get; set; }

}
public partial class Contact : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("_partition")]
    public string Partition { get; set; }

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("address")]
    public Address? Address { get; set; } // embed a single address

}
public partial class Business : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("_partition")]
    public string Partition { get; set; }

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("addresses")]
    public IList<Address> Addresses { get; }
}

```

### JSON Schema
Embedded objects map to embedded documents in the parent type's schema.

```json
{
  "title": "Contact",
  "bsonType": "object",
  "required": ["_id"],
  "properties": {
    "_id": { "bsonType": "objectId" },
    "name": { "bsonType": "string" },
    "address": {
      "title": "Address",
      "bsonType": "object",
      "properties": {
        "street": { "bsonType": "string" },
        "city": { "bsonType": "string" },
        "country": { "bsonType": "string" },
        "postalCode": { "bsonType": "string" }
      }
    }
  }
}
```

```json
{
  "title": "Business",
  "bsonType": "object",
  "required": ["_id", "name"],
  "properties": {
    "_id": { "bsonType": "objectId" },
    "name": { "bsonType": "string" },
    "addresses": {
      "bsonType": "array",
      "items": {
        "title": "Address",
        "bsonType": "object",
        "properties": {
          "street": { "bsonType": "string" },
          "city": { "bsonType": "string" },
          "country": { "bsonType": "string" },
          "postalCode": { "bsonType": "string" }
        }
      }
    }
  }
}
```

## Read and Write Embedded Objects
### Create an Embedded Object
To create an embedded object, assign an instance of the embedded object
to a parent object's property:

```csharp
var address = new Address() // Create an Address
{
    Street = "123 Fake St.",
    City = "Springfield",
    Country = "USA",
    PostalCode = "90710"
};

var contact = new Contact() // Create a Contact
{
    Name = "Nick Riviera",
    Address = address // Embed the Address Object
};

realm.Write(() =>
{
    realm.Add(contact);
});

```

### Update an Embedded Object Property
To update a property in an embedded object, modify the property in a
write transaction:

```csharp
var resultContact = realm.All<Contact>() // Find the First Contact (Sorted By Name)
    .OrderBy(c => c.Name)
    .FirstOrDefault();

// Update the Result Contact's Embedded Address Object's Properties
realm.Write(() =>
{
    resultContact.Address.Street = "Hollywood Upstairs Medical College";
    resultContact.Address.City = "Los Angeles";
    resultContact.Address.PostalCode = "90210";
});

```

### Overwrite an Embedded Object
To overwrite an embedded object, reassign the embedded object property
of a party to a new instance in a write transaction:

```csharp
var oldContact = realm.All<Contact>() // Find the first contact
.OrderBy(c => c.Name)
.FirstOrDefault();

var newAddress = new Address() // Create an Address
{
    Street = "100 Main Street",
    City = "Los Angeles",
    Country = "USA",
    PostalCode = "90210"
};

realm.Write(() =>
{
    oldContact.Address = newAddress;
});

```

### Query a Collection on Embedded Object Properties
Use dot notation to filter or sort a collection of objects based on an embedded object
property value:

> **NOTE:**
> It is not possible to query embedded objects directly. Instead,
access embedded objects through a query for the parent object type.
>

```csharp
// Find All Contacts with an Address of "Los Angeles"
var losAngelesContacts = realm.All<Contact>()
    .Filter("address.city == 'Los Angeles'");

foreach (var contact in losAngelesContacts)
{
    Console.WriteLine("Los Angeles Contact:");
    Console.WriteLine(contact.Name);
    Console.WriteLine(contact.Address.Street);
}

```
