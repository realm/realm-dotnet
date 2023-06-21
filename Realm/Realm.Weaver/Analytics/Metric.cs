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
using System.Diagnostics.CodeAnalysis;

namespace RealmWeaver
{
    internal static class Metric
    {
        public static string Unknown(string? clarifier = null)
        {
            var result = "Unknown";
            if (!clarifier.IsNullOrEmpty())
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
            public const string CrossPlatform = "Cross Platform";
            public const string Android = "Android";
            public const string Ios = "iOS";
            public const string IpadOs = "iPadOS";
            public const string WatchOs = "watchOS";
            public const string TvOs = "tvOS";
            public const string XboxOne = "XboxOne";
        }

        public static class CpuArchitecture
        {
            public const string X86 = "x86";
            public const string X64 = "x64";
            public const string Arm = "Arm";
            public const string Arm64 = "Arm64";
            public const string Universal = "Universal";
        }

        public static class Framework
        {
            public const string Unity = "Unity";
            public const string UnityEditor = "Unity Editor";
            public const string Maui = "MAUI";
            public const string Xamarin = "Xamarin";
            public const string XamarinForms = "Xamarin Forms";
            public const string Uwp = "UWP";
            public const string MacCatalyst = "MacCatalyst";
        }

        public static class Environment
        {
            public const string LegacyUserId = "distinct_id";
            public const string UserId = "builder_id";
            public const string ProjectId = "Anonymized Bundle ID";
            public const string RealmSdk = "Binding";
            public const string RealmSdkVersion = "Realm Version";
            public const string Language = "Language";
            public const string LanguageVersion = "Language Version";
            public const string HostOsType = "Host OS Type";
            public const string HostOsVersion = "Host OS Version";
            public const string HostCpuArch = "Host CPU Arch";
            public const string TargetOsType = "Target OS Type";
            public const string TargetCpuArch = "Target CPU Arch";
            public const string TargetOsMinimumVersion = "Target OS Minimum Version";
            public const string TargetOsVersion = "Target OS Version";
            public const string CoreVersion = "Core Version";
            public const string IsSyncEnabled = "Sync Enabled";
            public const string FrameworkUsedInConjunction = "Framework"; // this refers to UI frameworks and similar Realm is used together with
            public const string FrameworkUsedInConjunctionVersion = "Framework Version";
            public const string SdkInstallationMethod = "Installation Method";
            public const string NetFramework = "Net Framework";
            public const string NetFrameworkVersion = "Net Framework Version";
            public const string Compiler = "Compiler";

            // These are not currently supported
            [SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Placeholder")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Placeholder")]
            private const string _IdeUsed = "IDE";

            [SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Placeholder")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Placeholder")]
            private const string _IdeUsedVersion = "IDE Version";
        }

        public static class Feature
        {
            // ReSharper disable InconsistentNaming
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
            public const string Function = nameof(Function);
            public const string CallAsync = nameof(CallAsync);
            public const string GetMongoClient = nameof(GetMongoClient);
            public const string DynamicApi = nameof(DynamicApi);
            public const string ConnectionNotification = nameof(ConnectionNotification);
            public const string ObjectNotification = nameof(ObjectNotification);
        }

        // This holds a mapping from Feature -> the name we send to DW.
        public static readonly Dictionary<string, string> SdkFeatures = new()
        {
            [Feature.IEmbeddedObject] = "Embedded_Object",
            [Feature.IAsymmetricObject] = "Asymmetric_Object",
            [Feature.ReferenceList] = "Object_List",
            [Feature.PrimitiveList] = "Primitive_List",
            [Feature.ReferenceDictionary] = "Object_Dict",
            [Feature.PrimitiveDictionary] = "Primitive_Dict",
            [Feature.ReferenceSet] = "Object_Set",
            [Feature.PrimitiveSet] = "Primitive_Set",
            [Feature.RealmInteger] = "Counter",
            [Feature.RealmObjectReference] = "Object_Link",
            [Feature.RealmValue] = "Mixed",
            [Feature.BacklinkAttribute] = "Backlink",

            [Feature.GetInstanceAsync] = "Async_Open",
            [Feature.GetInstance] = "Sync_Open",

            [Feature.Find] = "Find_PK",
            [Feature.WriteAsync] = "Write_Async",
            [Feature.ThreadSafeReference] = "Thread_Safe_Reference",
            [Feature.Add] = "Insert_Modified",
            [Feature.ShouldCompactOnLaunch] = "Compact_On_Launch",
            [Feature.MigrationCallback] = "Migration_Block",
            [Feature.RealmChanged] = "Realm_Notifications",
            [Feature.ListSubscribeForNotifications] = "List_Notifications",
            [Feature.SetSubscribeForNotifications] = "Set_Notifications",
            [Feature.DictionarySubscribeForNotifications] = "Dict_Notifications",
            [Feature.ResultSubscribeForNotifications] = "Results_Notifications",
            [Feature.ObjectNotification] = "Object_Notifications",
            [Feature.RecoverOrDiscardUnsyncedChangesHandler] = "CR_Recover_Discard",
            [Feature.RecoverUnsyncedChangesHandler] = "CR_Recover",
            [Feature.DiscardUnsyncedChangesHandler] = "CR_Discard",
            [Feature.ManualRecoveryHandler] = "CR_Manual",
            [Feature.GetProgressObservable] = "Progress_Notification",
            [Feature.PartitionSyncConfiguration] = "Pbs_Sync",
            [Feature.FlexibleSyncConfiguration] = "Flx_Sync",
            [Feature.Anonymous] = "Auth_Anon",
            [Feature.EmailPassword] = "Auth_Email",
            [Feature.Facebook] = "Auth_Facebook",
            [Feature.Google] = "Auth_Google",
            [Feature.Apple] = "Auth_Apple",
            [Feature.JWT] = "Auth_JWT",
            [Feature.ApiKey] = "Auth_API_Key",
            [Feature.Function] = "Auth_Function",
            [Feature.CallAsync] = "Remote_Function",
            [Feature.GetMongoClient] = "Remote_Mongo",
            [Feature.DynamicApi] = "Dynamic_API",
            [Feature.ConnectionNotification] = "Connection_Notification",

            // ["NOT_SUPPORTED_YET"] = "Query_Async",
        };
    }
}
