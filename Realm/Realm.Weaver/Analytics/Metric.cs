////////////////////////////////////////////////////////////////////////////
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
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Realm.Fody.Tests")]

namespace RealmWeaver
{
    internal static class Metric
    {
        public static string Unknown(string clarifier = null)
        {
            var result = "Unknown";
            if (!string.IsNullOrEmpty(clarifier))
            {
                result += $" ({clarifier})";
            }

            return result;
        }

        public static class OperatingSystem
        {
            public const string Linux = "Linux";
            public const string MacOS = "macOS";
            public const string Windows = "Windows";
            public const string Android = "Android";
            public const string Ios = "iOS";
            public const string IpadOs = "iPadOS";
            public const string WatchOs = "watchOS";
            public const string TvOs = "tvOS";
        }

        public static class CpuArchitecture
        {
            public const string X86 = "x86";
            public const string X64 = "x64";
            public const string Arm = "Arm";
            public const string Arm64 = "Arm64";
        }

        public static class Framework
        {
            public const string Unity = "Unity";
            public const string UnityEditor = "Unity Editor";
            public const string Maui = "MAUI";
            public const string Xamarin = "Xamarin";
            public const string Uwp = "UWP";
        }

        public static class Environment
        {
            public const string UserId = "distinct_id";
            public const string ProjectId = "Anonymized Bundle ID";
            public const string RealmSdk = "Binding";
            public const string RealmSdkVersion = "Realm Version";
            public const string Language = "Language";
            public const string LanguageVersion = "Language Version";
            public const string HostOsType = "Host OS Type";
            public const string HostOsVersion = "Host OS Version";
            public const string HostCpuArch = "Host CPU Arch";
            public const string TargetOsType = "Target OS Type";
            public const string TargetOsMinimumVersion = "Target OS Minimum Version";
            public const string TargetOsVersion = "Target OS Version";
            public const string TargetCpuArch = "Target CPU Arch";
            public const string CoreVersion = "Core Version";
            public const string IsSyncEnabled = "Sync Enabled";
            public const string FrameworkUsedInConjunction = "Framework"; // this refers to UI frameworks and similar Realm is used together with
            public const string FrameworkUsedInConjunctionVersion = "Framework Version";
            public const string SdkInstallationMethod = "Installation Method";
            public const string IdeUsed = "IDE";
            public const string IdeUsedVersion = "IDE Version"; // this holds info about the msbuild version
            public const string NetFramework = "Net Framework";
            public const string NetFrameworkVersion = "Net Framework Version";
        }

        public static class Feature
        {
            public const string IEmbeddedObject = nameof(IEmbeddedObject);
            public const string IAsymmetricObject = nameof(IAsymmetricObject);
            public const string ReferenceList = nameof(ReferenceList);
            public const string PrimitiveList = nameof(PrimitiveList);
            public const string ReferenceDictionary = nameof(ReferenceDictionary);
            public const string PrimitiveDictionary = nameof(PrimitiveDictionary);
            public const string ReferenceSet = nameof(ReferenceSet);
            public const string PrimitiveSet = nameof(PrimitiveSet);
            public const string RealmInteger = nameof(RealmInteger);
            public const string RealmObjectReference = nameof(RealmObjectReference);
            public const string RealmValue = nameof(RealmValue);
            public const string BacklinkAttribute = nameof(BacklinkAttribute);
            public const string GetInstanceAsync = nameof(GetInstanceAsync);
            public const string GetInstance = nameof(GetInstance);
            public const string Find = nameof(Find);
            public const string WriteAsync = nameof(WriteAsync);
            public const string ThreadSafeReference = nameof(ThreadSafeReference);
            public const string Add = nameof(Add);
            public const string ShouldCompactOnLaunch = nameof(ShouldCompactOnLaunch);
            public const string MigrationCallback = nameof(MigrationCallback);
            public const string RealmChanged = nameof(RealmChanged);
            public const string ListSubscribeForNotifications = nameof(ListSubscribeForNotifications);
            public const string SetSubscribeForNotifications = nameof(SetSubscribeForNotifications);
            public const string DictionarySubscribeForNotifications = nameof(DictionarySubscribeForNotifications);
            public const string ResultSubscribeForNotifications = nameof(ResultSubscribeForNotifications);
            public const string PropertyChanged = nameof(PropertyChanged);
            public const string RecoverOrDiscardUnsyncedChangesHandler = nameof(RecoverOrDiscardUnsyncedChangesHandler);
            public const string RecoverUnsyncedChangesHandler = nameof(RecoverUnsyncedChangesHandler);
            public const string DiscardUnsyncedChangesHandler = nameof(DiscardUnsyncedChangesHandler);
            public const string ManualRecoveryHandler = nameof(ManualRecoveryHandler);
            public const string GetProgressObservable = nameof(GetProgressObservable);
            public const string PartitionSyncConfiguration = nameof(PartitionSyncConfiguration);
            public const string FlexibleSyncConfiguration = nameof(FlexibleSyncConfiguration);
            public const string Anonymous = nameof(Anonymous);
            public const string EmailPassword = nameof(EmailPassword);
            public const string Facebook = nameof(Facebook);
            public const string Google = nameof(Google);
            public const string Apple = nameof(Apple);
            public const string JWT = nameof(JWT);
            public const string ApiKey = nameof(ApiKey);
            public const string ServerApiKey = nameof(ServerApiKey);
            public const string Function = nameof(Function);
            public const string CallAsync = nameof(CallAsync);
            public const string GetMongoClient = nameof(GetMongoClient);
            public const string DynamicApi = nameof(DynamicApi);
        }

        // This holds a mapping from Feature -> the name we send to DW.
        public static readonly Dictionary<string, string> SdkFeatures = new()
        {
            [Feature.IEmbeddedObject] = "Embedded_Object",
            [Feature.IAsymmetricObject] = "Asymmetric_Object",
            [Feature.ReferenceList] = "Reference_List",
            [Feature.PrimitiveList] = "Primitive_List",
            [Feature.ReferenceDictionary] = "Reference_Dictionary",
            [Feature.PrimitiveDictionary] = "Primitive_Dictionary",
            [Feature.ReferenceSet] = "Reference_Set",
            [Feature.PrimitiveSet] = "Primitive_Set",
            [Feature.RealmInteger] = "Realm_Integer",
            [Feature.RealmObjectReference] = "Reference_Link",
            [Feature.RealmValue] = "Mixed",
            [Feature.BacklinkAttribute] = "Backlink",

            [Feature.GetInstanceAsync] = "Asynchronous_Realm_Open",
            [Feature.GetInstance] = "Synchronous_Realm_Open",

            // ["NOT_SUPPORTED_YET"] = "Query_Async",
            [Feature.Find] = "Query_Primary_Key",
            [Feature.WriteAsync] = "Write_Async",
            [Feature.ThreadSafeReference] = "Thread_Safe_Reference",
            [Feature.Add] = "Insert_Modified",
            [Feature.ShouldCompactOnLaunch] = "Compact_On_Launch",
            [Feature.MigrationCallback] = "Schema_Migration_Block",
            [Feature.RealmChanged] = "Realm_Change_Listener",
            [Feature.ListSubscribeForNotifications] = "List_Change_Listener",
            [Feature.SetSubscribeForNotifications] = "Set_Change_Listener",
            [Feature.DictionarySubscribeForNotifications] = "Dictionary_Change_Listener",
            [Feature.ResultSubscribeForNotifications] = "Result_Change_Listener",
            [Feature.PropertyChanged] = "Object_Change_Listener",
            [Feature.RecoverOrDiscardUnsyncedChangesHandler] = "Client_Reset_Recover_Or_Discard",
            [Feature.RecoverUnsyncedChangesHandler] = "Client_Reset_Recover",
            [Feature.DiscardUnsyncedChangesHandler] = "Client_Reset_Discard",
            [Feature.ManualRecoveryHandler] = "Client_Reset_Manual",
            [Feature.GetProgressObservable] = "Progress_Notification",
            [Feature.PartitionSyncConfiguration] = "Pbs_Sync",
            [Feature.FlexibleSyncConfiguration] = "Flexible_Sync",
            [Feature.Anonymous] = "Auth_Anonymous",
            [Feature.EmailPassword] = "Auth_Email_Password",
            [Feature.Facebook] = "Auth_Facebook",
            [Feature.Google] = "Auth_Google",
            [Feature.Apple] = "Auth_Apple",
            [Feature.JWT] = "Auth_Custom_JWT",
            [Feature.ApiKey] = "Auth_API_Key",
            [Feature.ServerApiKey] = "Auth_Server_API_Key",
            [Feature.Function] = "Auth_Function",
            [Feature.CallAsync] = "Remote_Function",
            [Feature.GetMongoClient] = "MongoDB_Data_Access",
            [Feature.DynamicApi] = "Dynamic_API",
        };
    }
}
