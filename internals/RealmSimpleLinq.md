Simple Linq in the Realm C# Product
======================================

Discussing how we implement Linq in detail is a topic that could fill many pages.

This document describes the implementation in extreme detail for a simple case. Future documentation will refer back to this without showing as deep a call strructure or as much detail.

The Sample Expressions
----------------------

We start with a simple search with one clause doing a comparison on a double field, similar to `SimpleLINQtests.cs`.

`var aQuery = _realm.All<Person>().Where(p => p.Latitude < 40);`

The type of `aQuery` is `Realms.RealmQuery<IntegrationTests.Person>` and it requires further evaluation to trigger actually doing the search.

A common way is to call the standard Linq to Objects call `var newList = aQuery.ToList()` which triggers the search and enumerates the result to copy objects to a list. 


LINQ Expression Evaluation with a simple search and ToList
---------------------------------------------------
@dot
digraph { 
    node[shape = box, style=rounded]
    edge[arrowhead=vee]

    /*  C# */
    node [fontcolor="orange", color="orange"]
    
    test [label="Test code\nor application code"]
    ToList [label="System.Linq.\nEnumerable.ToList"]
    Listctor[label="System.Collections.Generic.List.ctor"]
    GetEnum [label="RealmQuery<Person>\m.GetEnumerator"]
    RealmQueryVisitor [shape=box3d]
    QueryHandle [shape=box3d]
    QPE [label="QueryProvider.\nExecute"]
    RQPE [label="RealmQueryProvider.\nExecute"]
    QVProcess [label="RealmQueryVisitor.\nProcess"]
    QVMethod [label="RealmQueryVisitor.\nVisitMethodCall"]
    QVBinary [label="RealmQueryVisitor.\nVisitBinary"] 
    EVVisit [label="ExpressionVisitor.\nVisit"]
    EVVisit2 [label="ExpressionVisitor.\nVisit"]
    EVVisit3 [label="ExpressionVisitor.\nVisit"]
    QVConstant [label="RealmQueryVisitor.\nVisitConstant"]
    CreateQuery [label="RealmQueryVisitor.\nCreateQuery"]
    cqHandle [label="RealmQueryVisitor.\n_coreQueryHandle", shape=none]
    TableWhere [label="TableHandle.\nTableWhere"]
    NativeWhereInterface [label="NativeTable.\nwhere"]
    AddQueryEqual [label="RealmQueryVisitor.\nAddQueryEqual"]
    getCol [label="NativeQuery.\nget_column_index"]
    queryDoubleEqual [label="NativeQuery.\ndouble_equal"]
    ExecuteQuery [label="RealmQueryVisitor.\nExecuteQuery"]
    NativeFindInterface [label="NativeQuery.\nfind"]
    listMakerLoop  [label=" loop creating object\nfor each row found", style=dotted, color=red, fontcolor=red]
    findYieldLoop  [label=" loop with yield\n each row found", style=dotted]

    /* c++ */
    node [fontcolor="blue", color="blue"] 
    table_where
    Query [label="Query", shape=box3d]
    NativeWhere[label="Table.\nwhere"]
    query_get_column_index
    query_double_equal
    query_find
    NativeFind [label="Query.\nfind"]
    
    
    test -> ToList -> Listctor -> GetEnum -> QPE -> RQPE
    RQPE -> RealmQueryVisitor [label="creates"]
    RQPE -> QVProcess
    QVProcess -> EVVisit
    EVVisit -> QVMethod [label=" Where"]
    QVMethod -> EVVisit2 [label=" value(RealmQuery`1[Person])"]
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
    QVProcess -> ExecuteQuery [label=" (_coreQueryHandle)"]
    ExecuteQuery -> findYieldLoop [label=" 'creates'\nIEnumerable\nyielder"]
    findYieldLoop -> NativeFindInterface [label=" one native call\nper row found", fontcolor=red]
    NativeFindInterface -> query_find -> NativeFind
    QVProcess -> listMakerLoop
    listMakerLoop -> findYieldLoop [color=red, fontcolor=red, label=" invokes" ]
    
}
@enddot  