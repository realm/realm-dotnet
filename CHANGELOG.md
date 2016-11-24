0.8x.x (TBD)
-------------------
### Breaking Changes
* The `IQueryable<T>.ToNotifyCollectionChanged` extension methods that accept parameters are now deprecated. There is a new parameterless one that you should use instead. If you want to handle errors, you can do so by subscribing to the `Realm.OnError` event. (#938)
* `RealmResults<T>` is now marked `internal` and `Realm.All<T>()` will instead return `IQueryable<T>`. We've added a new extension method `IQueryable<T>.SubscribeForNotifications(NotificationCallbackDelegate<T>)` that allows subscribing for notifications. (#942)

### Enhancements
* In data-binding scenarios, if a setter is invoked by the binding outside of write transaction, we'll create an implicit one and commit it. This enables two-way data bindings without keeping around long-lived transactions. (#901)
* The Realm schema can now express non-nullable reference type properties with the new `[Required]` attribute. (#349)
* Exposed a new `Realm.OnError` event that you can subscribe for to get notified for exceptions that occur outside user code. (#938)
* The runtime collection, returned from `Realm.All` now implements `INotifyCollectionChanged` so you can pass it for data-binding without any additional casting. (#938)

### Bug fixes

0.80.0 (2016-10-27)
-------------------
### Breaking Changes
* This version updates the file format. Older versions will not be able to open files created with this version. (#846)
* `RealmList<T>` is now marked as internal. If you were using it anywhere, you should migrate to `IList<T>`. (#880)

### Enhancements
* iOS Linking all should work - we now add a [Preserve] attribue to all woven members of your `RealmObject` subclasses so you do not need to manually add `[Preserve(allMembers=true)]`  (#822)
* `Realm.Manage` calls are now much faster. You should prefer that to `Realm.CreateObject` unless you are setting only a few properties, while leaving the rest with default values. (#857)
* Added `bool update` argument to `Realm.Manage`. When `update: true` is passed, Realm will try to find and update a persisted object with the same PrimaryKey. If an object with the same PrimaryKey is not found, the umnamaged object is added. If the passed in object does not have a PrimaryKey, it will be added. Any related objects will be added or updated depending on whether they have PrimaryKeys. (#871)
    
    **NOTE**: cyclic relationships, where object references are not identical, will not be reconciled. E.g. this will work as expected:
    ```csharp
    var person = new Person { Name = "Peter", Id = 1 };
    person.Dog = new Dog();
    person.Dog.Owner = person;
    ```
    However this will not - it will set the Person's properties to the ones from the last instance it sees:
    ```csharp
    var person = new Person { Name = "Peter", Id = 1 };
    person.Dog = new Dog();
    person.Dog.Owner = new Person { Id = 1 };
    ```
    This is important when deserializing data from json, where you may have multiple instances of object with the same Id, but with different properties.

* `Realm.Manage` will no longer throw an exception if a managed object is passed. Instead, it will immediately return. (#871)
* Added non-generic version of `Realm.Manage`. (#871)
* Added support for nullable integer PrimaryKeys. Now you can have `long?` PrimaryKey property where `null` is a valid unique value. (#877)
* Added a weaver warning when applying Realm attributes (e.g. `[Indexed]` or `[PrimaryKey]`) on non-persisted properties. (#882)
* Added support for `==` and `!=` comparisons to realm objects in LINQ (#896), e.g.:
    ```csharp
    var peter = realm.All<Person>().FirstOrDefault(d => d.Name == "Peter");
    var petersDogs = realm.All<Dog>().Where(d => d.Owner == peter);
    ```
* Added support for `StartsWith(string, StringComparison)`, `EndsWith(string, StringComparison)`, and `Equals(string, StringComparison)` filtering in LINQ. (#893)

    **NOTE**: Currently only `Ordinal` and `OrdinalIgnoreCase` comparisons are supported. Trying to pass in a different one will result in runtime error. If no argument is supplied, `Ordinal` will be used.

0.78.1 (2016-09-15)
-------------------
### Bug fixes
* `Realm.ObjectForPrimaryKey()` now returns null if it failed to find an object (#833).
* Querying anything but persisted properties now throws instead of causing a crash (#251 and #723)

Uses core 1.5.1


0.78.0 (2016-09-09)
-------------------
### Breaking Changes
* The term `ObjectId` has been replaced with `PrimaryKey` in order to align with the other SDKs. This affects the `[ObjectId]` attribute used to decorate a property.

### Enhancements
* You can retrieve single objects quickly using `Realm.ObjectForPrimaryKey()` if they have a `[PrimaryKey]` property specified. (#402)
* Manual migrations are now supported. You can specify exactly how your data should be migrated when updating your data model. (#545)
* LINQ searches no longer throw a `NotSupportedException` if your integer type on the other side of an expression fails to exactly match your property's integer type.
* Additional LINQ methods now supported: (#802)
    * Last
    * LastOrDefault
    * FirstOrDefault
    * SingleOrDefault
    * ElementAt
    * ElementAtOrDefault

### Bug fixes
* Searching char field types now works. (#708)
* Now throws a RealmMigrationSchemaNeededException if you have changed a `RealmObject` subclass declaration and not incremented the `SchemaVersion` (#518)
* Fixed a bug where disposing a `Transaction` would throw an `ObjectDisposedException` if its `Realm` was garbage-collected (#779)
* Corrected the exception being thrown `IndexOutOfRangeException` to be  `ArgumentOutOfRangeException`

Uses core 1.5.1


0.77.2 (2016-08-11)
-------------------
### Enhancements
* Setting your **Build Verbosity** to `Detailed` or `Normal` will now display a message for every property woven, which can be useful if you suspect errors with Fody weaving.
* Better exception messages will helo diagnose _EmptySchema_ problems (#739)
* Partial evaluation of LINQ expressions means more expressions types are supported as operands in binary expressions (#755)
* Support for LINQ queries that check for `null` against `string`, `byte[]` and `Nullable<T>` properties.
* Support for `string.IsNullOrEmpty` on persisted properties in LINQ queries.
* Schema construction has been streamlined to reduce overhead when opening a Realm
* Schema version numbers now start at 0 rather than UInt64.MaxValue

### Bug fixes
* `RealmResults<T>` should implement `IQueryable.Provider` implicitly (#752)
* Realms that close implicitly will no longer invalidate other instances (#746)

Uses core 1.4.2


0.77.1 (2016-07-25)
-------------------
### Minor Changes
* Fixed a bug weaving pure PCL projects, released in v0.77.0 (#715)
* Exception messages caused by using incompatible arguments in LINQ now include the offending argument (#719)
* PCL projects using ToNotifyCollectionChanged may have crashed due to mismatch between PCL signatures and platform builds.

Uses core 1.4.0


0.77.0 (2016-07-18)
-------------------
**Broken Version** - will not build PCL projects

### Breaking Changes
* Sort order change in previous version was reverted.

### Major Changes
* It is now possible to introspect the schema of a Realm. (#645)
* The Realm class received overloads for `Realm.CreateObject` and `Realm.All` that accept string arguments instead of generic parameters, enabling use of the `dynamic` keyword with objects whose exact type is not known at compile time. (#646)
* _To Many_ relationships can now be declared with an `IList<DestClass>` rather than requiring `RealmList<DestClass>`. This is **significantly faster** than using `RealmList` due to caching the list.   (Issue #287)
* Creating standalone objects with lists of related objects is now possible. Passing such an object into `Realm.Manage` will cause the entire object graph from that object down to become managed.

### Minor Changes
* Fixed a crash on iOS when creating many short-lived realms very rapidly in parallel (Issue #653)
* `RealmObject.IsValid` can be called to check if a managed object has been deleted
* Accessing properties on invalid objects will throw an exception rather than crash with a segfault (#662)
* Exceptions thrown when creating a Realm no longer leave a leaking handle (Issue #503)

Uses core 1.4.0


0.76.1 (2016-06-15)
-------------------

### Minor Changes
* The `Realm` static constructor will no longer throw a `TypeLoadException` when there is an active `System.Reflection.Emit.AssemblyBuilder` in the current `AppDomain`.
* Fixed `Attempting to JIT compile` exception when using the Notifications API on iOS devices. (Issue #620)

### Breaking Changes
No API change but sort order changes slightly with accented characters grouped together and some special characters sorting differently. "One third" now sorts ahead of "one-third".

It uses the table at ftp://ftp.unicode.org/Public/UCA/latest/allkeys.txt

It groups all characters that look visually identical, that is, it puts a, à, å together and before ø, o, ö even. This is a flaw because, for example, å should come last in Denmark. But it's the best we can do now, until we get more locale aware.

Uses core 1.1.2

0.76.0 (2016-06-09)
-------------------

### Major Changes
* `RealmObject` classes will now implicitly implement `INotifyPropertyChanged` if you specify the interface on your class. Thanks to [Joe Brock](https://github.com/jdbrock) for this contribution!

### Minor Changes
* `long` is supported in queries (Issue #607)
* Linker error looking for `System.String System.String::Format(System.IFormatProvider,System.String,System.Object)` fixed (Issue #591)
* Second-level descendants of `RealmObject` and static properties in `RealmObject` classes now cause the weaver to properly report errors as we don't (yet) support those. (Issue #603)
* Calling `.Equals()` on standalone objects no longer throws. (Issue #587)


0.75.0 (2016-06-02)
-------------------

### Breaking Changes
* File format of Realm files is changed. Files will be automatically upgraded but opening a Realm file with older versions of Realm is not possible. NOTE: If you were using the Realm Browser specified for the old format you need to upgrade. Pick up the newest version [here](https://itunes.apple.com/app/realm-browser/id1007457278).
* `RealmResults<T>` no longer implicitly implements `INotifyCollectionChanged`. Use the new `ToNotifyCollectionChanged` method instead.

### Major Changes
* `RealmResults<T>` can be observed for granular changes via the new `SubscribeForNotifications` method.
* `Realm` gained the `WriteAsync` method which allows a write transaction to be executed on a background thread.
* Realm models can now use `byte[]` properties to store binary data.
* `RealmResults<T>` received a new `ToNotifyCollectionChanged` extension method which produces an `ObservableCollection<T>`-like wrapper suitable for MVVM data binding.

### Minor Fixes
* Nullable `DateTimeOffset` properties are supported now.
* Setting `null` to a string property will now correctly return `null`
* Failure to install Fody will now cause an exception like "Realms.RealmException: Fody not properly installed. RDB2_with_full_Realm.Dog is a RealmObject but has not been woven." instead of a `NullReferenceException`
* The PCL `RealmConfiguration` was missing some members.
* The Fody weaver is now discoverable at non-default nuget repository paths.


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
