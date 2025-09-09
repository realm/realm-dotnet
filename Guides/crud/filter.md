# Filter and Sort Data - .NET SDK
To query, filter, and sort data in a realm, use the Realm query engine.
There are two ways to use the query engine with the .NET SDK:

- LINQ Syntax
- Realm Query Language

You should use LINQ syntax for querying when possible, as it aligns with
.NET conventions.

> **NOTE:**
> The examples in this page use a simple data set for a
task list app. The two Realm object types are `Project`
and `Task`. A `Task` has a name, assignee's name, and
completed flag. There is also an arbitrary number for
priority -- higher is more important -- and a count of
minutes spent working on it. A `Project` has zero or more
`Tasks`.
>
> See the schema for these two classes, `Project` and
`Task`, below:
>
> ```csharp
> public partial class Items : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
>     public string Name { get; set; }
>     public string Assignee { get; set; }
>     public bool IsComplete { get; set; }
>     public int Priority { get; set; }
>     public int ProgressMinutes { get; set; }
> }
>
> public partial class Project : IRealmObject
> {
>     [PrimaryKey]
>     [MapTo("_id")]
>     public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
>     public string Name { get; set; }
>     public IList<Items> Items { get; }
> }
>
> ```
>

## Query with LINQ
Realm's query engine implements standard [LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/introduction-to-linq-queries)
syntax.

There are several operators available to filter a
Realm collection with LINQ.
Filters work by evaluating an operator expression for every object in the
collection being filtered. If the expression resolves to `true`, realm
includes the object in the results collection.

An expression consists of one of the following:

- The name of a property of the object currently being evaluated
- An operator
- A value of any type used by realm (string, date, number, boolean, etc.)

> **NOTE:**
> The Realm .NET SDK does not currently support all of the LINQ operators. Refer
to the Unsupported LINQ Operators section for a list of those unsupported
operators.
>

### Comparison Operators
Value comparisons

|Operator|Description|
| --- | --- |
|==|Evaluates to `true` if the left-hand expression is equal to the right-hand expression.|
|>|Evaluates to `true` if the left-hand numerical or date expression is greater than the right-hand numerical or date expression. For dates, this evaluates to `true` if the left-hand date is later than the right-hand date.|
|>=|Evaluates to `true` if the left-hand numerical or date expression is greater than or equal to the right-hand numerical or date expression. For dates, this evaluates to `true` if the left-hand date is later than or the same as the right-hand date.|
|<|Evaluates to `true` if the left-hand numerical or date expression is less than the right-hand numerical or date expression. For dates, this evaluates to `true` if the left-hand date is earlier than the right-hand date.|
|<=|Evaluates to `true` if the left-hand numeric expression is less than or equal to the right-hand numeric expression. For dates, this evaluates to `true` if the left-hand date is earlier than or the same as the right-hand date.|
|!=|Evaluates to `true` if the left-hand expression is not equal to the right-hand expression.|

> **EXAMPLE:**
> The following example uses the query engine's
comparison operators to:
>
> - Find high priority tasks by comparing the value of the `priority` property
value with a threshold number, above which priority can be considered high.
> - Find just-started or short-running tasks by seeing if the `progressMinutes`
property falls within a certain range.
> - Find unassigned tasks by finding tasks where the `assignee` property is
equal to `null`.
> - Find tasks assigned to specific teammates Ali or Jamie by seeing if the
`assignee` property is in a list of names.
>
> ```csharp
> var highPri = items.Where(i => i.Priority > 5);
>
> var quickItems = items.Where(i =>
>     i.ProgressMinutes >= 1 &&
>     i.ProgressMinutes < 15);
>
> var unassignedItems = items.Where(i =>
>     i.Assignee == null);
>
> var AliOrJamieItems = items.Where(i =>
>    i.Assignee == "Ali" ||
>    i.Assignee == "Jamie");
>
> ```
>

### Logical Operators
You can use the logical operators listed in the following table to make compound
predicates:

|Operator|Description|
| --- | --- |
|&&|Evaluates to `true` if both left-hand and right-hand expressions are `true`.|
|!|Negates the result of the given expression.|
|\\|\\||Evaluates to `true` if either expression returns `true`.|

> **EXAMPLE:**
> We can use the query language's logical operators to find
all of Ali's completed tasks. That is, we find all tasks
where the `assignee` property value is equal to 'Ali' AND
the `isComplete` property value is `true`:
>
> ```csharp
> var completedItemsForAli = items
>     .Where(i => i.Assignee == "Ali" && i.IsComplete);
>
> ```
>

### String Operators
You can compare string values using the string operators listed in the following
table. Regex-like wildcards allow more flexibility in search.

|Operator|Description|
| --- | --- |
|`StartsWith`|Evaluates to `true` if the left-hand string expression begins with the right-hand string expression. This is similar to `contains`, but only matches if the left-hand string expression is found at the beginning of the right-hand string expression.|
|EndsWith|Evaluates to `true` if the left-hand string expression ends with the right-hand string expression. This is similar to `contains`, but only matches if the left-hand string expression is found at the very end of the right-hand string expression.|
|Like|Evaluates to `true` if the left-hand string expression matches the right-hand string wildcard string expression. A wildcard string expression is a string that uses normal characters with two special wildcard characters: The `*` wildcard matches zero or more of any character The `?` wildcard matches any character. For example, the wildcard string "d?g" matches "dog", "dig", and "dug", but not "ding", "dg", or "a dog".|
|Equals|Evaluates to `true` if the left-hand string is [lexicographically](https://en.wikipedia.org/wiki/Lexicographic_order) equal to the right-hand string.|
|Contains|Evaluates to `true` if the left-hand string expression is found anywhere in the right-hand string expression.|
|string.IsNullOrEmpty|Evaluates to `true` if the left-hand string expression is null or empty. Note that `IsNullOrEmpty()` is a static method on `string`.|

> **EXAMPLE:**
> The following examples use the query engine's string operators to find
tasks:
>
> ```csharp
>
> // **NOTE:** In each of the following examples, you can replace the
> // Where() method with First(), FirstOrDefault(),
> // Single(), SingleOrDefault(),
> // Last(), or LastOrDefault().
>
> // Get all items where the Assignee's name starts with "E" or "e"
> var ItemssStartWithE = items.Where(i => i.Assignee.StartsWith("E",
>     StringComparison.OrdinalIgnoreCase));
>
> // Get all items where the Assignee's name ends wth "is"
> // (lower case only)
> var endsWith = items.Where(t =>
>     t.Assignee.EndsWith("is", StringComparison.Ordinal));
>
> // Get all items where the Assignee's name contains the
> // letters "ami" in any casing
> var itemsContains = items.Where(i => i.Assignee.Contains("ami",
>      StringComparison.OrdinalIgnoreCase));
>
> // Get all items that have no assignee
> var null_or_empty = items.Where(i => string.IsNullOrEmpty(i.Assignee));
>
>
> ```
>

> **IMPORTANT:**
> When evaluating strings, the second parameter in all functions *except* `Like`
must be either `StringComparison.OrdinalIgnoreCase` or
`StringComparison.Ordinal`. For the `Like()` method, the second
parameter is a boolean value (where "true" means "case sensitive").
>

### Full Text Search
You can use LINQ to query on properties that have Full-Text Search Indexes (FTS) on
them. To query these properties, use `QueryMethods.FullTextSearch`.
The following examples query the `Person.Biography` field:

```csharp
// Find all people with "scientist" and "Nobel" in their biography
var scientists = realm.All<Person>()
    .Where(p => QueryMethods.FullTextSearch(p.Biography, "scientist Nobel"));

// Find all people with "scientist" in their biography, but not "physics"
var scientistsButNotPhysicists = realm.All<Person>()
    .Where(p => QueryMethods.FullTextSearch(p.Biography, "scientist -physics"));

```

### Unsupported LINQ Operators
The following LINQ operators are not currently supported by the Realm .NET SDK:

|Category|Unsupported Operators|
| --- | --- |
|Concatenation|`Concat` `Join`. While `Join` is not supported, it is not needed with Realm. Instead of using keys in a Join statement, as you would in a traditional relational database, you can reference another type as a property. For more information, refer to Embedded Objects - .NET SDK. `GroupJoin`|
|Grouping|`GroupBy`|
|Partitioning|`Take` `Skip` `TakeWhile` `SkipWhile`|
|Projection|`SelectMany` `Select`, with one exception: when used with the query syntax, `Select` is supported as long as you select the Realm object itself and not a derivative: `var tasks = from t in realm.All<Task>() where t.Assignee == "Caleb" select t;`|
|Sets|`Distinct` `Union` `Intersect` `Except`|

## Query with Realm Query Language
You can also use the Realm Query Language (RQL)
to query realms. RQL is a string-based query language used to access the query
engine. When using RQL, you use the
`Filter()`
method:

```csharp
var elvisProjects = projects.Filter("Items.Assignee == $0", "Elvis");

```

> **IMPORTANT:**
> Because LINQ provides compile-time error checking
of queries, you should use it instead of RQL in most cases. If you require
features beyond LINQ's current capabilities, such as using
aggregation, use RQL.
>

### Aggregate Operators
Aggregate operators traverse a
collection and reduce it
to a single value. Note that aggregations use the
`Filter()`
method, which can be used to create more complex queries that are currently
unsupported by the LINQ provider. `Filter()` supports SORT and DISTINCT
clauses in addition to filtering.

For more information on the available aggregate operators, refer to the
Realm Query Language aggregate operator reference.

The following examples show different ways to aggregate data:

```csharp
// Get all projects with an average Item priorty > 5:
var avgPriority = projects.Filter(
    "Items.@avg.Priority > $0", 5);

// Get all projects where all Items are high-priority:
var highPriProjects = projects.Filter(
    "Items.@min.Priority > $0", 5);

// Get all projects with long-running Items:
var longRunningProjects = projects.Filter(
    "Items.@sum.ProgressMinutes > $0", 100);

```

### Full Text Search
You can use RQL to query on properties that have Full-Text Search Indexes (FTS) on
them. To query these properties, use the `TEXT` operator. The following example
queries the `Person.Biography` field:

```csharp
// Find all people with "scientist" and "Nobel" in their biography
var filteredScientists = realm.All<Person>()
    .Filter("Biography TEXT $0", "scientist Nobel");

// Find all people with "scientist" in their biography, but not "physics"
var filteredScientistsButNotPhysicists = realm.All<Person>()
    .Filter("Biography TEXT $0", "scientist -physics");

```

## Sort Query Results
A **sort** operation allows you to configure the order in
which Realm returns queried objects. You can sort based on
one or more properties of the objects in the results
collection.

Realm only guarantees a consistent order of results when the
results are sorted.

> **EXAMPLE:**
> The following code sorts the projects by name in reverse
alphabetical order (i.e. "descending" order).
>
> ```csharp
> var projectsSorted = projects.OrderByDescending(p => p.Name);
>
> ```
>
