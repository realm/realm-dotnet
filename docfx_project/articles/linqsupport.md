LINQ support in Realm Xamarin
=============================

To make a query with Realm, you use the `Realm.All<T>()` method to get a `IQueryable<T>` instance. 
On this you can then apply the operators listed below.


## Restriction Operators
`Where` is supported. `OfType` is not but it would be redundant as a query in Realm will always 
consist of a collection of the class initially specified.

`Where` takes a predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.

Example:
```csharp
var oldDogs = realm.All<Dog>().Where(dog => dog.Age > 8);
```


## Ordering Operators
`OrderBy`, `OrderByDescending`, `ThenBy` and `ThenByDescending` are all supported. `Reverse` is not yet supported.
Currently, you can only order by persisted properties on the class that you are querying. 
This means that `dogs.OrderBy(dog => dog.Owner.FirstName)` and the like is not yet supported. 

Example:
```csharp
var contacts = realm.All<Person>().OrderBy(p => p.LastName).ThenBy(p => p.FirstName);
```

## Conversion Operators
`ToArray`, `ToList`, `ToDictionary` and `ToLookup` are all supported. `Cast` isn't, 
but it would be redundant as a query in Realm will always consist of a collection of the class initially specified.

Example:
```csharp
var phoneBook = realm.All<Person>().ToDictionary(person => person.PhoneNumber);
```


## Element Operators
All of the main element operators are supported:

* `First` and `FirstOrDefault` 
* `Last` and `LastOrDefault`
* `Single` and `SingleOrDefault`. 

These methods take an optional predicate. To see the supported operations for predicates in Realm queries, 
refer to the [Predicate Operations](#predicate-operations) section.

Access to a single element by an index is supported by `ElementAt` and `ElementAtOrDefault`.

Note that, as is standard C# behaviour of `default(T)`, the variants `...OrDefault` return a single null `RealmObject` if there is no matching element.

`DefaultIfEmpty` is not yet supported.


## Quantifiers
`Any` is supported.

`All` and `Contains` are not yet supported.

`Any` takes an optional predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.


## Aggregate Operators 
`Count` is supported.

`LongCount`, `Sum`, `Min`, `Max` and `Average` are not yet supported. 

`Count` takes an optional predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.



<a name="predicate-operations"> </a>
## Predicate Operations

As a general rule, you can only create predicates with conditions that rely on data in Realm. Imagine a class
```csharp
class Person : RealmObject
{
    // Persisted properties
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // Non-persisted property
    public string FullName { get { return FirstName + " " + LastName; } }
}
```
Given this class, you can create queries with conditions that apply to the `FirstName` and `LastName` properties but 
not to the `FullName` property. Likewise, methods, public fields and properties with the `[Ignored]` attribute cannot be
used. 

Note that currently, the property must be the left side of a condition. This means that
```csharp
var oldDogs = realm.All<Dog>().Where(dog => 7 < dog.Age); // INVALID query, do not copy
```
is illegal and would have to be changed into the equivalent
```csharp
var oldDogs = realm.All<Dog>().Where(dog => dog.Age > 7); // Fixed
```
 

## Relational Operators
Equality operators can be applied to all property types:
`==`, `!=`

Furthermore, the following can be used for numerical types:
`<`, `<=`, `>`, `>=`


## String Operators
With strings, you can use:
`Contains`, `StartsWith` and `EndsWith`. Currently, only case sensitive comparisons are supported.

Example:
```csharp
var peopleWhoseNameBeginsWithJ = realm.All<Person>.Where(person => person.FirstName.StartsWith("J"));
```


## Composition
You can use parentheses and the `||` and `&&` operators to compose queries.

Example:
```csharp
var PuppyRexes = realm.All<Dog>().Where(dog => dog.Age < 2 && dog.Name == "Rex");
```


## A note on liveness

Realm queries are *live*, in the sense that they will continue to represent the current state of the database. 

```csharp
realm.Write(() => 
{
    var p1 = realm.CreateObject<Person>();
    p1.FirstName = "John";

    var p2 = realm.CreateObject<Person>();
    p2.FirstName = "Peter";
});

var js = realm.All<Person>().Where(p => p.FirstName.StartsWith("J"));

foreach(var j in js)
    Console.WriteLine(j.FirstName); // ==> John

realm.Write(() =>
{
    var p3 = realm.CreateObject<Person>();
    p3.FirstName = "Joe";
});

foreach(var j in js)
    Console.WriteLine(j.FirstName); // ==> John, Joe
```

This differs from the typical behavior of object/relational mappers (ORM's) where the result of a query is fetched 
and kept in memory as it was.

However, it also differs from the behavior of LINQ to Objects, where every iteration will reevaluate expressions,
meaning that changes to both sides of a condition will affect the result. A Realm query will evaluate the right-hand sides
of the conditions on the first run. So imagine you have a query like this:

```csharp
var recentLogEntries = realm.All<LogEntry>().Where(l => l.TimeStamp > DateTime.Now.AddHours(-1));
``` 
Here, the `recentLogEntries` variable will contain all the log entries that have a `TimeStamp` later than one hour
before the time when the query was first run (via `foreach`, `ToList` etc.)
Newly added log entries will be included on subsequent runs, but the time they are compared to will not be updated.


## Not yet supported

The following features are not yet supported. A few of them will not be supported as the Realm architecture
renders them unnecessary.


### Grouping Operators
`GroupBy`is not supported.


### Set Operators 
`Distinct`, `Union`, `Intersect` and `Except` are not supported.


### Partitioning Operators 
`Take`, `Skip`, `TakeWhile` and `SkipWhile` are not yet supported.

These are less important than when using an ORM. Given Realm's zero-copy pattern, data is only read from the
database when the properties on the objects are accessed, so there is little overhead in simply iterating
over a part of a result.


### Projection Operators
`Select` and `SelectMany` are not yet supported.

The `select` keyword used with the query syntax is supported as long as you select the `RealmObject` itself
and not some derivative:
```csharp
var oldDogs = from d in realm.All<Dog>() where d.Age > 8 select d;
```


## Concatenation Operators
`Concat` is not supported.


### Join Operators 
`Join` and `GroupJoin` are not supported. 

Note that joins are less vital when using Realm than when using a relational database and an ORM. Instead of
using keys to identify relations, you simply refer to the related object. 

So given a class
```csharp
public class Address : RealmObject
{
    public string StreetName { get; set; }
    public int Number { get; set; }
}
```
you can simply use that in another class like so:
```csharp
public class Customer : RealmObject
{
    public string Name { get; set; }
    public Address Address { get; set; }
}
```
This works like an ordinary reference in C# which means that if two `Customer` instances are assigned the same `Address`
object, changes to that address will apply to both customer objects.

