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
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

using static RealmWeaver.AnalyticsUtils;

using Feature = RealmWeaver.Metric.Feature;
using UserEnvironment = RealmWeaver.Metric.Environment;

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

        private readonly Dictionary<string, string> _realmEnvMetrics = new();

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

            _classAnalysisSetters = new()
            {
                [Feature.IEmbeddedObject] = member => (true, Feature.IEmbeddedObject),
                [Feature.IAsymmetricObject] = member => (true, Feature.IAsymmetricObject),
                ["Class"] = member =>
                    member is PropertyDefinition property &&
                    (property.PropertyType.IsIRealmObjectBaseImplementor(_references) ||
                    property.PropertyType.IsRealmObjectDescendant(_references)) ?
                    (true, Feature.RealmObjectReference) : default,
                [Feature.RealmValue] = member => (true, Feature.RealmValue),
                ["IList`1"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveList, Feature.ReferenceList),
                ["IDictionary`2"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveDictionary, Feature.ReferenceDictionary, 1),
                ["ISet`1"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveSet, Feature.ReferenceSet),
                ["RealmInteger`1"] = member => (true, Feature.RealmInteger),
                [Feature.BacklinkAttribute] = member => (true, Feature.BacklinkAttribute)
            };

            _apiAnalysisSetters = new ()
            {
                [Feature.GetInstanceAsync] = instruction => AnalyzeRealmApi(instruction, Feature.GetInstanceAsync),
                [Feature.GetInstance] = instruction => AnalyzeRealmApi(instruction, Feature.GetInstance),
                [Feature.Find] = instruction => AnalyzeRealmApi(instruction, Feature.Find),
                [Feature.WriteAsync] = instruction => AnalyzeRealmApi(instruction, Feature.WriteAsync),
                [Feature.ThreadSafeReference] = instruction => AnalyzeRealmApi(instruction, Feature.ThreadSafeReference),

                // check if it's the right signature, that is 2 params in total of which
                // the second a bool and that it's set to true.
                // TODO - Nikola: why?
                [Feature.Add] = instruction =>
                    IsInRealmNamespace(instruction.Operand) &&
                    instruction.Operand is MethodSpecification methodSpecification &&
                    methodSpecification.Parameters.Count == 2 &&
                    methodSpecification.Parameters[1].ParameterType.MetadataType == MetadataType.Boolean &&
                    instruction.Previous.OpCode == OpCodes.Ldc_I4_1 ?
                    (true, Feature.Add) : default,
                [Feature.ShouldCompactOnLaunch] = instruction => (true, Feature.ShouldCompactOnLaunch),
                [Feature.MigrationCallback] = instruction => (true, Feature.MigrationCallback),
                [Feature.RealmChanged] = instruction => (true, Feature.RealmChanged),
                ["SubscribeForNotifications"] = instruction =>
                {
                    if (instruction.Operand is not MethodSpecification methodSpecification || !IsInRealmNamespace(instruction.Operand))
                    {
                        return default;
                    }

                    var collectionType = ((TypeSpecification)methodSpecification.Parameters[0].ParameterType).Name;
                    var key = collectionType switch
                    {
                        "IQueryable`1" or "IOrderedQueryable`1" => Feature.ResultSubscribeForNotifications,
                        "IList`1" => Feature.ListSubscribeForNotifications,
                        "ISet`1" => Feature.SetSubscribeForNotifications,
                        "IDictionary`2" => Feature.DictionarySubscribeForNotifications,
                        _ => $"{collectionType} unknown collection"
                    };

                    var shouldDelete = ContainsAllRelatedFeatures(key,
                        Feature.ResultSubscribeForNotifications,
                        Feature.ListSubscribeForNotifications,
                        Feature.SetSubscribeForNotifications,
                        Feature.DictionarySubscribeForNotifications);

                    return (shouldDelete, key);
                },
                [Feature.PropertyChanged] = instruction => (true, Feature.PropertyChanged),
                [Feature.RecoverOrDiscardUnsyncedChangesHandler] = instruction => (true, Feature.RecoverOrDiscardUnsyncedChangesHandler),
                [Feature.RecoverUnsyncedChangesHandler] = instruction => (true, Feature.RecoverUnsyncedChangesHandler),
                [Feature.DiscardUnsyncedChangesHandler] = instruction => (true, Feature.DiscardUnsyncedChangesHandler),
                [Feature.ManualRecoveryHandler] = instruction => (true, Feature.ManualRecoveryHandler),
                [Feature.GetProgressObservable] = instruction => (true, Feature.GetProgressObservable),
                [Feature.PartitionSyncConfiguration] = instruction => (true, Feature.PartitionSyncConfiguration),
                [Feature.FlexibleSyncConfiguration] = instruction => (true, Feature.FlexibleSyncConfiguration),
                [Feature.Anonymous] = instruction => AnalyzeRealmApi(instruction, Feature.Anonymous),
                [Feature.EmailPassword] = instruction => AnalyzeRealmApi(instruction, Feature.EmailPassword),
                [Feature.Facebook] = instruction => AnalyzeRealmApi(instruction, Feature.Facebook),
                [Feature.Google] = instruction => AnalyzeRealmApi(instruction, Feature.Google),
                [Feature.Apple] = instruction => AnalyzeRealmApi(instruction, Feature.Apple),
                [Feature.JWT] = instruction => AnalyzeRealmApi(instruction, Feature.JWT),
                [Feature.ApiKey] = instruction => AnalyzeRealmApi(instruction, Feature.ApiKey),
                [Feature.ServerApiKey] = instruction => AnalyzeRealmApi(instruction, Feature.ServerApiKey),
                [Feature.Function] = instruction => AnalyzeRealmApi(instruction, Feature.Function),
                [Feature.CallAsync] = instruction => AnalyzeRealmApi(instruction, Feature.CallAsync),
                [Feature.GetMongoClient] = instruction => (true, Feature.GetMongoClient),
                [Feature.DynamicApi] = instruction => (true, Feature.DynamicApi)
            };

            (bool ShouldDelete, string DictKey) AnalyzeCollectionProperty(IMemberDefinition member, string primitiveKey, string referenceKey, int genericArgIndex = 0)
            {
                if (member is not PropertyDefinition property ||
                    property.PropertyType is not GenericInstanceType genericType ||
                    genericType.GenericArguments.Count < genericArgIndex + 1)
                {
                    return default;
                }

                var isPrimitive = genericType.GenericArguments[genericArgIndex].IsPrimitive;

                var keyToAdd = isPrimitive ? primitiveKey : referenceKey;

                var shouldDelete = ContainsAllRelatedFeatures(keyToAdd, referenceKey, primitiveKey);
                return (shouldDelete, keyToAdd);
            }

            (bool ShouldDelete, string DictKey) AnalyzeRealmApi(Instruction instruction, string key)
            {
                if (IsInRealmNamespace(instruction.Operand))
                {
                    return (true, key);
                }

                return default;
            }

            bool ContainsAllRelatedFeatures(string key, params string[] features)
            {
                foreach (var feature in features)
                {
                    if (feature != key && (!_realmFeaturesToAnalyze.TryGetValue(feature, out var value) || value == 0))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal void AnalyzeUserAssembly(ModuleDefinition module)
        {
            try
            {
                // collect environment details
                var frameworkInfo = GetFrameworkAndVersion(module, _config);

                _realmEnvMetrics[UserEnvironment.UserId] = AnonymizedUserID;
                _realmEnvMetrics[UserEnvironment.ProjectId] = SHA256Hash(Encoding.UTF8.GetBytes(module.Name));
                _realmEnvMetrics[UserEnvironment.RealmSdk] = ".NET";
                _realmEnvMetrics[UserEnvironment.Language] = "C#";
                _realmEnvMetrics[UserEnvironment.HostOsType] = GetHostOsName();
                _realmEnvMetrics[UserEnvironment.HostOsVersion] = Environment.OSVersion.Version.ToString();
                _realmEnvMetrics[UserEnvironment.HostCpuArch] = GetHostCpuArchitecture;
                _realmEnvMetrics[UserEnvironment.TargetOsType] = _config.TargetOSName;
                _realmEnvMetrics[UserEnvironment.TargetCpuArch] = GetTargetCpuArchitecture(module);
                _realmEnvMetrics[UserEnvironment.FrameworkUsedInConjunction] = frameworkInfo.Name;
                _realmEnvMetrics[UserEnvironment.FrameworkUsedInConjunctionVersion] = frameworkInfo.Version;
                _realmEnvMetrics[UserEnvironment.LanguageVersion] = GetLanguageVersion(_config.TargetFramework);
                _realmEnvMetrics[UserEnvironment.RealmSdkVersion] = module.FindReference("Realm").Version.ToString();
                _realmEnvMetrics[UserEnvironment.CoreVersion] = "FILL ME";
                _realmEnvMetrics[UserEnvironment.SdkInstallationMethod] = "FILL ME";
                _realmEnvMetrics[UserEnvironment.IdeUsed] = "FILL ME";
                _realmEnvMetrics[UserEnvironment.NetFramework] = _config.TargetFramework;
                _realmEnvMetrics[UserEnvironment.NetFrameworkVersion] = _config.TargetFrameworkVersion;

                foreach (var type in module.Types)
                {
                    InternalAnalyzeSdkApi(type);
                }

                // We need to first analyze the features before we can set `IsSyncEnabled`.
                _realmFeaturesToAnalyze.TryGetValue(Feature.PartitionSyncConfiguration, out var isPbsUsed);
                _realmFeaturesToAnalyze.TryGetValue(Feature.FlexibleSyncConfiguration, out var isFlxSUsed);
                _realmEnvMetrics[UserEnvironment.IsSyncEnabled] = (isPbsUsed == 1 || isFlxSUsed == 1).ToString().ToLower();
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

                foreach (var propertyResult in type.Properties)
                {
                    var property = propertyResult.Property;

                    var key = property.PropertyType.Name;
                    if (!_classAnalysisSetters.ContainsKey(key) && property.PropertyType.MetadataType == MetadataType.Class)
                    {
                        key = "Class";
                    }

                    AnalyzeClassFeature(key, property);

                    foreach (var attribute in property.CustomAttributes)
                    {
                        AnalyzeClassFeature(attribute.AttributeType.Name, property);
                    }
                }
            }

            void AnalyzeClassFeature(string key, PropertyDefinition property)
            {
                if (_classAnalysisSetters.TryGetValue(key, out var featureFunc))
                {
                    var (shouldDelete, keyToSet) = featureFunc(property);

                    if (!string.IsNullOrEmpty(keyToSet))
                    {
                        _realmFeaturesToAnalyze[keyToSet] = 1;
                    }

                    if (shouldDelete)
                    {
                        _classAnalysisSetters.Remove(key);
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

            if (type.IsIAsymmetricObjectImplementor(_references) || type.IsAsymmetricObjectDescendant(_references))
            {
                _realmFeaturesToAnalyze[Feature.IAsymmetricObject] = 1;
            }
            else if (type.IsIEmbeddedObjectImplementor(_references) || type.IsEmbeddedObjectDescendant(_references))
            {
                _realmFeaturesToAnalyze[Feature.IEmbeddedObject] = 1;
            }

            AnalyzeClassMethods(type);

            foreach (var innerType in type.NestedTypes)
            {
                InternalAnalyzeSdkApi(innerType);
            }
        }

        private void AnalyzeClassMethods(TypeDefinition type)
        {
            string[] prefixes = new[] { "get_", "set_", "add_" };

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

                    var (prefix, index) = prefixes.Select(p => (p, key.IndexOf(p, StringComparison.Ordinal)))
                        .OrderByDescending(p => p.Item2)
                        .First();

                    if (index > -1)
                    {
                        // when dealing with:
                        // set_ShouldCompactOnLaunch
                        // add_RealmChanged
                        // add_PropertyChanged
                        // get_DynamicApi
                        key = key.Substring(prefix.Length);
                    }

                    if (!_apiAnalysisSetters.ContainsKey(key) &&
                        cil.Operand is MethodReference methodReference &&
                        methodReference.ReturnType.DeclaringType != null)
                    {
                        // when dealing with ThreadSafeReference
                        // TODO nikola: why?
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
                AppendKeyValue(kvp.Key, kvp.Value);
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
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(4);
            await httpClient.GetAsync(new Uri(prefixAddr + payload + suffixAddr));
        }

        private static bool IsInRealmNamespace(object operand)
        {
            if (operand is not MemberReference memberReference)
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
