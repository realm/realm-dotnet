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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
    // - An anonymized identifier and assembly name ID to aggregate the other information on.
    internal class Analytics
    {
        // The value of this field is modified by CI in the "prepare-release" action, so do not change its name.
        private const string CoreVersion = "13.15.0";

        private readonly ImportedReferences _references;
        private readonly ILogger _logger;

        private readonly Dictionary<string, string> _realmEnvMetrics = new();
        private readonly Dictionary<string, byte> _realmFeaturesToAnalyze;

        private readonly Dictionary<string, Func<Instruction, FeatureAnalysisResult>> _apiAnalysisSetters;

        private readonly Dictionary<string, Func<IMemberDefinition, FeatureAnalysisResult>> _classAnalysisSetters;

        private readonly Config _config;

        private readonly Task _analyzeUserAssemblyTask;

        public Analytics(Config config, ImportedReferences references, ILogger logger, ModuleDefinition module)
        {
            _config = config;
            _references = references;
            _logger = logger;

            if (config.AnalyticsCollection == AnalyticsCollection.Disabled)
            {
                _realmFeaturesToAnalyze = new();
                _classAnalysisSetters = new();
                _apiAnalysisSetters = new();
                _analyzeUserAssemblyTask = Task.CompletedTask;
                return;
            }

            _realmFeaturesToAnalyze = Metric.SdkFeatures.Keys.ToDictionary(c => c, _ => (byte)0);

            _classAnalysisSetters = new()
            {
                ["Class"] = member =>
                    member is PropertyDefinition property && property.PropertyType.IsAnyRealmObject(_references) ?
                    new(true, Feature.RealmObjectReference) : default,
                [Feature.RealmValue] = _ => new(true, Feature.RealmValue),
                ["IList`1"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveList, Feature.ReferenceList),
                ["IDictionary`2"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveDictionary, Feature.ReferenceDictionary, 1),
                ["ISet`1"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveSet, Feature.ReferenceSet),
                ["RealmInteger`1"] = _ => new(true, Feature.RealmInteger),
                [Feature.BacklinkAttribute] = _ => new(true, Feature.BacklinkAttribute)
            };

            _apiAnalysisSetters = new()
            {
                [Feature.GetInstanceAsync] = instruction => AnalyzeRealmApi(instruction, Feature.GetInstanceAsync),
                [Feature.GetInstance] = instruction => AnalyzeRealmApi(instruction, Feature.GetInstance),
                [Feature.Find] = instruction => AnalyzeRealmApi(instruction, Feature.Find),
                [Feature.WriteAsync] = instruction => AnalyzeRealmApi(instruction, Feature.WriteAsync),
                [Feature.ThreadSafeReference] = instruction => AnalyzeRealmApi(instruction, Feature.ThreadSafeReference),

                // check if it's the right signature, that is 2 params in total of which
                // the second a bool and that it's set to true.
                [Feature.Add] = instruction =>
                    IsInRealmNamespace(instruction.Operand) &&
                    instruction.Operand is MethodSpecification methodSpecification &&
                    methodSpecification.Parameters.Count == 2 &&
                    methodSpecification.Parameters[1].ParameterType.MetadataType == MetadataType.Boolean &&
                    instruction.Previous.OpCode == OpCodes.Ldc_I4_1 ?
                    new(true, Feature.Add) : default,
                [Feature.ShouldCompactOnLaunch] = _ => new(true, Feature.ShouldCompactOnLaunch),
                [Feature.MigrationCallback] = _ => new(true, Feature.MigrationCallback),
                [Feature.RealmChanged] = _ => new(true, Feature.RealmChanged),
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

                    return new(shouldDelete, key);
                },
                ["PropertyChanged"] = instruction =>
                {
                    string? key = null;
                    if (instruction.Operand is MemberReference reference)
                    {
                        if (reference.DeclaringType.IsAnyRealmObject(_references))
                        {
                            key = Feature.ObjectNotification;
                        }
                        else if (reference.DeclaringType.IsSameAs(_references.SyncSession))
                        {
                            key = Feature.ConnectionNotification;
                        }
                    }

                    if (key == null)
                    {
                        return default;
                    }

                    var shouldDelete = ContainsAllRelatedFeatures(key, Feature.ObjectNotification, Feature.ConnectionNotification);
                    return new(shouldDelete, key);
                },
                [Feature.RecoverOrDiscardUnsyncedChangesHandler] = _ => new(true, Feature.RecoverOrDiscardUnsyncedChangesHandler),
                [Feature.RecoverUnsyncedChangesHandler] = _ => new(true, Feature.RecoverUnsyncedChangesHandler),
                [Feature.DiscardUnsyncedChangesHandler] = _ => new(true, Feature.DiscardUnsyncedChangesHandler),
                [Feature.ManualRecoveryHandler] = _ => new(true, Feature.ManualRecoveryHandler),
                [Feature.GetProgressObservable] = _ => new(true, Feature.GetProgressObservable),
                [Feature.PartitionSyncConfiguration] = _ => new(true, Feature.PartitionSyncConfiguration),
                [Feature.FlexibleSyncConfiguration] = _ => new(true, Feature.FlexibleSyncConfiguration),
                [Feature.Anonymous] = instruction => AnalyzeRealmApi(instruction, Feature.Anonymous),
                [Feature.EmailPassword] = instruction => AnalyzeRealmApi(instruction, Feature.EmailPassword),
                [Feature.Facebook] = instruction => AnalyzeRealmApi(instruction, Feature.Facebook),
                [Feature.Google] = instruction => AnalyzeRealmApi(instruction, Feature.Google),
                [Feature.Apple] = instruction => AnalyzeRealmApi(instruction, Feature.Apple),
                [Feature.JWT] = instruction => AnalyzeRealmApi(instruction, Feature.JWT),
                [Feature.ApiKey] = instruction => AnalyzeRealmApi(instruction, Feature.ApiKey),
                [Feature.Function] = instruction => AnalyzeRealmApi(instruction, Feature.Function),
                [Feature.CallAsync] = instruction => AnalyzeRealmApi(instruction, Feature.CallAsync),
                [Feature.GetMongoClient] = _ => new(true, Feature.GetMongoClient),
                [Feature.DynamicApi] = _ => new(true, Feature.DynamicApi)
            };

            _analyzeUserAssemblyTask = Task.Run(() =>
            {
                AnalyzeUserAssembly(module);
            });

            FeatureAnalysisResult AnalyzeCollectionProperty(IMemberDefinition member, string primitiveKey, string referenceKey, int genericArgIndex = 0)
            {
                if (member is not PropertyDefinition property ||
                    property.PropertyType is not GenericInstanceType genericType ||
                    genericType.GenericArguments.Count < genericArgIndex + 1)
                {
                    return default;
                }

                var keyToAdd = genericType.GenericArguments[genericArgIndex].IsPrimitive ?
                    primitiveKey : referenceKey;

                var shouldDelete = ContainsAllRelatedFeatures(keyToAdd, referenceKey, primitiveKey);
                return new(shouldDelete, keyToAdd);
            }

            FeatureAnalysisResult AnalyzeRealmApi(Instruction instruction, string key)
            {
                if (IsInRealmNamespace(instruction.Operand))
                {
                    return new(true, key);
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

        private void AnalyzeUserAssembly(ModuleDefinition module)
        {
            try
            {
                // collect environment details
                _realmEnvMetrics[UserEnvironment.UserId] = AnonymizedUserId;
                _realmEnvMetrics[UserEnvironment.LegacyUserId] = GetLegacyAnonymizedUserId();
                _realmEnvMetrics[UserEnvironment.ProjectId] = SHA256Hash(Encoding.UTF8.GetBytes(_config.ProjectId ?? module.Assembly.Name.Name));
                _realmEnvMetrics[UserEnvironment.RealmSdk] = "dotnet";
                _realmEnvMetrics[UserEnvironment.RealmSdkVersion] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                _realmEnvMetrics[UserEnvironment.Language] = "c#";
                _realmEnvMetrics[UserEnvironment.LanguageVersion] = InferLanguageVersion(_config.NetFrameworkTarget, _config.NetFrameworkTargetVersion);
                _realmEnvMetrics[UserEnvironment.HostOsType] = GetHostOsName();
                _realmEnvMetrics[UserEnvironment.HostOsVersion] = Environment.OSVersion.Version.ToString();
                _realmEnvMetrics[UserEnvironment.HostCpuArch] = GetHostCpuArchitecture();
                _realmEnvMetrics[UserEnvironment.TargetOsType] = _config.TargetOSName;
                _realmEnvMetrics[UserEnvironment.TargetCpuArch] = _config.TargetArchitecture;
                _realmEnvMetrics[UserEnvironment.TargetOsVersion] = _config.TargetOsVersion;
                _realmEnvMetrics[UserEnvironment.TargetOsMinimumVersion] = _config.TargetOsMinimumVersion;
                _realmEnvMetrics[UserEnvironment.CoreVersion] = CoreVersion;
                _realmEnvMetrics[UserEnvironment.FrameworkUsedInConjunction] = _config.FrameworkName;
                _realmEnvMetrics[UserEnvironment.FrameworkUsedInConjunctionVersion] = _config.FrameworkVersion;
                _realmEnvMetrics[UserEnvironment.SdkInstallationMethod] = _config.InstallationMethod;
                _realmEnvMetrics[UserEnvironment.NetFramework] = _config.NetFrameworkTarget;
                _realmEnvMetrics[UserEnvironment.NetFrameworkVersion] = _config.NetFrameworkTargetVersion;
                _realmEnvMetrics[UserEnvironment.Compiler] = _config.Compiler;

                foreach (var type in module.Types.ToArray())
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

        public void AnalyzeRealmClassProperties(WeaveTypeResult[] types)
        {
            if (_config.AnalyticsCollection == AnalyticsCollection.Disabled)
            {
                return;
            }

            _analyzeUserAssemblyTask.Wait();

            foreach (var type in types)
            {
                if (type.Properties == null)
                {
                    continue;
                }

                foreach (var propertyResult in type.Properties.Where(p => p.Woven))
                {
                    var property = propertyResult.Property;

                    var key = property!.PropertyType.Name;
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
                    var analysisResult = featureFunc(property);

                    if (!string.IsNullOrEmpty(analysisResult.DictKey))
                    {
                        _realmFeaturesToAnalyze[analysisResult.DictKey] = 1;
                    }

                    if (analysisResult.ShouldDelete)
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
            var prefixes = new[] { "get_", "set_", "add_" };

            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                {
                    continue;
                }

                foreach (var cil in method.Body.Instructions)
                {
                    var key = (cil.Operand as MemberReference)?.Name;
                    if (key.IsNullOrEmpty())
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
                        key = key[prefix.Length..];
                    }

                    if (!_apiAnalysisSetters.ContainsKey(key) &&
                        cil.Operand is MethodReference methodReference &&
                        methodReference.ReturnType.DeclaringType != null)
                    {
                        // when dealing with ThreadSafeReference
                        key = methodReference.ReturnType.DeclaringType.Name;
                    }

                    if (!_apiAnalysisSetters.ContainsKey(key) && key == ".ctor")
                    {
                        key = ((MemberReference)cil.Operand).DeclaringType.Name;
                    }

                    if (!_apiAnalysisSetters.TryGetValue(key, out var featureFunc))
                    {
                        continue;
                    }

                    var analysisResult = featureFunc.Invoke(cil);

                    if (!string.IsNullOrEmpty(analysisResult.DictKey))
                    {
                        _realmFeaturesToAnalyze[analysisResult.DictKey] = 1;
                    }

                    if (analysisResult.ShouldDelete)
                    {
                        _apiAnalysisSetters.Remove(key);
                    }
                }
            }
        }

        public async Task SubmitAnalytics()
        {
            var payload = "Analytics disabled";

            if (_config.AnalyticsCollection != AnalyticsCollection.Disabled)
            {
                // this is necessary since when not in the assembly that has the models
                // AnalyzeRealmClassProperties won't be called
                _analyzeUserAssemblyTask.Wait();

                try
                {
                    const string sendAddr = "https://data.mongodb-api.com/app/realmsdkmetrics-zmhtm/endpoint/v2/metric?data=";
                    payload = GetJsonPayload();

                    if (_config.AnalyticsCollection != AnalyticsCollection.DryRun)
                    {
                        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
                        await SendRequest(sendAddr, base64Payload, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    payload = e.Message;
                }
            }

            if (!_config.AnalyticsLogPath.IsNullOrEmpty())
            {
                File.WriteAllText(_config.AnalyticsLogPath, payload);
            }
        }

        private string GetJsonPayload()
        {
            var jsonPayload = new StringBuilder();

            jsonPayload.Append('{');

            AppendKeyValues(_realmEnvMetrics);
            jsonPayload.Append(',');
            AppendKeyValues(_realmFeaturesToAnalyze, Metric.SdkFeatures);
            jsonPayload.Append('}');

            return jsonPayload.ToString();

            void AppendKeyValues<TValue>(IDictionary<string, TValue> dict, IDictionary<string, string>? keyMapping = null)
            {
                var mapping = dict
                    .Select(kvp =>
                    {
                        if (kvp.Value is byte and 0 ||
                            (kvp.Value is string s && string.IsNullOrEmpty(s)))
                        {
                            // skip empty strings/0
                            return null;
                        }

                        var key = keyMapping == null ? kvp.Key : keyMapping[kvp.Key];
                        var value = kvp.Value is string ? $"\"{kvp.Value}\"" : $"{kvp.Value}";
                        return $"\"{key}\":{value}";
                    })
                    .Where(s => s != null);

                jsonPayload.Append(string.Join(",", mapping));
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
            public AnalyticsCollection AnalyticsCollection { get; init; }

            public string? AnalyticsLogPath { get; init; }

            required public string TargetOSName { get; init; }

            required public string NetFrameworkTarget { get; init; }

            required public string NetFrameworkTargetVersion { get; init; }

            required public string InstallationMethod { get; init; }

            required public string FrameworkName { get; init; }

            required public string FrameworkVersion { get; init; }

            required public string Compiler { get; init; }

            // These are only available on Unity for now.
            public string TargetArchitecture { get; init; } = Metric.Unknown();

            public string TargetOsVersion { get; init; } = Metric.Unknown();

            public string TargetOsMinimumVersion { get; init; } = Metric.Unknown();

            public string? ProjectId { get; init; }
        }

        public enum AnalyticsCollection
        {
            Disabled,
            DryRun,
            Full,
        }

        private readonly struct FeatureAnalysisResult
        {
            public bool ShouldDelete { get; }

            public string DictKey { get; }

            public FeatureAnalysisResult(bool isToDelete = false, string dictKey = "")
            {
                ShouldDelete = isToDelete;
                DictKey = dictKey;
            }
        }
    }
}
