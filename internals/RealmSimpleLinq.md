Simple Linq in the Realm C# Product
======================================

Discussing how we implement Linq in detail is a topic that could fill many pages.

The first diagram in this document describes the implementation in extreme detail for a simple case. Further diagrams will refer back to this without showing as deep a call strructure or as much detail.

The top-level Differentiator
----------------------------

As seen in the diagrams below, the entry point into our processing from the `System.Linq` space is `QueryProvider.Execute`.

This is a generic method which is sometimes invoked directly from Linq code.

* `System.Linq.Queryable.Any()` calls `Execute<bool>`
* `System.Linq.Queryable.Count()` calls `Execute<int>`
* `ToList()` (or `foreach`) calls **our** `GetEnumerator()` 
* Under the hood, searches are mapped to the ObjectStore `Resuls` type which provides an abstraction for Query-based access as well as entire Tables.



LINQ Expression Evaluation with a simple search and ToList
---------------------------------------------------
We start with a simple search with one clause doing a comparison on a double field, similar to `SimpleLINQtests.cs`.

`var aQuery = _realm.All<Person>().Where(p => p.Latitude < 40);`

The type of `aQuery` is `Realms.RealmResults<IntegrationTests.Person>` and it requires further evaluation to trigger actually doing the search.

A common way is to call the standard Linq to Objects call `var newList = aQuery.ToList()` which triggers the search and enumerates the result to copy objects to a list. 

@dot
digraph { 
    node[shape = box, style=rounded]
    edge[arrowhead=vee]

    /*  C# */
    node [fontcolor="orange", color="orange"]
    
    test [label="Test code\nor application code"]
    LinqWhere [label="System.Linq.\nQueryable.Where<Person>"]
    QPCreateQuery [label="QueryProvider.\nCreateQuery<Person>"]
    RealmResults [shape=box3d, label="RealmResults<Person>"]
    RealmResultsEnumerator [shape=box3d, label="RealmResultsEnumerator<Person>"]

    ToList [label="System.Linq.\nEnumerable.ToList"]
    Listctor[label="System.Collections.Generic.List.ctor"]
    GetEnum [label="RealmResults<Person>\n.GetEnumerator"]
    EnumMove [label="RealmResultsEnumerator.\nMoveNext"]
    EnumCurrent [label="RealmResultsEnumerator.\nCurrent"]
    RealmResultsVisitor [shape=box3d]
    QueryHandle [shape=box3d]
    QVMethod [label="RealmResultsVisitor.\nVisitMethodCall"]
    QVBinary [label="RealmResultsVisitor.\nVisitBinary"] 
    EVVisit [label="ExpressionVisitor.\nVisit"]
    EVVisit2 [label="ExpressionVisitor.\nVisit"]
    EVVisit3 [label="ExpressionVisitor.\nVisit"]
    QVConstant [label="RealmResultsVisitor.\nVisitConstant"]
    CreateQuery [label="RealmResultsVisitor.\nCreateQuery"]
    cqHandle [label="RealmResultsVisitor.\n_coreQueryHandle", shape=none]
    TableWhere [label="TableHandle.\nTableWhere"]
    NativeWhereInterface [label="NativeTable.\nwhere"]
    AddQueryEqual [label="RealmResultsVisitor.\nAddQueryEqual"]
    getCol [label="NativeQuery.\nget_column_index"]
    queryDoubleEqual [label="NativeQuery.\ndouble_equal"]
    NativeFindInterface [label="NativeQuery.\nfind"]

    /* c++ */
    node [fontcolor="blue", color="blue"] 
    table_where
    Query [label="Query", shape=box3d]
    NativeWhere[label="Table.\nwhere"]
    query_get_column_index
    query_double_equal
    query_find
    NativeFind [label="Query.\nfind"]
    
    
    test -> LinqWhere -> QPCreateQuery
    QPCreateQuery -> RealmResults [label=" creates as\n IQueryable<Person>"]
    test -> ToList
    ToList -> Listctor -> GetEnum
    GetEnum -> RealmResultsVisitor [label="creates"]
    GetEnum -> RealmResultsEnumerator [label=" creates"]
    Listctor -> EnumMove
    Listctor -> EnumCurrent
    RealmResultsVisitor -> EVVisit
    EVVisit -> QVMethod [label=" Where"]
    QVMethod -> EVVisit2 [label=" value(RealmResults`1[Person])"]
    EVVisit2 -> QVConstant
    QVConstant -> CreateQuery -> TableWhere
    CreateQuery -> cqHandle [label=" set"]
    TableWhere -> QueryHandle [label=" creates"]
    TableWhere -> NativeWhereInterface -> table_where
    table_where -> NativeWhere
    NativeWhere -> Query [label=" creates"]

    QVMethod -> EVVisit3 [label=" p => p.Latitude < 40)"]
    EVVisit3 -> QVBinary
    QVBinary -> AddQueryEqual
    AddQueryEqual -> getCol
    getCol -> query_get_column_index [label=" (_coreQueryHandle)"]
    AddQueryEqual -> queryDoubleEqual
    queryDoubleEqual -> query_double_equal  [label=" (_coreQueryHandle)"]
    query_double_equal -> "Query.equal(col, double)"
    EnumMove -> NativeFindInterface [label=" one native call\nper row found\nas needed"]
    NativeFindInterface -> query_find -> NativeFind   
}
@enddot  




Simplified LINQ Expression Evaluation with a simple search and Count
---------------------------------------------------

This expression will recurse down from the Count through a Where

`var countAfterWhere = _realm.All<Person>().Where(p => p.Latitude < 40).Count();`

The diagram here is elided to shortcut details shown in the detailed initial diagram above.

Note that because we're not doing anything with the results, we use the simple `query_count` call rather than needing to create an internal `Results`.

@dot
digraph { 
    node[shape = box, style=rounded]
    edge[arrowhead=vee]

    /*  C# */
    node [fontcolor="orange", color="orange"]
    
    test [label="Test code\nor application code"]
    LinqWhere [label="System.Linq.\nQueryable.Where<Person>"]
    QPCreateQuery [label="QueryProvider.\nCreateQuery<Person>"]
    RealmResults [shape=box3d, label="RealmResults<Person>"]
    LinqCount [label="System.Linq.\nQueryable.Count"]
    RQPEint [label="RealmResultsProvider<int>.\nExecute"]
    elide2 [label="...", shape=none]
    elide3 [label="...", shape=none]
    QVMethod0 [label="RealmResultsVisitor.\nVisitMethodCall"]
    QVMethod [label="RealmResultsVisitor.\nVisitMethodCall"]
    QVBinary [label="RealmResultsVisitor.\nVisitBinary"] 
    EVVisit0 [label="ExpressionVisitor.\nVisit"]
    EVVisit [label="ExpressionVisitor.\nVisit"]
    EVVisit2 [label="ExpressionVisitor.\nVisit"]
    EVVisit3 [label="ExpressionVisitor.\nVisit"]
    QVConstant [label="RealmResultsVisitor.\nVisitConstant"]
    NativeCountInterface [label="NativeQuery.\ncount"]

    /* c++ */
    node [fontcolor="blue", color="blue"] 
    query_count
    queryCount [label="Query.\ncount"] 
    
    
    test -> LinqWhere -> QPCreateQuery
    QPCreateQuery -> RealmResults [label=" creates as\n IQueryable<Person>"]
    test -> LinqCount
    LinqCount -> RQPEint
    RQPEint -> EVVisit0 [label=" Extract count\n from returned\n ConstantExpression"]
    EVVisit0 -> QVMethod0 [label=" Count"]
    QVMethod0 -> EVVisit [label=" value(RealmResults`1[Person])\l.Where(p => p.Latitude < 40)"]
    EVVisit -> QVMethod [label=" Where"]
    QVMethod -> EVVisit2 [label=" value(RealmResults`1[Person])"]
    EVVisit2 -> QVConstant
    QVConstant -> elide2  [style=dotted]

    QVMethod -> EVVisit3 [label=" p => p.Latitude < 40)"]
    EVVisit3 -> QVBinary
    QVBinary -> elide3 [style=dotted]

    QVMethod0 -> NativeCountInterface [label=" (_coreQueryHandle)"]
    NativeCountInterface -> query_count -> queryCount
}
@enddot  



Iterating All Objects of a Given Architecture
---------------------------------------------
This is to show what's happening behind the scenes in a simple iteration of all objects.

`foreach (var p in _realm.All<Person>()) {...}`

The type of `aQuery` is `Realms.RealmResults<IntegrationTests.Person>` and it requires further evaluation to trigger actually doing the search.

A common way is to call the standard Linq to Objects call `var newList = aQuery.ToList()` which triggers the search and enumerates the result to copy objects to a list. 

@dot
digraph { 
    node[shape = box, style=rounded]
    edge[arrowhead=vee]

    /*  C# */
    node [fontcolor="orange", color="orange"]
    
    test [label="foreach"]
    All [label="Realm.All<Person>()"]
    GetEnum [label="RealmResults<Person>\n.GetEnumerator"]
    EVVisit [label="ExpressionVisitor.\nVisit"]
    RealmResults [shape=box3d, label="RealmResults<Person>"]
    RealmResultsVisitor [shape=box3d]
    RealmResultsEnumerator [shape=box3d, label="RealmResultsEnumerator<Person>"]
    QVConstant [label="RealmResultsVisitor.\nVisitConstant"]
    CreateQuery [label="RealmResultsVisitor.\nCreateQuery"]
    TableWhere [label="TableHandle.\nTableWhere"]
    cqHandle [label="RealmResultsVisitor.\n_coreQueryHandle", shape=none]
    EnumMove [label="RealmResultsEnumerator.\nMoveNext"]
    EnumCurrent [label="RealmResultsEnumerator.\nCurrent"]
    QueryHandle [shape=box3d]
    NativeWhereInterface [label="NativeTable.\nwhere"]
    NativeFindInterface [label="NativeQuery.\nfind"]

    /* c++ */
    node [fontcolor="blue", color="blue"] 
    table_where
    Query [label="Query", shape=box3d]
    NativeWhere[label="Table.\nwhere"]
    query_find
    NativeFind [label="Query.\nfind"]
    
    
    test -> All
    All -> RealmResults [label="creates"]
    test -> GetEnum
    test -> EnumMove
    test -> EnumCurrent
    GetEnum -> RealmResultsVisitor [label="creates"]
    GetEnum -> RealmResultsEnumerator [label=" creates"]
    RealmResultsEnumerator -> EVVisit
    EVVisit -> QVConstant
    QVConstant -> CreateQuery -> TableWhere
    CreateQuery -> cqHandle [label=" set"]
    TableWhere -> QueryHandle [label=" creates"]
    TableWhere -> NativeWhereInterface -> table_where
    table_where -> NativeWhere
    NativeWhere -> Query [label=" creates"]
    EnumMove -> NativeFindInterface [label=" one native call\nper row found\nas needed"]
    NativeFindInterface -> query_find -> NativeFind   
}
@enddot  
