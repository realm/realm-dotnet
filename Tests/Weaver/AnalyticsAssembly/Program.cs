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

//#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
//#else
//using TestAsymmetricObject = Realms.IAsymmetricObject;
//using TestEmbeddedObject = Realms.IEmbeddedObject;
//using TestRealmObject = Realms.IRealmObject;
//#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using Realms.Sync.ErrorHandling;

public class Program
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    public static async Task Main(string[] args)
    {
        GetInstanceMethod();
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

#if FIND && GET_INSTANCE 
    public static void FindByPk()
    {
        var realm = Realm.GetInstance();
        _ = realm.Find<RootRealmClass>("aKey");
    }
#endif


#if WRITE_ASYNC && GET_INSTANCE
    public static async Task WriteAsyncMethod()
    {
        var realm = Realm.GetInstance();
        await realm.WriteAsync(() => { });
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

#if REALM_CHANGED && GET_INSTANCE
    public static void RealmChangedMethod()
    {
        var realm = Realm.GetInstance();
        realm.RealmChanged += (sender, args) => { };
    }
#endif

#if LIST_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void ListSubscribeForNotificationsMethod()
    {
        _ = new List<RootRealmClass>().SubscribeForNotifications((sender, changes, error) => { });
    }
#endif

#if SET_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void SetSubscribeForNotificationsMethod()
    {
        _ = new HashSet<RootRealmClass>().SubscribeForNotifications((sender, changes, error) => { });
    }
#endif

#if DICTIONARY_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void DictionarySubscribeForNotificationsMethod()
    {
        _ = new Dictionary<string, RootRealmClass>().SubscribeForNotifications((sender, changes, error) => { });
    }
#endif

#if RESULT_SUBSCRIBE_FOR_NOTIFICATIONS
    public static void ResultSubscribeForNotificationsMethod()
    {
        _ = Enumerable.Empty<RootRealmClass>().AsQueryable().SubscribeForNotifications((sender, changes, error) => { });
    }
#endif

#if PROPERTY_CHANGED
    public static void PropertyChangedMethod()
    {
        var obj = new RootRealmClass();
        obj.PropertyChanged += (sender, e) => { };
    }
#endif

#if RECOVER_OR_DISCARD_UNSYNCED_CHANGES_HANDLER && FLEXIBLE_SYNC_CONFIGURATION
    public static void RecoverOrDiscardUnsyncedChangesHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(new User())
        {
            ClientResetHandler = new RecoverOrDiscardUnsyncedChangesHandler()
        };
    }
#endif

#if RECOVER_UNSYNCED_CHANGES_HANDLER && FLEXIBLE_SYNC_CONFIGURATION
    public static void RecoverUnsyncedChangesHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(new User())
        {
            ClientResetHandler = new RecoverUnsyncedChangesHandler()
        };
    }
#endif

#if DISCARD_UNSYNCED_CHANGES_HANDLER && FLEXIBLE_SYNC_CONFIGURATION
    public static void DiscardUnsyncedChangesHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(new User())
        {
            ClientResetHandler = new DiscardUnsyncedChangesHandler()
        };
    }
#endif

#if RECOVER_OR_DISCARD_UNSYNCED_CHANGES_HANDLER && FLEXIBLE_SYNC_CONFIGURATION
    public static void ManualRecoveryHandlerMethod()
    {
        _ = new FlexibleSyncConfiguration(new User())
        {
            ClientResetHandler = new ManualRecoveryHandler()
        };
    }
#endif

#if GET_PROGRESS_OBSERVABLE
    public static void GetProgressObservableMethod()
    {
        _ = new Session().GetProgressObservable(ProgressDirection.Upload, ProgressMode.ReportIndefinitely);
    }
#endif

#if PARTITION_SYNC_CONFIGURATION
    public static void PartitionSyncConfigurationMethod()
    {
        _ = new PartitionSyncConfiguration("aPartition", new User());
    }
#endif

#if FLEXIBLE_SYNC_CONFIGURATION
    public static void FlexibleSyncConfigurationMethod()
    {
        _ = new FlexibleSyncConfiguration(new User());
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

#if JWT
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

#if SERVER_API_KEY
    public static void ServerApiKeyAuthenticationMethod()
    {
        _ = Credentials.ServerApiKey("serverApiKey");
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
        _ = new User().Functions.CallAsync("functionName");
    }
#endif

#if GET_MONGO_CLIENT
    public static void GetMongoClientMethod()
    {
        _ = new User().GetMongoClient("serviceName");
    }
#endif

    // TODO andrea: missing only DynamicApi

#if EMBEDDED_OBJECT
    public partial class EmbeddedTestClass : TestEmbeddedObject
    {
        public int Int32Property { get; set; }
    }
#endif

    // IRealmObject in here is just temporary to make Find work
    public class RootRealmClass : TestRealmObject, IRealmObject
    {
#if EMBEDDED_OBJECT || REALM_OBJECT_REFERENCE
        EmbeddedTestClass Embedded { get; set; }
#endif

#if REFERENCE_LIST
        IList<object> ReferenceList { get; }
#endif

#if PRIMITIVE_LIST
        IList<int> PrimitiveList { get; }
#endif

#if REFERENCE_DICTIONARY
        IDictionary<int, object> ReferenceDictionary { get; }
#endif

#if PRIMITIVE_DICTIONARY
        IDictionary<int, int> PrimitiveDictionary { get; }
#endif

#if REFERENCE_SET
        ISet<object> ReferenceSet { get; }
#endif

#if PRIMITIVE_SET
        ISet<int> PrimitiveSet { get; }
#endif

#if REALM_INTEGER
        RealmInteger<int> Counter { get; set; }
#endif

#if REALM_VALUE
        RealmValue RealmValue { get; set; }
#endif
    }

#if ASYMMETRIC_OBJECT
    public partial class AsymmetricTestClass : TestAsymmetricObject
    {
        public int Int32Property { get; set; }
    }
#endif

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
