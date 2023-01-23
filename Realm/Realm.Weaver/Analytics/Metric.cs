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
            public const string Unknown = "Unknown";
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
            public const string UnityEditor = "Unity_Editor";
            public const string Maui = "MAUI";
            public const string Xamarin = "Xamarin";
        }

        public static readonly Dictionary<string, string> UserEnvironment = new Dictionary<string, string>()
        {
            ["UserId"] = "User_Id",
            ["ProjectId"] = "Project_Id",
            ["RealmSdk"] = "Realm_SDK",
            ["Language"] = "Language",
            ["LanguageVersion"] = "Language_Version",
            ["HostOsType"] = "Host_OS_Type",
            ["HostOsVersion"] = "Host_OS_Version",
            ["HostCpuArch"] = "Host_CPU_Arch",
            ["TargetOsType"] = "Target_OS_Type",
            ["TargetOsMinimumVersion"] = "Target_OS_Minimum_Version",
            ["TargetOsVersion"] = "Target_OS_Version",
            ["TargetCpuArch"] = "Target_CPU_Arch",
            ["RealmSdkVersion"] = "Realm_SDK_Version",
            ["CoreVersion"] = "Core_Version",
            ["FrameworkUsedInConjunction"] = "Framework", // this refers to UI frameworks and similar Realm is used together with
            ["FrameworkUsedInConjunctionVersion"] = "Framework_Version",
            ["SdkInstallationMethod"] = "Installation_Method",
            ["IdeUsed"] = "IDE",
            ["IdeUsedVersion"] = "IDE_Version", // this holds info about the msbuild version
            ["NetFramework"] = "Net_Framework",
            ["NetFrameworkVersion"] = "Net_Framework_Version"
        };

        public static readonly Dictionary<string, string> SdkFeatures = new Dictionary<string, string>()
        {
            ["IEmbeddedObject"] = "Embedded_Object",
            ["IAsymmetricObject"] = "Asymmetric_Object",
            ["ReferenceList"] = "Reference_List",
            ["PrimitiveList"] = "Primitive_List",
            ["ReferenceDictionary"] = "Reference_Dictionary",
            ["PrimitiveDictionary"] = "Primitive_Dictionary",
            ["ReferenceSet"] = "Reference_Set",
            ["PrimitiveSet"] = "Primitive_Set",
            ["RealmInteger"] = "Realm_Integer",
            ["RealmObjectReference"] = "Reference_Link",
            ["RealmValue"] = "Mixed",
            ["BacklinkAttribute"] = "Backlink",

            ["GetInstanceAsync"] = "Asynchronous_Realm_Open",
            ["GetInstance"] = "Synchronous_Realm_Open",

            // ["NOT_SUPPORTED_YET"] = "Query_Async",
            ["Find"] = "Query_Primary_Key",
            ["WriteAsync"] = "Write_Async",
            ["ThreadSafeReference"] = "Thread_Safe_Reference",
            ["Add"] = "Insert_Modified",
            ["ShouldCompactOnLaunch"] = "Compact_On_Launch",
            ["MigrationCallback"] = "Schema_Migration_Block",
            ["RealmChanged"] = "Realm_Change_Listener",
            ["ListSubscribeForNotifications"] = "List_Change_Listener",
            ["SetSubscribeForNotifications"] = "Set_Change_Listener",
            ["DictionarySubscribeForNotifications"] = "Dictionary_Change_Listener",
            ["ResultSubscribeForNotifications"] = "Result_Change_Listener",
            ["PropertyChanged"] = "Object_Change_Listener",
            ["RecoverOrDiscardUnsyncedChangesHandler"] = "Client_Reset_Recover_Or_Discard",
            ["RecoverUnsyncedChangesHandler"] = "Client_Reset_Recover",
            ["DiscardUnsyncedChangesHandler"] = "Client_Reset_Discard",
            ["ManualRecoveryHandler"] = "Client_Reset_Manual",
            ["GetProgressObservable"] = "Progress_Notification",
            ["PartitionSyncConfiguration"] = "Pbs_Sync",
            ["FlexibleSyncConfiguration"] = "Flexible_Sync",
            ["Anonymous"] = "Auth_Anonymous",
            ["EmailPassword"] = "Auth_Email_Password",
            ["Facebook"] = "Auth_Facebook",
            ["Google"] = "Auth_Google",
            ["Apple"] = "Auth_Apple",
            ["JWT"] = "Auth_Custom_JWT",
            ["ApiKey"] = "Auth_API_Key",
            ["ServerApiKey"] = "Auth_Server_API_Key",
            ["Function"] = "Auth_Function",
            ["CallAsync"] = "Remote_Function",
            ["GetMongoClient"] = "MongoDB_Data_Access",
            ["DynamicApi"] = "Dynamic_API",
        };
    }
}
