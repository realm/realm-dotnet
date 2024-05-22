## 12.2.0 (2024-05-22)

### Enhancements
* Added support for `Migration.FindInNewRealm` which is a helper that allows you to lookup the object in the post-migration Realm that corresponds to an object from the pre-migration Realm. (Issue [#3600](https://github.com/realm/realm-dotnet/issues/3600))
* Added `[System.Reflection.Obfuscation]` on the generated `RealmSchema` field to improve compatibility with obfuscation tools that change field and property names of generated classes. (Issue [#3574](https://github.com/realm/realm-dotnet/issues/3574))
* Added support for list and dictionaries of `RealmValue` (`IList<RealmValue>` and `IDictionary<string, RealmValue>`) to be contained in a `RealmValue`. Lists and dictionaries can contain an arbitrary number of collections themselves. It is possible to convert an existing collection to a `RealmValue` using the new static methods `RealmValue.List` and `RealmValue.Dictionary` or using the implicit operators if converting from common types like `List`, `RealmValue[]` or `Dictionary`. Finally, it is possible to obtain the contained collections by using the new conversion method `AsList` and `AsDictionary`. For example:

  ```csharp
  var list = new List<RealmValue> { 1, true, "stringVal" };

  var rvo = realm.Write(() =>
  {
      return realm.Add(new RealmValueObject { RealmValueProperty = list});
  });

  var retrievedList = rvo.RealmValueProperty.AsList();
  ```
  (PR [#3441](https://github.com/realm/realm-dotnet/pull/3441))

### Fixed
* Accessing `App.CurrentUser` from within a `User.Changed` notification would deadlock. (Core 14.7.0)
* Inserting the same link to the same key in a dictionary more than once would incorrectly create multiple backlinks to the object. This did not appear to cause any crashes later, but would have affected the value returned by `RealmObject.BacklinksCount` and queries involving backlinks counts. (Core 14.7.0)
* Fixed an issue that would cause `RealmObject.DynamicApi.GetList/Set/Dictionary` to fail when the collection contains primitive values. (Issue [#3597](https://github.com/realm/realm-dotnet/issues/3597))

### Compatibility
* Realm Studio: 15.0.0 or later.

### Internal
* Using Core 14.7.0.

## 12.1.0 (2024-05-01)

### Enhancements
* Added an experimental API to update the base url for an application at runtime - `App.UpdateBaseUriAsync()`. This intended to be used for roaming between edge server and cloud. (Issue [#3521](https://github.com/realm/realm-dotnet/issues/3521))

### Fixed
* The returned value from `MongoClient.Collection.FindOneAsync` is now a nullable document to more explicitly convey that `null` may be returned in case no object matched the filter. (PR [#3586](https://github.com/realm/realm-dotnet/pull/3586))
* Fixed crash when integrating removal of already removed dictionary key. (Core 14.5.2)
* `App.AllUsers` included logged out users only if they were logged out while the App instance existed. It now always includes all logged out users. (Core 14.6.0)
* Fixed several issues around encrypted file portability (copying a "bundled" encrypted Realm from one device to another): (Core 14.6.0)
  * Fixed `Assertion failed: new_size % (1ULL << m_page_shift) == 0` when opening an encrypted Realm less than 64Mb that was generated on a platform with a different page size than the current platform.
  * Fixed a `DecryptionFailed` exception thrown when opening a small (<4k of data) Realm generated on a device with a page size of 4k if it was bundled and opened on a device with a larger page size.
  * Fixed an issue during a subsequent open of an encrypted Realm for some rare allocation patterns when the top ref was within ~50 bytes of the end of a page. This could manifest as a DecryptionFailed exception or as an assertion: `encrypted_file_mapping.hpp:183: Assertion failed: local_ndx < m_page_state.size()`.
* Schema initialization could hit an assertion failure if the sync client applied a downloaded changeset while the Realm file was in the process of being opened. (Core 14.6.0)
* Improve perfomance of "chained OR equality" queries for UUID/ObjectId types and RQL parsed "IN" queries on string/int/uuid/objectid types. (Core 14.6.0)
* Fixed a bug when running a IN query (or a query of the pattern `x == 1 OR x == 2 OR x == 3`) when evaluating on a string property with an empty string in the search condition. Matches with an empty string would have been evaluated as if searching for a null string instead. (Core 14.6.1)

### Compatibility
* Realm Studio: 15.0.0 or later.

### Internal
* Using Core 14.6.1.

## 12.0.0 (2024-04-17)

**File format version bumped. Old files will be automatically upgraded but cannot be downgraded and opened with older versions of the .NET SDK.**

### Breaking Changes
* Added automatic serialization and deserialization of Realm classes when using methods on `MongoClient.Collection`, without the need to annotate classes with `MongoDB.Bson`attributes. This feature required to change the default serialization for various types (including `DateTimeOffset`). If you prefer to use the previous serialization, you need to call `Realm.SetLegacySerialization` before any kind of serialization is done, otherwise it may not work as epxected. [#3459](https://github.com/realm/realm-dotnet/pull/3459)
* `SyncProgress.TransferredBytes` and `SyncProgress.TransferableBytes` have been removed in favour of `SyncProgress.ProgressEstimate`, a double value between 0.0 and 1.0 that expresses the percentage estimate of the current progress. (Issue [#3478](https://github.com/realm/realm-dotnet/issues/3478]))
* Support for upgrading from Realm files produced by RealmCore v5.23.9 (Realm .NET v5.0.1) or earlier is no longer supported. (Core 14.0.0)
* `String` and `byte[]` are now strongly typed for comparisons and queries. This change is especially relevant when querying for a string constant on a `RealmValue` property, as now only strings will be returned. If searching for binary data is desired, then that type must be specified by the constant. In RQL (`.Filter()`) the new way to specify a binary constant is to use `RealmValueProp = bin('xyz')` or `RealmValueProp = binary('xyz')`. (Core 14.0.0)
* Sorting order of strings has changed to use standard unicode codepoint order instead of grouping similar english letters together. A noticeable change will be from "aAbBzZ" to "ABZabz". (Core 14.0.0)
* In RQL (`Filter()`), if you want to query using `@type` operation, you must use `objectlink` to match links to objects. `object` is reserved for dictionary types. (Core 14.0.0)
* Opening realm with file format 23 or lower (Realm .NET versions earlier than 12.0.0) in read-only mode will crash. (Core 14.0.0)

### Enhancements
* Reduced memory usage of `RealmValue`. (PR [#3441](https://github.com/realm/realm-dotnet/pull/3441))
* Add support for passing a key paths collection (`KeyPathsCollection`) when using `IRealmCollection.SubscribeForNotifications`. Passing a `KeyPathsCollection` allows to specify which changes in properties should raise a notification.

  A `KeyPathsCollection` can be obtained by:
  - building it explicitly by using the methods `KeyPathsCollection.Of` or `KeyPathsCollection.Of<T>`;
  - building it implicitly with the conversion from a `List` or array of `KeyPath` or strings;
  - getting one of the static values `Full` and `Shallow` for full and shallow notifications respectively.

  A `KeyPath` can be obtained by implicit conversion from a string or built from an expression using the `KeyPath.ForExpression<T>` method.

  For example:
  ```csharp
  var query = realm.All<Person>();

  KeyPath kp1 = "Email";
  KeyPath kp2 = KeyPath.ForExpression<Person>(p => p.Name);

  KeyPathsCollection kpc;

  //Equivalent declarations
  kpc = KeyPathsCollection.Of("Email", "Name");
  kpc = KeyPathsCollection.Of<Person>(p => p.Email, p => p.Name);
  kpc = new List<KeyPath> {"Email", "Name"};
  kpc = new List<KeyPath> {kp1, kp2};

  query.SubscribeForNotifications(NotificationCallback, kpc);
  ```
  (PR [#3501 ](https://github.com/realm/realm-dotnet/pull/3501))
* Added the `MongoClient.GetCollection<T>` method to get a collection of documents from MongoDB that can be deserialized in Realm objects. This methods works the same as `MongoClient.GetDatabase(dbName).GetCollection(collectionName)`, but the database name and collection name are automatically derived from the Realm object class.  [#3414](https://github.com/realm/realm-dotnet/pull/3414)
* Improved performance of RQL (`.Filter()`) queries on a non-linked string property using: >, >=, <, <=, operators and fixed behaviour that a null string should be evaulated as less than everything, previously nulls were not matched. (Core 13.27.0)
* Updated bundled OpenSSL version to 3.2.0. (Core 13.27.0)
* Storage of Decimal128 properties has been optimised so that the individual values will take up 0 bits (if all nulls), 32 bits, 64 bits or 128 bits depending on what is needed. (Core 14.0.0)
* Add support for collection indexes in RQL (`Filter()`) queries.
  For example:
  ```csharp
  var people = realm.All<Person>();

  //People whose first dog is called "Fluffy"
  var query1 = people.Filter("ListOfDogs[FIRST].Name = $0", "Fluffy")

  //People whose last dog is called "Fluffy"
  var query2 = people.Filter("ListOfDogs[LAST].Name = $0", "Fluffy")

  //People whose second dog is called "Fluffy"
  var query3 = people.Filter("ListOfDogs[2].Name = $0", "Fluffy")

  //People that have a dog called "Fluffy"
  var query4 = people.Filter("ListOfDogs[*].Name = $0", "Fluffy")

  //People that have 3 dogs
  var query5 = people.Filter("ListOfDogs[SIZE] = $0", 3)
  ```
  (Core 14.0.0)
* Added support for indexed `RealmValue` properties. (PR [#3544](https://github.com/realm/realm-dotnet/pull/3544))
* Improve performance of object notifiers with complex schemas and very simple changes to process by as much as 20%. (Core 14.2.0)
* Improve performance with very large number of notifiers as much as 75%. (Core 14.2.0)
* Improve file compaction performance on platforms with page sizes greater than 4k (for example arm64 Apple platforms) for files less than 256 pages in size. (Core 14.4.0)
* The default base url in `AppConfiguration` has been updated to point to `services.cloud.mongodb.com`. See https://www.mongodb.com/docs/atlas/app-services/domain-migration/ for more information. (Issue [#3551](https://github.com/realm/realm-dotnet/issues/3551))

### Fixed
* Fixed RQL (`.Filter()`) queries like `indexed_property == NONE {x}` which mistakenly matched on only x instead of not x. This only applies when an indexed property with equality (==, or IN) matches with `NONE` on a list of one item. If the constant list contained more than one value then it was working correctly. (Core 13.27.0)
* Uploading the changesets recovered during an automatic client reset recovery may lead to 'Bad server version' errors and a new client reset. (Core 13.27.0)
* Fixed crash in fulltext index using prefix search with no matches. (Core 13.27.0)
* Fixed a crash with Assertion `failed: m_initiated` during sync session startup. (Core 13.27.0)
* Fixed a TSAN violation where the user thread could race to read m_finalized with the sync event loop. (Core 13.27.0)
* Fix a minor race condition when backing up Realm files before a client reset which could have lead to overwriting an existing file. (Core 13.27.0)
* Boolean property `ChangeSet.IsCleared` that is true when the collection gets cleared is now also raised for `IDictionary`, aligning it to `ISet` and `IList`. (Core 14.0.0)
* Fixed equality queries on `RealmValue` properties with an index. (Core 14.0.0)
* Fixed a crash that would happen when more than 8388606 links were pointing to a specific object.
* Fixed wrong results when querying for `NULL` value in `IDictionary`. (Core 14.0.0)
* A Realm generated on a non-apple ARM 64 device and copied to another platform (and vice-versa) were non-portable due to a sorting order difference. This impacts strings or binaries that have their first difference at a non-ascii character. These items may not be found in a set, or in an indexed column if the strings had a long common prefix (> 200 characters). (Core 14.0.0)
* Fixed an issue when removing items from a LnkLst that could result in invalidated links becoming visable which could cause crashes or exceptions when accessing those list items later on. This affects sync Realms where another client had previously removed a link in a linklist that has over 1000 links in it, and then further local removals from the same list caused the list to have fewer than 1000 items. (Core 14.2.0)
* Fix a spurious crash related to opening a Realm on background thread while the process was in the middle of exiting. (Core 14.3.0)
* Fix opening realm with cached user while offline results in fatal error and session does not retry connection. (Core 14.4.0)
* Fix an assertion failure "m_lock_info && m_lock_info->m_file.get_path() == m_filename" that appears to be related to opening a Realm while the file is in the process of being closed on another thread. (Core 14.5.0)
* Fixed diverging history due to a bug in the replication code when setting default null values (embedded objects included). (Core 14.5.0)
* Null pointer exception may be triggered when logging out and async commits callbacks not executed. (Core 14.5.0)

### Compatibility
* Realm Studio: 15.0.0 or later.

### Internal
* Using Core 14.5.1.

## 11.7.0 (2024-02-05)

### Enhancements
* Automatic client reset recovery now does a better job of recovering changes when changesets were downloaded from the server after the unuploaded local changes were committed. If the local Realm happened to be fully up to date with the server prior to the client reset, automatic recovery should now always produce exactly the same state as if no client reset was involved. (Core 13.24.1)
* Exceptions thrown during bootstrap application will now be surfaced to the user rather than terminating the program with an unhandled exception. (Core 13.25.0)
* Allow the using `>`, `>=`, `<`, `<=` operators in `Realm.Filter()` queries for string constants. This is a case sensitive lexicographical comparison. Improved performance of RQL (`.Filter()`) queries on a non-linked string property using: >, >=, <, <=, operators and fixed behaviour that a null string should be evaluated as less than everything, previously nulls were not matched. (Core 13.26.0-14-gdf25f)
* `Session.GetProgressObservable` can now be used with Flexible Sync. (Issue [#3478](https://github.com/realm/realm-dotnet/issues/3478]))

### Fixed
* Automatic client reset recovery would duplicate insertions in a list when recovering a write which made an unrecoverable change to a list (i.e. modifying or deleting a pre-existing entry), followed by a subscription change, followed by a write which added an entry to the list. (Core 13.24.0)
* During a client reset recovery a Set of links could be missing items, or an exception could be thrown that prevents recovery. (Core 13.24.0)
* During a client reset with recovery when recovering a move or set operation on a `IList<RealmObject>` or `IList<RealmValue>` that operated on indices that were not also added in the recovery, links to an object which had been deleted by another client while offline would be recreated by the recovering client. But the objects of these links would only have the primary key populated and all other fields would be default values. Now, instead of creating these zombie objects, the lists being recovered skip such deleted links. (Core 13.24.0)
* Errors encountered while reapplying local changes for client reset recovery on partition-based sync Realms would result in the client reset attempt not being recorded, possibly resulting in an endless loop of attempting and failing to automatically recover the client reset. (Core 13.24.0)
* Changesets have wrong timestamps if the local clock lags behind 2015-01-01T00:00:00Z. The sync client now throws an exception if that happens. (Core 13.24.1)
* If the very first open of a flexible sync Realm triggered a client reset, the configuration had an initial subscriptions callback, both before and after reset callbacks, and the initial subscription callback began a read transaction without ending it (which is normally going to be the case), opening the frozen Realm for the after reset callback would trigger a BadVersion exception. (Core 13.24.1)
* Automatic client reset recovery on flexible sync Realms would apply recovered changes in multiple write transactions, releasing the write lock in between. (Core 13.24.1)
* Having a class name of length 57 would make client reset crash as a limit of 56 was wrongly enforced. (Core 13.24.1)
* Fixed several causes of "decryption failed" exceptions that could happen when opening multiple encrypted Realm files in the same process while using Apple/linux and storing the Realms on an exFAT file system. (Core 13.24.1)
* Fixed several errors that could cause a crash of the sync client. (Core 13.25.0)
* Bad performance of initial Sync download involving many backlinks. (Core 13.25.1)
* Explicitly bumped the minimum version of System.Net.Security to 4.3.2 as 4.3.0 has been marked as vulnerable (more details can be found in the deprecation notice on the [NuGet page](https://www.nuget.org/packages/System.Net.Security/4.3.0)).
* Handle EOPNOTSUPP when using posix_fallocate() and fallback to manually consume space. This should enable android users to open a Realm on restrictive filesystems. (Core 13.26.0)
* Application may crash with incoming_changesets.size() != 0 when a download message is mistaken for a bootstrap message. This can happen if the synchronization session is paused and resumed at a specific time. (Core 13.26.0)
* Fixed errors complaining about missing symbols such as `__atomic_is_lock_free` on ARMv7 Linux (Core 13.26.0)
* Uploading the changesets recovered during an automatic client reset recovery may lead to 'Bad server version' errors and a new client reset. (Core 13.26.0-14-gdf25f)
* Fixed invalid data in error reason string when registering a subscription change notification after the subscription has already failed. (Core 13.26.0-14-gdf25f)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core v13.26.0-14-gdf25f.

## 11.6.1 (2023-11-17)

### Fixed
* Fixed FLX subscriptions not being sent to the server if the session was interrupted during bootstrapping. (Core 13.23.3)
* Fixed FLX subscriptions not being sent to the server if an upload message was sent immediately after a subscription was committed but before the sync client checks for new subscriptions. (Core 13.23.3)
* Fixed application crash with 'KeyNotFound' exception when subscriptions are marked complete after a client reset. (Core 13.23.3)
* A crash at a very specific time during a DiscardLocal client reset on a FLX Realm could leave subscriptions in an invalid state. (Core 13.23.4)
* Fixed an error "Invalid schema change (UPLOAD): cannot process AddColumn instruction for non-existent table" when using automatic client reset with recovery in dev mode to recover schema changes made locally while offline. (Core 13.23.4)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.23.4.

## 11.6.0 (2023-11-03)

### Enhancements
* Added the `App.EmailPasswordAuth.RetryCustomConfirmationAsync` method to be able to run again the confirmation function on the server for a given email. (Issue [#3463](https://github.com/realm/realm-dotnet/issues/3463))
* Added `User.Changed` event that can be used to notify subscribers that something about the user changed - typically this would be the user state or the access token. (Issue [#3429](https://github.com/realm/realm-dotnet/issues/3429))
* Added support for customizing the ignore attribute applied on certain generated properties of Realm models. The configuration option is called `realm.custom_ignore_attribute` and can be set in a global configuration file (more information about global configuration files can be found in the [.NET documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files)). The Realm generator will treat this as an opaque string, that will be appended to the `IgnoreDataMember` and `XmlIgnore` attributes already applied on these members. The attributes must be fully qualified unless the namespace they reside in is added to a global usings file. For example, this is how you would add `JsonIgnore` from `System.Text.Json`:

  ```
  realm.custom_ignore_attribute = [System.Text.Json.Serialization.JsonIgnore]
  ```
  (Issue [#2579](https://github.com/realm/realm-dotnet/issues/2579))
* The Realm source generator will now error out in case a collection in the model classes is assigned to a non-null value either in a property initializer or in a constructor. Realm collections are initialized internally and assigning non-null values to the property is not supported, where the `null!` assignment is only useful to silence nullable reference type warnings, in reality the collection will never be null. (Issue [#3455](https://github.com/realm/realm-dotnet/issues/3455))
* Made WebSocket error logging more verbose when using `AppConfiguration.UseManagedWebSockets = true`. [#3459](https://github.com/realm/realm-dotnet/pull/3459)

### Fixed
* Added an error that is raised when interface based Realm classes are used with a language version lower than 8.0. At the same time, removed the use of `not` in the generated code, so that it's compatible with a minumum C# version of 8.0. (Issue [#3265](https://github.com/realm/realm-dotnet/issues/3265))
* Logging into a single user using multiple auth providers created a separate SyncUser per auth provider. This mostly worked, but had some quirks:
  - Sync sessions would not necessarily be associated with the specific SyncUser used to create them. As a result, querying a user for its sessions could give incorrect results, and logging one user out could close the wrong sessions.
  - Existing local synchronized Realm files created using version of Realm from August - November 2020 would sometimes not be opened correctly and would instead be redownloaded.
  - Removing one of the SyncUsers would delete all local Realm files for all SyncUsers for that user.
  - Deleting the server-side user via one of the SyncUsers left the other SyncUsers in an invalid state.
  - A SyncUser which was originally created via anonymous login and then linked to an identity would still be treated as an anonymous users and removed entirely on logout.
  (Core 13.21.0)
* If a user was logged out while an access token refresh was in progress, the refresh completing would mark the user as logged in again and the user would be in an inconsistent state (Core 13.21.0).
* If querying over a geospatial dataset that had some objects with a type property set to something other than 'Point' (case insensitive) an exception would have been thrown. Instead of disrupting the query, those objects are now just ignored. (Core 13.21.0)
* Receiving a write_not_allowed error from the server would have led to a crash. (Core 13.22.0)
* Updating subscriptions did not trigger Realm autorefreshes, sometimes resulting in async refresh hanging until another write was performed by something else. (Core 13.23.1)
* Fix interprocess locking for concurrent realm file access resulting in a interprocess deadlock on FAT32/exFAT filesystems. (Core 13.23.1)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.23.1.

## 11.5.0 (2023-09-15)

### Enhancements
* Streamlined some of the error codes reported in `SessionException`. A few error codes have been combined and some have been deprecated since they are no longer reported by the server. (Issue [#3295](https://github.com/realm/realm-dotnet/issues/3295))
* Full text search supports searching for prefix only. Eg. "description TEXT 'alex*'". (Core 13.18.0)
* Unknown protocol errors received from Atlas Device Sync will no longer cause the application to crash if a valid error action is also received. Unknown error actions will be treated as an ApplicationBug error action and will cause sync to fail with an error via the sync error handler. (Core 13.18.0)
* Added support for server log messages that are enabled by sync protocol version 10. Appservices request id will be provided in a server log message in a future server release. (Core 13.19.0)

### Fixed
* Fixed the message of the `MissingMemberException` being thrown when attempting to access a non-existent property with the dynamic API. (PR [#3432](https://github.com/realm/realm-dotnet/pull/3432))
* Fixed a `Cannot marshal generic Windows Runtime types with a non Windows Runtime type as a generic type argument` build error when using .NET Native. (Issue [#3434](https://github.com/realm/realm-dotnet/issues/3434), since 11.4.0)
* Fix failed assertion for unknown app server errors. (Core 13.17.2)
* Running a query on @keys in a Dictionary would throw an exception. (Core 13.17.2)
* Fixed crash in slab allocator (`Assertion failed: ref + size <= next->first`). (Core 13.20.1)
* Sending empty UPLOAD messages may lead to 'Bad server version' errors and client reset. (Core 13.20.1)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.20.1.

## 11.4.0 (2023-08-16)

### Enhancements
* Added `IQueryable.SubscribeAsync` API as a shorthand for using `SubscriptionSet.Add`. It is a syntax sugar that roughly translates to:
  ```csharp
  realm.Subscriptions.Update(() =>
  {
    realm.Subscriptions.Add(query);
  });

  await realm.Subscriptions.WaitForSynchronization();

  // This can now be expressed as
  await query.SubscribeAsync();
  ```
  It offers a parameter to control whether to wait every time for synchronization or just the first time a subscription is added, as well as cancellation token support. (PR [#3403](https://github.com/realm/realm-dotnet/pull/3403))
* Added an optional `cancellationToken` argument to `Session.WaitForDownloadAsync/WaitForUploadAsync`. (PR [#3403](https://github.com/realm/realm-dotnet/pull/3403))
* Added an optional `cancellationToken` argument to `SubscriptionSet.WaitForSynchronization`. (PR [#3403](https://github.com/realm/realm-dotnet/pull/3403))
* Fixed a rare corruption of files on streaming format (often following compact, convert or copying to a new file). (Core 13.17.1)
* Trying to search a full-text indexes created as a result of an additive schema change (i.e. applying the differences between the local schema and a synchronized realm's schema) could have resulted in an IllegalOperation error with the error code `Column has no fulltext index`. (Core 13.17.1)
* Sync progress for DOWNLOAD messages from server state was updated wrongly. This may have resulted in an extra round-trip to the server. (Core 13.17.1)
* Added option to use managed WebSockets ([`System.Net.WebSockets.ClientWebSocket`](https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket)) instead of Realm's built-in WebSocket client for Sync traffic. Managed WebSockets offer improved support for proxies and firewalls that require authentication. This feature is currently opt-in and can be enabled by setting `AppConfiguration.UseManagedWebSockets` to true. Managed WebSockets will become the default in a future version. ([PR #3412](https://github.com/realm/realm-dotnet/pull/3412)).
* Fixed an issue that would make `realm.SyncSession` garbage collected even when there are subscribers to `realm.SyncSession.PropertyChanged`.

### Fixed
* Fixed a race condition between canceling an async write transaction and closing the Realm file, which could result in an `ObjectDisposedException : Safe handle has been closed` being thrown. ([PR #3400](https://github.com/realm/realm-dotnet/pull/3400))
* Fixed an issue where in the extremely rare case that an exception is thrown by `Realm.RefreshAsync`, that exception would have been ignored and `false` would have been returned. ([PR #3400](https://github.com/realm/realm-dotnet/pull/3400))
* Fixed the nullability annotation of `SubscriptionSet.Find` to correctly indicate that `null` is returned if the subscription doesn't exist in the subscription set. (PR [#3403](https://github.com/realm/realm-dotnet/pull/3403))
* Fixed an issue where executing `Filter` queries using remapped properties would only work with the native name rather than the managed one. Now both will work - e.g.:
  ```csharp
  partial class MyModel : IRealmObject
  {
    [MapTo("Bar")]
    public int Foo { get; set; }
  }

  // Both of these are valid now
  realm.All<MyModel>().Filter("Foo > 5");
  realm.All<MyModel>().Filter("Bar > 5");
  ```
  (Issue [#3149](https://github.com/realm/realm-dotnet/issues/3149))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.17.1

## 11.3.0 (2023-07-26)

### Breaking Changes
* `AppConfiguration.LocalAppName` and `AppConfiguration.LocalAppVersion` have been deprecated and will be removed in a future version. They have never had an effect as the values supplied by the SDK was never sent to the server. (PR [#3387](https://github.com/realm/realm-dotnet/pull/3387))

### Enhancements
* Added `App.BaseFilePath`, `App.BaseUri`, and `App.Id` properties that return the values supplied in `AppConfiguration`. (PR [#3385](https://github.com/realm/realm-dotnet/pull/3385))
* Added `AppConfiguration.UseAppCache` property that controls whether the `App` instance returned from `App.Create` should be cached or not. The general recommendation is to not set it (i.e. leave the default value of `true`), but it can be useful when writing unit tests. (Issue [#3382](https://github.com/realm/realm-dotnet/issues/3382)).

### Fixed
* Fixed a Unity Editor crash when the domain is reloaded while a `Realm.GetInstanceAsync` operation is in progress. (Issue [#3344](https://github.com/realm/realm-dotnet/issues/3344))
* Fixed the implementation `App.Equals` and `App.GetHashCode` to return correct results, particularly when the `App` instance is cached. (PR [#3385](https://github.com/realm/realm-dotnet/pull/3385))
* Fixed an issue where building for Android on Unity would fail with "Could not analyze the user's assembly. Object reference not set to an instance of an object". (Issue [#3380](https://github.com/realm/realm-dotnet/issues/3380))
* A GeoBox is now just a shortcut for the equivilent GeoPolygon. This provides consistent query results and error checking. (Core 13.15.2)
* Fixed several corner cases (eg. around the poles) where invalid points matched a geoWithin query. (Core 13.15.2)
* Fixed an error during async open and client reset if properties have been added to the schema. This fix applies to PBS to FLX migration if async open is used. (Core 13.16.1)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.17.0

## 11.2.0 (2023-07-07)

### Enhancements
* Added validation checks to the geospatial type constructors. This means that an exception will now be thrown when constructing an invalid geospatial shape rather than when using it in a query. (PR [#3362](https://github.com/realm/realm-dotnet/pull/3362))
* Relaxed some validations when invoking `IndexOf(null)` on a collection of non-nullable types. Previously, this would throw an `ArgumentNullException` whereas now it will return `-1`. This is particularly useful for data-binding scenarios where the binding engine might invoke it as `IndexOf(SelectedItem)` which would throw an exception when `SelectedItem` is `null`. (PR [#3369](https://github.com/realm/realm-dotnet/pull/3369))
* Changed `RealmSet.IndexOf` implementation to return the actual result rather than throw a `NotSupportedException`. The order of persisted sets is still non-deterministic, but is stable between write transactions. Again, this is mostly useful for data-binding scenarios where the set is passed as a binding context to a collection control. (PR [#3369](https://github.com/realm/realm-dotnet/pull/3369))

### Fixed
* Fixed an issue on Unity on Windows when the weaver would trigger excessive terminal windows to open. (Issue [3364]https://github.com/realm/realm-dotnet/issues/3364)
* Fixed an issue on Unity on CI where weaving would fail with the following error: `Could not analyze the user's assembly. Cannot access a closed Stream.`. (Issue [3364]https://github.com/realm/realm-dotnet/issues/3364)
* Fixed a `NullReferenceException` when weaving classes on Unity in batch mode. (Issue [#3363](https://github.com/realm/realm-dotnet/issues/3363))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.15.0

## 11.1.2 (2023-06-20)

### Fixed
* Fixed a namespacing issue that would cause Maui Android projects to fail to build due to `'Realm' is a namespace but is used like a type`. (Issue [#3351](https://github.com/realm/realm-dotnet/issues/3351))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.15.0

## 11.1.1 (2023-06-19)

### Fixed
* Fixed a namespacing issue that would cause Unity projects to fail to build due to `'Realm' is a namespace but is used like a type`. (Issue [#3351](https://github.com/realm/realm-dotnet/issues/3351))
* Improved the warning message when adding Realm attributes on a non-persisted property. (Issue [#3352](https://github.com/realm/realm-dotnet/issues/3352))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.15.0.

## 11.1.0 (2023-06-17)

### Enhancements
* Deprecate the `Realm.SourceGenerator` and `Realm.Fody` packages. The source generation and weaver assemblies are now contained in the main `Realm` package. This should be a transparent change for users who only referenced the `Realm` package, but if you explicitly added a package reference to `Realm.SourceGenerator` or `Realm.Fody`, you should remove it. (PR [#3319](https://github.com/realm/realm-dotnet/pull/3319))
* Automatically handle `RealmObject`->`EmbeddedObject` migrations by duplicating objects referenced by multiple parents as well as removing "orphaned" objects. (Issue [#2408](https://github.com/realm/realm-dotnet/issues/2408))
* New notifiers can now be registered in write transactions until changes have actually been made in the write transaction. This makes it so that new notifications can be registered inside change notifications triggered by beginning a write transaction (unless a previous callback performed writes). (Core 13.10.1)
* Partition-Based to Flexible Sync Migration for migrating a client app that uses partition based sync to use flexible sync under the hood if the server has been migrated to flexible sync is officially supported with this release. Any clients using an older version of Realm (including the original support released in Core 11.0.0) will receive a "switch to flexible sync" error message when trying to sync with the app. (Core 13.11.0)
* Support sort/distinct based on values from a dictionary e.g. `.Filter("TRUEPREDICATE SORT(meta['age'])")`. (Core 13.14.0)
* Added initial support for geospatial queries on points. (Issue [#3299](https://github.com/realm/realm-dotnet/issues/3299))
  * In this version, only queries of the form "is this point contained in this shape" (equivalent to [$geoWithin](https://www.mongodb.com/docs/manual/reference/operator/query/geoWithin/) in MongoDB) are supported.
  * There is no index support right now.
  * There is no dedicated type for persisted geospatial points. Instead, points should be stored as GeoJson-shaped embedded object and queries will use duck-typing to check if the shape contains the object. For convenience, here's an example embedded object that you can use in lieu of a Realm-provided dedicated type:
    ```csharp
    public partial class Location : IEmbeddedObject
    {
      // The coordinates and type properties are mandatory but may be private.
      // You can add more fields if necessary - those will be ignored when doing
      // geospatial queries.
      [MapTo("coordinates")]
      private IList<double> Coordinates { get; } = null!;

      [MapTo("type")]
      private string Type { get; set; } = "Point";

      public double Latitude => Coordinates.Count > 1 ? Coordinates[1] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");

      public double Longitude => Coordinates.Count > 1 ? Coordinates[0] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");

      public Location(double latitude, double longitude)
      {
        // According to the GeoJson spec, longitude must come first in the
        // coordinates array.
        Coordinates.Add(longitude);
        Coordinates.Add(latitude);
      }
    }

    // Example usage
    public partial class Company : IRealmObject
    {
      public Location Location { get; set; }
    }
    ```
  * Three new shape types and one helper point type are added to allow you to check for containment:
    * `GeoPoint`: a building block for the other shape types - it cannot be used as a property type on your models and is only intended to construct the other shape types. It can be constructed implicitly from a value tuple of latitude and longitude:
      ```csharp
      var point = new GeoPoint(latitude: 12.345, longitude: 67.890);
      var point = (12.345, 67.890);
      ```
    * `GeoCircle`: a shape representing a circle on a sphere constructed from a center and radius:
      ```csharp
      var circle = new GeoCircle(center: (12.34, 56.78), radius: 10); // radius in radians
      var circle = new GeoCircle((12.34, 56.78), Distance.FromKilometers(10));
      ```
    * `GeoBox`: a shape representing a box on a sphere constructed from its bottom left and top right corners:
      ```csharp
      var box = new GeoBox((12.34, 56.78), (15.34, 59.78));
      ```
    * `GeoPolygon`: an arbitrary polygon constructed from an outer ring and optional holes:
      ```csharp
      var polygon = new GeoPolygon((10, 10), (20, 20), (0, 20), (10, 10)); // a triangle with no holes

      var outerRing = new GeoPoint[] { (10, 10), (20, 20), (0, 20), (10, 10) };
      var hole1 = new GeoPoint[] { (1, 1), (2, 2), (0, 2), (1, 1) };
      var hole2 = new GeoPoint[] { (5, 5), (6, 6), (4, 6), (5, 5) };

      var polygon = new GeoPolygon(outerRing, hole1, hole2); // A triangle with two smaller triangular holes
      ```
  * Querying can be done either via LINQ or RQL:
    ```csharp
    var matches = realm.All<Company>().Where(c => QueryMethods.GeoWithin(c.Location, circle));
    var matches = realm.All<Company>().Filter("Location GEOWITHIN $0", circle);
    ```
* Support sort/distinct based on values from a dictionary e.g. `realm.All<MyModel>().Filter("TRUEPREDICATE SORT(meta['age'])")`. (Core 13.14.0)
* Fixed a potential crash when opening the realm after failing to download a fresh FLX realm during an automatic client reset. (Core 13.14.0)

### Fixed
* Fixed a fatal error (reported to the sync error handler) during client reset (or automatic PBS to FLX migration) if the reset has been triggered during an async open and the schema being applied has added new classes. (Core 13.11.0)
* Full text search would sometimes find words where the word only matches the beginning of the search token. (Core 13.11.0)
* We could crash when removing backlinks in cases where forward links did not have a corresponding backlink due to corruption. We now silently ignore this inconsistency in release builds, allowing the app to continue. (Core 13.12.0)
* `IDictionary<string, IRealmObject?>` would expose unresolved links rather than mapping them to null. In addition to allowing invalid objects to be read from Dictionaries, this resulted in queries on Dictionaries sometimes having incorrect results. (Core 13.12.0)
* Access token refresh for websockets was not updating the location metadata. (Core 13.13.0)
* Using both synchronous and asynchronous transactions on the same thread or scheduler could hit the assertion failure "!realm.is_in_transaction()" if one of the callbacks for an asynchronous transaction happened to be scheduled during a synchronous transaction. (Core 13.13.0)
* Fixed a potential crash when opening the realm after failing to download a fresh FLX realm during an automatic client reset. (Core 13.14.0)
* Setting a property containing an embedded object to the same embedded object used to throw an exception with the text `Can't link to an embedded object that is already managed`. Now it is a no-op instead. (Issue [#3262](https://github.com/realm/realm-dotnet/issues/3262))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.15.0.
* Overhauled and extended the metrics collection of the SDK to better drive future development effort. (PR [#3209](https://github.com/realm/realm-dotnet/pull/3209))

## 11.0.0 (2023-05-08)

### Breaking changes
* The `error` argument in `NotificationCallbackDelegate` and `DictionaryNotificationCallbackDelegate` used in `*collection*.SubscribeForNotifications` has been removed. It has been unused for a long time, since internal changes to the database made it impossible for errors to occur during notification callbacks. (Issue [#3014](https://github.com/realm/realm-dotnet/issues/3014))
* Removed `RealmObjectBase.GetBacklinks` - instead `RealmObjectBase.DynamicApi.GetBacklinksFromType` should be used. (Issue [#2391](https://github.com/realm/realm-dotnet/issues/2391))
* Removed `Realm.DynamicApi.CreateObject(string, object)` and replaced it with more specialized overloads:
  * `RealmObjectBase.DynamicApi.CreateObject(string)` can be used to create an object without a primary key.
  * `RealmObjectBase.DynamicApi.CreateObject(string, string/long?/ObjectId?/Guid?)` can be used to create an object with a primary key of the corresponding type.
* The API exposed by `Realm.DynamicApi` no longer return `dynamic`, instead opting to return concrete types, such as `IRealmObject`, `IEmbeddedObject`, and so on. You can still cast the returned objects to `dynamic` and go through the dynamic API, but that's generally less performant than using the string-based API, such as `IRealmObjectBase.DynamicApi.Get/Set`, especially on AOT platforms such as iOS or Unity. (Issue [#2391](https://github.com/realm/realm-dotnet/issues/2391))
* Removed `Realm.WriteAsync(Action<Realm>)` in favor of `Realm.WriteAsync(Action)`. The new `WriteAsync` method introduced in 10.14.0 is more efficient and doesn't require reopening the Realm on a background thread. While not recommended, if you prefer to get the old behavior, you can write an extension method like:
  ```csharp
  public static async Task WriteAsync(this Realm realm, Action<Realm> writeAction)
  {
    await Task.Run(() =>
    {
      using var bgRealm = Realm.GetInstance(realm.Config);
      bgRealm.Write(() =>
      {
        writeAction(bgRealm);
      });
    });

    await realm.RefreshAsync();
  }
  ```
  (PR [#3234](https://github.com/realm/realm-dotnet/pull/3234))
* Removed `InMemoryConfiguration.EncryptionKey`. It was never possible to encrypt in-memory Realms and setting that property would have resulted in runtime errors. (PR [#3236](https://github.com/realm/realm-dotnet/pull/3236))
* Removed `SyncConfiguration` - use `PartitionSyncConfiguration` or `FlexibleSyncConfiguration` instead. (PR [#3237](https://github.com/realm/realm-dotnet/pull/3237))
* Removed `Realm.GetSession` - use `Realm.SyncSession` instead. (PR [#3237](https://github.com/realm/realm-dotnet/pull/3237))
* Removed `DiscardLocalResetHandler` - use `DiscardUnsyncedChangedHandler` instead. (PR [#3237](https://github.com/realm/realm-dotnet/pull/3237))
* Removed `Session.SimulateClientReset` extensions. These didn't work with automatic reset handlers and were more confusing than helpful. (PR [#3237](https://github.com/realm/realm-dotnet/pull/3237))
* Removed `AppConfiguration.CustomLogger` and `AppConfiguration.LogLevel` - use `Logger.Default` and `Logger.LogLevel` instead. (PR [#3238](https://github.com/realm/realm-dotnet/pull/3238))
* Removed `RealmConfigurationBase.ObjectClasses` - use `RealmConfigurationBase.Schema` instead. (PR [#3240](https://github.com/realm/realm-dotnet/pull/3240))
* Removed `ObjectSchema.IsEmbedded` - use `ObjectSchema.BaseType` instead. (PR [#3240](https://github.com/realm/realm-dotnet/pull/3240))
* Removed `ObjectSchema.Builder.IsEmbedded` - use `ObjectSchema.Builder.RealmSchemaType` instead. (PR [#3240](https://github.com/realm/realm-dotnet/pull/3240))
* Removed `ObjectSchema.Builder(string name, bool isEmbedded = false)` - use `Builder(string name, ObjectSchemaType schemaType)` instead. (PR [#3240](https://github.com/realm/realm-dotnet/pull/3240))
* Removed `RealmSchema.Find` - use `RealmSchema.TryFindObjectSchema` instead. (PR [#3240](https://github.com/realm/realm-dotnet/pull/3240))
* Removed `User.GetPushClient` as it has been deprecated in Atlas App Services - see https://www.mongodb.com/docs/atlas/app-services/reference/push-notifications/. (PR [#3241](https://github.com/realm/realm-dotnet/pull/3241))
* Removed `SyncSession.Error` event - use `SyncConfigurationBase.OnSessionError` when opening a Realm instead. (PR [#3241](https://github.com/realm/realm-dotnet/pull/3242))
* Removed the parameterless constructor for `ManualRecoveryHandler` - use the one that takes a callback instead. (PR [#3241](https://github.com/realm/realm-dotnet/pull/3242))
* `RealmValue.AsString` will now throw an exception if the value contains `null`. If you want to get a nullable string, use `AsNullableString`. (PR [#3245](https://github.com/realm/realm-dotnet/pull/3245))
* `RealmValue.AsData` will now throw an exception if the value contains `null`. If you want to get a nullable `byte[]`, use `AsNullableData`. (PR [#3245](https://github.com/realm/realm-dotnet/pull/3245))
* `RealmValue.AsRealmObject` will now throw an exception if the value contains `null`. If you want to get a nullable string, use `AsNullableRealmObject`. (PR [#3245](https://github.com/realm/realm-dotnet/pull/3245))
* `Realm.SyncSession` will now throw an error if the Realm is not opened with a `PartitionSyncConfiguration` or `FlexibleSyncConfiguration` - before it used to return `null`. (PR [#3245](https://github.com/realm/realm-dotnet/pull/3245))
* `Realm.Subscriptions` will now throw an error if the Realm is not opened with a `FlexibleSyncConfiguration` - before it used to return `null`. (PR [#3245](https://github.com/realm/realm-dotnet/pull/3245))
* Removed `PermissionDeniedException` as it was no longer possible to get it. (Issue [#3272](https://github.com/realm/realm-dotnet/issues/3272))
* Removed some obsolete error codes from the `ErrorCode` enum. All codes removed were obsolete and no longer emitted by the server. (PR [3273](https://github.com/realm/realm-dotnet/issues/3273))
* Removed `IncompatibleSyncedFileException` as it was no longer possible to get it. (Issue [#3167](https://github.com/realm/realm-dotnet/issues/3167))
* The `Realms.Schema.Property` API now use `IndexType` rather than a boolean indicating whether a property is indexed. (Issue [#3281](https://github.com/realm/realm-dotnet/issues/3281))
* The extension methods in `StringExtensions` (`Like`, `Contains`) are now deprecated. Use the identical ones in `QueryMethods` instead - e.g. `realm.All<Foo>().Where(f => f.Name.Like("Mic*l"))` would need to be rewritten like `realm.All<Foo>().Where(f => QueryMethods.Like(f.Name, "Mic*l"))`.

### Enhancements
* Added nullability annotations to the Realm assembly. Now methods returning reference types are correctly annotated to indicate whether the returned value may or may not be null. (Issue [#3248](https://github.com/realm/realm-dotnet/issues/3248))
* Replacing a value at an index (i.e. `myList[1] = someObj`) will now correctly raise `CollectionChange` notifications with the `Replace` action. (Issue [#2854](https://github.com/realm/realm-dotnet/issues/2854))
* It is now possible to change the log level at any point of the application's lifetime. (PR [#3277](https://github.com/realm/realm-dotnet/pull/3277))
* Some log messages have been added to the Core database. Events, such as opening a Realm or committing a transaction will now be logged. (Issue [#2910](https://github.com/realm/realm-dotnet/issues/2910))
* Added support for Full-Text search (simple term) queries. (Issue [#3281](https://github.com/realm/realm-dotnet/issues/3281))
  * To enable FTS queries on string properties, add the `[Indexed(IndexType.FullText)]` attribute.
  * To run LINQ queries, use `QueryMethods.FullTextSearch`: `realm.All<Book>().Where(b => QueryMethods.FullTextSearch(b.Description, "fantasy novel"))`.
  * To run `Filter` queries, use the `TEXT` operator: `realm.All<Book>().Filter("Description TEXT $0", "fantasy novel")`.
* Performance improvement for the following queries (Core 13.8.0):
  * Significant (~75%) improvement when counting (`IQueryable.Count()`) the number of exact matches (with no other query conditions) on a string/int/UUID/ObjectID property that has an index. This improvement will be especially noticiable if there are a large number of results returned (duplicate values).
  * Significant (~99%) improvement when querying for an exact match on a `DateTimeOffset` property that has an index.
  * Significant (~99%) improvement when querying for a case insensitive match on a `RealmValue` property that has an index.
  * Moderate (~25%) improvement when querying for an exact match on a Boolean property that has an index.
  * Small (~5%) improvement when querying for a case insensitive match on a `RealmValue` property that does not have an index.
  * Moderate (~30%) improvement of equality queries on a non-indexed `RealmValue`.
* Enable multiple processes to operate on an encrypted Realm simultaneously. (Core 13.9.0)
* Improve performance of rolling back write transactions after making changes. If no notifications events are subscribed to, this is now constant time rather than taking time proportional to the number of changes to be rolled back. Rollbacks when there are notifications subscriptions are 10-20% faster. (Core 13.9.4)
* PBS to FLX Migration for migrating a client app that uses partition based sync to use flexible sync under the hood if the server has been migrated to flexible sync. (Core 13.10.0)

### Fixed
* Fixed an issue that could cause a `The specified table name is already in use` exception when creating a new Realm file on multiple threads. (Issue [#3302](https://github.com/realm/realm-dotnet/issues/3302))
* Fixed a bug that may have resulted in arrays being in different orders on different devices. Some cases of “Invalid prior_size” may be fixed too. (Core 13.7.1)
* Fixed a crash when querying a `RealmValue` property with a string operator (contains/like/beginswith/endswith) or with case insensitivity. (Core 13.8.0)
* Querying for equality of a string on an indexed `RealmValue` property was returning case insensitive matches. For example querying for `myIndexedValue == "Foo"` would incorrectly match on values of "foo" or "FOO" etc. (Core 13.8.0)
* Adding an index to a `RealmValue` property on a non-empty table would crash with an assertion. (Core 13.8.0)
* `SyncSession.Stop()` could hold a reference to the database open after shutting down the sync session, preventing users from being able to delete the realm. (Core 13.8.0)
* Fix a stack overflow crash when using the query parser with long chains of AND/OR conditions. (Core 13.9.0)
* `ClientResetException.InitiateClientReset()` no longer ignores the result of trying to remove a realm. This could have resulted in a client reset action being reported as successful when it actually failed on windows if the `Realm` was still open. (Core 13.9.0)
* Fix a data race where if one thread committed a write transaction which increased the number of live versions above the previous highest seen during the current session at the same time as another thread began a read, the reading thread could read from a no-longer-valid memory mapping (Core 13.9.0).
* Performing a query like `{1, 2, 3, ...} IN list` where the array is longer than 8 and all elements are smaller than some values in list, the program would crash (Core 13.9.4)
* Performing a large number of queries without ever performing a write resulted in steadily increasing memory usage, some of which was never fully freed due to an unbounded cache (Core 13.9.4)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.10.0.

## 10.21.1 (2023-04-21)

### Fixed
* Fixed a crash that occurs when the server sends a PermissionDenied error. (Issue [#3292](https://github.com/realm/realm-dotnet/issues/3292))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.6.0.

## 10.21.0 (2023-03-24)

### Enhancements
* Added `SyncConfiguration.CancelAsyncOperationsOnNonFatalErrors` which controls whether async operations (such as `Realm.GetInstanceAsync`, `Session.WaitForUploadAsync` and so on) should throw an exception whenever a non-fatal session error occurs. (Issue [#3222](https://github.com/realm/realm-dotnet/issues/3222))
* Added `AppConfiguration.SyncTimeoutOptions` which has a handful of properties that control sync timeouts, such as the connection timeout, ping-pong intervals, and others. (Issue [#3223](https://github.com/realm/realm-dotnet/issues/3223))
* Updated some of the exceptions being thrown by the SDK to align them better with system exceptions and include more information - for example, we'll now throw `ArgumentException` when invalid arguments are provided rather than `RealmException`. (Issue [#2796](https://github.com/realm/realm-dotnet/issues/2796))
* Added a new exception - `CompensatingWriteException` that contains information about the writes that have been reverted by the server due to permissions. It will be passed to the supplied `FlexibleSyncConfiguration.OnSessionError` callback similarly to other session errors. (Issue [#3258](https://github.com/realm/realm-dotnet/issues/3258))
* Added support for Linux Arm/Arm64 in .NET applications. (Issue [#721](https://github.com/realm/realm-dotnet/issues/721))

### Fixed
* Changed the way the Realm SDK registers BsonSerializers. Previously, it would indiscriminately register them via `BsonSerializer.RegisterSerializer`, which would conflict if your app was using the `MongoDB.Bson` package and defined its own serializers for `DateTimeOffset`, `decimal`, or `Guid`. Now, registration happens via `BsonSerializer.RegisterSerializationProvider`, which means that the default serializers used by the SDK can be overriden by calling `BsonSerializer.RegisterSerializer` at any point before a serializer is instantiated or by calling `BsonSerializer.RegisterSerializationProvider` after creating an App/opening a Realm. (Issue [#3225](https://github.com/realm/realm-dotnet/issues/3225))
* Creating subscriptions with queries having unicode parameters causes a server error. (Core 13.6.0)
* Fixed an issue with Unity 2022 and later that would result in builds failing with `Specified method is not supported` error. (Issue [#3306](https://github.com/realm/realm-dotnet/issues/3306))

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.6.0.
* Cancel existing builds when a new commit is pushed to a PR. (PR [#3260](https://github.com/realm/realm-dotnet/pull/3260))

## 10.20.0 (2023-02-10)

**File format version bumped. Old files will be automatically upgraded but cannot be downgraded and opened with older versions of the .NET SDK.**

### Breaking Changes
* `User.GetPushClient` has been deprecated as it will be disabled soon on the server. (Issue [#3073](https://github.com/realm/realm-dotnet/issues/3073))

### Enhancements
* The realm file will be shrunk if the larger file size is no longer needed. (Core 13.0.0)
* Most of the file growth caused by version pinning is eliminated. (Core 13.0.0)
* Improve performance of acquiring read locks when a read lock for that version is already held. This speeds up many operations related to change notifications, and particularly refreshing a Realm which has change notifiers registered. (Core 13.2.0)
* Upgrade OpenSSL from 1.1.1n to 3.0.7. (Core 13.2.0)
* Converting flexible sync realms to bundled and local realms is now supported (Core 13.2.0)
* Add support for nested classes for source generated classes. (Issue [#3031](https://github.com/realm/realm-dotnet/issues/3031))
* Enhanced support for nullable reference types in the model definition for source generated classes. This allows to use realm models as usual when nullable context is active, and removes the need to use of the `Required` attribute to indicate required properties, as this information will be inferred directly from the nullability status. There are some considerations regarding the nullability of properties that link to realm object:
  - Properties that link to a single realm object are inherently nullable, and thus the type must be defined as nullable.
  - List, Sets and Backlinks cannot contain null objects, and thus the type parameter must be non-nullable.
  - Dictionaries can contain null values, and thus the type parameter must be nullable.

  Defining the properties with a different nullability annotation than what has been outlined here will raise a diagnostic error. For instance:
  ```cs
  public partial class Person: IRealmObject
  {
      //Single values
      public Dog? MyDog { get; set; } //Correct

      public Dog MyDog { get; set; } //Error

      //List
      public IList<Dog> MyDogs { get; } //Correct

      public IList<Dog?> MyDogs { get; } //Error

      //Set
      public ISet<Dog> MyDogs { get; } //Correct

      public ISet<Dog?> MyDogs { get; } //Error

      //Dictionary
      public IDictionary<string, Dog?> MyDogs { get; } //Correct

      public IDictionary<string, Dog> MyDogs { get; } //Error

      //Backlink
      [Realms.Backlink("...")]
      public IQueryable<Dog> MyDogs { get; } //Correct

      [Realms.Backlink("...")]
      public IQueryable<Dog?> MyDogs { get; } //Error
  }
  ```
  We realise that some developers would still prefer to have more freedom in the nullability annotation of such properties, and it is possible to do so by setting  `realm.ignore_objects_nullability = true` in a global configuration file (more information about global configuration files can be found in the [.NET documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files)). If this is enabled, all the previous properties will be considered valid, and the nullability annotations for properties linking to objects will be ignored.
* Improved performance of `PropertyChanged` and `CollectionChanged` notifications. (Issue [#3112](https://github.com/realm/realm-dotnet/issues/3112))
* Added support for tvOS in Xamarin/MAUI and Unity applications. (Issue [#3161](https://github.com/realm/realm-dotnet/issues/3161))
* Improve the performance of `Realm.Freeze()`. (Core 13.3.0)

### Fixed
* `ISet<RealmValue>` consider string and binary data equivalent. This could cause the client to be inconsistent with the server if a string and some binary data with equivalent content was inserted from Atlas. (Core 13.0.0)
* Fixed wrong assertion on query error that could result in a crash. (Core 13.1.0)
* Fixed an issue preventing opening an encrypted file on a device with a page size bigger than the one on which the file was produced. (Core 13.1.1)
* Fixed possible segfault in sync client where async callback was using object after being deallocated (Core 13.2.0)
* Fixed crash when using client reset with recovery and flexible sync with a single subscription (Core 13.2.0)
* Added a more descriptive error message when a model's property is unsupported. It'll now suggest that the target type may need to inherit from `RealmObject`. (Issue [#3162](https://github.com/realm/realm-dotnet/issues/3162))
* Disposing a Realm instance while an active transaction is running will now correctly roll back the transaction. (Issue [#2924](https://github.com/realm/realm-dotnet/issues/2924))
* Fixed an issue that would cause `PropertyChanged` notifications to be delivered for collection properties when the content of the collection was modified even if the collection itself was not replaced. (Issue [#3112](https://github.com/realm/realm-dotnet/issues/3112))
* Fixed an issue where weaving assemblies on Unity could result in `System.InvalidOperationException`. (Issue [#3199](https://github.com/realm/realm-dotnet/issues/3199))
* `Session.Stop` will now correctly keep the session stopped until `Session.Start` is called. Previously, there were a number of circumstances that could cause the session to be resumed, even if not explicitly requested. (Core 13.3.0)
* When client reset with recovery is used and the recovery does not actually result in any new local commits, the sync client may have gotten stuck in a cycle with a A fatal error occured during client reset: 'A previous 'Recovery' mode reset from <timestamp> did not succeed, giving up on 'Recovery' mode to prevent a cycle' error message. (Core 13.3.0)
* Fixed diverging history in flexible sync if writes occur during bootstrap to objects that just came into view. (Core 13.3.0)
* Fix several data races when opening cached frozen Realms. New frozen Realms were added to the cache and the lock released before they were fully initialized, resulting in races if they were immediately read from the cache on another thread. (Core 13.3.0)
* If a client reset w/recovery or discard local is interrupted while the "fresh" realm is being downloaded, the sync client may crash with a MultpleSyncAgents exception. (Core 13.3.0)
* Changesets from the server sent during FLX bootstrapping that are larger than 16MB can cause the sync client to crash with a LogicError. (Core 13.3.0)
* Sharing Realm files between a Catalyst app and Realm Studio did not properly synchronize access to the Realm file. (Core 13.4.0)

### Compatibility
* Realm Studio: 13.0.0 or later.

### Internal
* Using Core 13.4.0.
* Updated `DynamicRealmObjectHelper.TryGetPrimaryKeyValue` not to use reflection. (Issue [#3166](https://github.com/realm/realm-dotnet/issues/3166))
* Fixed UWP tests workflow when running a debug build. (Issue [#3030](https://github.com/realm/realm-dotnet/issues/3030))

## 10.19.0 (2023-01-06)

### Enhancements
* Removed redundant serialization/deserialization of arguments in CallAsync. (Issue [#3079](https://github.com/realm/realm-dotnet/issues/3079))
* Added a field `Transaction.State` which describes the current state of the transaction. (Issue [#2551](https://github.com/realm/realm-dotnet/issues/2551))
* Improved error message when null is passed as argument to params for EmailPasswordAuth.CallResetPasswordFunctionAsync. (Issue [#3011](https://github.com/realm/realm-dotnet/issues/3011))
* Removed backing fields of generated classes' properties which should provide minor improvements to memory used by Realm Objects (Issue [#2647](https://github.com/realm/realm-dotnet/issues/2994))
* Added two extension methods on `IDictionary` to get an `IQueryable` collection wrapping the dictionary's values:
  * `dictionary.AsRealmQueryable()` allows you to get a `IQueryable<T>` from `IDictionary<string, T>` that can be then treated as a regular queryable collection and filtered/ordered with LINQ or `Filter(string)`.
  * `dictionary.Filter(query, arguments)` will filter the list and return a filtered collection of dictionary's values. It is roughly equivalent to `dictionary.AsRealmQueryable().Filter(query, arguments)`.

  The resulting queryable collection will behave identically to the results obtained by calling `realm.All<T>()`, i.e. it will emit notifications when it changes and automatically update itself. (Issue [#2647](https://github.com/realm/realm-dotnet/issues/2647))
* Improve performance of client reset with automatic recovery and converting top-level tables into embedded tables. (Core upgrade)
* Flexible sync will now wait for the server to have sent all pending history after a bootstrap before marking a subscription as Complete. (Core upgrade)
* Slightly improve performance of `Realm.RemoveAll()` which removes all objects from an open Realm database. (Issue [#2233](https://github.com/realm/realm-dotnet/issues/2194))
* Improve error messages when not setting a BaseFilePath for realm or app configuration. (Issue [2863](https://github.com/realm/realm-dotnet/issues/2863))
* Added `IList` implementation to all Realm collections to allow for UWP ListView databinding. (Issue [#1759](https://github.com/realm/realm-dotnet/issues/1759))

### Fixed
* Fixed issue where Realm parameters' initialization would get run twice, resulting in unexpected behavior.
* Prevented `IEmbeddedObject`s and `IAsymmetricObject`s from being used as `RealmValue`s when added to a realm, and displaying more meaningful error messages.
* Fix a use-after-free if the last external reference to an encrypted Realm was closed between when a client reset error was received and when the download of the new Realm began. (Core upgrade)
* Fixed an assertion failure during client reset with recovery when recovering a list operation on an embedded object that has a link column in the path prefix to the list from the top level object. (Core upgrade)
* Opening an unencrypted file with an encryption key would sometimes report a misleading error message that indicated that the problem was something other than a decryption failure. (Core upgrade)
* Fix a rare deadlock which could occur when closing a synchronized Realm immediately after committing a write transaction when the sync worker thread has also just finished processing a changeset from the server. (Core upgrade)
* Fix a race condition which could result in "operation cancelled" errors being delivered to async open callbacks rather than the actual sync error which caused things to fail. (Core upgrade)
* Bootstraps will not be applied in a single write transaction - they will be applied 1MB of changesets at a time, or as configured by the SDK. (Core upgrade)
* Fix database corruption and encryption issues on apple platforms. (Core upgrade)
* Added fully qualified names for source generated files, to avoid naming collisions. (Issue [#3099](https://github.com/realm/realm-dotnet/issues/3099)
* Fixed an issue that would cause an exception when using unmanaged objects in bindings (Issue [#3094](https://github.com/realm/realm-dotnet/issues/3094))
* Fixed an issue where fetching a user's profile while the user logs out would result in an assertion failure. (Core upgrade)
* Removed the ".tmp_compaction_space" file being left over after compacting a Realm on Windows. (Core upgrade)
* Fixed a crash that would occur if you close a synchronized Realm while waiting for `SubscriptionSet.WaitForSynchronizationAsync`. (Issue [#2952](https://github.com/realm/realm-dotnet/issues/2952))
* Avoid calling the setter on UI-bound properties in case the new value of the property is the same as the current one. This avoids some issue with MAUI, that seems to be calling the setter of bound properties unnecessarily when CollectionView/ListView are shown on screen. This is problematic if the object does not belong to the current user's permissions, as it will cause a compensanting write. In some limited cases this could cause an error loop (verified on iOS) when recycling of cells is involved. (Issue [#3128](https://github.com/realm/realm-dotnet/issues/3128))
* Fixes an issue with where the source generator will not add the namespace for types used in properties' initializers. (Issue [#3135](https://github.com/realm/realm-dotnet/issues/3135))
* Fixed an issue that would prevent Realm from working correctly in Unity applications that have [Domain Reloading](https://docs.unity3d.com/Manual/DomainReloading.html) turned off. (Issue [#2898](https://github.com/realm/realm-dotnet/issues/2898))
* Fixed a bug when using `string.Contains` in .NET 2.1 or later where the search string is not a literal. (Issue [#3134](https://github.com/realm/realm-dotnet/issues/3134))
* Added `[Obsolete]` notice for a few `ErrorCode` enum members that are no longer in use. (Issue [#3155](https://github.com/realm/realm-dotnet/issues/3155)

### Compatibility
* Realm Studio: 12.0.0 or later.

### Internal
* Using Core 12.13.0.
* Replaced `Realm.RefreshAsync` with a native implementation. (PR [#2995](https://github.com/realm/realm-dotnet/pull/2995))

## 10.18.0 (2022-11-02)

### Enhancements
* Introduced `Realm.SourceGenerator`, a [Source Generator](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) that can generate Realm model classes. This is part of our ongoing effort to modernize the Realm library, and will allow to introduce certain language level features easier in the future.
In order to use the source generation the model classes need to be declared implementing one of the base interfaces (`IRealmObject`, `IEmbeddedObject` or `IAsymmetricObject`) and be declared partial. For example:
  ```cs
  public partial class Person: IRealmObject
  {
      public int Age { get; set; }

      public string Name { get; set; }

      public PhoneNumber Phone { get; set; }
  }

  public partial class PhoneNumber: IEmbeddedObject
  {
      public string Number { get; set; }

      public string Prefix { get; set; }
  }
  ```
  The source generator will then take care of adding the full implementation for the interfaces.

  Most of the time converting the "classic" Realm model classes (classes derived from `RealmObject`, `EmbeddedObject` or `AsymmetricObject`) to use the new source generation means just defining the class as partial and switching out the base class for the corresponding interface implementation.
  The classic Realm model definition will still be supported, but will be phased out in the future.

  Please note that the source generator is still in beta, so let us know if you experience any issue while using them.
  Some additional notes:
  * `OnManaged` and `OnPropertyChanged` are now partial methods.
  * Inheritance is not supported, so the Realm models cannot derive from any other class.
  * Nested classes are not supported.

### Fixed
* Fixed a NullReferenceException being thrown when subscribing to `PropertyChanged` notifications on a `Session` instance that is then garbage collected prior to unsubscribing. (PR [#3061](https://github.com/realm/realm-dotnet/pull/3061))
* Removed bitcode support from the iOS binary as it's no longer accepted for App Store submissions. (Issue [#3059](https://github.com/realm/realm-dotnet/issues/3059))
* Fixed returning the parent when accessing it on an `IEmbeddedObject`. (Issue [#2742](https://github.com/realm/realm-dotnet/issues/2742))
* Slightly increased performance and reduced allocations when creating an enumerator for frozen collections (Issue [#2815](https://github.com/realm/realm-dotnet/issues/2815)).

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 12.9.0.
* Added workflow to automatically assign users to issues and PRs. (PR [#3069](https://github.com/realm/realm-dotnet/pull/3069))
* Added workflow to validate changelog has been updated. (PR [#3069](https://github.com/realm/realm-dotnet/pull/3069))

## 10.17.0 (2022-10-06)

### Enhancements
* Prioritize integration of local changes over remote changes - shorten the time users may have to wait when committing local changes. Stop storing downloaded changesets in history. (Core upgrade)
* Greatly improve the performance of sorting or distincting a Dictionary's keys or values. The most expensive operation is now performed O(log N) rather than O(N log N) times, and large Dictionaries can see upwards of 99% reduction in time to sort. (Core upgrade)
* Seamlessly handle migrating an App Services application deployment model. (Core upgrade)

### Fixed
* Fix a use-after-free when a sync session is closed and the app is destroyed at the same time. (Core upgrade)
* Fixed a `NullReferenceException` occurring in `RealmObjectBase`'s finalizer whenever an exception is thrown before the object gets initialized. (Issue [#3045](https://github.com/realm/realm-dotnet/issues/3045))

### Compatibility
* Realm Studio: 12.0.0 or later.

### Internal
* Using Core 12.9.0

## 10.16.0 (2022-10-03)

### Enhancements
* Introduced `AsymmetricObject` intended for write-heavy workloads, where high performance is generally important. This new object:
  1. syncs data unidirectionaly, from the clients to the server
  1. can't be queried, deleted, or modified once added to the Realm
  1. is only usable with flexible sync
  1. can't be the receiveing end of any type of relationship
  1. can contain `EmbeddedObject`s but cannot link to `RealmObject` or `AsymmetricObject`.

  In the same write transaction, it is legal to add `AsymmetricObject`s and `RealmObject`s
  ```cs
  class Measurement : AsymmetricObject
  {
      [PrimaryKey, MapTo("_id")]
      public Guid Id { get; private set; } = Guid.NewGuid();

      public double Value { get; set; }

      public DataTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;
  }

  class Person : RealmObject
  {
      //............
  }

  //.....

  var measurement = new Measurement
  {
    Value = 9.876
  };

  realm.Write(() =>
  {
      realm.Add(measurement);

      realm.Add(new Person());
  });

  _ = asymmetricObject.Value;   // runtime error
  _ = realm.All<Measurement>(); // compile time error
  ```
* Added two client reset handlers, `RecoverUnsyncedChangesHandler` and `RecoverOrDiscardUnsyncedChangesHandler`, that try to automatically merge the unsynced local changes with the remote ones in the event of a client reset. Specifically with `RecoverOrDiscardUnsyncedChangesHandler`, you can fallback to the discard local strategy in case the automatic merge can't be performed as per your server's rules. These new two stragegies simplify even more the handling of client reset events when compared to `DiscardUnsyncedChangesHandler`.`RecoverOrDiscardUnsyncedChangesHandler` is going to be the default from now on. An example is as follows
* Added two client reset handlers, `RecoverUnsyncedChangesHandler` and `RecoverOrDiscardUnsyncedChangesHandler`, that try to automatically merge the unsynced local changes with the remote ones in the event of a client reset. Specifically with `RecoverOrDiscardUnsyncedChangesHandler`, you can fallback to the discard unsynced strategy in case the automatic merge can't be performed as per your server's rules. These new two stragegies simplify even more the handling of client reset events when compared to `DiscardUnsyncedChangesHandler`.`RecoverOrDiscardUnsyncedChangesHandler` is going to be the default from now on. More info on the aforementioned strategies can be found in our [docs page](https://www.mongodb.com/docs/realm/sdk/dotnet/advanced-guides/client-reset/). An example usage of one of the new handler is as follows:
  ```cs
  var conf = new PartitionSyncConfiguration(partition, user)
  {
    ClientResetHandler = new RecoverOrDiscardUnsyncedChangesHandler
    {
      // As always, the following callbacks are optional

      OnBeforeReset = (beforeFrozen) =>
      {
        // executed right before a client reset is about to happen
      },
      OnAfterRecovery = (beforeFrozen, after) =>
      {
        // executed right after an automatic recovery from a client reset has completed
      },
      OnAfterDiscard = (beforeFrozen, after) =>
      {
        // executed after an automatic recovery from a client reset has failed but the DiscardUnsyncedChanges fallback has completed
      },
      ManualResetFallback = (session, err) =>
      {
        // handle the reset manually
      }
    }
  };
  ```
  (PR [#2745](https://github.com/realm/realm-dotnet/issues/2745))
* Introducing string query support for constant list expressions such as `realm.All<Car>().Filter("Color IN {'blue', 'orange'}")`. This also includes general query support for list vs list matching such as `realm.All<Car>().Filter("NONE Features IN {'ABS', 'Seat Heating'}")`. (Core upgrade)
* Improve performance when a new Realm file connects to the server for the first time, especially when significant amounts of data has been written while offline. (Core upgrade)
* Shift more of the work done on the sync worker thread out of the write transaction used to apply server changes, reducing how long it blocks other threads from writing. (Core upgrade)
* Improve the performance of the sync changeset parser, which speeds up applying changesets from the server. (Core upgrade)

### Fixed
* Added a more meaningful error message whenever a project doesn't have `[TargetFramework]` defined. (Issue [#2843](https://github.com/realm/realm-dotnet/issues/2843))
* Opening a read-only Realm for the first time with a `SyncConfiguration` did not set the schema version, which could lead to `m_schema_version != ObjectStore::NotVersioned` assertion failures. (Core upgrade)
* Upload completion callbacks (i.e. `Session.WaitForUploadAsync`) may have called before the download message that completed them was fully integrated. (Core upgrade)
* Fixed an exception "fcntl() with F_BARRIERFSYNC failed: Inappropriate ioctl for device" when running with MacOS on an exFAT drive. (Core upgrade)
* Syncing of a Decimal128 with big significand could result in a crash. (Core upgrade)
* `Realm.Refresh()` did not actually advance to the latest version in some cases. If there was a version newer than the current version which did not require blocking it would advance to that instead, contrary to the documented behavior. (Core upgrade)
* Several issues around notifications were fixed. (Core upgrade)
  * Fix a data race on RealmCoordinator::m_sync_session which could occur if multiple threads performed the initial open of a Realm at once.
  * If a SyncSession outlived the parent Realm and then was adopted by a new Realm for the same file, other processes would not get notified for sync writes on that file.
  * Fix one cause of QoS inversion warnings when performing writes on the main thread on Apple platforms. Waiting for async notifications to be ready is now done in a QoS-aware ways.
* If you set a subscription on a link in flexible sync, the server would not know how to handle it ([#5409](https://github.com/realm/realm-core/issues/5409), since v11.6.1)
* If a case insensitive query searched for a string including an 4-byte UTF8 character, the program would crash. (Core upgrade)
* Added validation to prevent adding a removed object using Realm.Add. (Issue [#3020](https://github.com/realm/realm-dotnet/issues/3020))

### Compatibility
* Realm Studio: 12.0.0 or later.

### Internal
* Using Core 12.7.0.

## 10.15.1 (2022-08-08)

### Fixed
* Fixed an issue introduced in 10.15.0 that would prevent non-anonoymous user authentication against Atlas App Services. (Issue [#2987](https://github.com/realm/realm-dotnet/issues/2987))
* Added override to `User.ToString()` that outputs the user id and provider. (PR [#2988](https://github.com/realm/realm-dotnet/pull/2988))
* Added == and != operator overloads to `User` that matches the behavior of `User.Equals`. (PR [#2988](https://github.com/realm/realm-dotnet/pull/2988))

### Compatibility
* Realm Studio: 12.0.0 or later.

### Internal
* Using Core 12.4.0.

## 10.15.0 (2022-08-05)

### Enhancements
* Preview support for .NET 6 with Mac Catalyst and MAUI. (PR [#2959](https://github.com/realm/realm-dotnet/pull/2959))
* Reduce use of memory mappings and virtual address space (Core upgrade)

### Fixed
* Fix a data race when opening a flexible sync Realm (Core upgrade).
* Fixed a missing backlink removal when setting a `RealmValue` from a `RealmObject` to null or any other non-RealmObject value. Users may have seen exception of "key not found" or assertion failures such as `mixed.hpp:165: [realm-core-12.1.0] Assertion failed: m_type` when removing the destination object. (Core upgrade)
* Fixed an issue on Windows that would cause high CPU usage by the sync client when there are no active sync sessions. (Core upgrade)
* Improved performance of sync clients during integration of changesets with many small strings (totalling > 1024 bytes per changeset) on iOS 14, and devices which have restrictive or fragmented memory. (Core upgrade)
* Fix exception when decoding interned strings in realm-apply-to-state tool. (Core upgrade)
* Fix a data race when committing a transaction while multiple threads are waiting for the write lock on platforms using emulated interprocess condition variables (most platforms other than non-Android Linux). (Core upgrade)
* Fix some cases of running out of virtual address space (seen/reported as mmap failures) (Core upgrade)
* Decimal128 values with more than 110 significant bits were not synchronized correctly with the server (Core upgrade)

### Compatibility
* Realm Studio: 12.0.0 or later.

### Internal
* Using Core 12.4.0.

## 10.14.0 (2022-06-02)

### Enhancements
* Added a more efficient replacement for `Realm.WriteAsync`. The previous API would start a background thread, open the Realm there and run a synchronous write transaction on the background thread. The new API will asynchronously acquire the write lock (begin transaction) and asynchronously commit the transaction, but the actual write block will execute on the original thread. This means that objects/queries captured before the block can be used inside the block without relying on threadsafe references. Importantly, you can mix and match async and sync calls. And when calling any `Realm.WriteAsync` on a background thread the call is just run synchronously, so you should use `Realm.Write` for readability sake. The new API is made of `Realm.WriteAsync<T>(Func<T> function, CancellationToken cancellationToken)`, `Realm.WriteAsync(Action action, CancellationToken cancellationToken)`, `Realm.BeginWriteAsync(CancellationToken cancellationToken)` and `Transaction.CommitAsync(CancellationToken cancellationToken)`. While the `Transaction.Rollback()` doesn't need an async counterpart. The deprecated API calls are `Realm.WriteAsync(Action<Realm> action)`, `Real.WriteAsync<T>(Func<Realm, IQueryable<T>> function)`, `Realm.WriteAsync<T>(Func<Realm, IList<T>> function)` and `Realm.WriteAsync<T>(Func<Realm, T> function)`. Here is an example of usage:
  ```csharp
  using Realms;

  var person = await _realm.WriteAsync(() =>
  {
    return _realm.Add(
      new Person
      {
        FirstName = "Marco"
      });
  });

  // you can use/modify person now
  // without the need of using ThreadSafeReference
  ```
  (PR [#2899](https://github.com/realm/realm-dotnet/pull/2899))
* Added the method `App.DeleteUserFromServerAsync` to delete a user from the server. It will also invalidate the user locally as well as remove all their local data. It will not remove any data the user has uploaded from the server. (Issue [#2675](https://github.com/realm/realm-dotnet/issues/2675))
* Added boolean property `ChangeSet.IsCleared` that is true when the collection gets cleared. Also Realm collections now raise `CollectionChanged` event with action `Reset` instead of `Remove` when the collections is cleared. Please note that this will work only with collection properties, such as `IList` and `ISet`. (Issue [#2856](https://github.com/realm/realm-dotnet/issues/2856))
* Added `PopulateInitialSubscriptions` to `FlexibleSyncConfiguration` - this is a callback that will be invoked the first time a Realm is opened. It allows you to create the initial subscriptions that will be added to the Realm before it is opened. (Issue [#2913](https://github.com/realm/realm-dotnet/issues/2913))
* Bump the SharedInfo version to 12. This requires update of any app accessing the file in a multiprocess scenario, including Realm Studio.
* The sync client will gracefully handle compensating write error messages from the server and pass detailed info to the SDK's sync error handler about which objects caused the compensating write to occur. ([#5528](https://github.com/realm/realm-core/pull/5528))

### Fixed
* Adding an object to a Set, deleting the parent object, and then deleting the previously mentioned object causes crash ([#5387](https://github.com/realm/realm-core/issues/5387))
* Flexible sync would not correctly resume syncing if a bootstrap was interrupted ([#5466](https://github.com/realm/realm-core/pull/5466))
* Flexible sync will now ensure that a bootstrap from the server will only be applied if the entire bootstrap is received - ensuring there are no orphaned objects as a result of changing the read snapshot on the server ([#5331](https://github.com/realm/realm-core/pull/5331))
* Partially fix a performance regression in write performance on Apple platforms. Committing an empty write transaction is ~10x faster than 10.13.0, but still slower than pre-10.7.1 due to using more crash-safe file synchronization (since v10.7.1). (Swift issue [#7740](https://github.com/realm/realm-swift/issues/7740)).

### Compatibility
* Realm Studio: 12.0.0 or later.

### Internal
* Using Core 12.1.0.

## 10.13.0 (2022-05-18)

### Enhancements
* Added the functionality to convert Sync Realms into Local Realms and Local Realms into Sync Realms. (Issue [#2746](https://github.com/realm/realm-dotnet/issues/2746))
* Added support for a new client reset strategy, called [Discard Unsynced Changes](https://docs.mongodb.com/realm/sync/error-handling/client-resets/#discard-unsynced-changes). This new stragegy greatly simplifies the handling of a client reset event on a synchronized Realm.
This addition makes `Session.Error` **deprecated**. In order to temporarily continue using the current `Session.Error` the following must be done:
  ```csharp
    var conf = new PartitionSyncConfiguration(partition, user)
    {
      ClientResetHandler = new ManualRecoveryHandler();
    };
  ```
  In order to take advantage of the new **Discard Unsynced Changes** feature, the following should be done (all callbacks are optional):
  ```csharp
    var conf = new PartitionSyncConfiguration(partition, user)
    {
      ClientResetHandler = new DiscardLocalResetHandler
      {
        OnBeforeReset = (beforeFrozen) =>
        {
          // executed right before a client reset is about to happen
        },
        OnAfterReset = (beforeFrozen, after) =>
        {
          // executed right after a client reset is has completed
        },
        ManualResetFallback = (session, err) =>
        {
          // handle the reset manually
        }
      }
    };
  ```
  If, instead, you want to continue using the manual solution even after the end of the deprecation period, the following should be done
  ```csharp
    var conf = new PartitionSyncConfiguration(partition, user)
    {
      ClientResetHandler = new ManualRecoveryHandler((sender, e) =>
      {
          // user's code for manual recovery
      });
  ```

### Fixed
* Fixed a `System.DllNotFoundException` being thrown by Realm APIs at startup on Xamarin.iOS (Issue [#2926](https://github.com/realm/realm-dotnet/issues/2926), since 10.12.0)

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.14.0.

## 10.12.0 (2022-05-05)

### Enhancements
* Preview support for .NET 6 with iOS, Android, and MAUI.
  We've added tentative support for the new .NET 6 Mobile workloads (except MacCatalyst, which will be enabled later). The .NET tooling itself is still in preview so we don't have good test coverage of the new platforms just yet. Please report any issues you find at https://github.com/realm/realm-dotnet/issues/new/choose.

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.14.0.

## 10.11.2 (2022-04-12)

### Fixed
* Fixed corruption bugs when encryption is used. (Core Issue [#5360](https://github.com/realm/realm-core/issues/5360))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.14.0.

## 10.11.1 (2022-03-31)

### Fixed
* Fixed an issue that would cause the managed HttpClientHandler to be used in Xamarin applications, even if the project is configured to use the native one. (Issue [#2892](https://github.com/realm/realm-dotnet/issues/2892))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.12.0.

## 10.11.0 (2022-03-28)

### Enhancements
* Added property `Session.ConnectionState` to get a `Session`'s `SessionConnectionState`. Additionally, `Session` now implements `INotifyPropertyChanged` so that you can listen for changes on `Session.ConnectionState`. (Issue [#2801](https://github.com/realm/realm-dotnet/issues/2801))
* Realm now supports running on Windows ARM64 for .NET Framework, .NET Core, and UWP apps. (Issues [#2704](https://github.com/realm/realm-dotnet/issues/2704) and [#2817](https://github.com/realm/realm-dotnet/issues/2817))
* Added a property `AppConfiguration.HttpClientHandler` that allows you to override the default http client handler used by the Realm .NET SDK to make http calls. Note that this only affects the behavior of http calls, such as user login, function calls, and remote mongodb calls. The sync client uses a native websocket implementation and will not use the provided message handler. (Issue [#2865](https://github.com/realm/realm-dotnet/issues/2865))

### Fixed
* [Unity] Fixed an issue that caused the weaver to fail when invoked via the `Tools->Realm->Weave Assemblies` editor menu with the error `UnityEngine.UnityException: get_dataPath can only be called from the main thread`. (Issue [#2836](https://github.com/realm/realm-dotnet/issues/2836))
* Fixed an issue that caused `RealmInvalidObjectException` to be caused when enumerating an invalid Realm collection (e.g. a list belonging to a deleted object). (Issue [#2840](https://github.com/realm/realm-dotnet/issues/2840))
* Query parser would not accept "in" as a property name (Core Issue [#5312](https://github.com/realm/realm-core/issues/5312))
* Application would sometimes crash with exceptions like 'KeyNotFound' or assertion "has_refs()". Other issues indicating file corruption may also be fixed by this. The one mentioned here is the one that lead to solving the problem. (Core Issue [#5283](https://github.com/realm/realm-core/issues/5283))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.12.0.
* Enabled running Benchmarks on iOS devices by turning on the interpreter for some windows assemblies.

## 10.10.0 (2022-02-28)

### Guid representation issue

This release fixes a major bug in the way Guid values are stored in the database. It provides an automatic migration for local (non-synchronized) databases, but extra caution is needed when upgrading an app that uses Sync.

#### **Context**

A Guid is represented by 4 components - `int`, `short`, `short`, and a `byte[8]`. Microsoft's Guids diverge from the UUID spec in that they encode the first three components with the endianness of the system (little-endian for all modern CPUs), while UUIDs encode their components as big-endian. The end result is that the same bytes have a different string representations when interpreted as a `Guid` by the .NET SDK vs when interpreted as a `UUID` by the Realm Database - e.g. `f2952191-a847-41c3-8362-497f92cb7d24` vs `912195f2-47a8-c341-8362-497f92cb7d24` (note the swapping of bytes in the first three components). You can see the issue by opening a database created by the .NET SDK in Realm Studio and inspecting the values for Guid properties.

#### **Fix**

The fix we're providing is to adjust the behavior of the .NET SDK to read/write Guids to the database with big-endian representation. This means that the SDK and the database will consistently display the same values. This has some implications which are described in the Local- and Synchronized Realms sections.

#### **Local Realms**

For local Realms, we're executing a one-time migration the first time the Realm is opened with the new SDK. During this migration, we'll update all Guid fields to big-endian format. This means that their string representation will remain the same, but the value in the database will change to match it. This means that the upgrade process should be seamless, but if you decide to downgrade to an older version of the SDK, you'll see the byte order get flipped. The migration will not execute multiple times, even if you downgrade.

#### **Synchronized Realms**

There's no client migration provided for synchronized Realms. This is because the distributed nature of the system would mean that there will inevitably be a period of inconsistent state. Instead, the values of the `Guid` properties are read as they're already stored in the database, meaning the string representation will be flipped compared to previous versions of the SDK but it will now match the representation in Atlas/Compass/Realm Studio. There are three general groups your app will fall under:
* If you don't care about the string values of Guid properties on the client, then you don't need to do anything. The values will still be unique and valid Guids.
* If you do use the string guid values from the client app - e.g. to correlate user ids with a CMS, but have complete control over your client devices - e.g. because this an internal company app, then it's advised that you execute a one-time migration of the data in Atlas and force all users to upgrade to the latest version of the app.
* If you can't force all users to update at the same time, you can do a live migration by adding an extra property for each Guid property that you have and write a trigger function that will migrate the data between the two. The old version of the app will write to the original property, while the new version will write to the new property and the trigger will convert between the two.

If you are using sync and need to update to the latest version of the SDK but are not ready to migrate your data yet, see the `Opting out` section.

#### **Opting out**

If for some reason, you want to opt out of the fixed behavior, you can temporarily opt out of it by setting the `Realm.UseLegacyGuidRepresentation` property to `true`. This is not recommended but can be used when you need more time to test out the migration while still getting bugfixes and other improvements. Setting it to `true` does two things:
1. It brings back the pre-10.10.0 behavior of reading/writing Guid values with little-endian representation.
1. It disables the migration code for local Realms. Note that it will not revert the migration if you already opened the Realm file when `UseLegacyGuidRepresentation` was set to `false`.

### Enhancements
* Lifted a limitation that would prevent you from changing the primary key of objects during a migration. It is now possible to do it with both the dynamic and the strongly-typed API:
  ```csharp
  var config = new RealmConfiguration
  {
    SchemaVersion = 5,
    MigrationCallback = (migration, oldVersion) =>
    {
      // Increment the primary key value of all Foos
      foreach (var obj in migration.NewRealm.All<Foo>())
      {
        obj.Id = obj.Id + 1000;
      }
    }
  }
  ```
* [Unity] The Realm menu item in the Unity Editor was moved to `Tools/Realm` to reduce clutter and align with other 3rd party editor plugins. (Issue [#2807](https://github.com/realm/realm-dotnet/issues/2807))

### Fixed
* Fixed an issue with xUnit tests that would cause `System.Runtime.InteropServices.SEHException` to be thrown whenever Realm was accessed in a non-async test. (Issue [#1865](https://github.com/realm/realm-dotnet/issues/1865))
* Fixed a bug that would lead to unnecessary metadata allocation when freezing a realm. (Issue [#2789](https://github.com/realm/realm-dotnet/issues/2789))
* Fixed an issue that would cause Realm-managed objects (e.g. `RealmObject`, list, results, and so on) allocated during a migration block to keep the Realm open until they are garbage collected. This had subtle implications, such as being unable to delete the Realm shortly after a migration or being unable to open the Realm with a different configuration. (PR [#2795](https://github.com/realm/realm-dotnet/pull/2795))
* Fixed an issue that prevented Unity3D's IL2CPP compiler to correctly process one of Realm's dependencies. (Issue [#2666](https://github.com/realm/realm-dotnet/issues/2666))
* Fixed the osx runtime path in the Realm NuGet package to also apply to Apple Silicon (universal) architectures (Issue [#2732](https://github.com/realm/realm-dotnet/issues/2732))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.10.0

## 10.9.0 (2022-01-21)

### Enhancements
* Added support for a new mode of synchronization with MongoDB Realm, called ["Flexible Sync"](https://docs.mongodb.com/realm/sync/data-access-patterns/flexible-sync/). When using Flexible Sync, the client decides which queries it's interested in and asks the server for all objects matching these queries. The matching objects will be stored in a local Realm, just like before and can be queried and accessed while offline. This feature is in beta, so feedback - both positive and negative - is greatly appreciated and, as usual, we don't recommend using it for production workloads yet.
  * Added a new configuration type, called `FlexibleSyncConfiguration`. Use this type to get a `Realm` instance that uses the new synchronization mode with the server.
  * Deprecated the `SyncConfiguration` class in favor of `PartitionSyncConfiguration`. The two classes are equivalent and the new type is introduced to better contrast with `FlexibleSyncConfiguration`. The two types are equivalent and allow you to open a `Realm` instance that is using the old "Partition Sync" mode.
  * Added a new type, called `SubscriptionSet`. It is a collection, holding the various active query subscriptions that have been created for this Realm. This collection can be accessed via the `Realm.Subscriptions` property. It will be `null` for local and partition sync Realms and non-null for flexible sync Realms.

  A minimal example would look like this:
  ```csharp
  var config = new FlexibleSyncConfiguration(user);
  var realm = Realm.GetInstance(config);

  // Add a new subscription
  realm.Subscriptions.Update(() =>
  {
    var year2022 = new DateTimeOffset(2022, 1, 1);
    var saleOrders = realm.All<SaleOrder>().Where(o => o.Created > year2022);
    realm.Subscriptions.Add(saleOrders);
  });

  // Wait for the server to acknowledge the subscription and return all objects
  // matching the query
  await realm.Subscriptions.WaitForSynchronizationAsync();

  // Now we have all orders that existed on the server at the time of
  // subscribing. From now on, the server will send us updates as new
  // orders get created.
  var orderCount = realm.All<SaleOrder>().Count();
  ```
  * Multiple subscriptions can be created for queries on the same class, in which case they'll be combined with a logical `OR`. For example, if you create a subscription for all orders created in 2022 and another for all orders created by the current user, your local Realm will contain the union of the two result sets.
  * Subscriptions can be named (which makes it easier to unsubscribe) or unnamed. Adding multiple unnamed subscriptions with the same query is a no-op.
  * Modifying the set of active subscriptions is an expensive operation server-side, even if the resulting diff is not large. This is why we recommend batching subscription updates as much as possible to avoid overloading the server instance. A good practice is to declare the user subscriptions upfront - usually the first time the Realm is opened, and only update them when absolutely necessary.
  * Find more information about the API and current limitations in the [docs](https://docs.mongodb.com/realm/sdk/dotnet/fundamentals/realm-sync/).

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.8.0.
* Release tests are executed against realm-qa instead of realm-dev. (PR [#2771](https://github.com/realm/realm-dotnet/pull/2771))

## 10.8.0 (2022-01-17)

### Enhancements
* Added the `RealmConfigurationBase.FallbackPipePath` property. In the majority of cases this property can be left null, but it should be used when a realm is opened on a filesystem where named pipes cannot be created, such as external storage on Android that uses FAT32. In this case the path needs to point to a location on another filesystem where named pipes can be created. (PR [#2766](https://github.com/realm/realm-dotnet/pull/2766))
* Added support arithmetric operations (+, -, *, /) in the string-based query syntax (`realm.All<Foo>().Filter("some-query")`). Operands can be properties and/or constants of numeric types (integer, float, double or Decimal128). You can now write a query like `"(age + 5) * 2 > child.age"`. (Core upgrade)

### Fixed
* Fixed a race condition that could result in `Sharing violation on path ...` error when opening a Unity project on macOS. (Issue [#2720](https://github.com/realm/realm-dotnet/issues/2720), fix by [@tomkrikorian](https://github.com/tomkrikorian))
* Fixed an error being thrown when `Realm.GetInstance` is called multiple times on a readonly Realm. (Issue [#2731](https://github.com/realm/realm-dotnet/pull/2731))
* Fixed a bug that would result in the `LIMIT` clause being ignored when `Count()` is invoked on a `IQueryable` - e.g. expressions like `realm.All<Foo>().Filter("Bar > 5 LIMIT(1)).Count()` would ignore the limit in the string-based predicate and return the count of all matches. (Issue [#2755](https://github.com/realm/realm-dotnet/issues/2755))
* Fixed the logic in `RealmResultsVisitor.TraverseSort` to allow sorting on interface properties. (Issue [#1373](https://github.com/realm/realm-dotnet/issues/1373), contribution by @daawaan)

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.8.0.
* Updated naming of prerelease packages to use lowercase "pr" - e.g. `10.7.1-pr-2695.1703` instead of `10.7.1-PR-2695.1703`. (PR [#2765](https://github.com/realm/realm-dotnet/pull/2765))
* Migrated from using the cli to import/export applications to configuring them via the admin API. (PR [#2768](https://github.com/realm/realm-dotnet/pull/2768))

## 10.7.1 (2021-11-19)

### Fixed
* A sync user's Realm was not deleted when the user was removed if the Realm path was too long such that it triggered the fallback hashed name (this is OS dependant but is 300 characters on linux). (Core upgrade)
* Don't keep trying to refresh the access token if the client's clock is more than 30 minutes ahead. (Core upgrade)
* Don't sleep the sync thread artificially if an auth request fails. This could be observed as a UI hang on applications when sync tries to connect after being offline for more than 30 minutes. (Core upgrade)

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.6.1.

## 10.7.0 (2021-11-09)

### Enhancements
* Added the `Realm.SyncSession` property which will return the sync session for this Realm if the Realm is a synchronized one or `null` for local Realms. This is replacing the `GetSession(this Realm)` extension method which is now deprecated. (PR [#2711](https://github.com/realm/realm-dotnet/pull/2711))

### Fixed
* Fixed a bug that would result in a `RealmException` being thrown when opening a readonly Realm with schema that is a superset of the schema on disk. Now the code will just work and treat any classes not present in the on-disk schema to be treated as empty collections - e.g. `realm.All<ThisIsNotInOnDiskSchema>().Count == 0`. (Issue [#2619](https://github.com/realm/realm-dotnet/issues/2619))
* Fixed a bug that would create a "Documents" folder in the binary app folder when the ransomware protection in Windows is turned on. (Issue [#2685](https://github.com/realm/realm-dotnet/pull/2685))
* Fixed an issue that would cause incorrect property implementation to be generated if `PropertyChanged.Fody` runs after the Realm weaver. (Issue [#1873](https://github.com/realm/realm-dotnet/issues/1873))
* [Unity] Preserved additional constructors necessary to serialize and deserialize Custom User Data. (PR [#2519](https://github.com/realm/realm-dotnet/pull/2519))
* Fixed an issue that would result in `InvalidOperationException` when concurrently creating a `RealmConfiguration` with an explicitly set `Schema` property. (Issue [#2701](https://github.com/realm/realm-dotnet/issues/2701))
* [Unity] Fixed an issue that would result in `NullReferenceException` when building for iOS when the Realm package hasn't been installed via the Unity Package Manager. (Issue [#2698](https://github.com/realm/realm-dotnet/issues/2698))
* Fixed a bug that could cause properties of frozen objects to return incorrect value/throw an exception if the provided Realm schema didn't match the schema on disk. (Issue [#2670](https://github.com/realm/realm-dotnet/issues/2670))
* Fixed a rare assertion failure or deadlock when a sync session is racing to close at the same time that external reference to the Realm is being released. (Core upgrade)
* Fixed an assertion failure when opening a sync Realm with a user who had been removed. Instead an exception will be thrown. (Core upgrade)
* Fixed a rare segfault which could trigger if a user was being logged out while the access token refresh response comes in. (Core upgrade)
* Fixed a bug where progress notifiers continue to be called after the download of a synced realm is complete. (Core upgrade)
* Allow for EPERM to be returned from fallocate(). This improves support for running on Linux environments with interesting filesystems, like AWS Lambda. Thanks to [@ztane](https://github.com/ztane) for reporting and suggesting a fix. (Core upgrade)
* Fixed a user being left in the logged in state when the user's refresh token expires. (Core upgrade)
* SyncManager had some inconsistent locking which could result in data races and/or deadlocks, mostly in ways that would never be hit outside of tests doing very strange things. (Core upgrade)

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.6.0.
* iOS wrappers are now built with the "new build system" introduced by Xcode 10 and used as default by Xcode 12. More info can be found in cmake's [docs](https://cmake.org/cmake/help/git-stage/variable/CMAKE_XCODE_BUILD_SYSTEM.html#variable:CMAKE_XCODE_BUILD_SYSTEM).
* We now refresh the resulting Realm instance when opening a synchronized Realm with `GetInstanceAsync`. (Issue [#2256](https://github.com/realm/realm-dotnet/issues/2256))
* Added Sync tests for all platforms running on cloud-dev. (Issue [#2049](https://github.com/realm/realm-dotnet/issues/2049))
* Added Android tests running on the emulator. (Issue [#2680](https://github.com/realm/realm-dotnet/pull/2680))
* Started publishing prerelease packages to S3 using Sleet ([feed url](https://s3.amazonaws.com/realm.nugetpackages/index.json)). (Issue [#2708](https://github.com/realm/realm-dotnet/issues/2708))
* Enable LTO for all builds. (PR [#2714](https://github.com/realm/realm-dotnet/pull/2714))

## 10.6.0 (2021-09-30)

### Enhancements
* Added two extension methods on `ISet` to get an `IQueryable` collection wrapping the set:
  * `set.AsRealmQueryable()` allows you to get a `IQueryable<T>` from `ISet<T>` that can be then treated as a regular queryable collection and filtered/ordered with LINQ or `Filter(string)`.
  * `set.Filter(query, arguments)` will filter the set and return the filtered collection. It is roughly equivalent to `set.AsRealmQueryable().Filter(query, arguments)`.

  The resulting queryable collection will behave identically to the results obtained by calling `realm.All<T>()`, i.e. it will emit notifications when it changes and automatically update itself. (Issue [#2555](https://github.com/realm/realm-dotnet/issues/2555))
* Added two new methods on `Migration` (Issue [#2543](https://github.com/realm/realm-dotnet/issues/2543)):
  * `RemoveType(typeName)` allows to completely remove a type and its schema from a realm during a migration.
  * `RenameProperty(typeName, oldPropertyName, newPropertyName)` allows to rename a property during a migration.
* A Realm Schema can now be constructed at runtime as opposed to generated automatically from the model classes. The automatic generation continues to work and should cover the needs of the vast majority of Realm users. Manually constructing the schema may be required when the shape of the objects depends on some information only known at runtime or in very rare cases where it may provide performance benefits by representing a collection of known size as properties on the class. (Issue [#824](https://github.com/realm/realm-dotnet/issues/824))
  * `RealmConfiguration.ObjectClasses` has now been deprecated in favor of `RealmConfiguration.Schema`. `RealmSchema` has an implicit conversion operator from `Type[]` so code that previously looked like `ObjectClasses = new[] { typeof(Foo), typeof(Bar) }` can be trivially updated to `Schema = new[] { typeof(Foo), typeof(Bar) }`.
  * `Property` has been converted to a read-only struct by removing the setters from its properties. Those didn't do anything previously, so we don't expect anyone was using them.
  * Added several factory methods on `Property` to simplify declaration of Realm properties by being explicit about the range of valid options - e.g. `Property.FromType<int>("IntProperty")` or `Property.Object("MyPersonProp", "Person")`. The constructor of `Property` is now public to support advanced scenarios, but we recommend using the factory methods.
  * Made `ObjectSchema.Builder` public and streamlined its API. It allows you to construct a mutable representation of the schema of a single object and add/remove properties to it. You can either get an empty builder or you can see it with the information from an existing model class (i.e. inheriting from `RealmObject` or `EmbeddedObject`).
  * Made `RealmSchema.Builder` public and streamlined its API. It allows you to construct a mutable representation of the schema of an entire Realm and add/remove object schemas to it.
  * A simple example for how to use the new API would look like:
  ```csharp
  public class Person : RealmObject
  {
    public string Name { get; set; }
    public Address Address { get; set; }
  }

  // Declare schema from existing model classes
  var config = new RealmConfiguration
  {
    Schema = new[] { typeof(Person), typeof(Address) }
  };

  // Manually construct a schema - we don't need to call .Build() on the builders
  // because we have implicit conversion operators defined that will call it for us.
  // Explicitly calling .Build() is also perfectly fine, if a little more verbose.
  var config = new RealmConfiguration
  {
    Schema = new RealmSchema.Builder
    {
      new ObjectSchema.Builder("MyClass", isEmbedded: false)
      {
        Property.FromType<int>("Id", isPrimaryKey: true),
        Property.PrimitiveDictionary("Tags", RealmValueType.String)
      },
      new ObjectSchema.Builder("EmbeddedClass", isEmbedded: true)
      {
        Property.Primitive("DateProp", RealmValueType.Date, isNullable: true)
      }
    }
  };

  // Enhance an existing model with new properties that will be accessible via
  // the dynamic API.
  var personSchema = new ObjectSchema.Builder(typeof(Person))
  {
    Property.FromType<string>("NewStringProp")
  };

  var config = new RealmConfiguration
  {
    Schema = new RealmSchema.Builder
    {
      personSchema,
      new ObjectSchema.Builder(typeof(Address))
    }
  };

  // Regular Person properties can be accessed as usual while runtime defined ones
  // need to go through the dynamic API.
  var person = realm.All<Person>().First();
  var name = person.Name;
  var stringPropValue = person.DynamicApi.Get<string>("NewStringProp");
  ```
* Fixed an issue that would result in SIGABORT on macOS/Linux when opening a Realm in dynamic mode (i.e. read the schema from disk) and the schema contains an object with no properties. (Issue [#1978](https://github.com/realm/realm-dotnet/issues/1978))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.4.1.
* Moved perf tests to run on a self-hosted runner. (PR [#2638](https://github.com/realm/realm-dotnet/pull/2638))

## 10.5.1 (2021-09-22)

### Fixed
* Fixed a bug that would cause a `NullReferenceException` to be reported during compilation of a class containing a getter-only `RealmObject` property. (Issue [#2576](https://github.com/realm/realm-dotnet/issues/2576))
* Fixed an issue that would result in `Unable to load DLL 'realm-wrappers'` when deploying a WPF .NET Framework application with ClickOnce. This was due to the incorrect BuildAction type being applied to the native libraries that Realm depends on. (Issue [#1877](https://github.com/realm/realm-dotnet/issues/1877))
* \[Unity] Fixed an issue that would fail Unity builds with `Multiple precompiled assemblies with the same name Mono.Cecil.dll` if importing the Realm package into a project that already references `Mono.Cecil`. (Issue [#2630](https://github.com/realm/realm-dotnet/issues/2630))
* Fixed a bug that would sometimes result in assemblies not found at runtime in a very specific edge scenario. More details about such a scenario can be found in its [PR](https://github.com/realm/realm-dotnet/pull/2639)'s description. (Issue [#1568](https://github.com/realm/realm-dotnet/issues/1568))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.4.1.

## 10.5.0 (2021-09-09)

### Enhancements
* ThreadSafeReference no longer pins the source transaction version for anything other than a Results backed by a Query. (Core upgrade)
* A ThreadSafeReference to a Results backed by a collection can now be created inside a write transaction as long as the collection was not created in the current write transaction. (Core upgrade)
* Synchronized Realms are no longer opened twice, cutting the address space and file descriptors used in half. (Core upgrade)

### Fixed
* If an object with a null primary key was deleted by another sync client, the exception `KeyNotFound: No such object` could be triggered. (Core upgrade)
* Fixed a race condition that could result in an assertion `m_state == SyncUser::State::LoggedIn` if the app previously crashed during user logout. (Core upgrade)

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.4.1.
* Added an action to post releases to Slack. (Issue [#2501](https://github.com/realm/realm-dotnet/issues/2501))
* Added MSBuild inline task to extract the changelog of the latest version. (Issue [#2558](https://github.com/realm/realm-dotnet/pull/2558))
* When a release succeeds, merge the original PR, tag the release, then update changelog. (PR [#2609](https://github.com/realm/realm-dotnet/pull/2609))

## 10.4.1 (2021-09-03)

### Fixed
* Fixed a regression that would prevent the SDK from working on older Linux versions. (Issue [#2602](https://github.com/realm/realm-dotnet/issues/2602))
* Fixed an issue that manifested in circumventing the check for changing a primary key when using the dynamic API - i.e. `myObj.DynamicApi.Set("Id", "some-new-value")` will now correctly throw a `NotSupportedException` if `"some-new-value"` is different from `myObj`'s primary key value. (PR [#2601](https://github.com/realm/realm-dotnet/pull/2601))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.3.1.
* Started uploading code coverage to coveralls. (Issue [#2586](https://github.com/realm/realm-dotnet/issues/2586))
* Removed the `[Serializable]` attribute from RealmObjectBase inheritors. (PR [#2600](https://github.com/realm/realm-dotnet/pull/2600))

## 10.4.0 (2021-08-31)

### Fixed
* Fixed an issue that would cause `Logger.Default` on Unity to always revert to `Debug.Log`, even when a custom logger was set. (Issue [#2481](https://github.com/realm/realm-dotnet/issues/2481))
* Fixed an issue where `Logger.Console` on Unity would still use `Console.WriteLine` instead of `Debug.Log`. (Issue [#2481](https://github.com/realm/realm-dotnet/issues/2481))
* Added serialization annotations to RealmObjectBase to prevent Newtonsoft.Json and similar serializers from attempting to serialize the base properties. (Issue [#2579](https://github.com/realm/realm-dotnet/issues/2579))
* Fixed an issue that would cause an `InvalidOperationException` when removing an element from an UI-bound collection in WPF. (Issue [#1903](https://github.com/realm/realm-dotnet/issues/1903))
* User profile now correctly persists between runs. (Core upgrade)
* Fixed a crash when delivering notifications over a nested hierarchy of lists of RealmValue that contain RealmObject inheritors. (Core upgrade)
* Fixed a crash when an object which is linked to by a RealmValue property is invalidated (sync only). (Core upgrade)
* Fixes prior_size history corruption when replacing an embedded object in a list. (Core upgrade)
* Fixed an assertion failure in the sync client when applying an AddColumn instruction for a RealmValue property when that property already exists locally. (Core upgrade)
* Fixed an `Invalid data type` assertion failure in the sync client when applying an `AddColumn` instruction for a `RealmValue` property when that property already exists locally. (Core upgrade)

### Enhancements
* Added two extension methods on `IList` to get an `IQueryable` collection wrapping the list:
  * `list.AsRealmQueryable()` allows you to get a `IQueryable<T>` from `IList<T>` that can be then treated as a regular queryable collection and filtered/ordered with LINQ or `Filter(string)`.
  * `list.Filter(query, arguments)` will filter the list and return the filtered collection. It is roughly equivalent to `list.AsRealmQueryable().Filter(query, arguments)`.

  The resulting queryable collection will behave identically to the results obtained by calling `realm.All<T>()`, i.e. it will emit notifications when it changes and automatically update itself. (Issue [#1499](https://github.com/realm/realm-dotnet/issues/1499))
* Added a cache for the Realm schema. This will speed up `Realm.GetInstance` invocations where `RealmConfiguration.ObjectClasses` is explicitly set. The speed gains will depend on the number and complexity of your model classes. A reference benchmark that tests a schema containing all valid Realm property types showed a 25% speed increase of Realm.GetInstance. (Issue [#2194](https://github.com/realm/realm-dotnet/issues/2194))
* Improve performance of creating collection notifiers for Realms with a complex schema. In the SDKs this means that the first run of a synchronous query, first call to subscribe for notifications will do significantly less work on the calling thread.
* Improve performance of calculating changesets for notifications, particularly for deeply nested object graphs and objects which have List or Set properties with small numbers of objects in the collection.
* Query parser now accepts `BETWEEN` operator. Can be used like `realm.All<Person>().Filter("Age BETWEEN {20, 60}")` which means "'Age' must be in the open interval ]20;60[". (Core upgrade)

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.3.1.
* Removed the RealmStates dictionary that used to hold a threadlocal dictionary of all the states for the opened Realms. It was only used for detecting open Realms during deletion and that is now handled by the native `delete_realm_files` method. (PR [#2251](https://github.com/realm/realm-dotnet/pull/2251))
* Stopped sending analytics to mixpanel.
* Started uploading benchmark results to [MongoDB Charts](https://charts.mongodb.com/charts-realm-sdk-metrics-yxjvt/public/dashboards/6115babd-c7fe-47ee-836f-efffd92ffae3). (Issue [#2226](https://github.com/realm/realm-dotnet/issues/2226))
* Removed the dedicated benchmark workflows from GHA. (Issue [#2562](https://github.com/realm/realm-dotnet/issues/2562))
* Use the Win81 SDK when building the Windows wrappers on Github Actions. (Issue [#2530](https://github.com/realm/realm-dotnet/issues/2530))
* Added CodeQL workflow. (Issue [#2155](https://github.com/realm/realm-dotnet/issues/2155))
* Started tracking package and wrapper sizes over time. (Issue [#2225](https://github.com/realm/realm-dotnet/issues/2225))
* Removed the `[Serializable]` attribute from RealmObjectBase as `BinarySerializer` is now obsolete. (PR [#2578](https://github.com/realm/realm-dotnet/pull/2578))
* Added code coverage job to Github Actions. (PR [#2581](https://github.com/realm/realm-dotnet/pull/2581))
* Added CI tests running on Windows 8.1 . (PR [#2580](https://github.com/realm/realm-dotnet/pull/2580))

## 10.3.0 (2021-07-07)

**Note**: This release uses xcframework and enables bitcode for the iOS native libraries. This significantly increases the package size and may appear to increase the .ipa size when compiling for iOS. However, the bitcode portion, as well as the unnecessary architectures, will be trimmed by the App Store, so the size of the actual download sent to users will be unchanged or smaller than before.

### Fixed
* Fixed an issue that would prevent `realm-wrappers.dll` from being loaded on Windows 8.1. (Issue [#2298](https://github.com/realm/realm-dotnet/issues/2298))
* Fixed an assertion failure when listening for changes to a list of primitive Mixed which contains links. (Core upgrade)
* Fixed an assertion failure when listening for changes to a dictionary or set which contains an invalidated link. (Core upgrade)
* Fixed an endless recursive loop that could cause a stack overflow when computing changes on a set of objects which contained cycles. (Core upgrade)
* Add collision handling to Dictionary implementation. (Core upgrade)
* Fixed a crash after clearing a list or set of Mixed containing links to objects. (Core upgrade)
* Fixed a recursive loop which would eventually crash trying to refresh a user app token when it had been revoked by an admin. Now this situation logs the user out and reports an error. (Core upgrade)
* Fixed a race between calling `Realm.DeleteRealm` and concurrent opening of the realm file. (Core upgrade)
* \[Unity\] Added code to preserve the constructors of several base serializers to ensure that most of the basic serialization/deserialization workloads work out of the box. (PR [#2489](https://github.com/realm/realm-dotnet/pull/2489))

### Enhancements
* Changed the native iOS library to use xcframework. This means that running in the simulator on M1 macs is now supported. (Issue [#2240](https://github.com/realm/realm-dotnet/issues/2240))
* Added bitcode to the native iOS library. This has no effect on Xamarin.iOS, but allows Unity applications to take advantage of optimizations performed by the App Store servers and eventually support new architectures as they are released. (Issue [#2240](https://github.com/realm/realm-dotnet/issues/2240))

### Compatibility
* Realm Studio: 11.0.0 or later.
* This release uses xcframework for the iOS native libraries, which requires Xamarin.iOS 14.14.2.5 or later.

### Internal
* Using Core 11.0.4.

## 10.2.1 (2021-06-30)

This release changes the way Unity binaries are packaged and obviates the need to have an extra Unity package that contains the dependencies as standalone modules. If you were using the `io.realm.unity-bundled` package, please remove it and add the newly released `io.realm.unity` one.

### Fixed
* \[Unity\] Fixed an issue where failing to weave an assembly due to modeling errors, would only show an error in the logs once and then fail opening a Realm with `No RealmObjects. Has linker stripped them?`. Now, the weaving errors will show up on every code change/weave attempt and the runtime error will explicitly suggest manually re-running the weaver. (Issue [#2310](https://github.com/realm/realm-dotnet/issues/2310))
* \[Unity\] Fixed an issue that would cause the app to hang on exit when using Sync. (PR [#2467](https://github.com/realm/realm-dotnet/pull/2467))
* \[Unity\] Fixed an issue that would cause the Unity editor on macOS to hang after assembly reload if the app uses Sync. (Issue [#2482](https://github.com/realm/realm-dotnet/issues/2482))
* Fixed an issue where a crash could happen on Android x86 due to converting UInt32 into TableKey and Int64 into ObjKey incorrectly. (Issue [#2456](https://github.com/realm/realm-dotnet/issues/2456))

### Enhancements
* None

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.0.3.
* GetHashCode() on objects now uses the table key in addition to the object key. (Issue [#2473](https://github.com/realm/realm-dotnet/issues/2473))

## 10.2.0 (2021-06-15)

### Fixed
* Fixed a bug where applying multiple `OrderBy` clauses on a query would result in the clauses being appended to each other as if they
were `.ThenBy` rather than the last clause replacing the preceding ones. (PR [#2255](https://github.com/realm/realm-dotnet/issues/2255))
* When explicitly specifying `SyncConfiguration.ObjectTypes`, added a check to validate the schema and ensure all `EmbeddedObject` classes
are reachable from a class inheriting from `RealmObject`. More info about this subject can be found
[here](https://docs.mongodb.com/realm/dotnet/objects/#provide-a-subset-of-classes-to-your-realm-schema). (PR [#2259](https://github.com/realm/realm-dotnet/pull/2259))
* Fixed a bug that would result in an error similar to `Undefined symbols for architecture xxx: "_realm_thread_safe_reference_destroy"`
when building a Unity project for iOS. (Issue [#2318](https://github.com/realm/realm-dotnet/issues/2318))
* The weaver will now emit an error if you try to define a collection of `RealmInteger` values. This has never been supported, but
previously it would fail silently whereas now it'll be a compile time error. (Issue [#2308](https://github.com/realm/realm-dotnet/issues/2308))
* Fixed an issue where using collections of managed objects (lists or results) in a Unity project would result in an invalid compiled binary. (PR [#2340](https://github.com/realm/realm-dotnet/pull/2340))
* Fixed a memory leak when a migration callback is defined, but the Realm didn't actually need to run it (PR [#2331](https://github.com/realm/realm-dotnet/pull/2331))
* Added back 32bit support for iOS builds. (Issue [#2429](https://github.com/realm/realm-dotnet/issues/2429))
* Removed redundant warnings when building a Unity project for device that mentioned that the schema for Realm and Realm.UnityUtils
is empty. (Issue [#2320](https://github.com/realm/realm-dotnet/issues/2320))
* Fixed an issue that could cause `NullReferenceException` to be thrown if you set `SyncConfiguration.OnProgress` to `null` shortly
after calling `Realm.GetInstanceAsync(syncConfig)`. (Issue [#2400](https://github.com/realm/realm-dotnet/issues/2400))
* When replacing an embedded object, emit a sync instruction that sets the link to the embedded object to null so that it is properly cleared.
This resolves an issue that would have manifested itself as `Failed to parse, or apply received changeset: ERROR: ArrayInsert: Invalid prior_size (list size = 4, prior_size = 0)`
([#4740](https://github.com/realm/realm-core/issues/4740)
* Made Linux implementation of ExternalCommitHelper work with new versions of Linux that
[changed epoll behavior](https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git/commit/?id=6a965666b7e7475c2f8c8e724703db58b8a8a445),
including Android 12 (Issue [#4666](https://github.com/realm/realm-core/issues/4666))
* The file format is changed in the way that we now - again - have search indexes on primary key columns. This is required as we now stop deriving the
ObjKeys from the primary key values, but just use an increasing counter value. This has the effect that all new objects will be created in the same
cluster and not be spread out as they would have been before. It also means that upgrading from file format version 11 and earlier formats will be much faster. (Core upgrade)

### Enhancements
* Add support for the `Guid` data type. It can be used as primary key and is indexable. (PR [#2120](https://github.com/realm/realm-dotnet/pull/2120))
* Add support for dictionaries. Currently only string keys are supported, while the value
  type may be any of the supported types (the primitive types, `RealmValue`, or custom types that inherit
  from RealmObject/EmbeddedObject). Lists, sets, or other dictionaries may not be used as
  the value type. To add a dictionary to your model, define a getter-only property of type
  `IDictionary<string, T>`:

  ```csharp
  public class MyObject : RealmObject
  {
      public IDictionary<string, decimal> Denominations { get; }
  }

  // Realm will automatically manage the underlying dictionary, so there's no need
  // to define a constructor  or assign it to some value.

  var obj = new MyObject();
  obj.Denominations.Add("quarter", 0.25d);
  ```
* Add support for `RealmValue` data type. This new type can represent any valid Realm data type, including objects. Collections
(lists, sets and dictionaries) of `RealmValue` are also supported, but `RealmValue` itself cannot contain collections. Please
note that a property of type `RealmValue` cannot be nullable, but can contain null, represented by the value `RealmValue.Null`.
(PR [#2252](https://github.com/realm/realm-dotnet/pull/2252))

  ```csharp
  public class MyObject : RealmObject
  {
      public RealmValue MyValue { get; set; }

      public IList<RealmValue> ValuesList { get; }

      public ISet<RealmValue> ValuesSet { get; }

      public IDictionary<string, RealmValue> ValuesDict { get; }
  }

  var obj = new MyObject();
  obj.MyValue = RealmValue.Null;
  obj.MyValue = 1;
  obj.MyValue = "abc";

  if (obj.MyValue.Type == RealmValueType.String)
  {
      var myString = obj.MyValue.AsString();
  }
  ```
* Add support for sets of objects or primitive values. Sets are unordered collections that ensure uniqueness of their elements. Realm uses its internal equality comparer
and it is not possible to customize its behavior by overriding `Equals` or `GetHashCode` on your custom classes. Objects will always be compared by db reference - i.e.
two distinct objects in the database will always be different, even if their contents are identical, and multiple references to the same database object will always be
equal.
  ```csharp
  public class MyObject : RealmObject
  {
      public ISet<string> UniqueStrings { get; }
  }

  // Realm will automatically manage the underlying set, so there's no need
  // to define a constructor  or assign it to some value.

  var obj = new MyObject();
  var didAdd = obj.UniqueStrings.Add("foo"); // true
  didAdd = obj.UniqueStrings.Add("foo"); // false
  ```
* Added support for value substitution in string based queries. This enables expressions following
[this syntax](https://docs.mongodb.com/realm/reference/realm-query-language/): `realm.All<T>().Filter("field1 = $0 && field2 = $1", 123, "some-string-value")`.
(Issue [#1822](https://github.com/realm/realm-dotnet/issues/1822))
* Reduced the size of the native binaries by ~5%. (PR [#2239](https://github.com/realm/realm-dotnet/pull/2239))
* Added a new class - `Logger`, which allows you to override the default logger implementation (previously writing to `stdout` or `stderr`) with a custom one by setting
`Logger.Default`. This replaces `AppConfiguration.CustomLogger` and `AppConfiguration.LogLevel` which will be removed in a future release. The built-in implementations are:
  * `Console` - uses the `System.Console` for most projects and `UnityEngine.Debug` for Unity projects: `Logger.Default = Logger.Console;`
  * `Null` - ignores all messages: `Logger.Default = Logger.Null;`
  * `Function` - proxies calls to a supplied function: `Logger.Default = Logger.Function(message => myExternalLogger.Log(message));`

  Custom loggers can derive from the `Logger` class and provide their own implementation for the `Log` method or use `Function` and provide an `Action<string>`. (PR [#2276](https://github.com/realm/realm-dotnet/pull/2276))
* `RealmObjectBase` now correctly overrides and implements `GetHashCode()`. (Issue [#1650](https://github.com/realm/realm-dotnet/issues/1650))
* Added an override of `RealmObject.ToString()` to output more meaningful information about the object content. It will output
the type of the object, the primary key (if one is defined), as well as information whether the object is managed or deleted.
(Issue [#2347](https://github.com/realm/realm-dotnet/pull/2347))
* Added new API for dynamically accessing object properties. These are designed to support
ahead-of-time compiled platforms, such as Xamarin.iOS and Unity with IL2CPP compilation. The
intention is to eventually make these the default API, while also supporting the legacy DLR-based
API. Example:
  ```csharp
  // Make sure to cast away the dynamic immediately on AOT platforms.
  var people = (IQueryable<RealmObject>)realm.DynamicApi.All("Person");
  foreach (var person in people)
  {
      var firstName = person.DynamicApi.Get<string>("FirstName");
      var address = person.DynamicApi.Get<EmbeddedObject>("Address");
      var city = address.DynamicApi.Get<string>("City");
  }

  // When casting a dynamic object, always cast first to object and then
  // to the actual object type to remove any callsites being generated.
  var newPerson = (RealmObject)(object)realm.DynamicApi.Create("Person", 123);
  newPerson.DynamicApi.Set("FirstName", "Peter");
  ```
* Added a Unity Editor option to enable weaving editor assemblies. This should be "off" unless your project has Editor assemblies
that reference Realm - for example, an EditMode test assembly that tests Realm-related functionality. Keeping it "on" may slow down
builds a little as more assemblies will need to be evaluated for weaving. (Issue [#2346](https://github.com/realm/realm-dotnet/issues/2346))
* We now make a backup of the realm file prior to any file format upgrade. The backup is retained for 3 months.
Backups from before a file format upgrade allows for better analysis of any upgrade failure. We also restore
a backup, if a) an attempt is made to open a realm file whith a "future" file format and b) a backup file exist
that fits the current file format. ([#4166](https://github.com/realm/realm-core/pull/4166))

### Compatibility
* Realm Studio: 11.0.0-alpha.0 or later.

### Internal
* Using Core 11.0.3.
* Enabled LTO builds for all platforms except Android. (PR [#2239](https://github.com/realm/realm-dotnet/pull/2239))
* Test projects updated to dotnetcore 3.1. This means that tests are no longer executed against dotnetcore 2.0.
* Removed Lambda compilation in ResultsVisitor when we encounter a conversion operator. This
  is needed because IL2CPP cannot comiple lambdas dynamically. Instead, we're now using
  `Operator.Convert<TTarget>(object)` which is slightly less efficient than `Operator.Convert<TSource, TTarget>`
  but still quite a bit faster than `Convert.ChangeType` and also doesn't suffer from the
  deficiencies around `Decimal128` conversion. The main downside is that we'll no longer
  support queries with an argument that is a custom user type with an implicit conversion
  operator defined.

## 10.1.4 (2021-05-12)
------------------

### Fixed
* Fixed a bug that could lead to crashes with a message similar to `Invalid ref translation entry [0, 78187493520]`. (Core upgrade)
* Fix assertion failures such as `!m_notifier_skip_version.version` or `m_notifier_sg->get_version() + 1 == new_version.version` when performing writes inside change notification callbacks. (Core upgrade)
* Fix collection notification reporting for modifications. This could be observed by receiving the wrong indices of modifications on sorted or distinct results, or notification blocks sometimes not being called when only modifications have occured. (Core upgrade)
* Proactively check the expiry time on the access token and refresh it before attempting to initiate a sync session. This prevents some error logs from appearing on the client such as: `ERROR: Connection[1]: Websocket: Expected HTTP response 101 Switching Protocols, but received: HTTP/1.1 401 Unauthorized`. (Core upgrade)
* Destruction of the TableRecycler at exit was unordered compared to other threads running. This could lead to crashes, some with the TableRecycler at the top of the stack. (Core upgrade)
* Fixed errors related to `uncaught exception in notifier thread: N5realm11KeyNotFoundE: No such object`. This could happen in a synchronized app when a linked object was deleted by another client. (Core upgrade)
* Opening a metadata realm with the wrong encryption key or different encryption configuration will remove that metadata realm and create a new metadata realm using the new key or configuration. (Core upgrade)
* Creting a `ThreadSafeReference` to a readonly Realm would result in a crash. (Core upgrade)

### Compatibility
* Realm Studio: 10.0.0 or later.

### Internal
* Using Core 10.7.2.

## 10.1.3 (2021-04-29)
------------------

### Fixed
* Fixed a compiler bug that would result in an `"Access violation"` error being thrown when using sync on Windows.

### Compatibility
* Realm Studio: 10.0.0 or later.

### Internal
* Using Core 10.5.6.

## 10.1.2 (2021-03-19)
------------------

### Fixed
* On 32bit devices you may get exception with "No such object" when upgrading to v10. (Core upgrade)
* The notification worker thread would rerun queries after every commit rather than only commits which modified tables which could affect the query results if the table had any outgoing links to tables not used in the query. (Core upgrade)
* Fix "Invalid ref translation entry [16045690984833335023, 78187493520]" assertion failure which could occur when using sync or multiple processes writing to a single Realm file. (Core upgrade)
* During integration of a large amount of data from the server, you may get `"Assertion failed: !fields.has_missing_parent_update()"`. (Core upgrade)
* Syncing large Decimal128 values will cause `"Assertion failed: cx.w[1] == 0"`. (Core upgrade)
* Avoid race condition leading to possible hangs on windows. (Core upgrade)

### Enhancements
* None

### Fixed
* None

### Compatibility
* Realm Studio: 10.0.0 or later.

### Internal
* Using Core 10.5.6.

## 10.1.1 (2021-02-25)
------------------

### Fixed
* Fixed an issue that would result in UWP apps being rejected from the Microsoft Store due to an unsupported API (`__C_specific_handler`) being used. (Issue [#2235](https://github.com/realm/realm-dotnet/issues/2235))
* The Realm notification listener thread could sometimes hit the assertion failure "!skip_version.version" if a write transaction was committed at a very specific time. (Core upgrade)

### Enhancements
* None

### Fixed
* None

### Compatibility
* Realm Studio: 10.0.0 or later.

### Internal
* Using Core 10.5.3.

## 10.1.0 (2021-02-09)

### Enhancements
* Sync client now logs error messages received from server rather than just the size of the error message. (Core upgrade)
* Errors returned from the server when sync WebSockets get closed are now captured and surfaced as a SyncError. (Core upgrade)
* Dramatically improved performance of sequential reads on a query without a filter. (Core upgrade)

### Fixed
* Fix an issue when using a frozen query across threads with different transaction versions which resulted in being able to access objects from a future version in the frozen collection. (Core upgrade)
* Fixed an issue where creating an object after file format upgrade may fail with assertion "Assertion failed: lo() <= std::numeric_limits<uint32_t>::max()" (Core upgrade)
* Fixed an issue where getting an element from a query result without a filter would give incorrect results if a new object was created at index zero in the source Table. (Core upgrade)
* Fixed an issue where during synchronization the app would crash with `Assertion failed: ref + size <= next->first`. (Core upgrade)

### Compatibility
* Realm Studio: 10.0.0 or later.

### Internal
* Using Core 10.5.0.
* Fixes the analytics version being sent.

## 10.0.1 (2021-02-02)

### Breaking Changes
* We no longer support Realm Cloud (legacy), but instead the new [MongoDB Realm Cloud](https://realm.mongodb.com). MongoDB Realm is a serverless platform that enables developers to quickly build applications without having to set up server infrastructure. MongoDB Realm is built on top of MongoDB Atlas, automatically integrating the connection to your database. ([#2011](https://github.com/realm/realm-dotnet/pull/2011))
* Remove support for Query-based sync, including the configuration parameters and the `SyncSubscription` types. ([#2011](https://github.com/realm/realm-dotnet/pull/2011))
* Remove everything related to sync permissions, including both the path-based permission system and the object-level privileges for query-based sync. [Permissions in MongoDB Realm](https://docs.mongodb.com/realm/sync/permissions/) are defined serverside. ([#2011](https://github.com/realm/realm-dotnet/pull/2011))
* Moved all API for dynamic access on the `Realm` class to `Realm.DynamicApi`:
  * `Realm.CreateObject(string className, object primaryKey)` is now `Realm.DynamicApi.CreateObject(string className, object primaryKey)`.
  * `Realm.All(string className)` is now `Realm.DynamicApi.All(string className)`.
  * `Realm.RemoveAll(string className)` is now `Realm.DynamicApi.RemoveAll(string className)`.
  * `Realm.Find(string className, long? primaryKey)` is now `Realm.DynamicApi.Find(string className, long? primaryKey)`.
  * `Realm.Find(string className, string primaryKey)` is now `Realm.DynamicApi.Find(string className, string primaryKey)`.
* It is now required that all top-level objects in a synchronized Realm have a primary key called `_id`. You can use the `MapTo("_id")` attribute to avoid using unidiomatic names for the model properties.
* Bumped the minimum target for Xamarin.iOS apps to iOS 9.
* Bumped the minimum API level for Xamarin.Android apps to 16 (Android 4.1).
* Renamed `FullSyncConfiguration` to `SyncConfiguration`.
* Removed `RealmObject.FreezeInPlace`. To freeze a realm object use the `Freeze` extension method. (Issue [#2180](https://github.com/realm/realm-dotnet/issues/2180))

### Enhancements
* Added support for syncing to MongoDB instead of Realm Object Server. Applications must be created at [realm.mongodb.com](https://realm.mongodb.com).
* Added an `App` class which is the entrypoint for synchronizing with a MongoDB Realm App.
* Added `User.CustomData` containing an unstructured document with additional information about the user. Custom data is configured in your MongoDB Realm App.
* Added `User.Functions`. This is the entry point for calling Remote MongoDB Realm functions. Functions allow you to define and execute server-side logic for your application. Functions are written in modern JavaScript (ES6+) and execute in a serverless manner. When you call a function, you can dynamically access components of the current application as well as information about the request to execute the function and the logged in user that sent the request.
* Added `User.GetMongoClient` exposing an API for CRUD operations on a Remote MongoDB Service.
* Added `User.GetPushClient` exposing an API for registering a device for push notifications.
* Change `SyncConfiguration` to accept partition value instead of a server Uri. Partition values can currently be of types `string`, `long`, or `ObjectId`. Opening a realm by partition value is the equivalent of previously opening a realm by URL. In this case, partitions are meant to be more closely associated with your data. E.g., if you are a large retailer with multiple locations, the partition key can be the store Id and you each Realm will only contain data related to the specified store.
* Add support for the Decimal128 data type. This is a 128-bit IEEE 754 decimal floating point number. Properties of this type can be declared either as `MongoDB.Bson.Decimal128` type or the built-in `decimal` type. Note that .NET's built-in decimal is 96-bit, so it cannot represent the full range of numbers, representable by `Decimal128`. (PR [#2014](https://github.com/realm/realm-dotnet/pull/2014))
* Add support for the `ObjectId` data type. This is a 12 byte unique identifier that is common as a document id in MongoDB databases. It can be used as primary key. (PR [#2035](https://github.com/realm/realm-dotnet/pull/2035))
* Add support for embedded objects. Embedded objects are objects which are owned by a single parent object, and are deleted when that parent object is deleted or their parent no longer references them. Embedded objects are declared by subclassing `EmbeddedObject` instead of `RealmObject`. Reassigning an embedded object is not allowed and neither is linking to it from multiple parents. Querying for embedded objects directly is also disallowed as they should be viewed as complex structures belonging to their parents as opposed to standalone objects. A trivial example is:

  ```csharp
  public class Address : EmbeddedObject
  {
      public string Street { get; set; }

      public string City { get; set; }
  }

  public class Person : RealmObject
  {
      public string Name { get; set; }

      // Address is an embedded object - you reference it as usual
      public Address Address { get; set; }
  }

  public class Company : RealmObject
  {
      public string PhoneNumber { get; set; }

      // Embedded objects can be contained in lists too
      public IList<Address> OfficeAddresses { get; }
  }
  ```

* Added new dynamic methods for instantiating embedded objects:
  * `Realm.DynamicApi.CreateEmbeddedObjectForProperty` should be used to create an embedded object and assign it to a parent's property. For example:

    ```csharp
    // static API
    var person = new Person();
    person.Address = new Address
    {
        City = "New York"
    };

    // dynamic API
    var dynamicPerson = realm.DynamicApi.CreateObject("Person");
    var address = realm.DynamicApi.CreateEmbeddedObjectForProperty(dynamicPerson, "Address")
    address.City = "New York";
    ```

  * `Realm.DynamicApi.AddEmbeddedObjectToList` should be used to create an embedded object and add it to a parent's list property.
  * `Realm.DynamicApi.InsertEmbeddedObjectInList` should be used to create an embedded object and insert it in a parent's list property at a specified index.
  * `Realm.DynamicApi.SetEmbeddedObjectInList` should be used to create an embedded object and set it at an index in a parent's list property.

    ```csharp
    // static API
    var company = new Company();
    company.OfficeAddresses.Add(new Address
    {
        City = "New York"
    });

    company.OfficeAddresses.Insert(0, new Address
    {
        City = "Palo Alto"
    });

    company.OfficeAddresses[1] = new Address
    {
        City = "New Jersey"
    };

    // dynamic API
    var dynamicCompany = realm.DynamicApi.CreateObject("Company");
    var officeToAdd = realm.DynamicApi.AddEmbeddedObjectToList(dynamicCompany.OfficeAddresses);
    officeToAdd.City = "New York";

    var officeToInsert = realm.DynamicApi.InsertEmbeddedObjectInList(dynamicCompany.OfficeAddresses, 0);
    officeToInsert.City = "Palo Alto";

    var officeToSet = realm.DynamicApi.SetEmbeddedObjectInList(dynamicCompany.OfficeAddresses, 1);
    officeToSet.City = "New Jersey";
    ```

* The memory mapping scheme for Realm files has changed to better support opening very large files.
* Replaced the implementation of the string query parser (the one used for [`realm.All().Filter("some-string-query")`](https://docs.mongodb.com/realm-sdks/dotnet/10.0.0-beta.3/reference/Realms.CollectionExtensions.html#Realms_CollectionExtensions_Filter__1_System_Linq_IQueryable___0__System_String_)). This results in ~5% reduction of the size of the native binary while keeping the query execution times on par with the old parser. (PR [#2185](https://github.com/realm/realm-dotnet/pull/2185), Core upgrade)
* Optimized the internal code that handles conversions between types. This should result in a minor performance increase
for most data operations that should be most noticeable on Ahead-of-Time compiled platforms, such as iOS/UWP. Due to the
nature of the change, it's possible that conversions that previously happened automatically when working with dynamic objects
no longer do. If you encounter a `NotSupportedException` with the message `No conversion exists from *type A* to *type B*`
and believe this is a bug, please open a Github Issue. (PR [#2149](https://github.com/realm/realm-dotnet/pull/2149))
* Added an extra compile-time check to detect erroneous List<T> declarations and suggest IList<T> for collection properties in Realm objects. (Issue [#2083](https://github.com/realm/realm-dotnet/pull/2083))
* Added overloads for `Realm.Write` and `Realm.WriteAsync` that can return a value. (Issue [#2081](https://github.com/realm/realm-dotnet/issues/2081))

### Fixed
* Worked around an issue with the .NET Native compiler (used in UWP projects) that would result in the following exception being thrown in Release: `Incompatible MarshalAs detected in parameter named 'value'. Please refer to MCG's warning message for more information.`. (Issue [#2169](https://github.com/realm/realm-dotnet/issues/2169))
* Fixed a bug that could cause incorrect property values to be read during a migration for apps running on .NET Core 3.0 or newer.
  The issue manifests itself when different classes have persisted properties with the same name and could result in
  the wrong property being accessed - e.g. `foo.Name` could return `foo.Bar`. This could only happen when using the
  dynamic API during a migration and does not affect apps that use the strongly typed API or run on platforms other
  than .NET Core 3.x/.NET 5.
* Fixed a bug that could cause a deadlock in a multiprocess scenario where multiple processes share the same Realm file and listen for notifications from the file. (Core upgrade)
* Fixed an issue with deleting and recreating objects with embedded objects. (Core upgrade)
* Fix a race condition which would lead to "uncaught exception in notifier thread: N5realm15InvalidTableRefE: transaction_ended" and a crash when the source Realm was closed or invalidated at a very specific time during the first run of a collection notifier (Core upgrade)
* Fix crash in case insensitive query on indexed string columns when nothing matches (Core upgrade)

### Compatibility
* Realm Studio: 10.0.0 or later.

### Internal
* Using Core 10.3.3.
* Migrated to bison parser.
* Submit Analytics to S3/Segment in addition to Mixpanel.
* Analytics now also reports if Sync functionality is in use.
* SDK is now also tested against .NET 5.
* This release uses monorepo releases that bundle Core, Sync, and OS.
* Replaced Expressions-based Operator with T4. (PR [#2149](https://github.com/realm/realm-dotnet/pull/2149))

## 5.1.3 (2021-02-10)

### Fixed
* If you make a case insensitive query on an indexed string column, it may fail in a way that results in a "No such key" exception. (Core upgrade)
* Fix crash in case insensitive query on indexed string columns when nothing matches. (Core upgrade)
* Files upgraded on 32-bit devices could end up being inconsistent resulting in "Key not found" exception to be thown. (Core upgrade)
* Fixed an issue where creating an object after file format upgrade may fail with assertion `Assertion failed: lo() <= std::numeric_limits<uint32_t>::max()`. (Core upgrade)

### Compatibility
* Realm Object Server: 3.23.1 or later.
* Realm Studio: 5.0.0 or later.

### Internal
* Using Sync 5.0.32 and Core 6.2.3.
* Updated the QuickJournal example to latest Realm and Xamarin.Forms versions. (PR [#2057](https://github.com/realm/realm-dotnet/pull/2057))

## 5.1.2 (2020-10-20)

### Fixed
* Fixed an issue that would result in `Realm accessed from incorrect thread` exception being thrown when accessing a Realm instance on the main thread in UWP apps. (Issue [#2045](https://github.com/realm/realm-dotnet/issues/2045))

### Compatibility
* Realm Object Server: 3.23.1 or later.
* Realm Studio: 5.0.0 or later.

### Internal
* Using Sync 5.0.28 and Core 6.1.3.
* Updated the QuickJournal example to latest Realm and Xamarin.Forms versions. (PR [#2057](https://github.com/realm/realm-dotnet/pull/2057))

## 5.1.1 (2020-10-02)

### Enhancements
* None

### Fixed
* Querying on an indexed property may give a “Key not found” exception. (Core upgrade)
* Fix queries for null on non-nullable indexed integer columns returning results for zero entries. (Core upgrade)

### Compatibility
* Realm Object Server: 3.23.1 or later.
* Realm Studio: 5.0.0 or later.

### Internal
* Using Sync 5.0.28 and Core 6.1.3.


## 5.1.0 (2020-09-30)

### Enhancements
* Greatly improve performance of NOT IN queries on indexed string or int columns. (Core upgrade)

### Fixed
* Fixed an issue that would cause using Realm on the main thread in WPF applications to throw an exception with a message "Realm accessed from the incorrect thread". (Issue [#2026](https://github.com/realm/realm-dotnet/issues/2026))
* Fixed an issue that could cause an exception with the message "Opening Realm files of format version 0 is not supported by this version of Realm" when opening an encrypted Realm. (Core upgrade)
* Slightly improve performance of most operations which read data from the Realm file. (Core upgrade)
* Rerunning an equals query on an indexed string column which previously had more than one match and now has one match would sometimes throw a "key not found" exception. (Core upgrade)
* When querying a table where links are part of the condition, the application may crash if objects has recently been added to the target table. (Core upgrade)

### Compatibility
* Realm Object Server: 3.23.1 or later.
* Realm Studio: 5.0.0 or later.

### Internal
* Using Sync 5.0.27 and Core 6.1.2.
* Added prerelease nuget feed via [GitHub packages](https://github.com/features/packages). (PR [#2028](https://github.com/realm/realm-dotnet/pull/2028))

## 5.0.1 (2020-09-10)

NOTE: This version bumps the Realm file format to version 11. It is not possible to downgrade to version 10 or earlier. Files created with older versions of Realm will be automatically upgraded. Only [Realm Studio 5.0.0](https://github.com/realm/realm-studio/releases/tag/v5.0.0) or later will be able to open the new file format.

### Enhancements
* Added the notion of "frozen objects" - these are objects, queries, lists, or Realms that have been "frozen" at a specific version. This allows you to access the data from any thread, but it will never change. All frozen objects can be accessed and queried as normal, but attempting to mutate them or add change listeners will throw an exception. (Issue [#1945](https://github.com/realm/realm-dotnet/issues/1945))
  * Added `Realm.Freeze()`, `RealmObject.Freeze()`, `RealmObject.FreezeInPlace()`, `IQueryable<RealmObject>.Freeze()`, `IList<T>.Freeze()`, and `IRealmCollection<T>.Freeze()`. These methods will produce the frozen version of the instance on which they are called.
  * Added `Realm.IsFrozen`, `RealmObject.IsFrozen`, and `IRealmCollection<T>.IsFrozen`, which returns whether or not the data is frozen.
  * Added `RealmConfigurationBase.MaxNumberOfActiveVersions`. Setting this will cause Realm to throw an exception if too many versions of the Realm data are live at the same time. Having too many versions can dramatically increase the filesize of the Realm.
* Add support for `SynchronizationContext`-confined Realms. Rather than being bound to a specific thread, queue-confined Realms are bound to a `SynchronizationContext`, regardless of whether it dispatches work on the same or a different thread. Opening a Realm when `SynchronizationContext.Current` is null - most notably `Task.Run(...)` - will still confine the Realm to the thread on which it was opened.
* Storing large binary blobs in Realm files no longer forces the file to be at least 8x the size of the largest blob.
* Reduce the size of transaction logs stored inside the Realm file, reducing file size growth from large transactions.
* String primary keys no longer require a separate index, improving insertion and deletion performance without hurting lookup performance.

### Fixed
* Fixed `Access to invalidated List object` being thrown when adding objects to a list while at the same time deleting the object containing the list. (Issue [#1971](https://github.com/realm/realm-dotnet/issues/1971))
* Fixed incorrect results being returned when using `.ElementAt()` on a query where a string filter with a sort clause was applied. (PR [#2002](https://github.com/realm/realm-dotnet/pull/2002))

### Compatibility
* Realm Object Server: 3.23.1 or later.
* Realm Studio: 5.0.0 or later.

### Internal
* Using Sync 5.0.22 and Core 6.0.25.

## 4.3.0 (2020-02-05)

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

## 4.2.0 (2019-10-07)

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

## 4.1.0 (2019-08-06)

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

## 4.0.1 (2019-06-27)

### Fixed
* Fixed an issue that would prevent iOS apps from being published to the app store with the following error:
  > This bundle Payload/.../Frameworks/realm-wrappers.framework is invalid. The Info.plist file is missing the required key: CFBundleVersion.

  ([Issue 1870](https://github.com/realm/realm-dotnet/issues/1870), since 4.0.0)
* Fixed an issue that would cause iOS apps to crash on device upon launching. ([Issue 1871](https://github.com/realm/realm-dotnet/issues/1871), since 4.0.0)

## 4.0.0 (2019-06-13)

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

## 3.4.0 (2019-01-09)

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


## 3.3.0 (2018-11-08)

### Enhancements
* Exposed an `OnProgress` property on `SyncConfigurationBase`. It allows you to specify a progress callback that will be invoked when using `Realm.GetInstanceAsync` to report the download progress. ([#1807](https://github.com/realm/realm-dotnet/pull/1807))

### Fixed
<!-- * <How to hit and notice issue? what was the impact?> ([#????](https://github.com/realm/realm-dotnet/issues/????), since v?.?.?) -->
* Trying to call `Subscription.WaitForSynchronizationAsync` on a background thread (without a `SynchronizationContext`) would previously hang indefinitely. Now a meaningful exception will be thrown to indicate that this is not supported and this method should be called on a thread with a synchronization context. ([dotnet-private#130](https://github.com/realm/realm-dotnet-private/issues/130), since v3.0.0)

### Compatibility
* Realm Object Server: 3.0.0 or later.
* APIs are backwards compatible with all previous releases in the 3.x.y series.
* File format: Generates Realms with format v9 (Reads and upgrades all previous formats)


## 3.2.1 (2018-09-27)

### Bug fixes
- Fixed a bug that would typically result in exceptions with a message like `An unknown error has occurred. State: *some-number-larger than 127*`
when subscribing to queries. ([dotnet-private#128](https://github.com/realm/realm-dotnet-private/issues/128), since `3.0.0`)

## 3.2.0 (2018-08-04)

### Enhancements
- `RealmObject` inheritors will now raise `PropertyChanged` after they have been removed from Realm.
The property name in the event arguments will be `IsValid`.
- Bundle some common certificate authorities on Linux so connecting to ROS instances over SSL should work out of the box
for most certificates. Notably, it will now work out of the box for Realm Cloud instances.

### Bug fixes
- When constructing queries that compare an invalid/unmanaged RealmObject (e.g. `realm.All<Foo>().Where(f => f.Bar == someBar)`),
a meaningful exception will now be thrown rather than an obscure ArgumentNullException.
- Added `ShouldCompactOnLaunch` to the PCL version of the library. ([dotnet-private#125](https://github.com/realm/realm-dotnet-private/issues/125))

## 3.1.0 (2018-07-04)

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

## 3.0.0 (2018-04-16)

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
  [query language reference docs](https://docs.mongodb.com/realm/reference/realm-query-language/) or the [NSPredicate Cheatsheet](https://academy.realm.io/posts/nspredicate-cheatsheet/).
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


## 2.2.0 (2017-03-22)

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

## 2.1.0 (2017-11-13)

### Enhancements
- Added an `[Explicit]` attribute that can be applied to classes or assemblies. If a class is decorated with it, then it will not be included in the default schema for the Realm (i.e. you have to explicitly set `RealmConfiguration.ObjectClasses` to an array that contains that class). Similarly, if it is applied to an assembly, all classes in that assembly will be considered explicit. This is useful when developing a 3rd party library that depends on Realm to avoid your internal classes leaking into the user's schema. ([#1602](https://github.com/realm/realm-dotnet/pull/1602))

### Bug fixes
- Fixed a bug that would prevent writing queries that check if a related object is null, e.g. `realm.All<Dog>().Where(d => d.Owner == null)`. ([#1601](https://github.com/realm/realm-dotnet/pull/1601))
- Addressed an issue that would cause the debugger to report an unobserved exception being thrown when "Just My Code" is disabled. ([#1603](https://github.com/realm/realm-dotnet/pull/1603))
- Calling `Realm.DeleteRealm` on a synchronized Realm will now properly delete the `realm.management` folder. ([#1621](https://github.com/realm/realm-dotnet/pull/1621))
- Fixed a crash when accessing primitive list properties on objects in realms opened with a dynamic schema (e.g. in migrations). ([#1629](https://github.com/realm/realm-dotnet/pull/1629))

## 2.0.0 (2017-10-17)

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

## 1.6.0 (2017-08-14)

### Enhancements
- Exposed `Realm.WriteCopy` API to copy a Realm file and optionally encrypt it with a different key. ([#1464](https://github.com/realm/realm-dotnet/pull/1464))
- The runtime representations of all Realm collections (`IQueryable<T>` and `IList<T>`) now implement the `IList` interface that is needed for data-binding to `ListView` in UWP applications. ([#1469](https://github.com/realm/realm-dotnet/pull/1469))
- Exposed `User.RetrieveInfoForUserAsync` API to allow admin users to lookup other users' identities in the Realm Object Server. This can be used, for example, to find a user by knowing their Facebook id. ([#1486](https://github.com/realm/realm-dotnet/pull/1486))
- Added a check to verify there are no duplicate object names when creating the schema. ([#1502](https://github.com/realm/realm-dotnet/pull/1502))
- Added more comprehensive error messages when passing an invalid url scheme to `SyncConfiguration` or `User.LoginAsync`. ([#1501](https://github.com/realm/realm-dotnet/pull/1501))
- Added more meaningful error information to exceptions thrown by `Realm.GetInstanceAsync`. ([#1503](https://github.com/realm/realm-dotnet/pull/1503))
- Added a new type - `RealmInteger<T>` to expose Realm-specific API over base integral types. It can be used to implement [counter functionality](https://docs.mongodb.com/realm-legacy/docs/dotnet/latest/index.html) in synced realms. ([#1466](https://github.com/realm/realm-dotnet/pull/1466))
- Added `PermissionCondition.Default` to apply default permissions for existing and new users. ([#1511](https://github.com/realm/realm-dotnet/pull/1511))

### Bug fixes
- Fix an exception being thrown when comparing non-constant character value in a query. ([#1471](https://github.com/realm/realm-dotnet/pull/1471))
- Fix an exception being thrown when comparing non-constant byte or short value in a query. ([#1472](https://github.com/realm/realm-dotnet/pull/1472))
- Fix a bug where calling the non-generic version of `IQueryProvider.CreateQuery` on Realm's IQueryable results, an exception would be thrown. ([#1487](https://github.com/realm/realm-dotnet/pull/1487))
- Trying to use an `IList` or `IQueryable` property in a LINQ query will now throw `NotSupportedException` rather than crash the app. ([#1505](https://github.com/realm/realm-dotnet/pull/1505))

### Breaking Changes

## 1.5.0 (2017-06-20)

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

## 1.4.0 (2017-05-19)

### Enhancements
- Expose `RealmObject.OnManaged` virtual method that can be used for init purposes, since the constructor is run before the object has knowledge of its Realm. (#1383)
- Expose `Realm.GetInstanceAsync` API to asynchronously open a synchronized Realm. It will download all remote content available at the time the operation began on a background thread and then return a usable Realm. It is also the only supported way of opening Realms for which the user has only read permissions.

## 1.3.0 (2017-05-16)

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

## 1.2.1 (2017-05-01)

### Bug fixes
- Fixed an issue where `EntryPointNotFoundException` would be thrown on some Android devices. (#1336)

### Enhancements
- Expose `IRealmCollection.IsValid` to indicate whether the realm collection is valid to use. (#1344)
- Update the Fody reference which adds support for building with Mono 5. (#1364)

## 1.2.0 (2017-04-04)

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

## 1.1.1 (2017-03-15)

### Bug fixes

- Resolved an issue that prevented compiling for iOS on Visual Studio. (#1277)

## 1.1.0 (2017-03-03)

### Enhancements
- Added Azure Active Directory (AzureAD) credentials provider. (#1254)

### Breaking Changes
This is a preparation release for adding UWP support. We have removed all platform-specific logic from the Realm assemblies, and instead weave them in compile time. While this has been tested in all common scenarios, it may create issues with very complex project graphs. If you encounter any of these issues with iOS projects:
- Compilation fails when running Task `WeaveRealmAssemblies`
- App crashes when first accessing a Realm

please file an issue and explain your solution setup.

## 1.0.4 (2017-02-21)

### Bug fixes

- The `Realm` NuGet package no longer clobbers the path to Win32 native binaries in `Realm.Database`. (#1239)
- Fixed a bug where garbage collecting an object with `PropertyChanged` subscribers would cause crashes. (#1237)

## 1.0.3 (2017-02-14)

### Out of Beta!
After about a year and a half of hard work, we are proud to call this a 1.0 release. There is still work to do, but Realm Xamarin is now being used by thousands of developers and has proven reliable.

### Sync
Realm Xamarin now works with the Realm Mobile Platform. This means that you can write Xamarin apps that synchronize seamlessly with a Realm Object Server, allowing you to write complex apps with Xamarin that are offline-first and automatically synchronised by adding just a few lines of code.
You can read about this in the [documentation](https://docs.mongodb.com/realm/sync/get-started/).

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


## 0.82.1 (2017-01-27)

### Bug fixes
- Addressed an issue where obtaining a Realm instance, reading an object, then obtaining another instance on the same thread would cause the object to become invalid and crash the application upon accessing any of its members.

## 0.82.0 (2017-01-23)

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

## 0.81.0 (2016-12-14)

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

## 0.80.0 (2016-10-27)

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

## 0.78.1 (2016-09-15)

### Bug fixes
* `Realm.ObjectForPrimaryKey()` now returns null if it failed to find an object (#833).
* Querying anything but persisted properties now throws instead of causing a crash (#251 and #723)

Uses core 1.5.1

## 0.78.0 (2016-09-09)

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


## 0.77.2 (2016-08-11)

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


## 0.77.1 (2016-07-25)

### Minor Changes
* Fixed a bug weaving pure PCL projects, released in v0.77.0 (#715)
* Exception messages caused by using incompatible arguments in LINQ now include the offending argument (#719)
* PCL projects using ToNotifyCollectionChanged may have crashed due to mismatch between PCL signatures and platform builds.

Uses core 1.4.0


## 0.77.0 (2016-07-18)

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


## 0.76.1 (2016-06-15)

### Minor Changes
* The `Realm` static constructor will no longer throw a `TypeLoadException` when there is an active `System.Reflection.Emit.AssemblyBuilder` in the current `AppDomain`.
* Fixed `Attempting to JIT compile` exception when using the Notifications API on iOS devices. (Issue #620)

### Breaking Changes
No API change but sort order changes slightly with accented characters grouped together and some special characters sorting differently. "One third" now sorts ahead of "one-third".

It uses the table at ftp://ftp.unicode.org/Public/UCA/latest/allkeys.txt

It groups all characters that look visually identical, that is, it puts a, à, å together and before ø, o, ö even. This is a flaw because, for example, å should come last in Denmark. But it's the best we can do now, until we get more locale aware.

Uses core 1.1.2

## 0.76.0 (2016-06-09)

### Major Changes
* `RealmObject` classes will now implicitly implement `INotifyPropertyChanged` if you specify the interface on your class. Thanks to [Joe Brock](https://github.com/jdbrock) for this contribution!

### Minor Changes
* `long` is supported in queries (Issue #607)
* Linker error looking for `System.String System.String::Format(System.IFormatProvider,System.String,System.Object)` fixed (Issue #591)
* Second-level descendants of `RealmObject` and static properties in `RealmObject` classes now cause the weaver to properly report errors as we don't (yet) support those. (Issue #603)
* Calling `.Equals()` on standalone objects no longer throws. (Issue #587)


## 0.75.0 (2016-06-02)

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


## 0.74.1 Released (2016-05-10)

### Minor Fixes
* Realms now refresh properly on Android when modified in other threads/processes.
* Fixes crashes under heavy combinations of threaded reads and writes.

### Minor Changes
* The two `Realm` and `RealmWeaver` NuGet packages have been combined into a single `Realm` package.
* The `String.Contains(String)`, `String.StartsWith(String)`, and `String.EndsWith(String)` methods now support variable expressions. Previously they only worked with literal strings.
* `RealmResults<T>` now implements `INotifyCollectionChanged` by raising the `CollectionChanged` event with `NotifyCollectionChangedAction.Reset` when its underlying table or query result is changed by a write transaction.

## 0.74.0 Private Beta (2016-04-02)

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


## 0.73.0 Private Beta (2016-02-26)

### Major Changes
* `RealmConfiguration.EncryptionKey` added so files can be encrypted and existing encrypted files from other Realm sources opened (assuming you have the key)


### Minor Fixes
* For PCL users, if you use `RealmConfiguration.DefaultConfiguration` without having linked a platform-specific dll, you will now get the warning message with a `PlatformNotSupportedException`. Previously threw a `TypeInitExepction`.
* Update to Core v0.96.2 and matching ObjectStore (issue #393)


## 0.72.1 Private Beta (2016-02-15)

No functional changes. Just added library builds for Android 64bit targets `x86_64` and `arm64-v8a`.


## 0.72.0 Private Beta (2016-02-13)
-
Uses Realm core 0.96.0

### Major Changes

* Added support for PCL so you can now use the NuGet in your PCL GUI or viewmodel libraries.

## 0.71.1 Private Beta (2016-01-29)

### Minor Fixes

Building IOS apps targeting the simulator sometimes got an error like:

    Error MT5209: Native linking error...building for iOS simulator,
    but linking in object file built for OSX, for architecture i386 (MT5209)

This was fixed by removing a redundant simulator library included in NuGet


## 0.71.0 Private Beta (2016-01-25)

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


## 0.70.0 First Private Beta (2015-12-08)

Requires installation from private copy of NuGet download.

### State

* Supported IOS with Xamarin Studio only.
* Basic model and read/write operations with simple LINQ `Where` searches.
* NuGet hosted as downloads from private realm/realm-dotnet repo.
