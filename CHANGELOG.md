0.75.0 (pending fix of issue 516 - core 0.99 onwards freeze)
-------------------

### Major Changes
* `RealmResults<T>` can be observed for granular changes via the new `SubscribeForNotifications` method.
* `Realm` gained the `WriteAsync` method which allows a write transaction to be executed on a background thread.
* Realm models can now use `byte[]` properties to store binary data.

### Breaking Changes
* File format of Realm files is changed. Files will be automatically upgraded but opening a Realm file with older versions of Realm is not possible.
* `RealmResults<T>` no longer implicitly implements `INotifyCollectionChanged`.

### Minor Fixes
* Nullable `DateTimeOffset` properties are supported now.

Work in progress
----------------

### Minor Fixes
* Setting `null` to a string property will now correctly return `null`
* Failure to install Fody will now cause an exception like "Realms.RealmException: Fody not properly installed. RDB2_with_full_Realm.Dog is a RealmObject but has not been woven." instead of a `NullReferenceException`


0.74.1 Released (2016-05-10)
-------------------
### Minor Fixes
* Realms now refresh properly on Android when modified in other threads/processes.
* Fixes crashes under heavy combinations of threaded reads and writes.

### Minor Changes
* The two `Realm` and `RealmWeaver` NuGet packages have been combined into a single `Realm` package.
* The `String.Contains(String)`, `String.StartsWith(String)`, and `String.EndsWith(String)` methods now support variable expressions. Previously they only worked with literal strings.  
* `RealmResults<T>` now implements `INotifyCollectionChanged` by raising the `CollectionChanged` event with `NotifyCollectionChangedAction.Reset` when its underlying table or query result is changed by a write transaction.

0.74.0 Private Beta (2016-04-02)
-------------------

### Major Changes
* The Realm assembly weaver now submits anonymous usage data during each build, so we can track statistics for unique builders, as done with the Java, Swift and Objective-C products (issue #182)
* `Realm.RemoveRange<>()` and `Realm.RemoveAll<>()` methods added to allow you to delete objects from a realm.
* `Realm.Write()` method added for executing code within an implicitly committed transaction
* You can now restrict the classes allowed in a given Realm using `RealmConfiguration.ObjectClasses`.
* LINQ improvements:
  * Simple bool searches work without having to use `== true` (issue #362)
  * ! operator works to negate either simple bool properties or complex expressions (issue #77)
  * Count, Single and First can now be used after a Where expression,  (#369) eg <br /> 
    `realm.All<Owner>().Where(p => p.Name == "Dani").First();` as well as with a lambda expression <br />
    `realm.All<Owner>().Single( p => p.Name == "Tim");` 
  * Sorting is now provided using the `OrderBy`, `OrderByDescending`, `ThenBy` and `ThenByDescending` clauses. Sorts can be applied to results of a query from a `Where` clause or sorting the entire class by applying after `All<>`.
  * The `String.Contains(String)`, `String.StartsWith(String)`, and `String.EndsWith(String)` methods can now be used in Where clauses.  
  * DateTimeOffset properties can be compared in queries.
* Support for `armeabi` builds on old ARM V5 and V6 devices has been removed.  

### Minor Changes
* Finish `RealmList.CopyTo` so you can apply `ToList` to related lists (issue #299)
* NuGet now inserts `libwrappers.so` for Android targets using `$(SolutionDir)packages` so it copes with the different relative paths in cross-platform (Xamarin Forms) app templates vs pure Android templates.  
* `Realm.RealmChanged` event notifies you of changes made to the realm
* `Realm.Refresh()` makes sure the realm is updated with changes from other threads.


0.73.0 Private Beta (2016-02-26)
-------------------
### Major Changes
* `RealmConfiguration.EncryptionKey` added so files can be encrypted and existing encrypted files from other Realm sources opened (assuming you have the key)


### Minor Fixes
* For PCL users, if you use `RealmConfiguration.DefaultConfiguration` without having linked a platform-specific dll, you will now get the warning message with a `PlatformNotSupportedException`. Previously threw a `TypeInitExepction`.
* Update to Core v0.96.2 and matching ObjectStore (issue #393)


0.72.1 Private Beta (2016-02-15) 
-------------------
No functional changes. Just added library builds for Android 64bit targets `x86_64` and `arm64-v8a`.


0.72.0 Private Beta (2016-02-13)
-------------------

Uses Realm core 0.96.0

### Major Changes

* Added support for PCL so you can now use the NuGet in your PCL GUI or viewmodel libraries.

0.71.1 Private Beta (2016-01-29)
-------------------
### Minor Fixes

Building IOS apps targeting the simulator sometimes got an error like:

    Error MT5209: Native linking error...building for iOS simulator, 
    but linking in object file built for OSX, for architecture i386 (MT5209) 

This was fixed by removing a redundant simulator library included in NuGet


0.71.0 Private Beta (2016-01-25)
-------------------

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


0.70.0 First Private Beta (2015-12-08)
--------------------------
Requires installation from private copy of NuGet download.

### State

* Supported IOS with Xamarin Studio only.
* Basic model and read/write operations with simple LINQ `Where` searches.
* NuGet hosted as downloads from private realm/realm-dotnet repo.
