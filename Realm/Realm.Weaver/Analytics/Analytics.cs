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
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

using static RealmWeaver.AnalyticsUtils;
using static RealmWeaver.Metric.SdkFeature;

namespace RealmWeaver
{
    // TODO andrea: review and update this comment
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
    // features and deprecating old features. Collecting an anonymized assembly name &
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
    // - What version of Realm is being used
    // - What OS you are running on
    // - What OS you are building for
    // - An anonymized MAC address and assembly name ID to aggregate the other information on.
    internal class Analytics
    {
        private readonly ImportedReferences _references;

        #region FeatureDiciontaries

        private Dictionary<string, byte> _realmFeaturesToAnalyse = new Dictionary<string, byte>()
        {
            [IEmbeddedObject] = 0,
            [IAsymmetricObject] = 0,
            [ReferenceList] = 0,
            [PrimitiveList] = 0,
            [ReferenceDictionary] = 0,
            [PrimitiveDictionary] = 0,
            [ReferenceSet] = 0,
            [PrimitiveSet ] = 0,
            [RealmInteger ] = 0,
            [RealmObjectReference ] = 0,
            [RealmValue ] = 0,
            [GetInstanceAsync] = 0,
            [GetInstance] = 0,
            [NOT_SUPPORTED_YET] = 0,
            [Find] = 0,
            [WriteAsync] = 0,
            [ThreadSafeReference] = 0,
            [FIXME_TWO] = 0,
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

        private Dictionary<string, string> _realmEnvMetrics = new Dictionary<string, string>()
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
            { Framework, string.Empty },
            { FrameworkVersion, string.Empty },
        };

        private Dictionary<string, Func<Instruction, Dictionary<string, byte>, ImportedReferences, bool>> _apiAnalysisSetters = new Dictionary<string, Func<Instruction, Dictionary<string, byte>, ImportedReferences, bool>>()
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
                nameof(FIXME_TWO),  (instruction, featureDict, references) =>
                {
                    featureDict[FIXME_TWO] = 1;
                    return true;
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
                    if (IsFromNamespace(instruction.Operand, "Realms.Sync.FunctionsClient"))
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

        private Dictionary<string, Func<IMemberDefinition, Dictionary<string, byte>, ImportedReferences, bool>> _classAnalysisSetters = new Dictionary<string, Func<IMemberDefinition, Dictionary<string, byte>, ImportedReferences, bool>>()
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
                // TODO andrea: I'm not sure if there's a better way to look for classes
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

                    return featureDict[PrimitiveList] == 1 &&
                        featureDict[ReferenceList] == 1;
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

                    return featureDict[PrimitiveDictionary] == 1 &&
                        featureDict[ReferenceDictionary] == 1;
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

                    return featureDict[PrimitiveSet] == 1 &&
                        featureDict[ReferenceSet] == 1;
                }
            },
            {
                "RealmInteger`1", (member, featureDict, references) =>
                {
                    featureDict[RealmInteger] = 1;
                    return true;
                }
            },
        };

        #endregion

        private readonly Config _config;

        internal Analytics(Config config, ImportedReferences references)
        {
            _references = references;
            _config = config;
        }

        private void AnalyzeRealmClass(TypeDefinition type)
        {
            if (_classAnalysisSetters.ContainsKey(nameof(IAsymmetricObject)) &&
                (type.IsIAsymmetricObjectImplementor(_references) ||
                type.IsAsymmetricObjectDescendant(_references)))
            {
                if (_classAnalysisSetters[nameof(IAsymmetricObject)].Invoke(type, _realmFeaturesToAnalyse, _references))
                {
                    _classAnalysisSetters.Remove(nameof(IAsymmetricObject));
                }
            }
            else if (_classAnalysisSetters.ContainsKey(nameof(IEmbeddedObject)) &&
                (type.IsIEmbeddedObjectImplementor(_references) ||
                type.IsEmbeddedObjectDescendant(_references)))
            {
                if (_classAnalysisSetters[nameof(IEmbeddedObject)].Invoke(type, _realmFeaturesToAnalyse, _references))
                {
                    _classAnalysisSetters.Remove(nameof(IEmbeddedObject));
                }
            }

            foreach (var property in type.Properties)
            {
                var key = property.PropertyType.Name;
                if (!_classAnalysisSetters.ContainsKey(key))
                {
                    // when looking for "Class" type
                    key = property.PropertyType.MetadataType.ToString();
                }

                if (_classAnalysisSetters.ContainsKey(key))
                {
                    if (_classAnalysisSetters[key].Invoke(property, _realmFeaturesToAnalyse, _references))
                    {
                        // if the byte is set, remove the entry from the dict to avoid unnecessary work
                        _classAnalysisSetters.Remove(key);
                    }
                }
            }
        }

        internal void AnalyzeUserAssembly(ModuleDefinition module)
        {
            try
            {
                // collect environment details
                ComputeHostOSNameAndVersion(out var osType, out var osVersion);

                _realmEnvMetrics[UserId] = AnonymizedUserID;
                _realmEnvMetrics[ProjectId] = SHA256Hash(Encoding.UTF8.GetBytes(module.Name));
                _realmEnvMetrics[HostOsType] = osType;
                _realmEnvMetrics[HostOsVersion] = osVersion;
                _realmEnvMetrics[HostCpuArch] = GetHostCpuArchitecture;
                _realmEnvMetrics[TargetOsType] = _config.TargetOSName;
                //_realmEnvMetrics[TargetOsMinimumVersion] = TODO: WHAT TO WRITE HERE?;
                _realmEnvMetrics[TargetOsVersion] = _config.TargetOSVersion;
                _realmEnvMetrics[TargetCpuArch] = GetTargetCpuArchitecture(module);

                // TODO andrea: need to find msbuild properties and not custom attributes
                //_realmEnvMetrics[LanguageVersion] = module.Assembly.CustomAttributes
                //    .Where(a => a.ToString() == "LangVersion").SingleOrDefault().ToString();
                
                //_realmEnvMetrics[Framework] = ;
                _realmEnvMetrics[RealmSdkVersion] = module.FindReference("Realm").Version.ToString();

                foreach (var type in module.Types)
                {
                    InternalAnalyzeSdkApi(type);
                }
            }
            catch (Exception e)
            {
                ErrorLog($"Could not analyze the user's assembly.{Environment.NewLine}{e.Message}");
            }
        }

        private void InternalAnalyzeSdkApi(TypeDefinition type)
        {
            if (!type.IsClass)
            {
                return;
            }

            if (IsValidRealmType(type, _references))
            {
                AnalyzeRealmClass(type);
            }

            AnalyzeTypeMethods(type);

            foreach (var innerType in type.NestedTypes)
            {
                InternalAnalyzeSdkApi(innerType);
            }
        }

        internal string SubmitAnalytics()
        {
            try
            {
                if (_config.AnalyticsCollection == AnalyticsCollection.Disabled)
                {
                    return "Analytics disabled";
                }

                var pretty = false;
                var sendAddr = "https://data.mongodb-api.com/app/realmsdkmetrics-zmhtm/endpoint/metric_webhook/metric?data=";
#if DEBUG
                pretty = true;
                sendAddr = "https://eu-central-1.aws.data.mongodb-api.com/app/realmmetricscollection-acxca/endpoint/realm_metrics/debug_route?data=";
#endif
                var payload = GetJsonPayload(pretty);

                if (_config.AnalyticsCollection != AnalyticsCollection.DryRun)
                {
                    var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
                    SendRequest(sendAddr, base64Payload, string.Empty);
                }

                return payload;
            }
            catch (Exception e)
            {
                ErrorLog($"Could not submit analytics.{Environment.NewLine}{e.Message}");
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

        private static void SendRequest(string prefixAddr, string payload, string suffixAddr)
        {
            var request = System.Net.HttpWebRequest.CreateHttp(new Uri(prefixAddr + payload + suffixAddr));
            request.Method = "GET";
            request.Timeout = 4000;
            request.ReadWriteTimeout = 2000;
            request.GetResponse();
        }

        private void AnalyzeTypeMethods(TypeDefinition type)
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

                    if (_apiAnalysisSetters.ContainsKey(key))
                    {
                        if (_apiAnalysisSetters[key].Invoke(cil, _realmFeaturesToAnalyse, _references))
                        {
                            _apiAnalysisSetters.Remove(key);
                        }
                    }
                }
            }
        }

        private static bool IsValidRealmType(TypeDefinition type, ImportedReferences references) =>
            type.IsRealmObjectDescendant(references) || type.IsIRealmObjectBaseImplementor(references);

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

            public string TargetOSVersion { get; set; }

            public string Framework { get; set; }

            public bool IsUsingSync { get; set; }

            public string ModuleName { get; set; }

            public string FrameworkVersion { get; set; }
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
