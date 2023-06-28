LINQ support in Realm .NET
=============================

To make a query with Realm, you use the [`Realm.All<T>()`](xref:Realms.Realm.All``1) method to get an [`IQueryable<T>`](xref:System.Linq.IQueryable`1) instance. On this you can then apply the operators listed below.


## Restriction Operators
[`Where`](xref:System.Linq.Queryable.Where``1(System.Linq.IQueryable{``0},System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}})) is supported. [`OfType`](xref:System.Linq.Queryable.OfType``1(System.Linq.IQueryable)) is not but it would be redundant as a query in Realm will always consist of a collection of the class initially specified.

[`Where`](xref:System.Linq.Queryable.Where``1(System.Linq.IQueryable{``0},System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}})) takes a predicate. To see the supported operations for predicates in Realm queries, refer to the [Predicate Operations](#predicate-operations) section.

Example:

```csharp
var oldDogs = realm.All<Dog>().Where(dog => dog.Age > 8);
```

## Ordering Operators
[`OrderBy`](xref:System.Linq.Queryable.OrderBy``2(System.Linq.IQueryable{``0},System.Linq.Expressions.Expression{System.Func{``0,``1}})), [`OrderByDescending`](xref:System.Linq.Queryable.OrderByDescending``2(System.Linq.IQueryable{``0},System.Linq.Expressions.Expression{System.Func{``0,``1}})), [`Thenby`](xref:System.Linq.Queryable.ThenBy``2(System.Linq.IOrderedQueryable{``0},System.Linq.Expressions.Expression{System.Func{``0,``1}})), and [`ThenByDescending`](xref:System.Linq.Queryable.ThenByDescending``2(System.Linq.IOrderedQueryable{``0},System.Linq.Expressions.Expression{System.Func{``0,``1}})) are all supported. [`Reverse`](xref:System.Linq.Queryable.Reverse``1(System.Linq.IQueryable{``0})) is not yet supported. Currently, you can only order by persisted properties on the class that you are querying. This means that `dogs.OrderBy(dog => dog.Owner.FirstName)` and the like is not yet supported.

Example:

```csharp
var contacts = realm.All<Person>().OrderBy(p => p.LastName).ThenBy(p => p.FirstName);
```

## Conversion Operators
[`ToArray`](xref:System.Linq.Enumerable.ToArray``1(System.Collections.Generic.IEnumerable{``0})), [`ToList`](xref:System.Linq.Enumerable.ToList``1(System.Collections.Generic.IEnumerable{``0})), [`ToDictionary`](xref:System.Linq.Enumerable.ToDictionary*), and [`ToLookup`](xref:System.Linq.Enumerable.ToLookup*) are all supported. [`Cast`](xref:System.Linq.Queryable.Cast``1(System.Linq.IQueryable)) isn't, but it would be redundant as a query in Realm will always consist of a collection of the class initially specified.

Example:

```csharp
var phoneBook = realm.All<Person>().ToDictionary(person => person.PhoneNumber);
```

## Element Operators
All of the main element operators are supported:

* [`First`](xref:System.Linq.Queryable.First*) and [`FirstOrDefault`](xref:System.Linq.Queryable.FirstOrDefault*)
* [`Last`](xref:System.Linq.Queryable.Last*) and [`LastOrDefault`](xref:System.Linq.Queryable.LastOrDefault*)
* [`Single`](xref:System.Linq.Queryable.Single*) and [`SingleOrDefault`](xref:System.Linq.Queryable.SingleOrDefault*)

These methods take an optional predicate. To see the supported operations for predicates in Realm queries, refer to the [Predicate Operations](#predicate-operations) section.

Access to a single element by an index is supported by [`ElementAt`](xref:System.Linq.Queryable.ElementAt*) and [`ElementAtOrDefault`](xref:System.Linq.Queryable.ElementAtOrDefault*).

Note that, as is standard C# behaviour of [default(T)](https://msdn.microsoft.com/en-us/library/xwth0h0d.aspx), the variants `...OrDefault` return a single null [`RealmObject`](xref:Realms.RealmObject) if there is no matching element.

[`DefaultIfEmpty`](xref:System.Linq.Queryable.DefaultIfEmpty*) is not yet supported.

## Quantifiers
[`Any`](xref:System.Linq.Queryable.Any*) is supported.

[`All`](xref:System.Linq.Queryable.All*) and [`Contains`](xref:System.Linq.Queryable.Contains*) are not yet supported.

[`Any`](xref:System.Linq.Queryable.Any*) takes an optional predicate. To see the supported operations for predicates in Realm queries, refer to the [Predicate Operations](#predicate-operations) section.

## Aggregate Operators
[`Count`](xref:System.Linq.Queryable.Count*) is supported.

[`LongCount`](xref:System.Linq.Queryable.LongCount*), [`Sum`](xref:System.Linq.Queryable.Sum*), [`Min`](xref:System.Linq.Queryable.Min*), [`Max`](xref:System.Linq.Queryable.Max*), and [`Average`](xref:System.Linq.Queryable.Average*) are not yet supported.

[`Count`](xref:System.Linq.Queryable.Count*) takes an optional predicate. To see the supported operations for predicates in Realm queries, refer to the [Predicate Operations](#predicate-operations) section.

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
    public string FullName => FirstName + " " + LastName;
}
```

Given this class, you can create queries with conditions that apply to the `FirstName` and `LastName` properties but not to the `FullName` property. Likewise, properties with the [`[Ignored]`](xref:Realms.IgnoredAttribute) attribute cannot be used.

Note that currently, the property must be the left side of a condition. This means that

```csharp
var oldDogs = realm.All<Dog>().Where(dog => 7 < dog.Age); // INVALID query, do not copy
```

is illegal and would have to be changed into the equivalent

```csharp
var oldDogs = realm.All<Dog>().Where(dog => dog.Age > 7); // Fixed
```

## Relational Operators
Equality operators can be applied to all property types: [==](https://msdn.microsoft.com/en-us/library/53k8ybth.aspx), [!=](https://msdn.microsoft.com/en-us/library/3tz250sf.aspx)

Furthermore, the following can be used for numerical types: [<](https://msdn.microsoft.com/en-us/library/z5wecxwa.aspx), [<=](https://msdn.microsoft.com/en-us/library/hx063734.aspx), [>](https://msdn.microsoft.com/en-us/library/yxk8751b.aspx), [>=](https://msdn.microsoft.com/en-us/library/a59bsyk4.aspx)

## String Operators
With strings, you can use:
[`Contains`](xref:System.String.Contains*), [`StartsWith`](xref:System.String.StartsWith*), and [`EndsWith`](xref:System.String.EndsWith*), [`Equals`](xref:System.String.Equals*), and [`Like`](xref:Realms.QueryMethods.Like*).

Example:

```csharp
var peopleWhoseNameBeginsWithJ = realm.All<Person>.Where(p => p.FirstName.StartsWith("J"));
```

By default, Realm will perform a case-sensitive comparison, but you can provide [`StringComparison.OrdinalIgnoreCase`](xref:System.StringComparison.OrdinalIgnoreCase) argument to overwrite that. Since there is no overload for `Contains` that accepts `StringComparison` on older .NET frameworks, we've provided a [helper method](xref:Realms.QueryMethods.Contains*) that can be used when querying:

```csharp
var peopleWhoseNameContainsA = realm.All<Person>().Where(p => QueryMethods.Contains(p.FirstName, "a", StringComparison.OrdinalIgnoreCase));
```

The [`Like`](xref:Realms.QueryMethods.Like*) query method can be used to compare a string property against a pattern. `?` and `*` are allowed as wildcard characters, where `?` matches 1 character and `*` matches 0 or more characters:
```csharp
var words = realm.All<Word>().Where(p => QueryMethods.Like(p.Value, "?bc*"));

// Matches abc, cbcde, but not bcd
```

When not used in a query expression, `Like` falls back to using RegEx to enforce the same rules.

## Composition
You can use parentheses and the [||](https://msdn.microsoft.com/en-us/library/6373h346.aspx) and [&&](https://msdn.microsoft.com/en-us/library/2a723cdk.aspx) operators to compose queries.

Example:

```csharp
var PuppyRexes = realm.All<Dog>().Where(dog => dog.Age < 2 && dog.Name == "Rex");
```

## A note on liveness

Realm queries are *live*, in the sense that they will continue to represent the current state of the database.

```csharp
realm.Write(() =>
{
    realm.Add(new Person { FirstName = "John" });
    realm.Add(new Person { FirstName = "Peter" });
});

var js = realm.All<Person>().Where(p => p.FirstName.StartsWith("J"));

foreach(var j in js)
{
    Console.WriteLine(j.FirstName); // ==> John
}

realm.Write(() =>
{
    realm.Add(new Person { FirstName = "Joe" });
});

foreach(var j in js)
{
    Console.WriteLine(j.FirstName); // ==> John, Joe
}
```

This differs from the typical behavior of object/relational mappers (ORM's) where the result of a query is fetched and kept in memory as it was.

However, it also differs from the behavior of LINQ to Objects, where every iteration will reevaluate expressions, meaning that changes to both sides of a condition will affect the result. A Realm query will evaluate the right-hand sides of the conditions on the first run. So imagine you have a query like this:

```csharp
var recentLogEntries = realm.All<LogEntry>().Where(l => l.TimeStamp > DateTime.Now.AddHours(-1));
```

Here, the `recentLogEntries` variable will contain all the log entries that have a `TimeStamp` later than one hour before the time when the query was first run (via [foreach](https://msdn.microsoft.com/en-us/library/ttw7t8t6.aspx), [`ToList`](xref:System.Linq.Enumerable.ToList``1(System.Collections.Generic.IEnumerable{``0})) etc.). Newly added log entries will be included on subsequent runs, but the time they are compared to will not be updated.

## Not yet supported

The following features are not yet supported. A few of them will not be supported as the Realm architecture renders them unnecessary.

### Grouping Operators
[`GroupBy`](xref:System.Linq.Queryable.GroupBy*) is not supported.

### Set Operators
[`Distinct`](xref:System.Linq.Queryable.Distinct*), [`Union`](xref:System.Linq.Queryable.Union*), [`Intersect`](xref:System.Linq.Queryable.Intersect*), and [`Except`](xref:System.Linq.Queryable.Except*) are not supported.

### Partitioning Operators
[`Take`](xref:System.Linq.Queryable.Take*), [`Skip`](xref:System.Linq.Queryable.Skip*), [`TakeWhile`](xref:System.Linq.Queryable.TakeWhile*), and [`SkipWhile`](xref:System.Linq.Queryable.SkipWhile*) are not yet supported.

These are less important than when using an ORM. Given Realm's zero-copy pattern, data is only read from the database when the properties on the objects are accessed, so there is little overhead in simply iterating over a part of a result.

### Projection Operators
[`Select`](xref:System.Linq.Queryable.Select*) and [`SelectMany`](xref:System.Linq.Queryable.SelectMany*) are not yet supported.

The [select](https://msdn.microsoft.com/en-us/library/bb384087.aspx) keyword used with the query syntax is supported as long as you select the [`RealmObject`](xref:Realms.RealmObject) itself and not some derivative:
```csharp
var oldDogs = from d in realm.All<Dog>() where d.Age > 8 select d;
```

## Concatenation Operators
[`Concat`](xref:System.Linq.Queryable.Concat*) is not supported.

### Join Operators
[`Join`](xref:System.Linq.Queryable.Join*) and [`GroupJoin`](xref:System.Linq.Queryable.GroupJoin*) are not supported.

Note that joins are less vital when using Realm than when using a relational database and an ORM. Instead of using keys to identify relations, you simply refer to the related object.

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

This works like an ordinary reference in C# which means that if two `Customer` instances are assigned the same `Address` object, changes to that address will apply to both customer objects.