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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Realms.SourceGenerator;

namespace Realms.SourceGenerator
{
    internal class Analytics
    {
        private const string JsonTemplate = @"{
   ""event"": ""Run"",
   ""properties"": {
      ""token"": ""ce0fac19508f6c8f20066d345d360fd0"",
      ""distinct_id"": ""%USER_ID%"",
      ""Anonymized MAC Address"": ""%USER_ID%"",
      ""Anonymized Bundle ID"": ""%APP_ID%"",
      ""Binding"": ""dotnet"",
      ""Language"": ""c#"",
      ""Framework"": ""%FRAMEWORK%"",
      ""Framework Version"": ""%FRAMEWORK_VERSION%"",
      ""Sync Enabled"": ""%SYNC_ENABLED%"",
      ""Realm Version"": ""%REALM_VERSION%"",
      ""Host OS Type"": ""%OS_TYPE%"",
      ""Host OS Version"": ""%OS_VERSION%"",
      ""Target OS Type"": ""%TARGET_OS%"",
      ""Target OS Version"": ""%TARGET_OS_VERSION%""
   }
}";

        private class FeatureMapping
        {
            // TODO andrea: What about backlinks?
            public const string IEmbeddedOjbect = "Embedded_Object";
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

            // API const strings precisely match the name of the API calls in the SDK
            public const string GetInstanceAsync = "Asynchronous_Realm_Open";
            public const string GetInstance = "Synchronous_Realm_Open";
            public const string FIXME = "Query_Async"; // this is not supported yet
            public const string Find = "Query_Primary_Key";
            public const string WriteAsync = "Write_Async";
            public const string ThreadSafeReference = "Thread_Safe_Reference";
            public const string FIXME_TWO = "Insert_Modified"; // TODO andrea: find out what this is, maybe modify a prop in realm.Write
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
            public const string CallAsync = "Remote_Function"; // TODO andrea: needs to be added
            public const string FIX_ME_THREE = "MongoDB_Data_Access"; // TODO andrea: needs to be added
            public const string DynamicApi = "Dynamic_API"; // TODO andrea: needs to be added
        }

        // TODO andrea: adding C#10's type aliases to give meaning to each string seems nice. Opinions?
        private Dictionary<string, byte> _realmFeaturesToAnalyse = new()
        {
            { FeatureMapping.IEmbeddedOjbect, 0 },
            { FeatureMapping.IAsymmetricObject, 0 },
            { FeatureMapping.ReferenceList, 0 },
            { FeatureMapping.PrimitiveList, 0 },
            { FeatureMapping.ReferenceDictionary, 0 },
            { FeatureMapping.PrimitiveDictionary, 0 },
            { FeatureMapping.ReferenceSet, 0 },
            { FeatureMapping.PrimitiveSet, 0 },
            { FeatureMapping.RealmInteger, 0 },
            { FeatureMapping.RealmObjectReference, 0 },
            { FeatureMapping.RealmValue, 0 },
            { FeatureMapping.GetInstanceAsync, 0 },
            { FeatureMapping.GetInstance, 0 },
            { FeatureMapping.FIXME, 0 },
            { FeatureMapping.Find, 0 },
            { FeatureMapping.WriteAsync, 0 },
            { FeatureMapping.ThreadSafeReference, 0 },
            { FeatureMapping.FIXME_TWO, 0 },
            { FeatureMapping.ShouldCompactOnLaunch, 0 },
            { FeatureMapping.MigrationCallback, 0 },
            { FeatureMapping.RealmChanged, 0 },
            { FeatureMapping.ListSubscribeForNotifications, 0 },
            { FeatureMapping.SetSubscribeForNotifications, 0 },
            { FeatureMapping.DictionarySubscribeForNotifications, 0 },
            { FeatureMapping.ResultSubscribeForNotifications, 0 },
            { FeatureMapping.PropertyChanged, 0 },
            { FeatureMapping.RecoverOrDiscardUnsyncedChangesHandler, 0 },
            { FeatureMapping.RecoverUnsyncedChangesHandler, 0 },
            { FeatureMapping.DiscardUnsyncedChangesHandler, 0 },
            { FeatureMapping.ManualRecoveryHandler, 0 },
            { FeatureMapping.GetProgressObservable, 0 },
            { FeatureMapping.PartitionSyncConfiguration, 0 },
            { FeatureMapping.FlexibleSyncConfiguration, 0 },
            { FeatureMapping.Anonymous, 0 },
            { FeatureMapping.EmailPassword, 0 },
            { FeatureMapping.Facebook, 0 },
            { FeatureMapping.Google, 0 },
            { FeatureMapping.Apple, 0 },
            { FeatureMapping.JWT, 0 },
            { FeatureMapping.ApiKey, 0 },
            { FeatureMapping.ServerApiKey, 0 },
            { FeatureMapping.Function, 0 },
            { FeatureMapping.CallAsync, 0 },
            { FeatureMapping.FIX_ME_THREE, 0 },
            { FeatureMapping.DynamicApi, 0 },
        };

        private static string AnonymizedUserID
        {
            get
            {
                try
                {
                    var id = GenerateComputerIdentifier();
                    return id != null ? SHA256Hash(id) : "UNKNOWN";
                }
                catch
                {
                    return "UNKNOWN";
                }
            }
        }

        private static byte[] GenerateComputerIdentifier()
        {
            // Assume OS X if not Windows.
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(n => n.Name == "en0" || (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                                   .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                                   .FirstOrDefault();
        }

        internal string SubmitAnalytics()
        {
            /*
            if (!_config.RunAnalytics ||
                Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") != null ||
                Environment.GetEnvironmentVariable("CI") != null)
            {
                return "Analytics disabled";
            }
            */

            // TODO andrea: properly load jsonFeatures in the payload
            var jsonFeatures = JsonConvert.SerializeObject(_realmFeaturesToAnalyse);

            var payload = GetJsonPayload();

#if !DEBUG
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            SendRequest(
                "https://data.mongodb-api.com/app/realmsdkmetrics-zmhtm/endpoint/metric_webhook/metric_stage?data=",
                base64Payload,
                string.Empty);
#endif

            return payload;
        }

        internal void AnalyzeRealmClass(ClassInfo classInfo)
        {
            if (classInfo.ObjectType == ObjectType.EmbeddedObject)
            {
                _realmFeaturesToAnalyse[FeatureMapping.IEmbeddedOjbect] = 1;
            }
            else if (classInfo.ObjectType == ObjectType.AsymmetricObject)
            {
                _realmFeaturesToAnalyse[FeatureMapping.IAsymmetricObject] = 1;
            }

            foreach (var property in classInfo.Properties)
            {
                if (property.TypeInfo.IsList)
                {
                    if (property.TypeInfo.InternalType.ScalarType == ScalarType.Object)
                    {
                        _realmFeaturesToAnalyse[FeatureMapping.ReferenceList] = 1;
                    }
                    else
                    {
                        _realmFeaturesToAnalyse[FeatureMapping.PrimitiveList] = 1;
                    }
                }
                else if (property.TypeInfo.IsDictionary)
                {
                    if (property.TypeInfo.InternalType.ScalarType == ScalarType.Object)
                    {
                        _realmFeaturesToAnalyse[FeatureMapping.ReferenceDictionary] = 1;
                    }
                    else
                    {
                        _realmFeaturesToAnalyse[FeatureMapping.PrimitiveDictionary] = 1;
                    }
                }
                else if (property.TypeInfo.IsSet)
                {
                    if (property.TypeInfo.InternalType.ScalarType == ScalarType.Object)
                    {
                        _realmFeaturesToAnalyse[FeatureMapping.ReferenceSet] = 1;
                    }
                    else
                    {
                        _realmFeaturesToAnalyse[FeatureMapping.PrimitiveSet] = 1;
                    }
                }
                else if (property.TypeInfo.IsRealmInteger)
                {
                    _realmFeaturesToAnalyse[FeatureMapping.RealmInteger] = 1;
                }
                else if (property.TypeInfo.TypeSymbol.IsAnyRealmObjectType())
                {
                    _realmFeaturesToAnalyse[FeatureMapping.RealmObjectReference] = 1;
                }
                else if (property.TypeInfo.TypeString == "RealmValue")
                {
                    _realmFeaturesToAnalyse[FeatureMapping.RealmValue] = 1;
                }
            }
        }

        internal void AnalyzeSyntaxNodeForApiUsage(GeneratorSyntaxContext context)
        {
            if (context.Node is not IdentifierNameSyntax identifierNameSyntax)
            {
                return;
            }

            switch (identifierNameSyntax.ToString())
            {
                // TODO andrea: better checks on type should be done to avoid collecting wrong information
                case nameof(FeatureMapping.GetInstance):
                    _realmFeaturesToAnalyse[FeatureMapping.GetInstance] = 1;
                    break;
                case nameof(FeatureMapping.GetInstanceAsync):
                    _realmFeaturesToAnalyse[FeatureMapping.GetInstanceAsync] = 1;
                    break;
                case nameof(FeatureMapping.FIXME):
                    _realmFeaturesToAnalyse[FeatureMapping.FIXME] = 1;
                    break;
                case nameof(FeatureMapping.Find):
                    _realmFeaturesToAnalyse[FeatureMapping.Find] = 1;
                    break;
                case nameof(FeatureMapping.WriteAsync):
                    _realmFeaturesToAnalyse[FeatureMapping.WriteAsync] = 1;
                    break;
                case nameof(FeatureMapping.ThreadSafeReference):
                    _realmFeaturesToAnalyse[FeatureMapping.ThreadSafeReference] = 1;
                    break;
                case nameof(FeatureMapping.FIXME_TWO):
                    _realmFeaturesToAnalyse[FeatureMapping.FIXME_TWO] = 1;
                    break;
                case nameof(FeatureMapping.ShouldCompactOnLaunch):
                    _realmFeaturesToAnalyse[FeatureMapping.ShouldCompactOnLaunch] = 1;
                    break;
                case nameof(FeatureMapping.MigrationCallback):
                    _realmFeaturesToAnalyse[FeatureMapping.MigrationCallback] = 1;
                    break;
                case nameof(FeatureMapping.RealmChanged):
                    _realmFeaturesToAnalyse[FeatureMapping.RealmChanged] = 1;
                    break;
                case "SubscribeForNotifications":
                {
                    ITypeSymbol referenceType = null;
                    try
                    {
                        // target.SubscribeForNotifications()
                        // ^^^^^^--------------------------------------------(              )
                        // of the SimpleMemberAccessExpression, get the first IdentifierName's type
                        referenceType = context.SemanticModel.GetTypeInfo(identifierNameSyntax.Parent?.ChildNodes().First()).Type;
                    }
                    catch
                    {
                        // most likely here "SubscribeForNotifications" is some user defined syntax token
                        break;
                    }

                    switch (referenceType.Name)
                    {
                        case "IQueryable":
                        case "IOrderedQueryable":
                            _realmFeaturesToAnalyse[FeatureMapping.ResultSubscribeForNotifications] = 1;
                            break;
                        case "IList":
                            _realmFeaturesToAnalyse[FeatureMapping.ListSubscribeForNotifications] = 1;
                            break;
                        case "ISet":
                            _realmFeaturesToAnalyse[FeatureMapping.SetSubscribeForNotifications] = 1;
                            break;
                        case "IDictionary":
                            _realmFeaturesToAnalyse[FeatureMapping.DictionarySubscribeForNotifications] = 1;
                            break;
                        default:
                            // TODO andrea: report a warning -> ($"{referenceType.Name} is not a collection type that's supported for notifications");
                            break;
                    }

                    break;
                }

                case nameof(FeatureMapping.PropertyChanged):
                    // TODO andrea: this is not enough, I need to check the type on which PropertyChanged is called
                    _realmFeaturesToAnalyse[FeatureMapping.PropertyChanged] = 1;
                    break;
                case nameof(FeatureMapping.RecoverOrDiscardUnsyncedChangesHandler):
                    _realmFeaturesToAnalyse[FeatureMapping.RecoverOrDiscardUnsyncedChangesHandler] = 1;
                    break;
                case nameof(FeatureMapping.RecoverUnsyncedChangesHandler):
                    _realmFeaturesToAnalyse[FeatureMapping.RecoverUnsyncedChangesHandler] = 1;
                    break;
                case nameof(FeatureMapping.DiscardUnsyncedChangesHandler):
                    _realmFeaturesToAnalyse[FeatureMapping.DiscardUnsyncedChangesHandler] = 1;
                    break;
                case nameof(FeatureMapping.ManualRecoveryHandler):
                    _realmFeaturesToAnalyse[FeatureMapping.ManualRecoveryHandler] = 1;
                    break;
                case nameof(FeatureMapping.GetProgressObservable):
                    _realmFeaturesToAnalyse[FeatureMapping.GetProgressObservable] = 1;
                    break;
                case nameof(FeatureMapping.PartitionSyncConfiguration):
                    _realmFeaturesToAnalyse[FeatureMapping.PartitionSyncConfiguration] = 1;
                    break;
                case nameof(FeatureMapping.FlexibleSyncConfiguration):
                    _realmFeaturesToAnalyse[FeatureMapping.FlexibleSyncConfiguration] = 1;
                    break;
                case nameof(FeatureMapping.Anonymous):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.Anonymous] = 1;
                    break;
                case nameof(FeatureMapping.EmailPassword):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.EmailPassword] = 1;
                    break;
                case nameof(FeatureMapping.Facebook):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.Facebook] = 1;
                    break;
                case nameof(FeatureMapping.Google):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.Google] = 1;
                    break;
                case nameof(FeatureMapping.Apple):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.Apple] = 1;
                    break;
                case nameof(FeatureMapping.JWT):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.JWT] = 1;
                    break;
                case nameof(FeatureMapping.ApiKey):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.ApiKey] = 1;
                    break;
                case nameof(FeatureMapping.ServerApiKey):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.ServerApiKey] = 1;
                    break;
                case nameof(FeatureMapping.Function):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[FeatureMapping.Function] = 1;
                    break;
                default:
                    // TODO andrea: report a warning -> ($"{entryToSearch} is not a feature that is understood");
                    break;
            }
        }

        private static bool IsCredentials(IdentifierNameSyntax identifierNameSyntax, GeneratorSyntaxContext context)
        {
            try
            {
                var parentType = context.SemanticModel.GetTypeInfo(identifierNameSyntax.Parent.ChildNodes().First()).Type.ToDisplayString();
                return parentType == "Realms.Sync.Credentials" || parentType == "Realms.Sync.Credentials.AuthProvider";
            }
            catch
            {
                // most likely some user defined syntax token
                return false;
            }
        }

        private static string GetJsonPayload()
        {
            ComputeHostOSNameAndVersion(out var osName, out var osVersion);
            return JsonTemplate;
                /*
                .Replace("%USER_ID%", AnonymizedUserID)
                .Replace("%APP_ID%", _config.ModuleName)

                .Replace("%SYNC_ENABLED%", _config.IsUsingSync.ToString())

                // Version of weaver is expected to match that of the library.
                .Replace("%REALM_VERSION%", Assembly.GetExecutingAssembly().GetName().Version.ToString())

                .Replace("%OS_TYPE%", osName)
                .Replace("%OS_VERSION%", osVersion)
                .Replace("%TARGET_OS%", _config.TargetOSName)
                .Replace("%TARGET_OS_VERSION%", _config.TargetOSVersion)
                .Replace("%FRAMEWORK%", _config.Framework)
                .Replace("%FRAMEWORK_VERSION%", _config.FrameworkVersion);
                */
        }

        private static void ComputeHostOSNameAndVersion(out string name, out string version)
        {
            var platformRegex = new Regex("^(?<platform>[^0-9]*) (?<version>[^ ]*)", RegexOptions.Compiled);
            var osDescription = platformRegex.Match(RuntimeInformation.OSDescription);
            if (osDescription.Success)
            {
                name = osDescription.Groups["platform"].Value;
                version = osDescription.Groups["version"].Value;
            }
            else
            {
                name = Environment.OSVersion.Platform.ToString();
                version = Environment.OSVersion.VersionString;
            }
        }

        private static void SendRequest(string prefixAddr, string payload, string suffixAddr)
        {
            var request = System.Net.HttpWebRequest.CreateHttp(new Uri(prefixAddr + payload + suffixAddr));
            request.Method = "GET";
            request.Timeout = 4000;
            request.ReadWriteTimeout = 2000;
            request.GetResponse();
        }

        private static string SHA256Hash(byte[] bytes)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(bytes));
            }
        }
    }
}
