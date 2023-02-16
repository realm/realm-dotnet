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
        // The value of this field is modified by CI in the "prepare-release" action, so do not change its name.
        private const string _coreVersion = "13.1.2";

        private readonly ImportedReferences _references;
        private readonly ILogger _logger;

        private readonly Dictionary<string, string> _realmEnvMetrics = new();

        private readonly Dictionary<string, byte> _realmFeaturesToAnalyze;

        private readonly Dictionary<string, Func<Instruction, ActionDriver>> _apiAnalysisSetters;

        private readonly Dictionary<string, Func<IMemberDefinition, ActionDriver>> _classAnalysisSetters;

        private readonly Config _config;

        private Task _analyzeUserAssemblyTask;

        public Analytics(Config config, ImportedReferences references, ILogger logger, ModuleDefinition module)
        {
            _config = config;

            if (config.AnalyticsCollection == AnalyticsCollection.Disabled)
            {
                return;
            }

            _references = references;
            _logger = logger;

            _realmFeaturesToAnalyze = Metric.SdkFeatures.Keys.ToDictionary(c => c, _ => (byte)0);

            _classAnalysisSetters = new()
            {
                ["Class"] = member =>
                    member is PropertyDefinition property &&
                    (property.PropertyType.IsIRealmObjectBaseImplementor(_references) ||
                    property.PropertyType.IsRealmObjectDescendant(_references)) ?
                    new ActionDriver(true, Feature.RealmObjectReference) : new ActionDriver(),
                [Feature.RealmValue] = member => new ActionDriver(true, Feature.RealmValue),
                ["IList`1"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveList, Feature.ReferenceList),
                ["IDictionary`2"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveDictionary, Feature.ReferenceDictionary, 1),
                ["ISet`1"] = member => AnalyzeCollectionProperty(member, Feature.PrimitiveSet, Feature.ReferenceSet),
                ["RealmInteger`1"] = member => new ActionDriver(true, Feature.RealmInteger),
                [Feature.BacklinkAttribute] = member => new ActionDriver(true, Feature.BacklinkAttribute)
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
                    new ActionDriver(true, Feature.Add) : new ActionDriver(),
                [Feature.ShouldCompactOnLaunch] = instruction => new ActionDriver(true, Feature.ShouldCompactOnLaunch),
                [Feature.MigrationCallback] = instruction => new ActionDriver(true, Feature.MigrationCallback),
                [Feature.RealmChanged] = instruction => new ActionDriver(true, Feature.RealmChanged),
                ["SubscribeForNotifications"] = instruction =>
                {
                    if (instruction.Operand is not MethodSpecification methodSpecification || !IsInRealmNamespace(instruction.Operand))
                    {
                        return new ActionDriver();
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

                    return new ActionDriver(shouldDelete, key);
                },
                [Feature.PropertyChanged] = instruction => new ActionDriver(true, Feature.PropertyChanged),
                [Feature.RecoverOrDiscardUnsyncedChangesHandler] = instruction => new ActionDriver(true, Feature.RecoverOrDiscardUnsyncedChangesHandler),
                [Feature.RecoverUnsyncedChangesHandler] = instruction => new ActionDriver(true, Feature.RecoverUnsyncedChangesHandler),
                [Feature.DiscardUnsyncedChangesHandler] = instruction => new ActionDriver(true, Feature.DiscardUnsyncedChangesHandler),
                [Feature.ManualRecoveryHandler] = instruction => new ActionDriver(true, Feature.ManualRecoveryHandler),
                [Feature.GetProgressObservable] = instruction => new ActionDriver(true, Feature.GetProgressObservable),
                [Feature.PartitionSyncConfiguration] = instruction => new ActionDriver(true, Feature.PartitionSyncConfiguration),
                [Feature.FlexibleSyncConfiguration] = instruction => new ActionDriver(true, Feature.FlexibleSyncConfiguration),
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
                [Feature.GetMongoClient] = instruction => new ActionDriver(true, Feature.GetMongoClient),
                [Feature.DynamicApi] = instruction => new ActionDriver(true, Feature.DynamicApi)
            };

            _analyzeUserAssemblyTask = Task.Run(() =>
            {
                AnalyzeUserAssembly(module);
            });

            ActionDriver AnalyzeCollectionProperty(IMemberDefinition member, string primitiveKey, string referenceKey, int genericArgIndex = 0)
            {
                if (member is not PropertyDefinition property ||
                    property.PropertyType is not GenericInstanceType genericType ||
                    genericType.GenericArguments.Count < genericArgIndex + 1)
                {
                    return new ActionDriver();
                }

                var keyToAdd = genericType.GenericArguments[genericArgIndex].IsPrimitive ?
                    primitiveKey : referenceKey;

                var shouldDelete = ContainsAllRelatedFeatures(keyToAdd, referenceKey, primitiveKey);
                return new ActionDriver(shouldDelete, keyToAdd);
            }

            ActionDriver AnalyzeRealmApi(Instruction instruction, string key)
            {
                if (IsInRealmNamespace(instruction.Operand))
                {
                    return new ActionDriver(true, key);
                }

                return new ActionDriver();
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
                var frameworkInfo = GetFrameworkAndVersion(module, _config);

                _realmEnvMetrics[UserEnvironment.UserId] = GetAnonymizedUserId();
                _realmEnvMetrics[UserEnvironment.ProjectId] = SHA256Hash(Encoding.UTF8.GetBytes(module.Name));
                _realmEnvMetrics[UserEnvironment.RealmSdk] = ".NET";
                _realmEnvMetrics[UserEnvironment.Language] = "C#";
                _realmEnvMetrics[UserEnvironment.HostOsType] = GetHostOsName();
                _realmEnvMetrics[UserEnvironment.HostOsVersion] = Environment.OSVersion.Version.ToString();
                _realmEnvMetrics[UserEnvironment.HostCpuArch] = GetHostCpuArchitecture();
                _realmEnvMetrics[UserEnvironment.TargetOsType] = _config.TargetOSName;
                _realmEnvMetrics[UserEnvironment.TargetCpuArch] = GetTargetCpuArchitecture(module);
                _realmEnvMetrics[UserEnvironment.FrameworkUsedInConjunction] = frameworkInfo.Name;
                _realmEnvMetrics[UserEnvironment.FrameworkUsedInConjunctionVersion] = frameworkInfo.Version;
                _realmEnvMetrics[UserEnvironment.LanguageVersion] = GetLanguageVersion(_config.TargetFramework);
                _realmEnvMetrics[UserEnvironment.RealmSdkVersion] = module.FindReference("Realm").Version.ToString();
                _realmEnvMetrics[UserEnvironment.CoreVersion] = _coreVersion;
                _realmEnvMetrics[UserEnvironment.SdkInstallationMethod] = _config.InstallationMethod;
                _realmEnvMetrics[UserEnvironment.IdeUsed] = "FILL ME";
                _realmEnvMetrics[UserEnvironment.NetFramework] = _config.TargetFramework;
                _realmEnvMetrics[UserEnvironment.NetFrameworkVersion] = _config.TargetFrameworkVersion;

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
                    var actionDriver = featureFunc(property);

                    if (!string.IsNullOrEmpty(actionDriver.DictKey))
                    {
                        _realmFeaturesToAnalyze[actionDriver.DictKey] = 1;
                    }

                    if (actionDriver.IsToDelete)
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
                        key = methodReference.ReturnType.DeclaringType.Name;
                    }

                    if (!_apiAnalysisSetters.ContainsKey(key) && key == ".ctor")
                    {
                        key = ((MemberReference)cil.Operand).DeclaringType.Name;
                    }

                    if (_apiAnalysisSetters.TryGetValue(key, out var featureFunc))
                    {
                        var actionDriver = featureFunc.Invoke(cil);

                        if (!string.IsNullOrEmpty(actionDriver.DictKey))
                        {
                            _realmFeaturesToAnalyze[actionDriver.DictKey] = 1;
                        }

                        if (actionDriver.IsToDelete)
                        {
                            _apiAnalysisSetters.Remove(key);
                        }
                    }
                }
            }
        }

        public async Task SubmitAnalytics()
        {
            var payload = "Analytics disabled";

            if (_config.AnalyticsCollection != AnalyticsCollection.Disabled)
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
                    payload = GetJsonPayload(pretty);

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

            if (!string.IsNullOrEmpty(_config.AnalyticsLogPath))
            {
                File.WriteAllText(_config.AnalyticsLogPath, payload);
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

            public string InstallationMethod { get; set; }
        }

        public enum AnalyticsCollection
        {
            Disabled,
            DryRun,
            Full,
        }

        private class ActionDriver
        {
            public bool IsToDelete { get; }

            public string DictKey { get; }

            public ActionDriver(bool isToDelete = false, string dictKey = "")
            {
                IsToDelete = isToDelete;
                DictKey = dictKey;
            }
        }
    }
}
