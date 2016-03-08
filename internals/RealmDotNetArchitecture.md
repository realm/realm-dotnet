Architecture
==============

The following architecture describes the C# code within the `Realm.Shared` project and the c++ wrappers it invokes.

Note that a significant amount of ObjectStore is used by being copied within the different `wrappers` projects. 

This high-level architecture doesn't go into detail about how queries work. See `RealmSimpleLinq.md` for that.

## ObjectStore-based Architecture

@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]

  /*  C# */
  node [fontcolor="orange", color="orange"]
  
  Realm
  RealmObject
  RealmResults
  "User Code"
  RealmConfiguration
  weave [label= "woven property\ncalls Get/Set", shape=plaintext]
  RowHandle
  TableHandle
  linq [label="assorted LINQ methods", shape=plaintext]
  
  /* C# native interfaces */
  node [shape=invhouse, color=purple, fontcolor=purple]
  NativeCommon
  NativeQuery
  NativeTable
  NativeSchema
  NativeSharedRealm
  NativeRow
  

/* c++ */
  node [fontcolor="blue", color="blue"]

/* relationships */
  Realm -> RealmResults [label=" create with All"]
  RealmObject -> "User Code" [arrowtail=empty, arrowhead=none, dir=both]
  "User Code" -> weave -> RealmObject 
  Realm -> RowHandle [label="creates"]
  RealmObject -> RowHandle [label="member referring\nto core Row"]
  RealmObject -> Realm
  Realm -> TableHandle [label="owns dictionary\nkey by RealmObject type"]
  RealmObject -> TableHandle [label="uses in all native calls\nlookup from Realm"]
  RealmResults -> linq -> NativeQuery
  Realm -> NativeCommon
  Realm -> RealmConfiguration [label=" gets paths\nenryption settings"]
  Realm -> NativeSchema
  Realm -> NativeSharedRealm
  RealmObject -> NativeTable
  RowHandle -> NativeRow
  
  
  {rank=same; Realm; RealmResults}
  {rank=same; RealmObject, RealmConfiguration}
  {rank=same; NativeCommon, NativeTable, NativeSchema, NativeSharedRealm, NativeRow, NativeQuery}
}
@enddot



Lifecycle of an Object
----------------------

For an object being created with the simple `CreateObject` syntax, just assigning one property. 
It must happen within a `Transaction` (which is actually a _WriteTransaction_ but we don't expose any other kind).


@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]

  /*  C# */
  node [fontcolor="orange", color="orange"]

  Transaction
  user [label="user code", shape=plaintext]
  beginTrans [label="Realm.BeginWrite()"]
  create [label="Realm.\nCreateObject()"]
  assign [label="Assign a property"]
  commit [label="Transaction.\nCommit()"]
  RealmObject
  RealmHandle
  RowHandle
  SetStringValue [label="RealmObject.\nSetStringValue()"]

  /* C# native interfaces */
  node [shape=invhouse, color=purple, fontcolor=purple]
  NativeSharedRealm
  NativeRow
  NativeTable

/* c++ */
  node [fontcolor="blue", color="blue", shape=box]
  begin_transaction
  commit_transaction
  table_add_empty_row
  get_column_index
  set_string
  row_get_row_index
  "Row::get_index"

/* subclasses */
  RealmHandle -> RowHandle [dir=back, arrowhead=empty, arrowtail=otriangle, style=bold]


/* relationships */
  user -> beginTrans
  beginTrans -> Transaction  [label=" creates"]  
  user -> create
  user -> assign
  user -> commit
  Transaction -> NativeSharedRealm -> begin_transaction
  NativeSharedRealm -> commit_transaction
  
  create -> RealmObject [label="\l creates using\l Activator.CreateInstance"]
  create -> NativeTable -> table_add_empty_row
  create -> RowHandle [label="\l creates to\l retain native rowPtr"]
  RowHandle -> NativeRow -> row_get_row_index ->   "Row::get_index"
  RealmObject -> RowHandle
  
  assign -> SetStringValue [label=" via woven\n property set"]
  SetStringValue -> NativeTable -> get_column_index
  SetStringValue -> RowHandle [style=dotted]
  NativeTable -> set_string


/* ordering */
  edge[style=invis, arrowhead=none]
  beginTrans -> create -> assign -> commit
  {rank=same; beginTrans, create, assign, commit}
  {rank=same; NativeTable, NativeSharedRealm, NativeRow}
}
@enddot


Handles in Native vs Managed Code
---------------------------------

The managed base class `RealmHandle` is subclassed by multiple classes to retain pointers to native objects.

For rows, the chain as shown above starts in `Realm.CreateObject`:

It calls `table_add_empty_row` to allocate a `Row` which is a typedef for a `BasicRow<Table>` and returns that pointer. The pointer is passed into `CreateRowHandle` to be adopted by a `RowHandle`.

