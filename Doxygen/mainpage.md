Realm for Xamarin
=================

The C# API of %Realm is incredibly simple.

The main classes you will use are:

- [Realm](@ref Realms.Realm)
- [RealmObject](@ref Realms.RealmObject)
- [RealmList](@ref Realms.RealmList)
- [Transaction](@ref Realms.Transaction)

Helper classes you may use are:

- [RealmConfiguration](@ref Realms.RealmConfiguration)


Documentation
-------------
The documentation can be found at [realm.io/docs/xamarin/latest](https://realm.io/docs/xamarin/latest/).

The API reference is located at [realm.io/docs/xamarin/latest/api](https://realm.io/docs/xamarin/latest/api/).


Source
------
Source is available [on github](https://github.com/realm/realm-dotnet).


Minimal Sample
--------------

This trivial sample shows the use of most of the classes mentioned above:

- getting a [Realm](@ref Realms.Realm) with default name
- declaring a simple model with a single [RealmObject](@ref Realms.RealmObject) class - notice how it uses standard types for properties
- using a [Transaction](@ref Realms.Transaction) to create an object in the realm

```
using Realms;

class MePersist : RealmObject
{
    public string MyName { get; set; }
    public int MyAwesomeness { get; set; }
}

/// put this code in an OnLoad or simple button-press handler

    var _realm = Realm.GetInstance();
    using (var trans = _realm.BeginWrite())
    {
        var MeNess = _realm.CreateObject<MePersist>();
        MeNess.MyName = "Thor";
        MeNess.MyAwesomeness = 100;
        trans.Commit();
    }
    var numAwe = _realm.All<MePersist>().Count();
    System.Diagnostics.Debug.WriteLine($"Created {numAwe} realm objects");
    _realm.Close();
```


Problem reports and Feature requests
------
The [github issue tracker](https://github.com/realm/realm-dotnet/issues) can be used to report problems or make feature requests.
