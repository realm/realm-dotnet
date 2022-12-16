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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Realm.Fody.Tests")]

namespace RealmWeaver
{
    internal static class Metric
    {
        public static class OperatingSystem
        {
            public const string Linux = "Linux";
            public const string MacOS = "macOS";
            public const string Windows = "Windows";
            public const string Android = "Android";
            public const string Uwp = "UWP";
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
            public const string Maui = "MAUI";
            public const string Xamarin = "Xamarin";
        }

        public static class SdkFeature
        {
            public const string UserId = "User_Id";
            public const string ProjectId = "Project_Id";
            public const string RealmSdk = "Realm_SDK";
            public const string Language = "Language";
            public const string LanguageVersion = "Language_Version";
            public const string HostOsType = "Host_OS_Type";
            public const string HostOsVersion = "Host_OS_Version";
            public const string HostCpuArch = "Host_CPU_Arch";
            public const string TargetOsType = "Target_OS_Type";
            public const string TargetOsMinimumVersion = "Target_OS_Minimum_Version";
            public const string TargetOsVersion = "Target_OS_Version";
            public const string TargetCpuArch = "Target_CPU_Arch";
            public const string RealmSdkVersion = "Realm_SDK_Version";
            public const string CoreVersion = "Core_Version";
            public const string Framework = "Framework";
            public const string FrameworkVersion = "Framework_Version";

            // TODO andrea: Add entry for backlinks
            public const string IEmbeddedObject = "Embedded_Object";
            public const string IAsymmetricObject = "Asymmetric_Object";
            public const string ReferenceList = "Reference_List";
            public const string PrimitiveList = "Primitive_List";
            public const string ReferenceDictionary = "Reference_Dictionary";
            public const string PrimitiveDictionary = "Primitive_Dictionary";
            public const string ReferenceSet = "Reference_Set";
            public const string PrimitiveSet = "Primitive_Set";
            public const string RealmInteger = "Realm_Integer";
            public const string RealmObjectReference = "Reference_Link";
            public const string RealmValue = "Mixed";

            public const string GetInstanceAsync = "Asynchronous_Realm_Open";
            public const string GetInstance = "Synchronous_Realm_Open";
            public const string NOT_SUPPORTED_YET = "Query_Async"; // this is not supported yet
            public const string Find = "Query_Primary_Key";
            public const string WriteAsync = "Write_Async";
            public const string ThreadSafeReference = "Thread_Safe_Reference";
            public const string Add = "Insert_Modified"; // TODO andrea: realm.Add(new Obj, true);
            public const string ShouldCompactOnLaunch = "Compact_On_Launch";
            public const string MigrationCallback = "Schema_Migration_Block";
            public const string RealmChanged = "Realm_Change_Listener";
            public const string ListSubscribeForNotifications = "List_Change_Listener";
            public const string SetSubscribeForNotifications = "Set_Change_Listener";
            public const string DictionarySubscribeForNotifications = "Dictionary_Change_Listener";
            public const string ResultSubscribeForNotifications = "Result_Change_Listener";
            public const string PropertyChanged = "Object_Change_Listener";
            public const string RecoverOrDiscardUnsyncedChangesHandler = "Client_Reset_Recover_Or_Discard";
            public const string RecoverUnsyncedChangesHandler = "Client_Reset_Recover";
            public const string DiscardUnsyncedChangesHandler = "Client_Reset_Discard";
            public const string ManualRecoveryHandler = "Client_Reset_Manual";
            public const string GetProgressObservable = "Progress_Notification";
            public const string PartitionSyncConfiguration = "Pbs_Sync";
            public const string FlexibleSyncConfiguration = "Flexible_Sync";
            public const string Anonymous = "Auth_Anonymous";
            public const string EmailPassword = "Auth_Email_Password";
            public const string Facebook = "Auth_Facebook";
            public const string Google = "Auth_Google";
            public const string Apple = "Auth_Apple";
            public const string JWT = "Auth_Custom_JWT";
            public const string ApiKey = "Auth_API_Key";
            public const string ServerApiKey = "Auth_Server_API_Key";
            public const string Function = "Auth_Function";
            public const string CallAsync = "Remote_Function";
            public const string GetMongoClient = "MongoDB_Data_Access";
            public const string DynamicApi = "Dynamic_API";
        }
    }
}
