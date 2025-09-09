# Results Collections - .NET SDK
## Results Collections
A Realm results collection represents the lazily-evaluated results of a query operation.
As such, a results collection contains zero or more instances of a single
Realm type.

> **IMPORTANT:**
> Results are immutable: you cannot add or remove elements on the
results collection. This is because the contents of a results collection are
determined by a query against the database.
>

You can filter and sort any collection using Realm's
query engine. Collections are
live, so they always reflect the
current state of the realm instance on the current
thread. You can also listen for changes in the collection by subscribing
to collection notifications.

### Results are Lazily Evaluated
Realm only runs a query when you actually request the
results of that query, e.g. by accessing elements of the
results collection. This lazy evaluation enables you to
write elegant, highly performant code for handling large
data sets and complex queries.

## Working wth Collections
### Collections are Live
Like live objects, Realm collections
are *usually* live:

- Live **results collections** always reflect the current results of the associated query.
- Live **lists** always reflect the current state of the relationship on the realm instance.

There are two cases, however, when a collection is **not** live:

- The collection is unmanaged. For example, a collection property of a Realm object
that has not been added to a realm yet, or that has been copied from a
realm.
- The collection is frozen.

Combined with collection notifications, live collections enable clean,
reactive code. For example, suppose your view displays the
results of a query. You can keep a reference to the results
collection in your view class, then read the results
collection as needed without having to refresh it or
validate that it is up-to-date.

> **IMPORTANT:**
> Since results update themselves automatically, do not
store the positional index of an object in the collection
or the count of objects in a collection. The stored index
or count value could be outdated by the time you use
it.
>

The following example shows how to query a results collection in several ways:

```csharp
var plants = realm.All<Plant>();

// Use the Where operator to find items
// in the results collection
var pricklyPear = plants
    .Where(plant => plant.Name == "Prickly Pear");

// Or apply a filter to the results collection
var pricklyPears = plants
    .Filter("Name == 'Prickly Pear'");

// You can query collection properties in the same way
var morePlants = realm.All<Inventory>().ElementAt(0).Plants;

// Convert the Ilist<Plant> to an IQueryable and
// use the Where operator
var pricklyPearCacti = morePlants.AsQueryable()
    .Where(plant => plant.Name == "Prickly Pear");

// Or apply a filter to the collection
var greenPlants = realm.All<Inventory>()
    .Filter("Plants.Color CONTAINS[c] 'Green'");

```

### Limiting Query Results
As a result of lazy evaluation, you do not need any special
mechanism to limit query results with Realm. For example, if
your query matches thousands of objects, but you only want
to load the first ten, simply access only the first ten
elements of the results collection.

### Pagination
Thanks to lazy evaluation, the common task of pagination
becomes quite simple. For example, suppose you have a
results collection associated with a query that matches
thousands of objects in your realm. You display one hundred
objects per page. To advance to any page, simply access the
elements of the results collection starting at the index
that corresponds to the target page.
