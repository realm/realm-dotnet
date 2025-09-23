# Object Models & Schemas - .NET SDK
## Object Types & Schemas
Realm applications model data as objects composed of property-value pairs
that each contain one or more primitive data types or other Realm
objects. Realm objects are essentially the same
as regular objects, but they inherit from
`RealmObject` or
`EmbeddedObject` and include
additional features like
real-time updating data views and reactive
change event handlers.

Every Realm object has an *object type* that refers to the object's
class. Objects of the same type share an object schema that defines the properties and relationships of those
objects.

### Schemas
In C#, you typically define object schemas by using the C# class declarations.
When Realm is initialized, it discovers the
Realm objects defined in all assemblies that have been loaded and
generates schemas accordingly. This is the simplest approach to defining a
schema, and is generally the least error-prone. However, this approach includes
all loaded Realm objects, and there may be cases where you only want
to use a subset of classes, or to
customize Realm object schemas. To do this, you can
programmatically define a schema.

> **NOTE:**
> .NET does not load an assembly until you reference a class in it, so if you
define your object models in one assembly and instantiate Realm
in another, be sure to call a method in the assembly that contains the object
models *before* initialization. Otherwise, Realm will not discover
the objects when it first loads.
>

## Working with Realm Objects
The following code block shows an object schema that describes a Dog.
Every Dog object must include a `Name` and may
optionally include the dog's `Age`, `Breed` and a list of people that
represents the dog's `Owners`.

```csharp
public partial class Dog : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string Name { get; set; }

    public int Age { get; set; }
    public string Breed { get; set; }
    public IList<Person> Owners { get; }
}

public partial class Person : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    public string Name { get; set; }
    // etc...

    /* To add items to the IList<T>:

    var dog = new Dog();
    var caleb = new Person { Name = "Caleb" };
    dog.Owners.Add(caleb);

    */
}

```

> **NOTE:**
> To define a collection of objects within an object, use an `IList<T>` with
only a getter. You do not need to initialize it in the constructor, as realm
will generate a collection instance the first time the property is accessed.
>

> **NOTE:**
> The CRUD section provides examples of creating, reading,
updating, filtering, and deleting Realm objects.
>
