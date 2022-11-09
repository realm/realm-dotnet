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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Realms.SourceGenerator.Metric;

namespace Realms.SourceGenerator
{
    // TODO andrea: add Unity support, for the example how one can
    // enable or disable analytics from the menu of Unity

    internal class Analytics
    {
        // TODO andrea: adding C#10's type aliases to give meaning to each string seems nice. Opinions?
        private Dictionary<string, byte> _realmFeaturesToAnalyse = new()
        {
            { SdkFeature.IEmbeddedOjbect, 0 },
            { SdkFeature.IAsymmetricObject, 0 },
            { SdkFeature.ReferenceList, 0 },
            { SdkFeature.PrimitiveList, 0 },
            { SdkFeature.ReferenceDictionary, 0 },
            { SdkFeature.PrimitiveDictionary, 0 },
            { SdkFeature.ReferenceSet, 0 },
            { SdkFeature.PrimitiveSet, 0 },
            { SdkFeature.RealmInteger, 0 },
            { SdkFeature.RealmObjectReference, 0 },
            { SdkFeature.RealmValue, 0 },
            { SdkFeature.GetInstanceAsync, 0 },
            { SdkFeature.GetInstance, 0 },
            { SdkFeature.NOT_SUPPORTED_YET, 0 },
            { SdkFeature.Find, 0 },
            { SdkFeature.WriteAsync, 0 },
            { SdkFeature.ThreadSafeReference, 0 },
            { SdkFeature.FIXME_TWO, 0 },
            { SdkFeature.ShouldCompactOnLaunch, 0 },
            { SdkFeature.MigrationCallback, 0 },
            { SdkFeature.RealmChanged, 0 },
            { SdkFeature.ListSubscribeForNotifications, 0 },
            { SdkFeature.SetSubscribeForNotifications, 0 },
            { SdkFeature.DictionarySubscribeForNotifications, 0 },
            { SdkFeature.ResultSubscribeForNotifications, 0 },
            { SdkFeature.PropertyChanged, 0 },
            { SdkFeature.RecoverOrDiscardUnsyncedChangesHandler, 0 },
            { SdkFeature.RecoverUnsyncedChangesHandler, 0 },
            { SdkFeature.DiscardUnsyncedChangesHandler, 0 },
            { SdkFeature.ManualRecoveryHandler, 0 },
            { SdkFeature.GetProgressObservable, 0 },
            { SdkFeature.PartitionSyncConfiguration, 0 },
            { SdkFeature.FlexibleSyncConfiguration, 0 },
            { SdkFeature.Anonymous, 0 },
            { SdkFeature.EmailPassword, 0 },
            { SdkFeature.Facebook, 0 },
            { SdkFeature.Google, 0 },
            { SdkFeature.Apple, 0 },
            { SdkFeature.JWT, 0 },
            { SdkFeature.ApiKey, 0 },
            { SdkFeature.ServerApiKey, 0 },
            { SdkFeature.Function, 0 },
            { SdkFeature.CallAsync, 0 },
            { SdkFeature.GetMongoClient, 0 },
            { SdkFeature.DynamicApi, 0 },
        };

        private Dictionary<string, string> _realmEnvMetrics = new()
        {
            { SdkFeature.UserId, string.Empty },
            { SdkFeature.RealmSdk, ".NET" },
            { SdkFeature.Language, string.Empty },
            { SdkFeature.LanguageVersion, string.Empty },
            { SdkFeature.HostOsType, string.Empty },
            { SdkFeature.HostOsVersion, string.Empty },
            { SdkFeature.HostCpuArch, string.Empty },
            { SdkFeature.TargetOsType, string.Empty },
            { SdkFeature.TargetOsMinimumVersion, string.Empty },
            { SdkFeature.TargetOsVersion, string.Empty },
            { SdkFeature.TargetCpuArch, string.Empty },
            { SdkFeature.RealmSdkVersion, string.Empty },
            { SdkFeature.CoreVersion, string.Empty },
            { SdkFeature.Framework, string.Empty },
            { SdkFeature.FrameworkVersion, string.Empty },
            { SdkFeature.TargetRuntime, string.Empty },
            { SdkFeature.TargetRuntimeVersion, string.Empty },
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

        public void SubmitAnalytics()
        {
            bool prettyJson = false;
#if DEBUG
            prettyJson = true;
#endif
            var payload = GetJsonPayload(prettyJson);

#if !DEBUG
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            // TODO andrea: this has never been tested yet
            SendRequest(
                "https://data.mongodb-api.com/app/realmsdkmetrics-zmhtm/endpoint/metric_webhook/metric_stage?data=",
                base64Payload,
                string.Empty);
#endif
            DebugLog(string.Format("{0}{1}", Environment.NewLine, payload));

            if (Environment.GetEnvironmentVariable("REALM_PRINT_ANALYTICS") != null)
            {
                InfoLog($@"
----------------------------------
Analytics payload
{payload}
----------------------------------");
            }
        }

        public void AnalyzeRealmClass(ClassInfo classInfo)
        {
            if (classInfo.ObjectType == ObjectType.EmbeddedObject)
            {
                _realmFeaturesToAnalyse[SdkFeature.IEmbeddedOjbect] = 1;
            }
            else if (classInfo.ObjectType == ObjectType.AsymmetricObject)
            {
                _realmFeaturesToAnalyse[SdkFeature.IAsymmetricObject] = 1;
            }

            foreach (var property in classInfo.Properties)
            {
                if (property.TypeInfo.IsList)
                {
                    if (property.TypeInfo.InternalType.ScalarType == ScalarType.Object)
                    {
                        _realmFeaturesToAnalyse[SdkFeature.ReferenceList] = 1;
                    }
                    else
                    {
                        _realmFeaturesToAnalyse[SdkFeature.PrimitiveList] = 1;
                    }
                }
                else if (property.TypeInfo.IsDictionary)
                {
                    if (property.TypeInfo.InternalType.ScalarType == ScalarType.Object)
                    {
                        _realmFeaturesToAnalyse[SdkFeature.ReferenceDictionary] = 1;
                    }
                    else
                    {
                        _realmFeaturesToAnalyse[SdkFeature.PrimitiveDictionary] = 1;
                    }
                }
                else if (property.TypeInfo.IsSet)
                {
                    if (property.TypeInfo.InternalType.ScalarType == ScalarType.Object)
                    {
                        _realmFeaturesToAnalyse[SdkFeature.ReferenceSet] = 1;
                    }
                    else
                    {
                        _realmFeaturesToAnalyse[SdkFeature.PrimitiveSet] = 1;
                    }
                }
                else if (property.TypeInfo.IsRealmInteger)
                {
                    _realmFeaturesToAnalyse[SdkFeature.RealmInteger] = 1;
                }
                else if (property.TypeInfo.TypeSymbol.IsAnyRealmObjectType())
                {
                    _realmFeaturesToAnalyse[SdkFeature.RealmObjectReference] = 1;
                }
                else if (property.TypeInfo.TypeString == "RealmValue")
                {
                    _realmFeaturesToAnalyse[SdkFeature.RealmValue] = 1;
                }
            }
        }

        public void AnalyzeEnvironment(GeneratorExecutionContext context)
        {
            _realmEnvMetrics[SdkFeature.UserId] = AnonymizedUserID;

            ComputeHostOSNameAndVersion(out var osName, out var osVersion);

            switch (osName)
            {
                case string name when name.Contains(Metric.OperatingSystem.Windows):
                    _realmEnvMetrics[SdkFeature.HostOsType] = Metric.OperatingSystem.Windows;
                    break;
                case string name when name.Contains(Metric.OperatingSystem.MacOS):
                    _realmEnvMetrics[SdkFeature.HostOsType] = Metric.OperatingSystem.MacOS;
                    break;
                case string name when name.Contains(Metric.OperatingSystem.Linux):
                    _realmEnvMetrics[SdkFeature.HostOsType] = Metric.OperatingSystem.Linux;
                    break;
                default:
                    DebugLog($"{osName} is not an operating system that we recognize.");
                    break;
            }

            _realmEnvMetrics[SdkFeature.HostOsVersion] = osVersion;

            var hostArch = RuntimeInformation.ProcessArchitecture.ToString();
            switch (hostArch)
            {
                case string arch when arch.Contains(nameof(CpuArchitecture.X64)):
                    _realmEnvMetrics[SdkFeature.HostCpuArch] = CpuArchitecture.X64;
                    break;
                case string arch when arch.Contains(nameof(CpuArchitecture.X86)):
                    _realmEnvMetrics[SdkFeature.HostCpuArch] = CpuArchitecture.X86;
                    break;
                case string arch when arch.Contains(nameof(CpuArchitecture.Arm)):
                    _realmEnvMetrics[SdkFeature.HostCpuArch] = CpuArchitecture.Arm;
                    break;
                default:
                    DebugLog($"{hostArch} is not an architecture that we recognize.");
                    break;
            }

            _realmEnvMetrics[SdkFeature.TargetCpuArch] = context.Compilation.Options.Platform.ToString();

            var compilation = (Microsoft.CodeAnalysis.CSharp.CSharpCompilation)context.Compilation;
            _realmEnvMetrics[SdkFeature.Language] = compilation.Language;
            _realmEnvMetrics[SdkFeature.LanguageVersion] = compilation.LanguageVersion.ToString();

            var targetFramework = compilation.Assembly.GetAttributes()
                .Where(x => x.ToString().Contains("System.Runtime.Versioning.TargetFrameworkAttribute"))
                .Single();
            var regexTargetFramework = new Regex(
                "System.Runtime.Versioning.TargetFrameworkAttribute\\(\"(?<Target>\\.?\\w+),Version=(?<Version>v?\\d+\\.?\\d+)",
                RegexOptions.Compiled);
            var targetMatch = regexTargetFramework.Match(targetFramework.ToString());
            _realmEnvMetrics[SdkFeature.TargetRuntime] = targetMatch.Groups["Target"].Value;
            _realmEnvMetrics[SdkFeature.TargetRuntimeVersion] = targetMatch.Groups["Version"].Value;

            foreach (var lib in compilation.ReferencedAssemblyNames)
            {
                if (_realmEnvMetrics[SdkFeature.RealmSdkVersion].Length != 0 &&
                    _realmEnvMetrics[SdkFeature.Framework].Length != 0)
                {
                    break;
                }

                // TODO andrea: beside the Realm case, all the others need to be tested
                switch (lib.Name)
                {
                    case "Realm":
                        _realmEnvMetrics[SdkFeature.RealmSdkVersion] = lib.Version.ToString();
                        break;
                    case string l when l.Contains(nameof(Framework.Xamarin)):
                        _realmEnvMetrics[SdkFeature.Framework] = Framework.Xamarin;
                        _realmEnvMetrics[SdkFeature.FrameworkVersion] = lib.Version.ToString();
                        break;
                    case string l when l.Contains(nameof(Framework.Maui)):
                        _realmEnvMetrics[SdkFeature.Framework] = Framework.Maui;
                        _realmEnvMetrics[SdkFeature.FrameworkVersion] = lib.Version.ToString();
                        break;
                    case string l when l.Contains(nameof(Framework.Unity)):
                        _realmEnvMetrics[SdkFeature.Framework] = Framework.Unity;
                        _realmEnvMetrics[SdkFeature.FrameworkVersion] = lib.Version.ToString();
                        break;
                    default:
                        break;
                }

                //TODO andrea: also search for other libs used in conjunction with Realm
            }
        }

        public void AnalyzeSyntaxNodeForApiUsage(GeneratorSyntaxContext context)
        {
            if (context.Node is not IdentifierNameSyntax identifierNameSyntax)
            {
                return;
            }

            switch (identifierNameSyntax.ToString())
            {
                // TODO andrea: better checks on type should be done to avoid collecting wrong information
                case nameof(SdkFeature.GetInstance):
                    _realmFeaturesToAnalyse[SdkFeature.GetInstance] = 1;
                    break;
                case nameof(SdkFeature.GetInstanceAsync):
                    _realmFeaturesToAnalyse[SdkFeature.GetInstanceAsync] = 1;
                    break;
                case nameof(SdkFeature.NOT_SUPPORTED_YET):
                    _realmFeaturesToAnalyse[SdkFeature.NOT_SUPPORTED_YET] = 1;
                    break;
                case nameof(SdkFeature.Find):
                    if (IsOfType(context, identifierNameSyntax, "Realms.Realm"))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.Find] = 1;
                    }
                    else
                    {
                        DebugLog($"{identifierNameSyntax} is likely some user define Find method.");
                    }

                    break;
                case nameof(SdkFeature.WriteAsync):
                    _realmFeaturesToAnalyse[SdkFeature.WriteAsync] = 1;
                    break;
                case nameof(SdkFeature.ThreadSafeReference):
                    _realmFeaturesToAnalyse[SdkFeature.ThreadSafeReference] = 1;
                    break;
                case nameof(SdkFeature.FIXME_TWO):
                    _realmFeaturesToAnalyse[SdkFeature.FIXME_TWO] = 1;
                    break;
                case nameof(SdkFeature.ShouldCompactOnLaunch):
                    _realmFeaturesToAnalyse[SdkFeature.ShouldCompactOnLaunch] = 1;
                    break;
                case nameof(SdkFeature.MigrationCallback):
                    _realmFeaturesToAnalyse[SdkFeature.MigrationCallback] = 1;
                    break;
                case nameof(SdkFeature.RealmChanged):
                    _realmFeaturesToAnalyse[SdkFeature.RealmChanged] = 1;
                    break;
                case "SubscribeForNotifications":
                    var parentType = GetSyntaxNodeParentType(context, identifierNameSyntax)?.Name;
                    switch (parentType)
                    {
                        case "IQueryable":
                        case "IOrderedQueryable":
                            _realmFeaturesToAnalyse[SdkFeature.ResultSubscribeForNotifications] = 1;
                            break;
                        case "IList":
                            _realmFeaturesToAnalyse[SdkFeature.ListSubscribeForNotifications] = 1;
                            break;
                        case "ISet":
                            _realmFeaturesToAnalyse[SdkFeature.SetSubscribeForNotifications] = 1;
                            break;
                        case "IDictionary":
                            _realmFeaturesToAnalyse[SdkFeature.DictionarySubscribeForNotifications] = 1;
                            break;
                        default:
                            DebugLog($"{parentType} is not a collection type that is supported for notifications.");
                            break;
                    }

                    break;
                case nameof(SdkFeature.PropertyChanged):
                    // TODO andrea: I'm not sure about this one. What happens when the PropertyChanged is
                    // only bind through Xamarin and not by hands?
                    if (GetSyntaxNodeParentType(context, identifierNameSyntax).ToDisplayString().Contains("Realms."))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.PropertyChanged] = 1;
                    }
                    else
                    {
                        DebugLog($"{identifierNameSyntax} is likely some PropertyChanged on a non Realm class.");
                    }

                    break;
                case nameof(SdkFeature.RecoverOrDiscardUnsyncedChangesHandler):
                    _realmFeaturesToAnalyse[SdkFeature.RecoverOrDiscardUnsyncedChangesHandler] = 1;
                    break;
                case nameof(SdkFeature.RecoverUnsyncedChangesHandler):
                    _realmFeaturesToAnalyse[SdkFeature.RecoverUnsyncedChangesHandler] = 1;
                    break;
                case nameof(SdkFeature.DiscardUnsyncedChangesHandler):
                    _realmFeaturesToAnalyse[SdkFeature.DiscardUnsyncedChangesHandler] = 1;
                    break;
                case nameof(SdkFeature.ManualRecoveryHandler):
                    _realmFeaturesToAnalyse[SdkFeature.ManualRecoveryHandler] = 1;
                    break;
                case nameof(SdkFeature.GetProgressObservable):
                    _realmFeaturesToAnalyse[SdkFeature.GetProgressObservable] = 1;
                    break;
                case nameof(SdkFeature.PartitionSyncConfiguration):
                    _realmFeaturesToAnalyse[SdkFeature.PartitionSyncConfiguration] = 1;
                    break;
                case nameof(SdkFeature.FlexibleSyncConfiguration):
                    _realmFeaturesToAnalyse[SdkFeature.FlexibleSyncConfiguration] = 1;
                    break;
                case nameof(SdkFeature.Anonymous):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.Anonymous] = 1;
                    }

                    break;
                case nameof(SdkFeature.EmailPassword):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.EmailPassword] = 1;
                    }

                    break;
                case nameof(SdkFeature.Facebook):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.Facebook] = 1;
                    }

                    break;
                case nameof(SdkFeature.Google):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.Google] = 1;
                    }

                    break;
                case nameof(SdkFeature.Apple):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.Apple] = 1;
                    }

                    break;
                case nameof(SdkFeature.JWT):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.JWT] = 1;
                    }

                    break;
                case nameof(SdkFeature.ApiKey):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.ApiKey] = 1;
                    }

                    break;
                case nameof(SdkFeature.ServerApiKey):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.ServerApiKey] = 1;
                    }

                    break;
                case nameof(SdkFeature.Function):
                    if (IsCredentials(context, identifierNameSyntax))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.Function] = 1;
                    }

                    break;
                case nameof(SdkFeature.CallAsync):
                    if (IsOfType(context, identifierNameSyntax, "Realms.Sync.User.FunctionsClient"))
                    {
                        _realmFeaturesToAnalyse[SdkFeature.CallAsync] = 1;
                    }
                    else
                    {
                        DebugLog($"{identifierNameSyntax} is likely some user defined CallAsync.");
                    }

                    break;
                case nameof(SdkFeature.GetMongoClient):
                    _realmFeaturesToAnalyse[SdkFeature.GetMongoClient] = 1;
                    break;
                case nameof(SdkFeature.DynamicApi):
                    _realmFeaturesToAnalyse[SdkFeature.DynamicApi] = 1;
                    break;
                default:
                    break;
            }
        }

        // Returns null if can't get parent of the parent has no children
        private static ITypeSymbol? GetSyntaxNodeParentType(GeneratorSyntaxContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            // target.CurrentSyntaxToken()
            // ^^^^^^--------------------------------------------(              )
            // of the SimpleMemberAccessExpression, get the first IdentifierName's type
            return context.SemanticModel.GetTypeInfo(
                identifierNameSyntax.Parent?.ChildNodes()?.FirstOrDefault()).Type;
        }

        private static bool IsOfType(GeneratorSyntaxContext context, IdentifierNameSyntax target, string typeToMatch)
        {
            return GetSyntaxNodeParentType(context, target)?.ToDisplayString() == typeToMatch;
        }

        private static bool IsCredentials(GeneratorSyntaxContext context, IdentifierNameSyntax identifierNameSyntax)
        {
            var parentType = GetSyntaxNodeParentType(context, identifierNameSyntax)?.ToDisplayString();
            var isCredential = parentType == "Realms.Sync.Credentials" || parentType == "Realms.Sync.Credentials.AuthProvider";

            if (!isCredential)
            {
                DebugLog($"{identifierNameSyntax} is not a credential that we recognize.");
            }

            return isCredential;
        }

        private string GetJsonPayload(bool pretty)
        {
            var jsonPayload = new StringBuilder();

            jsonPayload.Append('{');
            if (pretty)
            {
                jsonPayload.AppendLine();
            }

            foreach (var entry in _realmEnvMetrics)
            {
                AppendKeyValue(entry);
            }

            foreach (var entry in _realmFeaturesToAnalyse)
            {
                AppendKeyValue(entry);
            }

            var trailingCommaIndex = Environment.NewLine.Length + 1;
            jsonPayload.Remove(jsonPayload.Length - trailingCommaIndex, 1);

            jsonPayload.Append('}');

            return jsonPayload.ToString();

            void AppendKeyValue<Tkey, Tvalue>(KeyValuePair<Tkey, Tvalue> entry)
            {
                if (pretty)
                {
                    jsonPayload.Append('\t');
                }

                jsonPayload.Append($"\"{entry.Key}\": \"{entry.Value}\",");

                if (pretty)
                {
                    jsonPayload.AppendLine();
                }
            }
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
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(bytes));
        }

        private static byte[] GenerateComputerIdentifier()
        {
            // Assume OS X if not Windows.
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(n => n.Name == "en0" || (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                                   .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                                   .FirstOrDefault();
        }

        public static bool ShouldCollectAnalytics =>
            Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") == null
                && Environment.GetEnvironmentVariable("CI") == null;

        public static void DebugLog(string message)
        {
            Debug.WriteLine($"** Analytics: {message}");
        }

        public static void ErrorLog(string message)
        {
            Console.WriteLine($"** Analytics, Error: {message}");
        }

        public static void WarningLog(string message)
        {
            Console.WriteLine($"** Analytics, Warning: {message}");
        }

        public static void InfoLog(string message)
        {
            Console.WriteLine($"** Analytics, Info: {message}");
        }
    }
}
