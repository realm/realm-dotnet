﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif

public class Program
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    public static void Main(string[] args)
    {

    }

#if GET_INSTANCE_ASYNC
    public static async Task GetInstanceAsyncMethod()
    {
        _ = await Realm.GetInstanceAsync();
    }
#endif

#if GET_INSTANCE

    public static void GetInstanceMethod()
    {
        _ = Realm.GetInstance();
    }
#endif

#if FIND
    public static void FindByPk()
    {
        _ = Realm.GetInstance().Find<RootRealmClass>("aKey");
    }
#endif

#if WRITE_ASYNC
    public static async Task WriteAsyncMethod()
    {
        _ = Realm.GetInstance().WriteAsync(() => { });
    }
#endif

#if THREAD_SAFE_REFERENCE
    public static void ThreadSafeReferenceMethod()
    {
        _ = ThreadSafeReference.Create(new RootRealmClass());
    }
#endif

#if SHOULD_COMPACT_ON_LAUNCH
    public static void ShouldCompactOnLaunchMethod()
    {
        _ = new RealmConfiguration
        {
            ShouldCompactOnLaunch = (totalBytes, bytesUsed) => true
        };
    }
#endif

#if MIGRATION_CALLBACK
    public static void MigrationCbMethod()
    {
        _ = new RealmConfiguration
        {
            MigrationCallback = (migration, oldSchemaVersion) => { }
        };
    }
#endif

#if REALM_CHANGED
    public static void RealmChangedMethod()
    {
        Realm.GetInstance().RealmChanged += (sender, args) => { };
    }
#endif

#if LIST_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void ListSubscribeForNotificationsMethod()
    {
        _ = new List<RootRealmClass>().SubscribeForNotifications((sender, changes) => { });
    }
#endif

#if SET_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void SetSubscribeForNotificationsMethod()
    {
        _ = new HashSet<RootRealmClass>().SubscribeForNotifications((sender, changes) => { });
    }
#endif

#if DICTIONARY_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void DictionarySubscribeForNotificationsMethod()
    {
        _ = new Dictionary<string, RootRealmClass>().SubscribeForNotifications((sender, changes) => { });
    }
#endif

#if RESULT_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void ResultSubscribeForNotificationsMethod()
    {
        _ = Enumerable.Empty<RootRealmClass>().AsQueryable().SubscribeForNotifications((sender, changes) => { });
    }
#endif

#if OBJECT_NOTIFICATION
    public static void PropertyChangedMethod()
    {
        new RootRealmClass().PropertyChanged += (sender, e) => { };
    }
#endif

#if ADD
    public static void AddUpdateMethod()
    {
        Realm.GetInstance().Add(new RootRealmClass(), update: true);
    }
#endif

#if RECOVER_OR_DISCARD_UNSYNCED_CHANGES_HANDLER
    public static void RecoverOrDiscardUnsyncedChangesHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(user: null)
        {
            ClientResetHandler = new RecoverOrDiscardUnsyncedChangesHandler()
        };
    }
#endif

#if RECOVER_UNSYNCED_CHANGES_HANDLER
    public static void RecoverUnsyncedChangesHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(user: null)
        {
            ClientResetHandler = new RecoverUnsyncedChangesHandler()
        };
    }
#endif

#if DISCARD_UNSYNCED_CHANGES_HANDLER
    public static void DiscardUnsyncedChangesHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(user: null)
        {
            ClientResetHandler = new DiscardUnsyncedChangesHandler()
        };
    }
#endif

#if MANUAL_RECOVERY_HANDLER
    public static void ManualRecoveryHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(user: null)
        {
            ClientResetHandler = new ManualRecoveryHandler((clientResetException) => { })
        };
    }
#endif

#if GET_PROGRESS_OBSERVABLE
    public static void GetProgressObservableMethod()
    {
        _ = ((Session)new object()).GetProgressObservable(ProgressDirection.Upload, ProgressMode.ReportIndefinitely);
    }
#endif

#if PARTITION_SYNC_CONFIGURATION
    public static void PartitionSyncConfigurationMethod()
    {
        _ = new PartitionSyncConfiguration("aPartition", user: null);
    }
#endif

#if FLEXIBLE_SYNC_CONFIGURATION
    public static void FlexibleSyncConfigurationMethod()
    {
        _ = new FlexibleSyncConfiguration(user: null);
    }
#endif

#if ANONYMOUS
    public static void AnonymousAuthenticationMethod()
    {
        _ = Credentials.Anonymous();
    }
#endif

#if EMAIL_PASSWORD
    public static void EmailPasswordAuthenticationMethod()
    {
        _ = Credentials.EmailPassword("email", "password");
    }
#endif

#if FACEBOOK
    public static void FacebookAuthenticationMethod()
    {
        _ = Credentials.Facebook("accessToken");
    }
#endif

#if GOOGLE
    public static void GoogleAuthenticationMethod()
    {
        _ = Credentials.Google("credential", GoogleCredentialType.IdToken);
    }
#endif

#if APPLE
    public static void AppleAuthenticationMethod()
    {
        _ = Credentials.Apple("accessToken");
    }
#endif

#if J_W_T
    public static void JwtAuthenticationMethod()
    {
        _ = Credentials.JWT("customToken");
    }
#endif

#if API_KEY
    public static void ApiKeyAuthenticationMethod()
    {
        _ = Credentials.ApiKey("key");
    }
#endif

#if FUNCTION
    public static void FunctionAuthenticationMethod()
    {
        _ = Credentials.Function(new object());
    }
#endif

#if CALL_ASYNC
    public static void CallAsyncMethod()
    {
        _ = ((User)new object()).Functions.CallAsync("functionName");
    }
#endif

#if GET_MONGO_CLIENT
    public static void GetMongoClientMethod()
    {
        _ = ((User)new object()).GetMongoClient("serviceName");
    }
#endif

#if DYNAMIC_API
    public static void DynamicApiMethod()
    {
        _ = Realm.GetInstance().DynamicApi;
    }
#endif

#if CONNECTION_NOTIFICATION
    public static void ConnectionNotificationMethod()
    {
        Realm.GetInstance().SyncSession.PropertyChanged += (sender, args) => { };
    }
#endif

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}

#if I_EMBEDDED_OBJECT
public partial class EmbeddedTestClass : TestEmbeddedObject
{
    public int Int32Property { get; set; }
}
#endif

public partial class JustForObjectReference : TestRealmObject
{
    public RootRealmClass UseAsBacklink { get; set; }
}

public partial class RootRealmClass : TestRealmObject
{

#if REALM_OBJECT_REFERENCE
    private JustForObjectReference JustForRef { get; set; }
#endif

#if REFERENCE_LIST
    private IList<JustForObjectReference> ReferenceList { get; }
#endif

#if PRIMITIVE_LIST
    private IList<int> PrimitiveList { get; }
#endif

#if REFERENCE_DICTIONARY
    private IDictionary<string, JustForObjectReference> ReferenceDictionary { get; }
#endif

#if PRIMITIVE_DICTIONARY
    private IDictionary<string, int> PrimitiveDictionary { get; }
#endif

#if REFERENCE_SET
    private ISet<JustForObjectReference> ReferenceSet { get; }
#endif

#if PRIMITIVE_SET
    private ISet<int> PrimitiveSet { get; }
#endif

#if REALM_INTEGER
    private RealmInteger<int> Counter { get; set; }
#endif

#if REALM_VALUE
    private RealmValue RealmValue { get; set; }
#endif

#if BACKLINK_ATTRIBUTE
    [Backlink(nameof(JustForObjectReference.UseAsBacklink))]
    private IQueryable<JustForObjectReference> JustBackLink { get; }
#endif
}

#if I_ASYMMETRIC_OBJECT
public partial class AsymmetricTestClass : TestAsymmetricObject
{
    public int Int32Property { get; set; }
}
#endif
