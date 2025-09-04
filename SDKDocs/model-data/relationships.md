# Relationships - .NET SDK
Realm allows you to define explicit relationships between the types of
objects in an App. A relationship is an object property that references
another Realm object rather than one of the primitive data types. You
define relationships by setting the property type to another Realm class.

> **IMPORTANT:**
> All Realm objects inherit from the
`IRealmObject`,
`IEmbeddedObject`, or
`IAsymmetricObject`
interface and must be declared `partial` classes.
>
> In versions of the .NET SDK older than 10.18.0, objects derive from
`RealmObject`,
`EmbeddedObject`, or
`AsymmetricObject`
base classes. This approach to Realm model definition is still supported, but
does not include new features such as the nullability annotations. In a future SDK release, the
base classes will become deprecated. You should use the interfaces for any
new classes that you write and should consider migrating your existing
classes.
>

Relationships are direct references to other objects in a realm, which
means that you don't need bridge tables or explicit joins to define a
relationship like you would in a relational database. Instead, you can
access related objects by reading and writing to the property that
defines the relationship. Realm executes read operations
lazily as they come in, so querying a relationship is just as performant
as reading a regular property.

## Define a Relationship Property
There are three primary types of relationships between objects:

- To-One Relationship
- To-Many Relationship
- Inverse Relationship

### To-One Relationship
A **to-one** relationship means that an object is related in a specific
way to no more than one other object. You define a to-one relationship
for an object type in its object schema by
specifying a property where the type is the related Realm object type.

> **EXAMPLE:**
> An application could use the following object schemas to indicate
that a Person may or may not own a single Dog by including it in its
`dog` property:
>
> ```csharp
> public partial class Person : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId Id { get; set; }
>     public string Name { get; set; }
>     public DateTimeOffset Birthdate { get; set; }
>     public Dog? Dog { get; set; }
> }
>
> public partial class Dog : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId Id { get; set; }
>     public string Name { get; set; }
>     public int Age { get; set; }
>     public string Breed { get; set; } = String.Empty;
> }
>
> ```
>

To query a direct relationship, you can use LINQ syntax.
See the following example for how to query a one-to-one relationship:

```csharp
var fidosPerson = realm.All<Person>().FirstOrDefault(p => p.Dog == dog);

```

### To-Many Relationship
A **to-many** relationship means that an object is related in a specific
way to multiple objects. You define a to-many relationship for an object
type by specifying a property where the type is an `IList<T>` of the related
Realm object type. Define the `IList<T>` with only a getter. You do not need
to initialize it in the constructor, as Realm will generate a collection
instance the first time the property is accessed:

```csharp
// To add items to the IList<T>:
var person = new Person();
person.Dogs.Add(new Dog
{
    Name = "Caleb",
    Age = 7,
    Breed = "mutt"
});

```

> **EXAMPLE:**
> An application could use the following object schemas to indicate
that a Person may own multiple Dogs by including them in its `dog`
property:
>
> ```csharp
> public partial class Person : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId Id { get; set; }
>     public string Name { get; set; }
>     public DateTimeOffset Birthdate { get; set; }
>     public IList<Dog> Dogs { get; }
> }
>
> public partial class Dog : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId Id { get; set; }
>     public string Name { get; set; }
>     public int Age { get; set; }
>     public string Breed { get; set; } = String.Empty;
> }
>
> ```
>
> To see the to-many relationship of Person to Dog, you query for the
Person and get that person's Dogs:
>
> ```csharp
> var katieAndHerDogs = realm.All<Person>().
>     Where(p => p.Name == "Katie")
>     .FirstOrDefault();
>
> ```
>

### Inverse Relationship
An **inverse relationship** links an object back to any other objects that refer
to it in a defined to-one or to-many relationship. Relationship definitions are
unidirectional, so you must explicitly define a property in the object's model
as an inverse relationship.

For example, the to-many relationship "a User has many Items" does not
automatically create the inverse relationship "an Item belongs to one User". If you
don't specify the inverse relationship in the object model, you need to
run a separate query to look up the user that is assigned to a given item.

To define the inverse relationship, define a getter-only `IQueryable<T>` property
in your object model, where `T` is the source type of the relationship, and
then annotate this property with a
`[Backlink(sourceProperty)]`
attribute, where "sourceProperty" is the name of the property on the other
side of the relationship. The following example shows how to do this with the
"User has many Items" scenario:

```csharp
public partial class User : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public string Name { get; set; }

    [Backlink(nameof(Item.Assignee))]
    public IQueryable<Item> Items { get; }
}

public partial class Item : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public string Text { get; set; }

    public User? Assignee { get; set; }
}

```

In this example, note that:

- The Item object's `Assignee` property is a User object.
- The User object's `Items` property inverts the relationship and
refers to all Item objects that contain this specific User in their
`Assignee` property.

This, then, allows us to query the Item collection to get all Items assigned to
a specific User.

To query the inverse relationship, you cannot use Linq. Instead, pass a string
predicate. The following example shows how you could find all Users who have Items
that contain the word "oscillator":

```csharp
var oscillatorAssignees = realm.All<User>()
    .Filter("Items.Text CONTAINS 'oscillator'").ToList();

foreach (User u in oscillatorAssignees)
{
    Console.WriteLine(u.Name);
}

```

## Summary
- A **relationship** is an object property that allows an object to
reference other objects of the same or another object type.
- Relationships are direct references. You can access related objects
directly through a relationship property without writing any type of join.
- Realm supports to-one, to-many, and inverse relationships.
