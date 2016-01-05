Schema Creation in the Realm C# Product
======================================

Currently this is a relatively heavy-weight activity which is repeated for every instance request (see [issue #308](https://github.com/realm/realm-dotnet/issues/308)). 

We iterate **all** the `RealmObject` subclasses defined in the application and add them to the schema. We then always pass that schema to the Realm, even if internal ObjectStore caching means we will get back an existing instance which already has a schema.

There's an optimisation in `Realm::get_shared_realm` when we have an instance of a Realm open (matched on path) we just grab the schema from that, assuming that the user has not been able to update the schema in memory since starting the process.

We push our schema down when getting the realm and in `Realm::update_schema`, if our schema is either a different version or has different object schemas, we migrate.

Note that internally in ObjectStore, the `Config.schema_version` is initialised to `ObjectStore::NotVersioned = std::numeric_limits<uint64_t>::max()`.

How Schemas are Passed to ObjectStore
--------------------------------------

@dot
digraph { 
    node[shape = box, style=rounded]
    edge[arrowhead=vee]

    /*  C# */
    node [fontcolor="orange", color="orange"]
    GetInstance [label="Realm.GetInstance\n(optional RealmConfiguration)"]
    Realm [shape=box3d]
    add_object_schema[label="NativeSchema.initializer_add_object_schema"]
    "new SchemaHandle"
    "NativeSchema.create"
    "NativeSharedRealm.open"

    /* c++ */
    node [fontcolor="blue", color="blue"]  
    shared_realm_open [label="shared_realm_open\nnative bridge\nin shared_realm_cs.cpp"]
    schema_initializer_add_object_schema
    schema_create
    get_shared_realm [label="Realm::get_shared_realm"]
    verify_schema [label="ObjectStore::\nverify_schema"]
    update_schema [label="Realm::update_schema"]
    "Schema::validate"
    update_with_schema [label="ObjectStore::\nupdate_realm_with_schema\n(automatic migration)"]
    verify_object_schema [label="ObjectStore::\nverify_object_schema"]
    
    /* Exceptions */
    node [color=red, style=solid]
    SchemaValidationException [label="SchemaValidationException\nindicates bad declarations"]
    MismatchedConfigException [label="MismatchedConfigException\nindicates schemas differ"]

    GetInstance -> add_object_schema [label=" loop all\l RealmObjectClasses\l"]
        add_object_schema -> schema_initializer_add_object_schema
        GetInstance -> "new SchemaHandle" -> "NativeSchema.create"
        "NativeSchema.create" -> schema_create [label=" Using SchemaInitializer\l built in the loop\l through objects\l"]
    GetInstance -> Realm [label=" creates"]
    GetInstance -> "NativeSharedRealm.open" [label="passing Schema"]
        "NativeSharedRealm.open" -> shared_realm_open
            shared_realm_open -> get_shared_realm -> update_schema
            
    "Schema::validate" -> SchemaValidationException [label=" throws"]
    get_shared_realm -> update_with_schema
    update_with_schema -> verify_schema
    get_shared_realm -> verify_schema
    get_shared_realm -> "Schema::validate" [label=" target"]
    update_schema -> "Schema::validate"
    update_schema -> verify_schema [label="same\lversion"]
    verify_schema -> verify_object_schema [label=" per class"]
    verify_schema -> SchemaValidationException [label=" throws"]
    get_shared_realm -> MismatchedConfigException [label=" when config cached\nthrows if version diff"]
}
@enddot  




