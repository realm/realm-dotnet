0.71.0 Private Beta
-------------------
Still requires installation from private copy of NuGet download.

### Platform Changes
Now supporting:

* Xamarin Studio on Mac - IOS and Android
* Xamarin Studio on Windows -  Android
* Visual Studio on Windows -  IOS and Android


### Major Changes

* Added Android support as listed above.
* Added `RealmConfiguration` to provide reusable way to specify path and other settings.
* Added `Realm.Equals`, `Realm.GetHashCode` and `Realm.IsSameInstance` to provide equality checking so we can cache realms for re-opening.
* Added `Realm.DeleteFiles(RealmConfiguration)` to aid in cleaning up related files.
* Added nullable basic types such as `int?`.
* Optimised `Realm.All<userclass>().Count()` to get rapid count of all objects of given class.
* Related lists are now supported in standalone objects.
* You can delare related lists as `IList` eg: `public IList<Dog> Dogs { get; }` (and **must** use `IList` if you will be using that class for standalone objects).

#### LINQ 
* `Count()` on `Where()` implemented.
* `Any()` on `Where()` implemented.
* `First( lambda )` and `Single( lambda )` implemented.
* Significant optimisation of `Where()` to be properly lazy, was instantiating all objects internally.

### Internal Major Changes
* Property access to core optimised to avoid using generic Get/SetValue calls.
* Large string data optimised to reduce calls to core.
* Moved to Realm core 0.95.6.
* Android C++ code now compiled with gcc 4.9 r10e using C++ 14.

### API-Breaking Changes

* `[PrimaryKey]` attribute renamed `[ObjectId]`
* `Realm.Attach(object)` renamed `Manage(object)`



0.70.0 First Private Beta
--------------------------

### State

* Supported IOS with Xamarin Studio only.
* Basic model and read/write operations with simple LINQ `Where` searches.
* NuGet hosted as downloads from private realm/realm-dotnet repo.