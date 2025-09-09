# RealmValue - .NET SDK
> Version changed: 12.2.0
> `RealmValue` can hold lists and dictionaries of mixed data.
>

The `RealmValue` data type is a mixed data type, and can represent any
other valid Realm data type except an embedded object or a set. You can create collections
(lists, sets, and dictionaries) of type `RealmValue`:

```csharp
// CS0029 - Cannot implicitly convert type:
RealmValue myList = new List<Inventory>();

// These are valid uses of RealmValue:
var rvList = new List<RealmValue>();
var rvDict = new Dictionary<string, RealmValue>();

```

> **NOTE:**
> You cannot create a nullable `RealmValue`. However, if you want a
`RealmValue` property to contain a null value, you can
use the special `RealmValue.Null` property.
>

## Create a RealmValue Property
The following code demonstrates creating a `RealmValue` property in a class
that inherits from `IRealmObject` and then setting and getting the value of
that property:

```csharp
public partial class MyRealmValueObject : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public Guid Id { get; set; }

    public RealmValue MyValue { get; set; }

    // A nullable RealmValue property is *not supported*
    // public RealmValue? NullableRealmValueNotAllowed { get; set; }

    private void TestRealmValue()
    {
        var obj = new MyRealmValueObject();

        // set the value to null:
        obj.MyValue = RealmValue.Null;

        // or an int...
        obj.MyValue = 1;

        // or a string...
        obj.MyValue = "abc";

        // Use RealmValueType to check the type:
        if (obj.MyValue.Type == RealmValueType.String)
        {
            var myString = obj.MyValue.AsString();
        }
    }

```

## Collections as Mixed
In version 12.2.0 and later, a `RealmValue` data type can hold collections
(a list or dictionary, but *not* a set) of `RealmValue` elements. You can use
mixed collections to model unstructured or variable data. For more information,
refer to Define Unstructured Data.

- You can nest mixed collections up to 100 levels.
- You can query mixed collection properties and
register a listener for changes,
as you would a normal collection.
- You can find and update individual mixed collection elements
- You *cannot* store sets or embedded objects in mixed collections.

To use mixed collections, define the `RealmValue` type property in your data model.
Then, set the property as a list or dictionary.
