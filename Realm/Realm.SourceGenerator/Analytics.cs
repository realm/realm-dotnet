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
        public List<Diagnostic> Diagnostics { get; } = new();

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
            { SdkFeature.FIXME, 0 },
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
            { SdkFeature.FIX_ME_THREE, 0 },
            { SdkFeature.DynamicApi, 0 },
        };

        private Dictionary<string, string> _realmEnvMetrics = new()
        {
            { SdkFeature.UserId, string.Empty },
            { SdkFeature.RealmSdk, ".NET" },
            { SdkFeature.Language, "C#" },
            { SdkFeature.HostOsType, string.Empty },
            { SdkFeature.HostOsVersion, string.Empty },
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

        public void SubmitAnalytics(GeneratorExecutionContext context)
        {
            var payload = string.Empty;

            if (Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") != null ||
                Environment.GetEnvironmentVariable("CI") != null)
            {
                return;
            }

            _realmEnvMetrics[SdkFeature.UserId] = AnonymizedUserID;

            ComputeHostOSNameAndVersion(out var osName, out var osVersion);

            // TODO andrea: check if searching strings in this way works on non Windows platforms
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
                    Diagnostics.Add(
                        SourceGenerator.Diagnostics.AnalyticsDebugInfo($"{osName} is not an operating system that we recognize."));
                    break;
            }

            _realmEnvMetrics[SdkFeature.HostOsVersion] = osVersion;

            bool prettyJson = false;
#if DEBUG
            prettyJson = true;
#endif
            payload = GetJsonPayload(prettyJson);

#if !DEBUG
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            // TODO andrea: this has never been tested yet
            SendRequest(
                "https://data.mongodb-api.com/app/realmsdkmetrics-zmhtm/endpoint/metric_webhook/metric_stage?data=",
                base64Payload,
                string.Empty);
#endif

            if (Environment.GetEnvironmentVariable("REALM_PRINT_ANALYTICS") != null &&
                Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") == null)
            {
                // TODO andrea: temporarily reusing the common diagnostic for analytics, this likely needs to have its own
                Diagnostics.Add(SourceGenerator.Diagnostics.AnalyticsDebugInfo($@"
----------------------------------
Analytics payload
{payload}
----------------------------------"));
            }

            // TODO andrea: we may actually look for something like INTERNAL_DEBUG, so that the user never sees this
#if DEBUG
            foreach (var diagnostic in Diagnostics)
            {
                // TODO andrea: this does not up show anywhere. Investigate why.
                context.ReportDiagnostic(diagnostic);
            }
#endif
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
                case nameof(SdkFeature.FIXME):
                    _realmFeaturesToAnalyse[SdkFeature.FIXME] = 1;
                    break;
                case nameof(SdkFeature.Find):
                    _realmFeaturesToAnalyse[SdkFeature.Find] = 1;
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
                    ITypeSymbol referenceType;
                    try
                    {
                        // target.SubscribeForNotifications()
                        // ^^^^^^--------------------------------------------(              )
                        // of the SimpleMemberAccessExpression, get the first IdentifierName's type
                        referenceType = context.SemanticModel.GetTypeInfo(identifierNameSyntax.Parent.ChildNodes().First()).Type;
                    }
                    catch (Exception ex)
                    {
                        Diagnostics.Add(SourceGenerator.Diagnostics.AnalyticsDebugInfo(
                            $"{identifierNameSyntax} is likely some user defined syntax token.{Environment.NewLine}{ex.Message}"));
                        break;
                    }

                    switch (referenceType.Name)
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
                            Diagnostics.Add(
                                SourceGenerator.Diagnostics.AnalyticsDebugInfo($"{referenceType.Name} is not a collection type that is supported for notifications"));
                            break;
                    }

                    break;
                case nameof(SdkFeature.PropertyChanged):
                    // TODO andrea: this is likely not enough, I need to check the type on which PropertyChanged is called
                    _realmFeaturesToAnalyse[SdkFeature.PropertyChanged] = 1;
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
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.Anonymous] = 1;
                    break;
                case nameof(SdkFeature.EmailPassword):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.EmailPassword] = 1;
                    break;
                case nameof(SdkFeature.Facebook):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.Facebook] = 1;
                    break;
                case nameof(SdkFeature.Google):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.Google] = 1;
                    break;
                case nameof(SdkFeature.Apple):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.Apple] = 1;
                    break;
                case nameof(SdkFeature.JWT):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.JWT] = 1;
                    break;
                case nameof(SdkFeature.ApiKey):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.ApiKey] = 1;
                    break;
                case nameof(SdkFeature.ServerApiKey):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.ServerApiKey] = 1;
                    break;
                case nameof(SdkFeature.Function):
                    if (!IsCredentials(identifierNameSyntax, context))
                    {
                        break;
                    }

                    _realmFeaturesToAnalyse[SdkFeature.Function] = 1;
                    break;
                default:
                    break;
            }
        }

        private bool IsCredentials(IdentifierNameSyntax identifierNameSyntax, GeneratorSyntaxContext context)
        {
            try
            {
                var parentType = context.SemanticModel.GetTypeInfo(identifierNameSyntax.Parent.ChildNodes().First()).Type.ToDisplayString();
                return parentType == "Realms.Sync.Credentials" || parentType == "Realms.Sync.Credentials.AuthProvider";
            }
            catch (Exception ex)
            {
                Diagnostics.Add(SourceGenerator.Diagnostics.AnalyticsDebugInfo(
                            $"{identifierNameSyntax} is likely some user defined syntax token.{Environment.NewLine}{ex.Message}"));
                return false;
            }
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
    }
}
