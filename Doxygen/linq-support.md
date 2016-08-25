LINQ support in Realm Xamarin
-----------------------------

To make a query with Realm, you use the `Realm.All<>()` method to get a `RealmResults` instance. 
On this you can further apply the operators listed below to qualify it.


## Restriction Operators
`Where` is supported. `OfType` is not but it would be redundant as a query in Realm will always 
consist of a collection of the class initially specified.


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
`First` is supported. 

`FirstOrDefault`, `ElementAt`, `ElementAtOrDefault`, `Single`, `SingleOrDefault`, `Last`, `LastOrDefault` and 
`DefaultIfEmpty` are not yet supported.


## Quantifiers
`Any` is supported.

`All` and `Contains` are not yet supported.


## Aggregate Operators 
`Count` is supported.

`LongCount`, `Sum`, `Min`, `Max` and `Average` are not yet supported. 


## Join Operators 
`Join` and `GroupJoin` are not supported. Note that joining is typically not necessary with Realm as you create
relationships via references and lists, not relations like you would in a traditional database. This means that
if you have a class `Customer` and a class `Address`, instead of referencing the address with a unique identifier,
you simply reference it from your customer class.





