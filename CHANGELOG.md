0.72.0 Private Beta
-------------------
Still requires installation from private copy of NuGet download.

Uses Realm core 0.96.0

### Major Changes

* Added support for PCL so you can now use the NuGet in your PCL GUI or viewmodel libraries.

0.71.1 Private Beta
-------------------
### Minor Fixes

Building IOS apps targeting the simulator sometimes got an error like:

    Error MT5209: Native linking error...building for iOS simulator, 
    but linking in object file built for OSX, for architecture i386 (MT5209) 

This was fixed by removing a redundant simulator library included in NuGet


0.71.0 Private Beta
-------------------
Still requires installation from private copy of NuGet download.

Uses Realm core 0.95.6.

### Platform Changes
Now supporting:

* Xamarin Studio on Mac - IOS and Android
* Xamarin Studio on Windows -  Android
* Visual Studio on Windows -  IOS and Android


### Major Changes

* Added Android support as listed above.
* Added `RealmConfiguration` to provide reusable way to specify path and other settings.
* Added `Realm.Equals`, `Realm.GetHashCode` and `Realm.IsSameInstance` to provide equality checking so you can confirm realms opened in the same thread are equal (shared internal instance).
* Added `Realm.DeleteFiles(RealmConfiguration)` to aid in cleaning up related files.
* Added nullable basic types such as `int?`.
* Optimised `Realm.All<userclass>().Count()` to get rapid count of all objects of given class.
* Related lists are now supported in standalone objects.

#### LINQ 
* `Count()` on `Where()` implemented.
* `Any()` on `Where()` implemented.
* `First( lambda )` and `Single( lambda )` implemented.
* Significant optimisation of `Where()` to be properly lazy, was instantiating all objects internally.


### API-Breaking Changes

* `[PrimaryKey]` attribute renamed `[ObjectId]`.
* `Realm.Attach(object)` renamed `Manage(object)`.
* Lists of related objects are now declared with `IList<otherClass>` instead of `RealmList`.

### Bug fixes

* Bug that caused a linker error for iPhone simulator fixed (#375)


0.70.0 First Private Beta
--------------------------

### State

* Supported IOS with Xamarin Studio only.
* Basic model and read/write operations with simple LINQ `Where` searches.
* NuGet hosted as downloads from private realm/realm-dotnet repo.
