# Advanced Data Modeling with Realm .NET

Realm's intuitive data model approach means that in most cases, you don't even think of Realm models as entities. You just declare your POCOs, have them inherit from `RealmObject`, and you're done. Now you have persistable models, with `INotifyPropertyChanged` capabilities all wired up, that are also "live"—i.e., every time you access a property, you get the latest state and not some snapshot from who knows how long ago. This is great and most of our users absolutely love the simplicity. Still, there are some use cases where being aware that you're working with a database can really bring your data models to the next level. In this blog post, we'll evaluate three techniques you can apply to make your models fit your needs even better.

## Constructor Validation

One of the core requirements of Realm is that all models need to have a parameterless constructor. This is needed because Realm needs to be able to instantiate an object without figuring out what arguments to pass to the constructor. What not many people know is that you can make this parameterless constructor private to communicate expectations  to your callers. This means that if you have a `Person` class where you absolutely expect that a `Name` is provided upon object creation, you can have a public constructor with a `name` argument and a private parameterless one for use by Realm:

```csharp
class Person : RealmObject
{
    public string Name { get; set; }

    public Person(string name)
    {
        ValidateName(name);

        Name = name;
    }

    // This is used by Realm, even though it's private
    private Person()
    {
    }
}
```

And I know what some of you may be thinking: "Oh no! 😱 Does that mean Realm uses the suuuuper slow reflection to create object instances?" Fortunately, the answer is no. Instead, at compile time, Realm injects a nested helper class in each model that has a `CreateInstance` method. Since the helper class is nested in the model classes, it has access to private members and is thus able to invoke the private constructor.

## Property Access Modifiers

Similar to the point above, another relatively unknown feature of Realm is that persisted properties don't need to be public. You can either have the entire property be private or just one of the accessors. This synergizes nicely with the private constructor technique that we mentioned above. If you expose a constructor that explicitly validates the person's name, it would be fairly annoying to do all that work and have some code accidentally set the property to `null` the very next line. So it would make sense to make the setter of the name property above private:

```csharp
class Person : RealmObject
{
    public string Name { get; private set; }

    public Person(string name)
    {
        // ...
    }
}
```

That way, you're communicating clearly to the class consumers that they need to provide the name at object creation time and that it can't be changed later. A very common use case here is to make the `Id` setter private and generate a random `Id` at object creation time:

```csharp
class Transaction : RealmObject
{
    public Guid Id { get; private set; } = Guid.NewGuid();
}
```

Sometimes, it makes sense to make the entire property private—typically, when you want to expose a different public property that wraps it. If we go back to our `Person` and `Name` example, perhaps we want to allow changing the name, but we want to still validate the new name before we persist it. Then, we create a private autoimplemented property that Realm will use for persistence, and a public one that does the validation:

```csharp
class Person : RealmObject
{
    [MapTo("Name")]
    private string _Name { get; set; }

    public string Name
    {
        get => _Name;
        set
        {
            ValidateName(value);
            _Name = value;
        }
    }
}
```

This is quite neat as it makes the public API of your model safe, while preserving its persistability. Of note is the `MapTo` attribute applied to `_Name`. It is not strictly necessary. I just added it to avoid having ugly column names in the database. You can use it or not use it.  It's totally up to you. One thing to note when utilizing this technique is that Realm is completely unaware of the relationship between `Name` and `_Name`. This has two implications. 1) Notifications will be emitted for `_Name` only, and 2) You can't use LINQ queries to filter `Person` objects by name. Let's see how we can mitigate both:

For notifications, we can override `OnPropertyChanged` and raise a notification for `Name` whenever `_Name` changes:

```csharp
class Person : RealmObject
{
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(_Name))
        {
            RaisePropertyChanged(nameof(Name));
        }
    }
}
```

The code is fairly straightforward. `OnPropertyChanged` will be invoked whenever any property on the object changes and we just re-raise it for the related `Name` property. Note that, as an optimization, `OnPropertyChanged` will only be invoked if there are subscribers to the `PropertyChanged` event. So if you're testing this out and don't see the code get executed, make sure you added a subscriber first.

The situation with queries is slightly harder to work around. The main issue is that because the property is private, you can't use it in a LINQ query—e.g., `realm.All<Person>().Where(p => p._Name == "Peter")` will result in a compile-time error. On the other hand, because Realm doesn't know that `Name` is tied to `_Name`, you can't use `p.Name == "Peter"` either. You can still use the string-based queries, though. Just remember to use the name that Realm knows about—i.e., the string argument of `MapTo` if you remapped the property name or the internal property (`_Name`) if you didn't:

```csharp
// _Name is mapped to 'Name' which is what we use here
var peters = realm.All<Person>().Filter("Name == 'Peter'");
```

## Using Unpersistable Data Types

Realm has a wide variety of supported data types—most primitive types in the Base Class Library (BCL), as  well as advanced collections, such as sets and dictionaries. But sometimes, you'll come across a data type that Realm can't store yet, the most obvious example being enums. In such cases, you can build on top of the previous technique to expose enum properties in your models and have them be persisted as one of the supported data types:

```csharp
enum TransactionState
{
    Pending,
    Settled,
    Error
}

class Transaction : RealmObject
{
    private string _State { get; set; }

    public TransactionState State
    {
        get => Enum.Parse<TransactionState>(_State);
        set => _State = value.ToString();
    }
}
```

Using this technique, you can persist many other types, as long as they can be converted losslessly to a persistable primitive type. In this case, we chose `string`, but we could have just as easily used integer. The string representation takes a bit more memory but is also more explicit and less error prone—e.g., if you rearrange the enum members, the data will still be consistent.

All that is pretty cool, but we can take it up a notch. By building on top of this idea, we can also devise a strategy for representing complex data types, such as `Vector3` in a Unity game or a `GeoCoordinate` in a location-aware app. To do so, we'll take advantage of embedded objects—a Realm concept that represents a complex data structure that is owned entirely by its parent. Embedded objects are a great fit for this use case because we want to have a strict 1:1 relationship and we want to make sure that deleting the parent also cleans up the embedded objects it owns. Let's see this in action:

```csharp
class Vector3Model : EmbeddedObject
{
    // Casing of the properties here is unusual for C#,
    // but consistent with the Unity casing.
    private float x { get; set; }
    private float y { get; set; }
    private float z { get; set; }

    public Vector3Model(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    private Vector3Model()
    {
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

class Powerup : RealmObject
{
    [MapTo("Position")]
    private Vector3Model _Position { get; set; }

    public Vector3 Position
    {
        get => _Position?.ToVector3() ?? Vector3.zero;
        set => _Position = new Vector3Model(value);
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(_Position))
        {
            RaisePropertyChanged(nameof(Position));
        }
    }
}
```

In this example, we've defined a `Vector3Model` that roughly mirrors Unity's `Vector3`. It has three float properties representing the three components of the vector. We've also utilized what we learned in the previous sections. It has a private constructor to force consumers to always construct it with a `Vector3` argument. We've also marked its properties as private as we don't want consumers directly interacting with them. We want users to always call `ToVector3` to obtain the Unity type. And for our `Powerup` model, we're doing exactly that in the publicly exposed `Position` property. Note that similarly to our `Person` example, we're making sure to raise a notification for `Position` whenever `_Position` changes.

And similarly to the exaple in the previous section, this approach makes querying via LINQ impossible and we have to fall back to the string query syntax if we want to find all powerups in a particular area:

```csharp
IQueryable<Powerup> PowerupsAroundLocation(Vector3 location, float radius)
{
    // Note that this query returns a cube around the location, not a sphere.
    var powerups = realm.All<Powerup>().Filter(
        "Position.x > $0 AND Position.x < $1 AND Position.y > $2 AND Position.y < $3 AND Position.z > $4 AND Position.z < $5",
        location.x - radius, location.x + radius,
        location.y - radius, location.y + radius,
        location.z - radius, location.z  + radius);
}
```

## Conclusion

The list of techniques above is by no means meant to be exhaustive. Neither is it meant to imply that this is the only, or even "the right," way to use Realm. For most apps, simple POCOs with a list of properties is perfectly sufficient. But if you need to add extra validations or persist complex data types that you're using a lot, but Realm doesn't support natively, we hope that these examples will give you ideas for how to do that. And if you do come up with an ingenious way to use Realm, we definitely want to hear about it. Who knows? Perhaps we can feature it in our "Advanced Data Modeling" blog post!