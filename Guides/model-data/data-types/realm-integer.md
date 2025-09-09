# RealmInteger - .NET SDK
## Overview
Realm offers
`RealmInteger` as a
special integer type you can use as a logical counter. `RealmInteger<T>`
exposes an additional API that can more clearly express intent and generate
better conflict resolution steps when using Synchronized Realms. The type
argument `<T>` can be of type `byte`, `short`, `int`, or `long`. The
following example shows how to use a `RealmInteger` property that maps to
an `int`:

```csharp
public partial class MyRealmClass : IRealmObject
{
    [PrimaryKey]
    public int _id { get; set; }
    public RealmInteger<int> Counter { get; set; }
}

```

## Implementing a Counter
Traditionally, you would implement a counter by reading a value, incrementing
it, and then setting it (`myObject.Counter += 1`). This does not work well in
an asynchronous situation like when two clients are offline. Consider
the following scenario:

- The realm object has a `counter` property of type `int`. It is currently
set to a value of `10`.
- Clients 1 and 2 both read the `counter` property (`10`) and each increments
the value by `1`.
- When each client regains connectivity and merges their changes, they expect a
value of 11, and there is no conflict. However, the counter value should be
`12`!

When using a `RealmInteger`, however, you can call the `Increment()` and
`Decrement()` methods, and to reset the counter, you set it to `0`, just as
you would an `int`:

```csharp
var myObject = realm.Find<MyRealmClass>(id);
// myObject.Counter == 0

realm.Write(() =>
{
    // Increment the value of the RealmInteger
    myObject.Counter.Increment(); // 1
    myObject.Counter.Increment(5); // 6

    // Decrement the value of the RealmInteger
    // Note the use of Increment with a negative number
    myObject.Counter.Decrement(); // 5
    myObject.Counter.Increment(-3); // 2

    // Reset the RealmInteger
    myObject.Counter = 0;

    // RealmInteger<T> is implicitly convertable to T:
    int bar = myObject.Counter;
});

```

> **IMPORTANT:**
> When you reset a `RealmInteger`, you may run into the offline merge issue
described above.
>

A `RealmInteger` is backed by traditional `integer` type, so no schema
migration is required when changing a property type from `T` to
`RealmInteger<T>`.
