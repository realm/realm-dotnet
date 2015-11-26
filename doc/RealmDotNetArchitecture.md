Architecture
==============

## ObjectStore-based Architecture
@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]
  
Realm
}
@enddot


Diagrams for the architecture discussion for refactoring the dotNet architecture.

Currently as at 2015-08-24
---------------------------
(this was the old architecture from the first proof of concept)

@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]
  
  Realm
  ICoreProvider [style=dotted]
  RealmObject -> ICoreProvider
  
  Realm -> ICoreProvider [label=" owns injected\l platform instance"]
  Realm -> RealmQuery [label=" create with All"]
  RealmQuery -> ICoreProvider [label=" querying"]
  
  /* subclasses */
  RealmObject -> "User Code" [arrowtail=empty, arrowhead=none, dir=both]
  ICoreProvider -> CoreProvider [arrowtail=empty, arrowhead=none, dir=both]
  ICoreProvider -> MockCoreProvider [arrowtail=empty, arrowhead=none, dir=both]
  ICoreProvider -> StandaloneCoreProvider [arrowtail=empty, arrowhead=none, dir=both]



  weave [label= "woven property\ncalls Get/Set", shape=plaintext]
  "User Code" -> weave -> RealmObject 

  CoreProvider -> UnsafeNativeMethods -> Core

  RealmObject -> Realm [label=" Add"]
  
  {rank=same; Realm; RealmQuery}
  {rank=same; RealmObject; ICoreProvider}
}
@enddot



Proposed
--------

@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]
  
  Realm
  ICoreProvider [style=dotted]
  IQueryCoreProvider [style=dotted, color=red, fontcolor=red]
  RealmObject -> ICoreProvider
  
  Realm -> ICoreProvider [label=" owns injected\l platform instance"]
  Realm -> RealmQuery [label=" create with All"]
  RealmQuery -> IQueryCoreProvider [label=" querying"]
  
  /* subclasses */
  RealmObject -> "User Code" [arrowtail=empty, arrowhead=none, dir=both]
  ICoreProvider -> MockCoreProvider [arrowtail=empty, arrowhead=none, dir=both]
  ICoreProvider -> StandaloneCoreProvider [arrowtail=empty, arrowhead=none, dir=both]
  ICoreProvider -> CoreProvider [arrowtail=empty, arrowhead=none, dir=both]
  
  
  
  QueryCoreProvider [color=red, fontcolor=red]
  CoreProvider -> QueryCoreProvider [label=" makes\l platform instance"]



  weave [label= "woven property\ncalls Get/Set", shape=plaintext]
  "User Code" -> weave -> RealmObject 

  /* new unsafe methods */
  UnsafeTableMethods [color=red, fontcolor=red]
  ManyFieldSpecific [label="UnsafeString/Int/Double...FieldMethods", color=red, fontcolor=red]
  UnsafeQueryMethods [color=red, fontcolor=red]

  CoreProvider -> UnsafeTableMethods -> Core
  CoreProvider -> ManyFieldSpecific -> Core
  IQueryCoreProvider -> QueryCoreProvider -> UnsafeQueryMethods -> Core

  RealmObject -> Realm [label=" Add"]
  
  {rank=same; Realm; RealmQuery}
  {rank=same; RealmObject; ICoreProvider}
}
@enddot

