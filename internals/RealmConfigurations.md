Configurations in the Realm C# Product
======================================

Unlike earlier products, we make heavy use of ObjectStore and try to push responsibilities down into that C++ layer, or at least maintain that as the **point of truth** with C# copies of values acting only as caches for fast lookup.

ObjectStore Classes
-------------------

The file `shared_realm.cpp` contains the `Realm::Config` which is used to parameterise opening  Realm.

`Realm::Config` is a simple structure which is used to satisfy the _Parameter Object_ pattern for `get_shared_realm` and, in turn, `Realm`.


How the C# code gets a Realm instance from Object Store with caching
---------------------------------------------------
@dot
digraph { 
    node[shape = box, style=rounded]
    edge[arrowhead=vee]

    /*  C# */
    node [fontcolor="orange", color="orange"]
    GetInstance [label="Realm.GetInstance\n(optional RealmConfiguration)"]
    
    RealmConfiguration [shape=box3d]
    DefaultConfiguration [label="RealmConfiguration.DefaultConfiguration"]
    Realm [shape=box3d]
    "NativeSharedRealm.open"

    /* c++ */
    node [fontcolor="blue", color="blue"]  
    shared_realm_open [label="shared_realm_open\nnative bridge\nin shared_realm_cs.cpp"]
    Config [label="Realm::Config", shape=box3d]
    get_shared_realm
    cached_get_realm [label="RealmCache::get_realm"]
    cached_realms [shape=record, style=solid, label=" {lock-protected cache as std::map | {path\nstd::string, | std::map | { std::thread::id | WeakRealm }  }   }"]

    GetInstance -> DefaultConfiguration
    DefaultConfiguration -> RealmConfiguration [label=" creates"]
    GetInstance -> "NativeSharedRealm.open" 
    GetInstance -> Realm [label=" creates"]
    RealmConfiguration -> "NativeSharedRealm.open" [label="path", style=dotted]
    "NativeSharedRealm.open" -> shared_realm_open
    "NativeSharedRealm.open" -> Realm [label="\lhandle to\lpossibly cached\lRealm core handle", style=dotted]
    shared_realm_open -> Config [label=" creates"]
    shared_realm_open -> get_shared_realm
    get_shared_realm -> cached_get_realm [label="lookup by path\n and current thread"]
    Config -> cached_get_realm [label="path", style=dotted]
    cached_get_realm -> cached_realms

    {rank=same; RealmConfiguration; "NativeSharedRealm.open";Realm}
}
@enddot  