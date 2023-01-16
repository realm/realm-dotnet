////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

using static RealmWeaver.AnalyticsUtils;
using static RealmWeaver.Metric.SdkFeature;

namespace RealmWeaver
{
    // Asynchronously submits build information to Realm when the assembly weaver
    // is running
    //
    // To be clear: this does *not* run when your app is in production or on
    // your end-user's devices; it will only run when you build your app from source.
    //
    // Why are we doing this? Because it helps us build a better product for you.
    // None of the data personally identifies you, your employer or your app, but it
    // *will* help us understand what Realm version you use, what host OS you use,
    // etc. Having this info will help with prioritizing our time, adding new
    // features and deprecating old ones. Collecting an anonymized assembly name &
    // anonymized MAC is the only way for us to count actual usage of the other
    // metrics accurately. If we don't have a way to deduplicate the info reported,
    // it will be useless, as a single developer building their app on Windows ten
    // times would report 10 times more than a single developer that only builds
    // once from Mac OS X, making the data all but useless. No one likes sharing
    // data unless it's necessary, we get it, and we've debated adding this for a
    // long long time. Since Realm is a free product without an email sign-up, we
    // feel this is a necessary step so we can collect relevant data to build a
    // better product for you.
    //
    // Currently the following information is reported:
    // - What OS and CPU architecture you are running on
    // - What OS and CPU architecture you are building for
    // - What version of the Realm SDK you're using
    // - What framework and what framework version Realm is being used with (e.g. Xamarin, MAUI, etc.)
    // - How the Realm SDK was installed (e.g. Nuget, manual, etc.)
    // - What APIs of the Realm SDK you're using
    // - An anonymized MAC address and assembly name ID to aggregate the other information on.
    internal class Analytics
    {
        private readonly ImportedReferences _references;
        private readonly ILogger _logger;

        #region FeatureDictionaries

        private readonly Dictionary<string, byte> _realmFeaturesToAnalyze = new Dictionary<string, byte>()
        {
            [IEmbeddedObject] = 0,
            [IAsymmetricObject] = 0,
            [ReferenceList] = 0,
            [PrimitiveList] = 0,
            [ReferenceDictionary] = 0,
            [PrimitiveDictionary] = 0,
            [ReferenceSet] = 0,
            [PrimitiveSet] = 0,
            [RealmInteger] = 0,
            [RealmObjectReference] = 0,
            [RealmValue] = 0,
            [BacklinkAttribute] = 0,
            [GetInstanceAsync] = 0,
            [GetInstance] = 0,
            [NOT_SUPPORTED_YET] = 0,
            [Find] = 0,
            [WriteAsync] = 0,
            [ThreadSafeReference] = 0,
            [Add] = 0,
            [ShouldCompactOnLaunch] = 0,
            [MigrationCallback] = 0,
            [RealmChanged] = 0,
            [ListSubscribeForNotifications] = 0,
            [SetSubscribeForNotifications] = 0,
            [DictionarySubscribeForNotifications] = 0,
            [ResultSubscribeForNotifications] = 0,
            [PropertyChanged] = 0,
            [RecoverOrDiscardUnsyncedChangesHandler] = 0,
            [RecoverUnsyncedChangesHandler] = 0,
            [DiscardUnsyncedChangesHandler] = 0,
            [ManualRecoveryHandler] = 0,
            [GetProgressObservable] = 0,
            [PartitionSyncConfiguration] = 0,
            [FlexibleSyncConfiguration] = 0,
            [Anonymous] = 0,
            [EmailPassword] = 0,
            [Facebook] = 0,
            [Google] = 0,
            [Apple] = 0,
            [JWT] = 0,
            [ApiKey] = 0,
            [ServerApiKey] = 0,
            [Function] = 0,
            [CallAsync] = 0,
            [GetMongoClient] = 0,
            [DynamicApi] = 0,
        };

        private readonly Dictionary<string, string> _realmEnvMetrics = new Dictionary<string, string>()
        {
            { UserId, string.Empty },
            { RealmSdk, ".NET" },
            { Language, "CSharp" },
            { LanguageVersion, string.Empty },
            { HostOsType, string.Empty },
            { HostOsVersion, string.Empty },
            { HostCpuArch, string.Empty },
            { TargetOsType, string.Empty },
            { TargetOsMinimumVersion, string.Empty },
            { TargetOsVersion, string.Empty },
            { TargetCpuArch, string.Empty },
            { RealmSdkVersion, string.Empty },
            { CoreVersion, string.Empty },
            { FrameworkUsedInConjunction, string.Empty },
            { FrameworkUsedInConjunctionVersion, string.Empty },
            { SdkInstallationMethod, string.Empty },
            { IdeUsed, string.Empty },
            { IdeUsedVersion, string.Empty },
        };

        private readonly Dictionary<string, Func<Instruction, Dictionary<string, byte>, ImportedReferences, bool>> _apiAnalysisSetters = new Dictionary<string, Func<Instruction, Dictionary<string, byte>, ImportedReferences, bool>>()
        {
            {
                nameof(GetInstanceAsync), (instruction, featureDict, references) =>
                {
                    if (IsInRealmNamespace(instruction.Operand))
                    {
                        featureDict[GetInstanceAsync] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(GetInstance),  (instruction, featureDict, references) =>
                {
                    if (IsInRealmNamespace(instruction.Operand))
                    {
                        featureDict[GetInstance] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(NOT_SUPPORTED_YET),  (instruction, featureDict, references) =>
                {
                    featureDict[NOT_SUPPORTED_YET] = 1;
                    return true;
                }
            },
            {
                nameof(Find), (instruction, featureDict, references) =>
                {
                    if (IsInRealmNamespace(instruction.Operand))
                    {
                        featureDict[Find] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(WriteAsync),  (instruction, featureDict, references) =>
                {
                    if (IsInRealmNamespace(instruction.Operand))
                    {
                        featureDict[WriteAsync] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(ThreadSafeReference),  (instruction, featureDict, references) =>
                {
                    featureDict[ThreadSafeReference] = 1;
                    return true;
                }
            },
            {
                nameof(Add),  (instruction, featureDict, references) =>
                {
                    // check if it's the right signature, that is 2 params in total of which
                    // the second a bool and that it's set to true.
                    if (IsInRealmNamespace(instruction.Operand) &&
                        instruction.Operand is MethodSpecification methodSpecification &&
                        methodSpecification.Parameters.Count == 2 &&
                        methodSpecification.Parameters[1].ParameterType.MetadataType == MetadataType.Boolean &&
                        instruction.Previous.OpCode == OpCodes.Ldc_I4_1)
                    {
                        featureDict[Add] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(ShouldCompactOnLaunch),  (instruction, featureDict, references) =>
                {
                    featureDict[ShouldCompactOnLaunch] = 1;
                    return true;
                }
            },
            {
                nameof(MigrationCallback),  (instruction, featureDict, references) =>
                {
                    featureDict[MigrationCallback] = 1;
                    return true;
                }
            },
            {
                nameof(RealmChanged),  (instruction, featureDict, references) =>
                {
                    featureDict[RealmChanged] = 1;
                    return true;
                }
            },
            {
                "SubscribeForNotifications", (instruction, featureDict, references) =>
                {
                    if (!(instruction.Operand is MethodSpecification methodSpecification))
                    {
                        return false;
                    }

                    switch (((TypeSpecification)methodSpecification.Parameters[0].ParameterType).Name)
                    {
                        case "IQueryable`1":
                        case "IOrderedQueryable`1":
                            featureDict[ResultSubscribeForNotifications] = 1;
                            break;
                        case "IList`1":
                            featureDict[ListSubscribeForNotifications] = 1;
                            break;
                        case "ISet`1":
                            featureDict[SetSubscribeForNotifications] = 1;
                            break;
                        case "IDictionary`2":
                            featureDict[DictionarySubscribeForNotifications] = 1;
                            break;
                        default:
                            break;
                    }

                    return (featureDict[ResultSubscribeForNotifications] &
                        featureDict[ListSubscribeForNotifications] &
                        featureDict[SetSubscribeForNotifications] &
                        featureDict[DictionarySubscribeForNotifications]) == 0x1;
                }
            },
            {
                nameof(PropertyChanged), (instruction, featureDict, references) =>
                {
                    featureDict[PropertyChanged] = 1;
                    return true;
                }
            },
            {
                nameof(RecoverOrDiscardUnsyncedChangesHandler),  (instruction, featureDict, references) =>
                {
                    featureDict[RecoverOrDiscardUnsyncedChangesHandler] = 1;
                    return true;
                }
            },
            {
                nameof(RecoverUnsyncedChangesHandler),  (instruction, featureDict, references) =>
                {
                    featureDict[RecoverUnsyncedChangesHandler] = 1;
                    return true;
                }
            },
            {
                nameof(DiscardUnsyncedChangesHandler), (instruction, featureDict, references) =>
                {
                    featureDict[DiscardUnsyncedChangesHandler] = 1;
                    return true;
                }
            },
            {
                nameof(ManualRecoveryHandler), (instruction, featureDict, references) =>
                {
                    featureDict[ManualRecoveryHandler] = 1;
                    return true;
                }
            },
            {
                nameof(GetProgressObservable), (instruction, featureDict, references) =>
                {
                    featureDict[GetProgressObservable] = 1;
                    return true;
                }
            },
            {
                nameof(PartitionSyncConfiguration), (instruction, featureDict, references) =>
                {
                    featureDict[PartitionSyncConfiguration] = 1;
                    return true;
                }
            },
            {
                nameof(FlexibleSyncConfiguration), (instruction, featureDict, references) =>
                {
                    featureDict[FlexibleSyncConfiguration] = 1;
                    return true;
                }
            },
            {
                nameof(Anonymous), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[Anonymous] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(EmailPassword), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[EmailPassword] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(Facebook), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[Facebook] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(Google), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[Google] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(Apple), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[Apple] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(JWT), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[JWT] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(ApiKey), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[ApiKey] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(ServerApiKey), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[ServerApiKey] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(Function), (instruction, featureDict, references) =>
                {
                    if (IsCredential(instruction.Operand))
                    {
                        featureDict[Function] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(CallAsync), (instruction, featureDict, references) =>
                {
                    if (IsFromNamespace(instruction.Operand, "Realms.Sync.User/FunctionsClient"))
                    {
                        featureDict[CallAsync] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(GetMongoClient), (instruction, featureDict, references) =>
                {
                    featureDict[GetMongoClient] = 1;
                    return true;
                }
            },
            {
                nameof(DynamicApi), (instruction, featureDict, references) =>
                {
                    featureDict[DynamicApi] = 1;
                    return true;
                }
            },
        };

        private readonly Dictionary<string, Func<IMemberDefinition, Dictionary<string, byte>, ImportedReferences, bool>> _classAnalysisSetters = new Dictionary<string, Func<IMemberDefinition, Dictionary<string, byte>, ImportedReferences, bool>>()
        {
            {
                nameof(IEmbeddedObject), (member, featureDict, references) =>
                {
                    featureDict[IEmbeddedObject] = 1;
                    return true;
                }
            },
            {
                nameof(IAsymmetricObject), (member, featureDict, references) =>
                {
                    featureDict[IAsymmetricObject] = 1;
                    return true;
                }
            },
            {
                "Class", (member, featureDict, references) =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return false;
                    }

                    if (property.PropertyType.IsIRealmObjectBaseImplementor(references) ||
                        property.PropertyType.IsRealmObjectDescendant(references))
                    {
                        featureDict[RealmObjectReference] = 1;
                        return true;
                    }

                    return false;
                }
            },
            {
                nameof(RealmValue), (member, featureDict, references) =>
                {
                    featureDict[RealmValue] = 1;
                    return true;
                }
            },
            {
                "IList`1", (member, featureDict, references) =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return false;
                    }

                    if (((GenericInstanceType)property.PropertyType).GenericArguments[0].IsPrimitive)
                    {
                        featureDict[PrimitiveList] = 1;
                    }
                    else
                    {
                        featureDict[ReferenceList] = 1;
                    }

                    return (featureDict[PrimitiveList] &
                        featureDict[ReferenceList]) == 0x1;
                }
            },
            {
                "IDictionary`2", (member, featureDict, references) =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return false;
                    }

                    if (((GenericInstanceType)property.PropertyType).GenericArguments[1].IsPrimitive)
                    {
                        featureDict[PrimitiveDictionary] = 1;
                    }
                    else
                    {
                        featureDict[ReferenceDictionary] = 1;
                    }

                    return (featureDict[PrimitiveDictionary] &
                        featureDict[ReferenceDictionary]) == 0x1;
                }
            },
            {
                "ISet`1", (member, featureDict, references) =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return false;
                    }

                    if (((GenericInstanceType)property.PropertyType).GenericArguments[0].IsPrimitive)
                    {
                        featureDict[PrimitiveSet] = 1;
                    }
                    else
                    {
                        featureDict[ReferenceSet] = 1;
                    }

                    return (featureDict[PrimitiveSet] &
                        featureDict[ReferenceSet]) == 0x1;
                }
            },
            {
                "RealmInteger`1", (member, featureDict, references) =>
                {
                    featureDict[RealmInteger] = 1;
                    return true;
                }
            },
            {
                nameof(BacklinkAttribute), (member, featureDict, references) =>
                {
                    featureDict[BacklinkAttribute] = 1;
                    return true;
                }
            }
        };

        #endregion

        private readonly Config _config;

        internal Analytics(Config config, ImportedReferences references, ILogger logger)
        {
            _references = references;
            _config = config;
            _logger = logger;
        }

        internal void AnalyzeUserAssembly(ModuleDefinition module)
        {
            try
            {
                // collect environment details
                var frameworkInfo = GetFrameworkAndVersion(module, _config);

                _realmEnvMetrics[UserId] = AnonymizedUserID;
                _realmEnvMetrics[ProjectId] = SHA256Hash(Encoding.UTF8.GetBytes(module.Name));
                _realmEnvMetrics[HostOsType] = ConvertPlatformIdOsToMetricVersion(Environment.OSVersion.Platform);
                _realmEnvMetrics[HostOsVersion] = Environment.OSVersion.Version.ToString();
                _realmEnvMetrics[HostCpuArch] = GetHostCpuArchitecture;
                _realmEnvMetrics[TargetOsType] = _config.TargetOSName;
                _realmEnvMetrics[TargetOsVersion] = "FILL ME";
                _realmEnvMetrics[TargetOsMinimumVersion] = "FILL ME";
                _realmEnvMetrics[TargetCpuArch] = GetTargetCpuArchitecture(module);
                _realmEnvMetrics[FrameworkUsedInConjunction] = frameworkInfo.Name;
                _realmEnvMetrics[FrameworkUsedInConjunctionVersion] = frameworkInfo.Version;
                _realmEnvMetrics[LanguageVersion] = GetLanguageVersion(module, _config.TargetFramework);
                _realmEnvMetrics[RealmSdkVersion] = module.FindReference("Realm").Version.ToString();
                _realmEnvMetrics[CoreVersion] = "FILL ME";
                _realmEnvMetrics[SdkInstallationMethod] = "FILL ME";
                _realmEnvMetrics[IdeUsed] = "FILL ME";
                _realmEnvMetrics[IdeUsedVersion] = "FILL ME";

                foreach (var type in module.Types)
                {
                    InternalAnalyzeSdkApi(type);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Could not analyze the user's assembly.{Environment.NewLine}{e.Message}");
            }
        }

        internal void AnalyzeRealmClassProperties(WeaveTypeResult[] types)
        {
            foreach (var type in types)
            {
                if (type.Properties == null)
                {
                    continue;
                }

                Func<IMemberDefinition, Dictionary<string, byte>, ImportedReferences, bool> featureFunc = null;

                foreach (var propertyResult in type.Properties)
                {
                    var property = propertyResult.Property;

                    var key = property.PropertyType.Name;
                    if (!_classAnalysisSetters.ContainsKey(key))
                    {
                        // when looking for "Class" type
                        key = property.PropertyType.MetadataType.ToString();
                    }

                    if (_classAnalysisSetters.TryGetValue(key, out featureFunc) &&
                        featureFunc.Invoke(property, _realmFeaturesToAnalyze, _references))
                    {
                        // if the byte is set, remove the entry from the dict to avoid future unnecessary work
                        _classAnalysisSetters.Remove(key);
                    }

                    foreach (var attribute in property.CustomAttributes)
                    {
                        if (_classAnalysisSetters.TryGetValue(attribute.AttributeType.Name, out featureFunc) &&
                            featureFunc.Invoke(property, _realmFeaturesToAnalyze, _references))
                        {
                            _classAnalysisSetters.Remove(key);
                        }
                    }
                }
            }
        }

        private void InternalAnalyzeSdkApi(TypeDefinition type)
        {
            if (!type.IsClass)
            {
                return;
            }

            Func<IMemberDefinition, Dictionary<string, byte>, ImportedReferences, bool> featureFunc = null;

            if (_classAnalysisSetters.TryGetValue(nameof(IAsymmetricObject), out featureFunc) &&
                (type.IsIAsymmetricObjectImplementor(_references) || type.IsAsymmetricObjectDescendant(_references)) &&
                featureFunc.Invoke(type, _realmFeaturesToAnalyze, _references))
            {
                _classAnalysisSetters.Remove(nameof(IAsymmetricObject));
            }
            else if (_classAnalysisSetters.TryGetValue(nameof(IEmbeddedObject), out featureFunc) &&
                (type.IsIEmbeddedObjectImplementor(_references) || type.IsEmbeddedObjectDescendant(_references)) &&
                featureFunc.Invoke(type, _realmFeaturesToAnalyze, _references))
            {
                _classAnalysisSetters.Remove(nameof(IEmbeddedObject));
            }

            AnalyzeClassMethods(type);

            foreach (var innerType in type.NestedTypes)
            {
                InternalAnalyzeSdkApi(innerType);
            }
        }

        private void AnalyzeClassMethods(TypeDefinition type)
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                {
                    continue;
                }

                foreach (var cil in method.Body.Instructions)
                {
                    var key = (cil.Operand as MemberReference)?.Name;
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    var index = new int[]
                    {
                        key.IndexOf("get_", StringComparison.Ordinal),
                        key.IndexOf("set_", StringComparison.Ordinal),
                        key.IndexOf("add_", StringComparison.Ordinal)
                    }.Max();

                    if (index > -1)
                    {
                        // when dealing with:
                        // set_ShouldCompactOnLaunch
                        // add_RealmChanged
                        // add_PropertyChanged
                        // get_DynamicApi
                        key = key.Substring(index + 4);
                    }

                    if (!_apiAnalysisSetters.ContainsKey(key) && cil.Operand is MethodReference methodReference &&
                        methodReference.ReturnType.DeclaringType != null)
                    {
                        // when dealing with ThreadSafeReference
                        key = methodReference.ReturnType.DeclaringType.ToString();
                        key = key.Replace("Realms.", string.Empty);
                    }

                    if (!_apiAnalysisSetters.ContainsKey(key) && key == ".ctor")
                    {
                        key = ((MemberReference)cil.Operand).DeclaringType.Name;
                    }

                    if (_apiAnalysisSetters.TryGetValue(key, out var featureFunc) &&
                        featureFunc.Invoke(cil, _realmFeaturesToAnalyze, _references))
                    {
                        _apiAnalysisSetters.Remove(key);
                    }
                }
            }
        }

        internal async Task<string> SubmitAnalytics()
        {
            try
            {
                var pretty = false;

                // TODO andrea: see what the correct address for production should be
                var sendAddr = "https://data.mongodb-api.com/app/realmsdkmetrics-zmhtm/endpoint/metric_webhook/metric?data=";
#if DEBUG
                pretty = true;
#endif

                // TODO andrea: find a general address that the whole team can use to do tests
                // sendAddr = "https://eu-central-1.aws.data.mongodb-api.com/app/realmmetricscollection-acxca/endpoint/realm_metrics/debug_route?data=";
                var payload = GetJsonPayload(pretty);

                if (_config.AnalyticsCollection != AnalyticsCollection.DryRun)
                {
                    var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
                    await SendRequest(sendAddr, base64Payload, string.Empty);
                }

                return payload;
            }
            catch (Exception e)
            {
                _logger.Error($"Could not submit analytics.{Environment.NewLine}{e.Message}");
                return e.Message;
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

            foreach (var entry in _realmFeaturesToAnalyze)
            {
                AppendKeyValue(entry);
            }

            var trailingCommaIndex = pretty ? Environment.NewLine.Length + 1 : 1;
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

        private static async Task SendRequest(string prefixAddr, string payload, string suffixAddr)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(4);
                await httpClient.GetAsync(new Uri(prefixAddr + payload + suffixAddr));
            }
        }

        private static bool IsFromNamespace(object operand, string targetNamespace)
        {
            if (!(operand is MemberReference memberReference))
            {
                return false;
            }

            return memberReference.DeclaringType.FullName == targetNamespace;
        }

        private static bool IsCredential(object operand) => IsFromNamespace(operand, "Realms.Sync.Credentials");

        private static bool IsInRealmNamespace(object operand) => IsFromNamespace(operand, "Realms.Realm");

        public class Config
        {
            public AnalyticsCollection AnalyticsCollection { get; set; }

            public string AnalyticsLogPath { get; set; }

            public string TargetOSName { get; set; }

            // When in Unity this holds the Unity editor's or Unity player's name; otherwise it holds
            // the .NET target name
            public string TargetFramework { get; set; }

            // When in Unity this holds the Unity editor's or Unity player's version;
            // otherwise it holds the .NET target version
            public string TargetFrameworkVersion { get; set; }
        }

        public enum AnalyticsCollection
        {
            Disabled,
            DryRun,
            Minimal,
            Full,
        }
    }
}
