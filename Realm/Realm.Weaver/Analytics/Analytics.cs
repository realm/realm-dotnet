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

        private readonly Dictionary<string, string> _realmEnvMetrics;

        private readonly Dictionary<string, byte> _realmFeaturesToAnalyze;

        private readonly Dictionary<string, Func<Instruction, (bool IsToDelete, string DictKey)>> _apiAnalysisSetters;

        private readonly Dictionary<string, Func<IMemberDefinition, (bool IsToDelete, string DictKey)>> _classAnalysisSetters;

        private readonly Config _config;

        internal Analytics(Config config, ImportedReferences references, ILogger logger)
        {
            _references = references;
            _config = config;
            _logger = logger;

            _realmFeaturesToAnalyze = Metric.SdkFeatures.Keys.ToDictionary(c => c, _ => (byte)0);
            _realmEnvMetrics = Metric.UserEnvironment.Keys.ToDictionary(c => c, _ => string.Empty);

            _classAnalysisSetters = new ()
            {
                ["IEmbeddedObject"] = member => (true, "IEmbeddedObject"),
                ["IAsymmetricObject"] = member => (true, "IAsymmetricObject"),
                ["Class"] = member =>
                    member is PropertyDefinition property &&
                    (property.PropertyType.IsIRealmObjectBaseImplementor(_references) ||
                    property.PropertyType.IsRealmObjectDescendant(_references)) ?
                    (true, "RealmObjectReference") : default,
                ["RealmValue"] = member => (true, "RealmValue"),
                ["IList`1"] = member =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return default;
                    }

                    var keyToSet = ((GenericInstanceType)property.PropertyType).GenericArguments[0].IsPrimitive ?
                        "PrimitiveList" : "ReferenceList";

                    return ((_realmFeaturesToAnalyze["PrimitiveList"] &
                        _realmFeaturesToAnalyze["ReferenceList"]) == 0x1, keyToSet);
                },
                ["IDictionary`2"] = member =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return default;
                    }

                    var keyToSet = ((GenericInstanceType)property.PropertyType).GenericArguments[1].IsPrimitive ?
                        "PrimitiveDictionary" : "ReferenceDictionary";

                    return ((_realmFeaturesToAnalyze["PrimitiveDictionary"] &
                        _realmFeaturesToAnalyze["ReferenceDictionary"]) == 0x1, keyToSet);
                },
                ["ISet`1"] = member =>
                {
                    if (!(member is PropertyDefinition property))
                    {
                        return default;
                    }

                    var keyToSet = ((GenericInstanceType)property.PropertyType).GenericArguments[0].IsPrimitive ?
                        "PrimitiveSet" : "ReferenceSet";

                    return ((_realmFeaturesToAnalyze["PrimitiveSet"] &
                        _realmFeaturesToAnalyze["ReferenceSet"]) == 0x1, keyToSet);
                },
                ["RealmInteger`1"] = member => (true, "RealmInteger"),
                ["BacklinkAttribute"] = member => (true, "BacklinkAttribute")
            };

            _apiAnalysisSetters = new ()
            {
                ["GetInstanceAsync"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "GetInstanceAsync") : default,
                ["GetInstance"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "GetInstance") : default,

                // ["NOT_SUPPORTED_YET"] = instruction => (true, NOT_SUPPORTED_YET),
                ["Find"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "Find") : default,
                ["WriteAsync"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "WriteAsync") : default,
                ["ThreadSafeReference"] = instruction => (true, "ThreadSafeReference"),

                // check if it's the right signature, that is 2 params in total of which
                // the second a bool and that it's set to true.
                ["Add"] = instruction =>
                    IsInRealmNamespace(instruction.Operand) &&
                    instruction.Operand is MethodSpecification methodSpecification &&
                    methodSpecification.Parameters.Count == 2 &&
                    methodSpecification.Parameters[1].ParameterType.MetadataType == MetadataType.Boolean &&
                    instruction.Previous.OpCode == OpCodes.Ldc_I4_1 ?
                    (true, "Add") : default,
                ["ShouldCompactOnLaunch"] = instruction => (true, "ShouldCompactOnLaunch"),
                ["MigrationCallback"] = instruction => (true, "MigrationCallback"),
                ["RealmChanged"] = instruction => (true, "RealmChanged"),
                ["SubscribeForNotifications"] = instruction =>
                {
                    if (!(instruction.Operand is MethodSpecification methodSpecification) || !IsInRealmNamespace(instruction.Operand))
                    {
                        return default;
                    }

                    var collectionType = ((TypeSpecification)methodSpecification.Parameters[0].ParameterType).Name;
                    var key = collectionType switch
                    {
                        "IQueryable`1" or "IOrderedQueryable`1" => "ResultSubscribeForNotifications",
                        "IList`1" => "ListSubscribeForNotifications",
                        "ISet`1" => "SetSubscribeForNotifications",
                        "IDictionary`2" => "DictionarySubscribeForNotifications",
                        _ => $"{collectionType} unknown collection"
                    };

                    return ((_realmFeaturesToAnalyze["ResultSubscribeForNotifications"] &
                        _realmFeaturesToAnalyze["ListSubscribeForNotifications"] &
                        _realmFeaturesToAnalyze["SetSubscribeForNotifications"] &
                        _realmFeaturesToAnalyze["DictionarySubscribeForNotifications"]) == 0x1, key);
                },
                ["PropertyChanged"] = instruction => (true, "PropertyChanged"),
                ["RecoverOrDiscardUnsyncedChangesHandler"] = instruction => (true, "RecoverOrDiscardUnsyncedChangesHandler"),
                ["RecoverUnsyncedChangesHandler"] = instruction => (true, "RecoverUnsyncedChangesHandler"),
                ["DiscardUnsyncedChangesHandler"] = instruction => (true, "DiscardUnsyncedChangesHandler"),
                ["ManualRecoveryHandler"] = instruction => (true, "ManualRecoveryHandler"),
                ["GetProgressObservable"] = instruction => (true, "GetProgressObservable"),
                ["PartitionSyncConfiguration"] = instruction => (true, "PartitionSyncConfiguration"),
                ["FlexibleSyncConfiguration"] = instruction => (true, "FlexibleSyncConfiguration"),
                ["Anonymous"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "Anonymous") : default,
                ["EmailPassword"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "EmailPassword") : default,
                ["Facebook"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "Facebook") : default,
                ["Google"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "Google") : default,
                ["Apple"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "Apple") : default,
                ["JWT"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "JWT") : default,
                ["ApiKey"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "ApiKey") : default,
                ["ServerApiKey"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "ServerApiKey") : default,
                ["Function"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "Function") : default,
                ["CallAsync"] = instruction => IsInRealmNamespace(instruction.Operand) ? (true, "CallAsync") : default,
                ["GetMongoClient"] = instruction => (true, "GetMongoClient"),
                ["DynamicApi"] = instruction => (true, "DynamicApi")
            };
        }

        internal void AnalyzeUserAssembly(ModuleDefinition module)
        {
            try
            {
                // collect environment details
                var frameworkInfo = GetFrameworkAndVersion(module, _config);

                _realmEnvMetrics["UserId"] = AnonymizedUserID;
                _realmEnvMetrics["ProjectId"] = SHA256Hash(Encoding.UTF8.GetBytes(module.Name));
                _realmEnvMetrics["RealmSdk"] = ".NET";
                _realmEnvMetrics["Language"] = "CSharp";
                _realmEnvMetrics["HostOsType"] = ConvertPlatformIdOsToMetricVersion(Environment.OSVersion.Platform);
                _realmEnvMetrics["HostOsVersion"] = Environment.OSVersion.Version.ToString();
                _realmEnvMetrics["HostCpuArch"] = GetHostCpuArchitecture;
                _realmEnvMetrics["TargetOsType"] = _config.TargetOSName;
                _realmEnvMetrics["TargetOsVersion"] = "FILL ME";
                _realmEnvMetrics["TargetOsMinimumVersion"] = "FILL ME";
                _realmEnvMetrics["TargetCpuArch"] = GetTargetCpuArchitecture(module);
                _realmEnvMetrics["FrameworkUsedInConjunction"] = frameworkInfo.Name;
                _realmEnvMetrics["FrameworkUsedInConjunctionVersion"] = frameworkInfo.Version;
                _realmEnvMetrics["LanguageVersion"] = GetLanguageVersion(module, _config.TargetFramework);
                _realmEnvMetrics["RealmSdkVersion"] = module.FindReference("Realm").Version.ToString();
                _realmEnvMetrics["CoreVersion"] = "FILL ME";
                _realmEnvMetrics["SdkInstallationMethod"] = _config.InstallationMethod;
                _realmEnvMetrics["IdeUsed"] = "msbuild";
                _realmEnvMetrics["IdeUsedVersion"] = "FILL ME";
                _realmEnvMetrics["NetFramework"] = _config.TargetFramework;
                _realmEnvMetrics["NetFrameworkVersion"] = _config.TargetFrameworkVersion;

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

                Func<IMemberDefinition, (bool IsToDelete, string DictKey)> featureFunc = null;

                foreach (var propertyResult in type.Properties)
                {
                    var property = propertyResult.Property;

                    var key = property.PropertyType.Name;
                    if (!_classAnalysisSetters.ContainsKey(key) && property.PropertyType.MetadataType == MetadataType.Class)
                    {
                        key = property.PropertyType.MetadataType.ToString();
                    }

                    if (_classAnalysisSetters.TryGetValue(key, out featureFunc))
                    {
                        HandleFeatureSetting(featureFunc.Invoke(property), key);
                    }

                    foreach (var attribute in property.CustomAttributes)
                    {
                        if (_classAnalysisSetters.TryGetValue(attribute.AttributeType.Name, out featureFunc))
                        {
                            HandleFeatureSetting(featureFunc.Invoke(property), key);
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

            Func<IMemberDefinition, (bool IsToDelete, string DictKey)> featureFunc = null;

            if (_classAnalysisSetters.TryGetValue("IAsymmetricObject", out featureFunc) &&
                (type.IsIAsymmetricObjectImplementor(_references) || type.IsAsymmetricObjectDescendant(_references)))
            {
                HandleFeatureSetting(featureFunc.Invoke(type), "IAsymmetricObject");
            }
            else if (_classAnalysisSetters.TryGetValue("IEmbeddedObject", out featureFunc) &&
                (type.IsIEmbeddedObjectImplementor(_references) || type.IsEmbeddedObjectDescendant(_references)))
            {
                HandleFeatureSetting(featureFunc.Invoke(type), "IEmbeddedObject");
            }

            AnalyzeClassMethods(type);

            foreach (var innerType in type.NestedTypes)
            {
                InternalAnalyzeSdkApi(innerType);
            }
        }

        private void HandleFeatureSetting((bool IsToDelete, string DictKey) tuple, string key)
        {
            if (!string.IsNullOrEmpty(tuple.DictKey))
            {
                _realmFeaturesToAnalyze[tuple.DictKey] = 1;
            }

            if (tuple.IsToDelete)
            {
                _classAnalysisSetters.Remove(key);
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

                    if (_apiAnalysisSetters.TryGetValue(key, out var featureFunc))
                    {
                        var (IsToDelete, DictKey) = featureFunc.Invoke(cil);

                        if (!string.IsNullOrEmpty(DictKey))
                        {
                            _realmFeaturesToAnalyze[DictKey] = 1;
                        }

                        if (IsToDelete)
                        {
                            _apiAnalysisSetters.Remove(key);
                        }
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

            foreach (var kvp in _realmEnvMetrics)
            {
                AppendKeyValue(Metric.UserEnvironment[kvp.Key], kvp.Value);
            }

            foreach (var kvp in _realmFeaturesToAnalyze)
            {
                AppendKeyValue(Metric.SdkFeatures[kvp.Key], kvp.Value);
            }

            var trailingCommaIndex = pretty ? Environment.NewLine.Length + 1 : 1;
            jsonPayload.Remove(jsonPayload.Length - trailingCommaIndex, 1);

            jsonPayload.Append('}');

            return jsonPayload.ToString();

            void AppendKeyValue<Tkey, Tvalue>(Tkey key, Tvalue value)
            {
                if (pretty)
                {
                    jsonPayload.Append('\t');
                }

                jsonPayload.Append($"\"{key}\": \"{value}\",");

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

        private static bool IsInRealmNamespace(object operand)
        {
            if (!(operand is MemberReference memberReference))
            {
                return false;
            }

            return memberReference.DeclaringType.FullName.StartsWith("Realms", StringComparison.Ordinal);
        }

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

            public string InstallationMethod { get; set; }
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
