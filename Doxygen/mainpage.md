Realm for Xamarin
=================

The C# API of %Realm is incredibly simple because it leverages the power of LINQ for querying and the [Fody](https://github.com/Fody/Fody) weaver to transform plain C# class declarations into persistent objects.

The main classes you will use are:

- [Realm](@ref Realms.Realm)
- [RealmObject](@ref Realms.RealmObject)
- [RealmList](@ref Realms.RealmList)
- [Transaction](@ref Realms.Transaction)

Helper classes you may use are:

- [RealmConfiguration](@ref Realms.RealmConfiguration)
- [RealmResults](@ref Realms.RealmResults)

Querying and Sorting are provided on a [Realm](@ref Realms.Realm) using standard LINQ syntax including `Where` and `OrderBy`.
To see what is supported, refer to the [LINQ Support](linq-support.md) page.

**Realm Overview Diagram**

<div id="overview-diagram">
<img src="UnderstandingRealmForXamarin.png" />
</div>

Documentation
-------------
The documentation can be found at [realm.io/docs/xamarin/latest](https://realm.io/docs/xamarin/latest/).

The API reference is located at [realm.io/docs/xamarin/latest/api](https://realm.io/docs/xamarin/latest/api/).

Source
------
Source is available [on github](https://github.com/realm/realm-dotnet).

Instructions on how to build from source are included in that repository's `README.md`. 


Minimal Sample
--------------

This trivial sample shows the use of most of the classes mentioned above:

- getting a [Realm](@ref Realms.Realm) with default name
- declaring a simple model with a single [RealmObject](@ref Realms.RealmObject) class - notice how it uses standard types for properties
- using a [Transaction](@ref Realms.Transaction) to create an object in the realm

Note that in most debugging situations the default Realm will be retained so if you run this sample fragment multiple times you should see the number created increase. This is on purpose so you are assured the database is being persisted between runs.

```
using Realms;

class Hero : RealmObject
{
    public string SuperName { get; set; }
    public int SuperScore { get; set; }
}

/// put this code in an OnLoad or simple button-press handler

    var _realm = Realm.GetInstance();
    using (var trans = _realm.BeginWrite())
    {
        var hero = _realm.CreateObject<Hero>();
        hero.SuperName = "Thor";
        hero.SuperScore = 100;
        trans.Commit();
    }
    var numAwe = _realm.All<Hero>().Count();
    System.Diagnostics.Debug.WriteLine($"Created {numAwe} heroes");
    _realm.Close();
```


Problem reports and Feature requests
------
The [github issue tracker](https://github.com/realm/realm-dotnet/issues) can be used to report problems or make feature requests.
