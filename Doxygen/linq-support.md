LINQ support in Realm Xamarin
-----------------------------

To make a query with Realm, you use the `Realm.All<>()` method to get a `RealmResults` instance. 
On this you can then apply the operators listed below.


## Restriction Operators
`Where` is supported. `OfType` is not but it would be redundant as a query in Realm will always 
consist of a collection of the class initially specified.

`Where` takes a predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.


## Projection Operators
`Select` and `SelectMany` are not yet supported.


## Partitioning Operators
`Take`, `Skip`, `TakeWhile` and `SkipWhile` are not yet supported.


## Ordering Operators
`OrderBy`, `OrderByDescending`, `ThenBy` and `ThenByDescending` are all supported. `Reverse` is not yet supported.
Currently, you can only order by persisted properties on the class that you are querying. 
That means, `dogs.OrderBy(dog => dog.Owner.FirstName)` and the like is not yet supported. 


## Grouping Operators
`GroupBy`is not supported.


## Set Operators 
`Distinct`, `Union`, `Intersect` and `Except` are not supported.


## Conversion Operators
`ToArray`, `ToList`, `ToDictionary` and `ToLookup` are all supported. `Cast` isn't, 
but it would be redundant as a query in Realm will always consist of a collection of the class initially specified.


## Concatenation Operators
`Concat` is not supported.


## Element Operators
`First` and `Single` are supported. 

`FirstOrDefault`, `ElementAt`, `ElementAtOrDefault`, `SingleOrDefault`, `Last`, `LastOrDefault` and 
`DefaultIfEmpty` are not yet supported.

`First` takes a predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.


## Quantifiers
`Any` is supported.

`All` and `Contains` are not yet supported.

`Any` takes a predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.


## Aggregate Operators 
`Count` is supported.

`LongCount`, `Sum`, `Min`, `Max` and `Average` are not yet supported. 

`Count` takes an optional predicate. To see the supported operations for predicates in Realm queries, refer to the
[Predicate Operations](#predicate-operations) section.


## Join Operators 
`Join` and `GroupJoin` are not supported. Note that joining is typically not necessary with Realm as you create
relationships via references and lists, not relations like you would in a traditional database. This means that
if you have a class `Customer` and a class `Address`, instead of referencing the address with a unique identifier,
you simply reference it from your customer class.


#Predicate Operations
As a general rule, you can only create predicates with conditions that rely on data in Realm. Imagine a class
```csharp
class Person : RealmObject
{
    // Non-persisted property
    public string FullName { get { return FirstName + " " + LastName; } }

    // Persisted properties
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```
Given this class, you can create queries with conditions that apply to the `FirstName` and `LastName` properties but 
not to the `FullName` property. Likewise, public fields and properties with the `[Ignored]` attribute cannot be
used. 

## Relational Operators
Equality operators can be applied to all property types:
`==`, `!=`

Furthermore, the following can be used for numerical types:
`<`, `<=`, `>`, `>=`


## String Operators
With strings, you can use:
`string.Contains`, `string.StartsWith` and `string.EndsWith`. Currently, only case sensitive comparisons are supported.





