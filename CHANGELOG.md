vNext(TBD)
------------------

### Enhancements
* Added the notion of "frozen objects" - these are objects, queries, lists, or Realms that have been "frozen" at a specific version. This allows you to access the data from any thread, but it will never change. All frozen objects can be accessed and queried as normal, but attempting to mutate them or add change listeners will throw an exception. (Issue [#1945](https://github.com/realm/realm-dotnet/issues/1945))
  * Added `Realm.Freeze()`, `RealmObject.Freeze()`, `RealmObject.FreezeInPlace()`, `IQueryable<RealmObject>.Freeze()`, `IList<T>.Freeze()`, and `IRealmCollection<T>.Freeze()`. These methods will produce the frozen version of the instance on which they are called.
  * Added `Realm.IsFrozen`, `RealmObject.IsFrozen`, and `IRealmCollection<T>.IsFrozen`, which returns whether or not the data is frozen.
  * Added `RealmConfigurationBase.MaxNumberOfActiveVersions`. Setting this will cause Realm to throw an exception if too many versions of the Realm data are live at the same time. Having too many versions can dramatically increase the filesize of the Realm.

### Fixed
* Fixed `Access to invalidated List object` being thrown when adding objects to a list while at the same time deleting the object containing the list. (Issue [#1971](https://github.com/realm/realm-dotnet/issues/1971))

### Compatibility
* Realm Object Server: 3.23.1 or later.

### Internal
* Using Sync 4.9.5 and Core 5.23.8.

4.3.0 (2020-02-05)
------------------

### Enhancements
* Exposed an API to configure the `userId` and `isAdmin` of a user when creating credentials via `Credentials.CustomRefreshToken`. Previously these values would be inferred from the JWT itself but as there's no way to enforce the server configuration over which fields in the JWT payload represent the `userId` and the `isAdmin` field, it is now up to the consumer to determine the values for these.
* Improved logging and error handling for SSL issues on Apple platforms.

### Fixed
* Realm objects can now be correctly serialized with `System.Runtime.Serialization.Formatters` and `System.Xml.Serialization` serializers. (Issue [#1913](https://github.com/realm/realm-dotnet/issues/1913))
  The private state fields of the class have been decorated with `[NonSerialized]` and `[XmlIgnore]` attributes so that eager opt-out
  serializers do not attempt to serialize fields such as `Realm` and `ObjectSchema` which contain handles to unmanaged data.
* Fixed an issue that would result in a compile error when `[Required]` is applied on `IList<string>` property. (Contributed by [braudabaugh](https://github.com/braudabaugh))
* Fixed an issue that prevented projects that include the Realm NuGet package from being debugged. (PR [#1927](https://github.com/realm/realm-dotnet/pull/1927))
* The sync client would fail to reconnect after failing to integrate a changeset. The bug would lead to further corruption of the client’s Realm file. (since 3.0.0).
* The string-based query parser (`results.Filter(...)`) used to need the `class_` prefix for class names when querying over backlink properties. This has been fixed so that only the public `ObjectSchema` name is necessary. For example, `@links.class_Person.Siblings` becomes `@links.Person.Siblings`.
* Fixed an issue where `ClientResyncMode.DiscardLocalRealm` wouldn't reset the schema.

### Compatibility
* Realm Object Server: 3.23.1 or later.

### Internal
* Upgraded Sync from 4.7.5 to 4.9.5 and Core from 5.23.3 to 5.23.8.

4.2.0 (2019-10-07)
------------------

### Enhancements
* Added `int IndexOf(object)` and `bool Contains(object)` to the `IRealmCollection` interface. (PR [#1893](https://github.com/realm/realm-dotnet/issues/1893))
* Exposed an API - `SyncConfigurationBase.EnableSessionMultiplexing()` that allows toggling session multiplexing on the sync client. (PR [1896](https://github.com/realm/realm-dotnet/pull/1896))
* Added support for faster initial downloads when using `Realm.GetInstanceAsync`. (Issue [1847](https://github.com/realm/realm-dotnet/issues/1847))
* Added an optional `cancellationToken` argument to `Realm.GetInstanceAsync` enabling clean cancelation of the in-progress download. (PR [1859](https://github.com/realm/realm-dotnet/pull/1859))
* Added support for Client Resync which automatically will recover the local Realm in case the server is rolled back. This largely replaces the Client Reset mechanism for fully synchronized Realms. Can be configured using `FullSyncConfiguration.ClientResyncMode`. (PR [#1901](https://github.com/realm/realm-dotnet/pull/1901))
* Made the `createUser` argument in `Credentials.UsernamePassword` optional. If not specified, the user will be created or logged in if they already exist. (PR [#1901](https://github.com/realm/realm-dotnet/pull/1901))
* Uses Fody 6.0.0, which resolves some of the compatibility issues with newer versions of other Fody-based projects. (Issue [#1899](https://github.com/realm/realm-dotnet/issues/1899))

### Fixed
* Fixed an infinite recursion when calling `RealmCollectionBase<T>.IndexOf`. (Issue [#1892](https://github.com/realm/realm-dotnet/issues/1892))

### Compatibility
* Realm Object Server: 3.23.1 or later.

### Internal
* Upgraded Sync from 4.7.0 to 4.7.1.
* Implemented direct access to sync workers on Cloud, bypassing the Sync Proxy: the binding will override the sync session's url prefix if the token refresh response for a realm contains a sync worker path field.

4.1.0 (2019-08-06)
------------------

### Breaking Changes
* Removed the `isAdmin` parameter from `Credentials.Nickname`. It doesn't have any effect on new ROS versions anyway as logging in an admin nickname user is not supported - this change just makes it explicit. (Issue [#1879](https://github.com/realm/realm-dotnet/issues/1879))
* Marked the `Credentials.Nickname` method as deprecated - support for the Nickname auth provider is deprecated in ROS and will be removed in a future version. (Issue [#1879](https://github.com/realm/realm-dotnet/issues/1879))
* Removed the `deleteRealm` parameter from `PermissionDeniedException.DeleteRealmInfo` as passing `false` has no effect. Calling the method is now equivalent to calling it with `deleteRealm: true`. (PR [#1890](https://github.com/realm/realm-dotnet/pull/1890))

### Enhancements
* Added support for unicode characters in realm path and filenames for Windows. (Core upgrade)
* Added new credentials type: `Credentials.CustomRefreshToken` that can be used to create a user with a custom refresh token. This will then be validated by ROS against the configured `refreshTokenValidators` to obtain access tokens when opening a Realm. If creating a user like that, it's the developer's responsibility to ensure that the token is valid and refreshed as necessary to ensure that access tokens can be obtained. To that end, you can now set the refresh token of a user object by calling `User.RefreshToken = "my-new-token"`. This should only be used in combination with users obtained by calling `Credentials.CustomRefreshToken`. (PR [#1889](https://github.com/realm/realm-dotnet/pull/1889))

### Fixed
* Constructing an IncludeDescriptor made unnecessary table comparisons. This resulted in poor performance when creating a query-based subscription (`Subscription.Subscribe`) with `includedBacklinks`. (Core upgrade)
* Queries involving an indexed int column which were constrained by a LinkList with an order different from the table's order would give incorrect results. (Core upgrade)
* Queries involving an indexed int column had a memory leak if run multiple times. (Core upgrade)

### Compatibility
* Realm Object Server: 3.23.1 or later.

### Internal
* Upgraded Sync from 4.5.1 to 4.7.0 and Core 5.20.0 to 5.23.1.

4.0.1 (2019-06-27)
------------------

### Fixed
* Fixed an issue that would prevent iOS apps from being published to the app store with the following error:
  > This bundle Payload/.../Frameworks/realm-wrappers.framework is invalid. The Info.plist file is missing the required key: CFBundleVersion.

  ([Issue 1870](https://github.com/realm/realm-dotnet/issues/1870), since 4.0.0)
* Fixed an issue that would cause iOS apps to crash on device upon launching. ([Issue 1871](https://github.com/realm/realm-dotnet/issues/1871), since 4.0.0)

4.0.0 (2019-06-13)
------------------

### Breaking Changes
* The following deprecated methods and classes have been removed:
  * The `SyncConfiguration` class has been split into `FullSyncConfiguration` and `QueryBasedSyncConfiguration`. Use one of these classes to connect to the Realm Object Server.
  * The `TestingExtensions.SimulateProgress` method has been removed as it hasn't worked for some time.
  * The `Property.IsNullable` property has been removed. To check if a property is nullable, check `Property.Type` for the `PropertyType.Nullable` flag.
  * The `Credentials.Provider` class has been removed. Previously, it contained a few constants that were intended for internal use mostly.
  * The `User.ConfigurePersistance` method has been superseded by `SyncConfigurationBase.Initialize`.
  * `User.LogOut` has been removed in favor of `User.LogOutAsync`.
  * `User.GetManagementRealm` has been removed in favor of the `User.ApplyPermissionsAsync` set of wrapper API.
  * `User.GetPermissionRealm` has been removed in favor of the `User.GetGrantedPermissions` wrapper API.
* Deprecated the `IQueryable<T>.Subscribe(string name)` extension method in favor of `IQueryable<T>.Subscribe(SubscriptionOptions options)`.
* Reworked the internal implementation of the permission API. For the most part, the method signatures haven't changed or where they have changed, the API have remained close to the original (e.g. `IQueryable<T>` has changed to `IEnumerable<T>`). ([Issue #1863](https://github.com/realm/realm-dotnet/issues/1863))
  * Changed the return type of `User.GetGrantedPermissionsAsync` from `IQueryable<PathPermission>` to `IEnumerable<PathPermission>`. This means that the collection is no longer observable like regular Realm-backed collections. If you need to be notified for changes of this collection, you need to implement a polling-based mechanism yourself.
  * `PathPermission.MayRead/MayWrite/MayManage` have been deprecated in favor of a more-consistent `AccessLevel` API.
  * In `User.ApplyPermissionsAsync`, renamed the `realmUrl` parameter to `realmPath`.
  * In `User.OfferPermissionsAsync`, renamed the `realmUrl` parameter to `realmPath`.
  * Removed the `PermissionOfferResponse` and `PermissionChange` classes.
  * Removed the `IPermissionObject` interface.
  * Removed the `ManagementObjectStatus` enum.
  * Removed the `User.GetPermissionChanges` and `User.GetPermissionOfferResponses` methods.
  * The `millisecondTimeout` argument in `User.GetGrantedPermissionsAsync` has been removed.
  * The `PermissionException` class has been replaced by `HttpException`.
* The `AuthenticationException` class has been merged into the `HttpException` class.

### Enhancements
* Added `Session.Start()` and `Session.Stop()` methods that allow you to pause/resume synchronization with the Realm Object Server. ([Issue #138](https://github.com/realm/realm-dotnet-private/issues/138))
* Added an `IQueryable<T>.Subscribe(SubscriptionOptions, params Expression<Func<T, IQueryable>>[] includedBacklinks)` extension method that allows you to configure additional options for the subscription, such as the name, time to live, and whether it should update an existing subscription. The `includedBacklinks` argument allows you to specify which backlink properties should be included in the transitive closure when doing query-based sync. For example:

  ```csharp
  class Dog : RealmObject
  {
      public Person Owner { get; set; }
  }

  class Person : RealmObject
  {
      [Backlink(nameof(Dog.Owner))]
      public IQueryable<Dog> Dogs { get; }
  }

  var options = new SubscriptionOptions
  {
      Name = "adults",
      TimeToLive = TimeSpan.FromDays(1),
      ShouldUpdate = true
  };

  var people = realm.All<Person>()
                    .Where(p => p.Age > 18)
                    .Subscribe(options, p => p.Dogs);

  await people.WaitForSynchronzationAsync();
  // Dogs that have an owner set to a person that is over 18
  // will now be included in the objects synchronized locally.
  var firstPersonDogs = people.Results.First().Dogs;
  ```
  ([Issue #1838](https://github.com/realm/realm-dotnet/issues/1838) & [Issue #1834](https://github.com/realm/realm-dotnet/issues/1834))
* Added a `Realm.GetAllSubscriptions()` extension method that allows you to obtain a collection of all registered query-based sync subscriptions. ([Issue #1838](https://github.com/realm/realm-dotnet/issues/1838))
* Added `AccessLevel` property to `PathPermission` to replace the now deprecated `MayRead/MayWrite/MayManage`. ([Issue #1863](https://github.com/realm/realm-dotnet/issues/1863))
* Added `RealmOwnerId` property to `PathPermission` that indicates who the owner of the Realm is. ([Issue #1863](https://github.com/realm/realm-dotnet/issues/1863))
* Added support for building with `dotnet build` (previously only the `msbuild` command line was supported). ([PR #1849](https://github.com/realm/realm-dotnet/pull/1849))
* Improved query performance for unindexed string columns when the query has a long chain of OR conditions. (Core upgrade)
* Improved performance of encryption and decryption significantly by utilizing hardware optimized encryption functions. (Core upgrade)
* Compacting a realm into an encrypted file could take a really long time. The process is now optimized by adjusting the write buffer size relative to the used space in the realm. (Core upgrade)
* The string-based query parser (`results.Filter("...")`) now supports readable timestamps with a 'T' separator in addition to the originally supported "@" separator. For example: `startDate > 1981-11-01T23:59:59:1` (Core upgrade)

### Fixed
* Fixes an issue where using the `StringExtensions.Contains(string, string, StringComparison)` extension method inside a LINQ query would result in an exception being thrown on .NET Core 2.1+ or Xamarin.iOS/Android projects.([Issue #1848](https://github.com/realm/realm-dotnet/issues/1848))
* Creating an object after creating an object with the int primary key of "null" would hit an assertion failure. (Core upgrade)

### Compatibility
* Realm Object Server: 3.23.1 or later.

### Internal
* Upgraded Sync from 3.14.11 to 4.5.1 and Core 5.12.7 to 5.20.0.

3.4.0 (2019-01-09)
------------------

**NOTE!!! You will need to upgrade your Realm Object Server to at least version 3.11.0 or use Realm Cloud. If you try to connect to a ROS v3.10.x or previous, you will see an error like `Wrong protocol version in Sync HTTP request, client protocol version = 25, server protocol version = 24`.**

### Enhancements
* Download progress is now reported to the server, even when there are no local changes. This allows the server to do history compaction much more aggressively, especially when there are many clients that rarely or never make local changes. ([#1772](https://github.com/realm/realm-dotnet/pull/1772))
* Reduce memory usage when integrating synchronized changes sent by ROS.
* Added ability to supply a custom log function for handling logs emitted by Sync by specifying `SyncConfigurationBase.CustomLogger`. It must be set before opening a synchronized Realm. ([#1824](https://github.com/realm/realm-dotnet/pull/1824))
* Clients using protocol 25 now report download progress to the server, even when they make no local changes. This allows the server to do history compaction much more aggressively, especially when there are many clients that rarely or never make local changes. ([#1772](https://github.com/realm/realm-dotnet/pull/1772))
* Add a User-Agent header to HTTP requests made to the Realm Object Server. By default, this contains information about the Realm library version and .NET platform. Additional details may be provided (such as the application name/version) by setting `SyncConfigurationBase.UserAgent` prior to opening a synchronized Realm. If developing a Xamarin app, you can use the Xamarin.Essentials plugin to automate that: `SyncConfiguration.UserAgent = $"{AppInfo.Name} ({AppInfo.PackageName} {AppInfo.VersionString})"`.

### Fixed
* Fixed a bug that could lead to crashes with a message such as `Assertion failed: ndx < size() with (ndx, size()) = [742, 742]`.
* Fixed a bug that resulted in an incorrect `LogLevel` being sent to Sync when setting `SyncConfigurationBase.LogLevel`. ([#1824](https://github.com/realm/realm-dotnet/pull/1824), since 2.2.0)
* Fixed a bug that prevented `Realm.GetInstanceAsync` from working when used with `QueryBasedSyncConfiguration`. ([#1827](https://github.com/realm/realm-dotnet/pull/1827), since 3.1.0)

### Breaking Changes
* The deprecated method `realm.SubscribeToObjectsAsync` has been removed in this version. ([#1772](https://github.com/realm/realm-dotnet/pull/1772))
* `User.ConfigurePersistence` has been deprecated in favor of `SyncConfigurationBase.Initialize`.

### Compatibility
* Realm Object Server: 3.11.0 or later.
The sync protocol version has been bumped to version 25. The server is backwards-compatible with clients using protocol version 24 or below, but clients at version 25 are not backwards-compatible with a server at protocol version 24. The server must be upgraded before any clients are upgraded.

### Internal
* Upgraded Sync from 3.9.2 to 3.14.11 and Core from 5.8.0 to 5.12.7.


3.3.0 (2018-11-08)
------------------

### Enhancements
* Exposed an `OnProgress` property on `SyncConfigurationBase`. It allows you to specify a progress callback that will be invoked when using `Realm.GetInstanceAsync` to report the download progress. ([#1807](https://github.com/realm/realm-dotnet/pull/1807))

### Fixed
<!-- * <How to hit and notice issue? what was the impact?> ([#????](https://github.com/realm/realm-dotnet/issues/????), since v?.?.?) -->
* Trying to call `Subscription.WaitForSynchronizationAsync` on a background thread (without a `SynchronizationContext`) would previously hang indefinitely. Now a meaningful exception will be thrown to indicate that this is not supported and this method should be called on a thread with a synchronization context. ([dotnet-private#130](https://github.com/realm/realm-dotnet-private/issues/130), since v3.0.0)

### Compatibility
* Realm Object Server: 3.0.0 or later.
* APIs are backwards compatible with all previous releases in the 3.x.y series.
* File format: Generates Realms with format v9 (Reads and upgrades all previous formats)


3.2.1 (2018-09-27)
------------------

### Bug fixes
- Fixed a bug that would typically result in exceptions with a message like `An unknown error has occurred. State: *some-number-larger than 127*`
when subscribing to queries. ([dotnet-private#128](https://github.com/realm/realm-dotnet-private/issues/128), since `3.0.0`)

3.2.0 (2018-08-04)
------------------

### Enhancements
- `RealmObject` inheritors will now raise `PropertyChanged` after they have been removed from Realm.
The property name in the event arguments will be `IsValid`.
- Bundle some common certificate authorities on Linux so connecting to ROS instances over SSL should work out of the box
for most certificates. Notably, it will now work out of the box for Realm Cloud instances.

### Bug fixes
- When constructing queries that compare an invalid/unmanaged RealmObject (e.g. `realm.All<Foo>().Where(f => f.Bar == someBar)`),
a meaningful exception will now be thrown rather than an obscure ArgumentNullException.
- Added `ShouldCompactOnLaunch` to the PCL version of the library. ([dotnet-private#125](https://github.com/realm/realm-dotnet-private/issues/125))

3.1.0 (2018-07-04)
------------------

### Enhancements
- Exposed a `ChangeSet.NewModifiedIndices` collection that contains information about the
indices of the objects that changed in the new version of the collection (i.e. after
accounting for the insertions and deletions).
- Update Fody to 3.0.

### Bug fixes
- `WriteAsync` will no longer perform a synchronous `Refresh` on the main thread. ([#1729](https://github.com/realm/realm-dotnet/pull/1729))
- Trying to add a managed Realm Object to a different instance of the same on-disk Realm will no
longer throw an exception.
- Removed the `IList` compliance for Realm collections. This fixes an issue which would cause the app to hang
on Android when deselecting an item from a ListView bound to a Realm collection.

### Breaking Changes
- `SyncConfiguration` is now deprecated and will be removed in a future version. Two new configuration
classes have been exposed - [QueryBasedSyncConfiguration](https://docs.realm.io/platform/using-synced-realms/syncing-data#using-query-based-synchronization)
and [FullSyncConfiguration](https://docs.realm.io/platform/using-synced-realms/syncing-data#full-synchronization).
If you were using a `SyncConfiguration` with `IsPartial = true`, then change your code to use
`QueryBasedSyncConfiguration`. Similarly, if `IsPartial` was not set or was set to `false`, use
`FullSyncConfiguration`.
- Removed the `IList` compliance for Realm collections. This will prevent automatic updates of ListViews
databound to Realm collections in UWP projects.

3.0.0 (2018-04-16)
------------------

### Enhancements
- Allow `[MapTo]` to be applied on classes to change the name of the table corresponding to that class. ([#1712](https://github.com/realm/realm-dotnet/pull/1712))
- Added an improved API for adding subscriptions in partially-synchronized Realms. `IQueryable<T>.Subscribe` can be used
to subscribe to any query, and the returned `Subscription<T>` object can be used to observe the state of the subscription
and ultimately remove the subscription. See the [documentation](https://docs.realm.io/platform/v/3.x/using-synced-realms/syncing-data)
for more information. ([#1679](https://github.com/realm/realm-dotnet/pull/1679))
- Added a fine-grained permissions system for use with partially-synchronized Realms. This allows permissions to be
defined at the level of individual objects or classes. See the
[documentation](https://docs.realm.io/platform/v/3.x/using-synced-realms/access-control)
for more information. ([#1714](https://github.com/realm/realm-dotnet/pull/1714))
- Exposed a string-based `IQueryable<T>.Filter(predicate)` method to enable more advanced querying
scenarios such as:
  - Following links: `realm.All<Dog>().Filter("Owner.FirstName BEGINSWITH 'J'")`.
  - Queries on collections: `realm.All<Child>().Filter("Parents.FirstName BEGINSWITH 'J'")` - find all
  children who have a parent whose name begins with J or `realm.All<Child>().Filter("Parents.@avg.Age > 50")` -
  find all children whose parents' average age is more than 50.
  - Subqueries: `realm.All<Person>().Filter("SUBQUERY(Dogs, $dog, $dog.Vaccinated == false).@count > 3")` - find all
  people who have more than 3 unvaccinated dogs.
  - Sorting: `realm.All<Dog>().Filter("TRUEPREDICATE SORT(Owner.FirstName ASC, Age DESC)")` - find all dogs and
  sort them by their owner's first name in ascending order, then by the dog's age in descending.
  - Distinct: `realm.All<Dog>().Filter("TRUEPREDICATE DISTINCT(Age) SORT(Name)")` - find all dogs, sort them
  by their name and pick one dog for each age value.
  - For more examples, check out the
  [javascript query language docs](https://github.com/realm/realm-js/blob/master/docs/tutorials/query-language.md) -
  the query syntax is identical - or the [NSPredicate Cheatsheet](https://academy.realm.io/posts/nspredicate-cheatsheet/).
- The `SyncConfiguration` constructor now accepts relative Uris. ([#1720](https://github.com/realm/realm-dotnet/pull/1720))
- Added the following methods for resetting the user's password and confirming their email:
`RequestPasswordResetAsync`, `CompletePasswordResetAsync`, `RequestEmailConfirmationAsync`, and `ConfirmEmailAsync`.
These all apply only to users created via `Credentials.UsernamePassword` who have provided their email as
the username. ([#1721](https://github.com/realm/realm-dotnet/pull/1721))

### Bug fixes
- Fixed a bug that could cause deadlocks on Android devices when resolving thread safe references. ([#1708](https://github.com/realm/realm-dotnet/pull/1708))

### Breaking Changes
- Uses the Sync 3.0 client which is incompatible with ROS 2.x.
- `Permission` has been renamed to `PathPermission` to more closely reflect its purpose.
Furthermore, existing methods to modify permissions only work on full Realms. New methods
and classes are introduced to configure access to a partially synchronized Realm.
- The type of `RealmConfiguration.DefaultConfiguration` has changed to `RealmConfigurationBase` to allow
any subclass to be set as default. ([#1720](https://github.com/realm/realm-dotnet/pull/1720))
- The `SyncConfiguration` constructor arguments are now optional. The `user` value will default to the
currently logged in user and the `serverUri` value will default to `realm://MY-SERVER-URL/default` where
`MY-SERVER-URL` is the host the user authenticated against. ([#1720](https://github.com/realm/realm-dotnet/pull/1720))
- The `serverUrl` argument in `User.LoginAsync(credentials, serverUrl)` and `User.GetLoggedInUser(identity, serverUrl)`
has been renamed to `serverUri` for consistency. ([#1721](https://github.com/realm/realm-dotnet/pull/1721))


2.2.0 (2017-03-22)
------------------

### Enhancements
- Added an `IsDynamic` property to `RealmConfigurationBase`, allowing you to open a Realm file and read its schema from disk. ([#1637](https://github.com/realm/realm-dotnet/pull/1637))
- Added a new `InMemoryConfiguration` class that allows you to create an in-memory Realm instance. ([#1638](https://github.com/realm/realm-dotnet/pull/1638))
- Allow setting elements of a list directly - e.g. `foo.Bars[2] = new Bar()` or `foo.Integers[3] = 5`. ([#1641](https://github.com/realm/realm-dotnet/pull/1641))
- Added Json Web Token (JWT) credentials provider. ([#1655](https://github.com/realm/realm-dotnet/pull/1655))
- Added Anonymous and Nickname credentials providers. ([#1671](https://github.com/realm/realm-dotnet/pull/1671))

### Bug fixes
- Fixed an issue where initial collection change notification is not delivered to all subscribers. ([#1696](https://github.com/realm/realm-dotnet/pull/1696))
- Fixed a corner case where `RealmObject.Equals` would return `true` for objects that are no longer managed by Realm. ([#1698](https://github.com/realm/realm-dotnet/pull/1698))

### Breaking Changes
- `SyncConfiguration.SetFeatureToken` is deprecated and no longer necessary in order to use Sync on Linux or server-side features. ([#1703](https://github.com/realm/realm-dotnet/pull/1703))

2.1.0 (2017-11-13)
------------------

### Enhancements
- Added an `[Explicit]` attribute that can be applied to classes or assemblies. If a class is decorated with it, then it will not be included in the default schema for the Realm (i.e. you have to explicitly set `RealmConfiguration.ObjectClasses` to an array that contains that class). Similarly, if it is applied to an assembly, all classes in that assembly will be considered explicit. This is useful when developing a 3rd party library that depends on Realm to avoid your internal classes leaking into the user's schema. ([#1602](https://github.com/realm/realm-dotnet/pull/1602))

### Bug fixes
- Fixed a bug that would prevent writing queries that check if a related object is null, e.g. `realm.All<Dog>().Where(d => d.Owner == null)`. ([#1601](https://github.com/realm/realm-dotnet/pull/1601))
- Addressed an issue that would cause the debugger to report an unobserved exception being thrown when "Just My Code" is disabled. ([#1603](https://github.com/realm/realm-dotnet/pull/1603))
- Calling `Realm.DeleteRealm` on a synchronized Realm will now properly delete the `realm.management` folder. ([#1621](https://github.com/realm/realm-dotnet/pull/1621))
- Fixed a crash when accessing primitive list properties on objects in realms opened with a dynamic schema (e.g. in migrations). ([#1629](https://github.com/realm/realm-dotnet/pull/1629))

2.0.0 (2017-10-17)
------------------

### Enhancements
- Added support for collections of primitive values. You can now define properties as `IList<T>` where `T` can be any
type supported by Realm, except for another `IList`. As a result, a lot of methods that previously had constraints on
`RealmObject` now accept any type and may throw a runtime exception if used with an unsupported type argument.
([#1517](https://github.com/realm/realm-dotnet/pull/1517))
- Added `HelpLink` pointing to the relevant section of the documentation to most Realm exceptions. ([#1521](https://github.com/realm/realm-dotnet/pull/1521))
- Added `RealmObject.GetBacklinks` API to dynamically obtain all objects referencing the current one. ([#1533](https://github.com/realm/realm-dotnet/pull/1533))
- Added a new exception type, `PermissionDeniedException`, to denote permission denied errors when working with synchronized Realms that
exposes a method - `DeleteRealmUserInfo` - to inform the binding that the offending Realm's files should be kept or deleted immediately.
This allows recovering from permission denied errors in a more robust manner. ([#1543](https://github.com/realm/realm-dotnet/pull/1543))
- The keychain service name used by Realm to manage the encryption keys for sync-related metadata on Apple platforms is now set to the
bundle identifier. Keys that were previously stored within the Realm-specific keychain service will be transparently migrated to the
per-application keychain service. ([#1522](https://github.com/realm/realm-dotnet/pull/1522))
- Added a new exception type -  `IncompatibleSyncedFileException` - that allows you to handle and perform data migration from a legacy (1.x) Realm file
to the new 2.x format. It can be thrown when using `Realm.GetInstance` or `Realm.GetInstanceAsync` and exposes a `GetBackupRealmConfig` method
that allows you to open the old Realm file in a dynamic mode and migrate any required data. ([#1552](https://github.com/realm/realm-dotnet/pull/1552))
- Enable encryption on Windows. ([#1570](https://github.com/realm/realm-dotnet/pull/1570))
- Enable Realm compaction on Windows. ([#1571](https://github.com/realm/realm-dotnet/pull/1571))
- `UserInfo` has been significantly enhanced. It now contains metadata about a user stored on the Realm Object Server, as well as a list of all user
account data associated with that user. ([#1573](https://github.com/realm/realm-dotnet/pull/1573))
- Introduced a new method - `User.LogOutAsync` to replace the now-deprecated synchronous call. ([#1574](https://github.com/realm/realm-dotnet/pull/1574))
- Exposed `BacklinksCount` property on `RealmObject` that returns the number of objects that refer to the current object via a to-one or a to-many relationship. ([#1578](https://github.com/realm/realm-dotnet/pull/1578))
- String primary keys now support `null` as a value. ([#1579](https://github.com/realm/realm-dotnet/pull/1579))
- Add preview support for partial synchronization. Partial synchronization allows a synchronized Realm to be opened in such a way
that only objects requested by the user are synchronized to the device. You can use it by setting the `IsPartial` property on a
`SyncConfiguration`, opening the Realm, and then calling `Realm.SubscribeToObjectsAsync` with the type of object you're interested in,
a string containing a query determining which objects you want to subscribe to, and a callback which will report the results. You may
add as many subscriptions to a synced Realm as necessary. ([#1580](https://github.com/realm/realm-dotnet/pull/1580))
- Ensure that Realm collections (`IList<T>`, `IQueryable<T>`) will not change when iterating in a `foreach` loop. ([#1589](https://github.com/realm/realm-dotnet/pull/1589))

### Bug fixes
- `Realm.GetInstance` will now advance the Realm to the latest version, so you no longer have to call `Refresh` manually after that. ([#1523](https://github.com/realm/realm-dotnet/pull/1523))
- Fixed an issue that would prevent iOS Share Extension projects from working. ([#1535](https://github.com/realm/realm-dotnet/pull/1535))

### Breaking Changes
- `Realm.CreateObject(string className)` now has additional parameter `object primaryKey`. You *must* pass that when creating a new object using the dynamic API. If the object you're creating doesn't have primary key declared, pass `null`. ([#1381](https://github.com/realm/realm-dotnet/pull/1381))
- `AcceptPermissionOfferAsync` now returns the relative rather than the absolute url of the Realm the user has been granted permissions to. ([#1595](https://github.com/realm/realm-dotnet/pull/1595))

1.6.0 (2017-08-14)
------------------

### Enhancements
- Exposed `Realm.WriteCopy` API to copy a Realm file and optionally encrypt it with a different key. ([#1464](https://github.com/realm/realm-dotnet/pull/1464))
- The runtime representations of all Realm collections (`IQueryable<T>` and `IList<T>`) now implement the `IList` interface that is needed for data-binding to `ListView` in UWP applications. ([#1469](https://github.com/realm/realm-dotnet/pull/1469))
- Exposed `User.RetrieveInfoForUserAsync` API to allow admin users to lookup other users' identities in the Realm Object Server. This can be used, for example, to find a user by knowing their Facebook id. ([#1486](https://github.com/realm/realm-dotnet/pull/1486))
- Added a check to verify there are no duplicate object names when creating the schema. ([#1502](https://github.com/realm/realm-dotnet/pull/1502))
- Added more comprehensive error messages when passing an invalid url scheme to `SyncConfiguration` or `User.LoginAsync`. ([#1501](https://github.com/realm/realm-dotnet/pull/1501))
- Added more meaningful error information to exceptions thrown by `Realm.GetInstanceAsync`. ([#1503](https://github.com/realm/realm-dotnet/pull/1503))
- Added a new type - `RealmInteger<T>` to expose Realm-specific API over base integral types. It can be used to implement [counter functionality](https://realm.io/docs/realm-object-server/#counters) in synced realms. ([#1466](https://github.com/realm/realm-dotnet/pull/1466))
- Added `PermissionCondition.Default` to apply default permissions for existing and new users. ([#1511](https://github.com/realm/realm-dotnet/pull/1511))

### Bug fixes
- Fix an exception being thrown when comparing non-constant character value in a query. ([#1471](https://github.com/realm/realm-dotnet/pull/1471))
- Fix an exception being thrown when comparing non-constant byte or short value in a query. ([#1472](https://github.com/realm/realm-dotnet/pull/1472))
- Fix a bug where calling the non-generic version of `IQueryProvider.CreateQuery` on Realm's IQueryable results, an exception would be thrown. ([#1487](https://github.com/realm/realm-dotnet/pull/1487))
- Trying to use an `IList` or `IQueryable` property in a LINQ query will now throw `NotSupportedException` rather than crash the app. ([#1505](https://github.com/realm/realm-dotnet/pull/1505))

### Breaking Changes

1.5.0 (2017-06-20)
------------------

### Enhancements
- Exposed new API on the `User` class for working with permissions: ([#1361](https://github.com/realm/realm-dotnet/pull/1361))
  - `ApplyPermissionsAsync`, `OfferPermissionsAsync`, and `AcceptPermissionOfferAsync` allow you to grant, revoke, offer, and accept permissions.
  - `GetPermissionOffers`, `GetPermissionOfferResponses`, and `GetPermissionChanges` allow you to review objects, added via the above mentioned methods.
  - `GetGrantedPermissionsAsync` allows you to inspect permissions granted to or by the current user.
- When used with `RealmConfiguration` (i.e. local Realm), `Realm.GetInstanceAsync` will perform potentially costly operation, such as executing migrations or compaction on a background thread. ([#1406](https://github.com/realm/realm-dotnet/pull/1406))
- Expose `User.ChangePasswordAsync(userId, password)` API to allow admin users to change other users' passwords. ([#1412](https://github.com/realm/realm-dotnet/pull/1412))
- Expose `SyncConfiguration.TrustedCAPath` API to allow providing a custom CA that will be used to validate SSL traffic to the Realm Object Server.  ([#1423](https://github.com/realm/realm-dotnet/pull/1423))
- Expose `Realm.IsInTransaction` API to check if there's an active transaction for that Realm. ([#1452](https://github.com/realm/realm-dotnet/pull/1452))

### Bug fixes
- Fix a crash when querying over properties that have `[MapTo]` applied. ([#1405](https://github.com/realm/realm-dotnet/pull/1405))
- Fix an issue where synchronized Realms did not connect to the remote server in certain situations, such as when an application was offline when the Realms were opened but later regained network connectivity. ([#1407](https://github.com/realm/realm-dotnet/pull/1407))
- Fix an issue where incorrect property name will be passed to `RealmObject.PropertyChanged` subscribers when the actual changed property is below a `Backlink` property. ([#1433](https://github.com/realm/realm-dotnet/pull/1433))
- Fix an exception being thrown when referencing Realm in a PCL test assembly without actually using it. ([#1434](https://github.com/realm/realm-dotnet/pull/1434))
- Fix a bug when `SyncConfiguration.EnableSSLValidation` would be ignored when passed to `Realm.GetInstanceAsync`. ([#1423](https://github.com/realm/realm-dotnet/pull/1423))

### Breaking Changes
- The constructors of `PermissionChange`, `PermissionOffer`, and `PermissionOfferResponse` are now private. Use the new `User.ApplyPermissionsAsync`, `User.OfferPermissionsAsync`, and `User.AcceptPermissionOfferAsync` API. ([#1361](https://github.com/realm/realm-dotnet/pull/1361))
- `User.GetManagementRealm` and `User.GetPermissionRealm` are now deprecated. Use the new permission related API on `User` to achieve the same results. ([#1361](https://github.com/realm/realm-dotnet/pull/1361))
- `User.ChangePassword(password)` has been renamed to `User.ChangePasswordAsync(password)`. ([#1412](https://github.com/realm/realm-dotnet/pull/1412))
- Removed the following obsolete API: ([#1425](https://github.com/realm/realm-dotnet/pull/1425))
  - `Realm.ObjectForPrimaryKey<T>(long id)`
  - `Realm.ObjectForPrimaryKey<T>(string id)`
  - `Realm.ObjectForPrimaryKey(string className, long id)`
  - `Realm.ObjectForPrimaryKey(string className, string id)`
  - `Realm.Manage<T>(T obj, bool update)`
  - `Realm.Close()`
  - `Realm.CreateObject<T>()`
  - `IOrderedQueryable<T>.ToNotifyCollectionChanged<T>(Action<Exception> errorCallback)`
  - `IOrderedQueryable<T>.ToNotifyCollectionChanged<T>(Action<Exception> errorCallback, bool coalesceMultipleChangesIntoReset)`
  - `IRealmCollection<T>.ObjectSchema`
- `Realm.DeleteRealm` now throws an exception if called while an instance of that Realm is still open.

1.4.0 (2017-05-19)
------------------

### Enhancements
- Expose `RealmObject.OnManaged` virtual method that can be used for init purposes, since the constructor is run before the object has knowledge of its Realm. (#1383)
- Expose `Realm.GetInstanceAsync` API to asynchronously open a synchronized Realm. It will download all remote content available at the time the operation began on a background thread and then return a usable Realm. It is also the only supported way of opening Realms for which the user has only read permissions.

1.3.0 (2017-05-16)
------------------

### Universal Windows Platform
Introducing Realm Mobile Database for Universal Windows Platform (UWP). With UWP support, you can now build mobile apps using Realm’s object database for the millions of mobile, PC, and Xbox devices powered by Windows 10. The addition of UWP support allows .NET developers to build apps for virtually any modern Windows Platform with Windows Desktop (Win32) or UWP as well as for iOS and Android via Xamarin. Note that sync support is not yet available for UWP, though we are working on it and you can expect it soon.

### Enhancements
- Case insensitive queries against a string property now use a new index based search. (#1380)
- Add `User.ChangePassword` API to change the current user's password if using Realm's 'password' authentication provider. Requires any edition of the Realm Object Server 1.4.0 or later. (#1386)
- `SyncConfiguration` now has an `EnableSSLValidation` property (default is `true`) to allow SSL validation to be specified on a per-server basis. (#1387)
- Add `RealmConfiguration.ShouldCompactOnLaunch` callback property when configuring a Realm to determine if it should be compacted before being returned. (#1389)
- Silence some benign linker warnings on iOS. (#1263)
- Use reachability API to minimize the reconnection delay if the network connection was lost. (#1380)

### Bug fixes
- Fixed a bug where `Session.Reconnect` would not reconnect all sessions. (#1380)
- Fixed a crash when subscribing for `PropertyChanged` multiple times. (#1380)
- Fixed a crash when reconnecting to Object Server (#1380)
- Fixed a crash on some Android 7.x devices when opening a realm (#1380)

1.2.1 (2017-05-01)
------------------

### Bug fixes
- Fixed an issue where `EntryPointNotFoundException` would be thrown on some Android devices. (#1336)

### Enhancements
- Expose `IRealmCollection.IsValid` to indicate whether the realm collection is valid to use. (#1344)
- Update the Fody reference which adds support for building with Mono 5. (#1364)

1.2.0 (2017-04-04)
------------------

Realm is now being distributed as a .NET Standard 1.4 library as this is a requirement for supporting UWP. While internally that is a rather big move, applications using it should not be affected. After the upgrade, you'll see a number of new NuGet dependencies being added - those are reference assemblies, already part of mscorlib, so will not affect your application's size or performance. Additionally, we're releasing a new platform specific DataBinding package that contains helper methods that enable two-way databinding scenarios by automatically creating transactions when setting a property.

If you encounter any issues after the upgrade, we recommend clearing the `bin` and `obj` folders and restarting Xamarin Studio. If this doesn't help, please file an issue explaining your solution setup and the type of problems you encounter.

Files written with this version cannot be read by earlier versions of Realm. This version is not compatible with versions of the Realm Object Server lower than 1.3.0.

### Bug fixes
- Fixes the `RemoveAll(string)` overload to work correctly. (#1288)
- Resolved an issue that would lead to crashes when refreshing the token for an invalid session. (#1289)
- The `IObservable` returned from `session.GetProgressObservable` will correctly call `OnComplete` when created with `mode: ProgressMode.ForCurrentlyOutstandingWork`. (#1292)
- Fixed a memory leak when accessing string properties. (#1318)
- Fixes an issue when using `EncryptionKey` with synchronized realms. (#1322)

### Enhancements
- Introduce APIs for safely passing objects between threads. Create a thread-safe reference to a thread-confined object by passing it to the `ThreadSafeReference.Create` factory method, which you can then safely pass to another thread to resolve in the new realm with `Realm.ResolveReference`. (#1300)
- Introduce API for attempting to reconnect all sessions. This could be used in conjunction with the [connectivity plugin](https://github.com/jamesmontemagno/ConnectivityPlugin) to monitor for connectivity changes and proactively request reconnecting, rather than rely on the built-in retry mechanism. (#1310)
- Enable sorting over to-one relationships, e.g. `realm.All<Parent>().OrderBy(p => p.Child.Age)`. (#1313)
- Introduce a `string.Like` extension method that can be used in LINQ queries against the underlying database engine. (#1311)
- Add an `User.IsAdmin` property that indicates whether a user is a Realm Object Server administrator. (#1320)

### Breaking Changes
- `DateTimeOffset` properties that are not set will now correctly default to `0001-1-1` instead of `1970-1-1` after the object is passed to `realm.Add`. (#1293)
- Attempting to get an item at index that is out of range should now correctly throw `ArgumentOutOfRangeException` for all `IRealmCollection` implementations. (#1295)
- The layout of the .lock file has changed, which may affect scenarios where different processes attempt to write to the same Realm file at the same time. (#1296)
- `PropertyChanged` notifications use a new, more reliable, mechanism, that behaves slightly differently from the old one. Notifications will be sent only after a transaction is committed (making it consistent with the way collection notifications are handled). To make sure that your UI is promptly updated, you should avoid keeping long lived transactions around. (#1316)

1.1.1 (2017-03-15)
------------------

### Bug fixes

- Resolved an issue that prevented compiling for iOS on Visual Studio. (#1277)

1.1.0 (2017-03-03)
------------------

### Enhancements
- Added Azure Active Directory (AzureAD) credentials provider. (#1254)

### Breaking Changes
This is a preparation release for adding UWP support. We have removed all platform-specific logic from the Realm assemblies, and instead weave them in compile time. While this has been tested in all common scenarios, it may create issues with very complex project graphs. If you encounter any of these issues with iOS projects:
- Compilation fails when running Task `WeaveRealmAssemblies`
- App crashes when first accessing a Realm

please file an issue and explain your solution setup.

1.0.4 (2017-02-21)
------------------

### Bug fixes

- The `Realm` NuGet package no longer clobbers the path to Win32 native binaries in `Realm.Database`. (#1239)
- Fixed a bug where garbage collecting an object with `PropertyChanged` subscribers would cause crashes. (#1237)

1.0.3 (2017-02-14)
------------------
### Out of Beta!
After about a year and a half of hard work, we are proud to call this a 1.0 release. There is still work to do, but Realm Xamarin is now being used by thousands of developers and has proven reliable.

### Sync
Realm Xamarin now works with the Realm Mobile Platform. This means that you can write Xamarin apps that synchronize seamlessly with a Realm Object Server, allowing you to write complex apps with Xamarin that are offline-first and automatically synchronised by adding just a few lines of code.
You can read about this in the [documentation](https://realm.io/docs/xamarin/latest/#sync).

### Windows Desktop
Realm Xamarin is no longer iOS and Android only. You can now use it to write .NET programs for Windows Desktop. Add the NuGet package to your regular .NET project and start using Realm. Some features are not supported on Windows yet. Most notably, sync does not yet work for Windows, but also encryption and notifications across processes are missing. We are working on it and you can expect support soon.

### Breaking Changes
 - `IRealmCollection<T>.ObjectSchema` is deprecated and replaced with `ISchemaSource.ObjectSchema`. (#1216)

### Bug fixes
 - `[MapTo]` attribute is now respected in queries. (#1219)
 - Letting a Realm instance be garbage collected instead of disposing it will no longer lead to crashes. (#1212)
 - Unsubscribing from `RealmObject.PropertyChanged` in a `PropertyChanged` callback should no longer lead to crashes. (#1207)
 - `WriteAsync` now advances the read transaction so the changes made asynchronously are available immediately in the original thread. (#1192)
 - Queries on backlink properties should no longer produce unexpected results. (#1177)


0.82.1 (2017-01-27)
-------------------
### Bug fixes
- Addressed an issue where obtaining a Realm instance, reading an object, then obtaining another instance on the same thread would cause the object to become invalid and crash the application upon accessing any of its members.

0.82.0 (2017-01-23)
-------------------
### Breaking Changes
- Moved all exceptions under the `Realms.Exceptions` namespace. (#1075)
- Moved `RealmSchema` to `Realms.Schema` namespace. (#1075)
- Made the `ErrorEventArgs` constructor internal. (#1075)
- Made `ObjectSchema.Builder` and `RealmSchema.Builder` internal. (#1075)
- Passing an object that has `IList` properties to `Add(obj, update: true)` will no longer merge the lists. Instead, the `IList` property will contain only the items in the object. (#1040)

### Enhancements
- Added virtual `OnPropertyChanged` method in `RealmObject` that you can override to be notified of changes to the current object. (#1047)
- Added compile time checks that `[Required]` is applied on correct property types. (#1072)
- `Realm.Add(RealmObject obj)` will now return the passed in object, similarly to `Realm.Add<T>(T obj)`. (#1162)
- Added an extension method for `string.Contains` that accepts `StringComparison` argument and can be used in queries. When querying, only `StringComparison.Ordinal` and `StringComparison.OrdinalIgnoreCase` can be used. When not used in queries, all values for `StringComparison` are valid. (#1141)

### Bug fixes
- Adding a standalone object, that has an `IList<T>` property that has never been accessed, to the Realm will no longer throw a `NullReferenceException`. (#1040)
- `IList<T>` properties will now correctly return `IsReadOnly = true` when managed by a readonly Realm. (#1070)
- The weaver should now correctly resolve references in PCL and netstandard assemblies. (#1117)
- Add some missing methods to the PCL reference assembly. (#1093)
- Disposed realms will not throw `ObjectDisposedException` when trying to access their members. Additionally, disposing a realm will not invalidate other instances on the same thread. (#1063)

0.81.0 (2016-12-14)
-------------------
### Breaking Changes
* The `IQueryable<T>.ToNotifyCollectionChanged` extension methods that accept parameters are now deprecated. There is a new parameterless one that you should use instead. If you want to handle errors, you can do so by subscribing to the `Realm.OnError` event. (#938)
* `RealmResults<T>` is now marked `internal` and `Realm.All<T>()` will instead return `IQueryable<T>`. We've added a new extension method `IQueryable<T>.SubscribeForNotifications(NotificationCallbackDelegate<T>)` that allows subscribing for notifications. (#942)
* `Realm.CreateObject<T>` has been deprecated and will be removed in the next major release. (It could cause a dangerous data loss when using the synchronised realms coming soon, if a class has a PrimaryKey). (#998)
* `RealmConfiguration.ReadOnly` has been renamed to `RealmConfiguration.IsReadOnly` and is now a property instead of a field. (#858)
* `Realm.All` has been renamed to `Realm.GetAll` and the former has been obsoleted. (#858)
* `Realm.ObjectForPrimaryKey` has been renamed to `Realm.Find` and the former has been obsoleted. (#858)
* `Realm.Manage` has been renamed to `Realm.Add` and the former has been obsoleted. (#858)
* `RealmConfiguration.PathToRealm` has been renamed to `Realm.GetPathToRealm` and the former has been obsoleted. (#858)
* `RealmResults.NotificationCallback` has been extracted as a non-nested class and has been renamed to `NotificationCallbackDelegate`. (#858)
* `Realm.Close` has been removed in favor of `Realm.Dispose`. (#858)
* `RealmList<T>` is now marked `internal`. You should use `IList<T>` to define collection relationships. (#858)

### Enhancements
* In data-binding scenarios, if a setter is invoked by the binding outside of write transaction, we'll create an implicit one and commit it. This enables two-way data bindings without keeping around long-lived transactions. (#901)
* The Realm schema can now express non-nullable reference type properties with the new `[Required]` attribute. (#349)
* Exposed a new `Realm.Error` event that you can subscribe for to get notified for exceptions that occur outside user code. (#938)
* The runtime types of the collection, returned from `Realm.All` and the collection created for `IList<T>` properties on `RealmObject` now implement `INotifyCollectionChanged` so you can pass them for data-binding without any additional casting. (#938, #909)
* All RealmObjects implement `INotifyPropertyChanged`. This allows you to pass them directly for data-binding.
* Added `Realm.Compact` method that allows you to reclaim the space used by the Realm. (#968)
* `Realm.Add` returns the added object. (#931)
* Support for backlinks aka `LinkingObjects`. (#219)
* Added an `IList<T>.Move` extension method that allows you to reorder elements within the collection. For managed Lists, it calls a native method, so it is slightly more efficient than removing and inserting an item, but more importantly, it will raise the `CollectionChanged` with `NotifyCollectionChangedAction.Move` which will result in a nice move animation, rather than a reload of a ListView. (#995)

### Bug fixes
* Subscribing to `PropertyChanged` on a RealmObject and modifying an instance of the same object on a different thread will now properly raise the event. (#909)
* Using `Insert` to insert items at the end of an `IList` property will no longer throw an exception. (#978)

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
