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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using RealmWeaver;

namespace Analytics
{
    extern alias realm;
    using Metric = realm::RealmWeaver.Metric;
    using SdkFeature = realm::RealmWeaver.Metric.SdkFeature;

    [TestFixture]
    internal class Tests : WeaverTestBase
    {
        private static readonly Dictionary<string, string> _featureMap = new Dictionary<string, string>()
        {
            [SdkFeature.IEmbeddedObject] = "EMBEDDED_OBJECT",
            [SdkFeature.IAsymmetricObject] = "ASYMMETRIC_OBJECT",
            [SdkFeature.ReferenceList] = "REFERENCE_LIST",
            [SdkFeature.PrimitiveList] = "PRIMITIVE_LIST",
            [SdkFeature.ReferenceDictionary] = "REFERENCE_DICTIONARY",
            [SdkFeature.PrimitiveDictionary] = "PRIMITIVE_DICTIONARY",
            [SdkFeature.ReferenceSet] = "REFERENCE_SET",
            [SdkFeature.PrimitiveSet] = "PRIMITIVE_SET",
            [SdkFeature.RealmInteger] = "REALM_INTEGER",
            [SdkFeature.RealmObjectReference] = "REALM_OBJECT_REFERENCE",
            [SdkFeature.RealmValue] = "REALM_VALUE",
            [SdkFeature.BacklinkAttribute] = "BACKLINK",
            [SdkFeature.GetInstanceAsync] = "GET_INSTANCE_ASYNC",
            [SdkFeature.GetInstance] = "GET_INSTANCE",
            //[SdkFeature.NOT_SUPPORTED_YET] = "NOT_SUPPORTED_YET",
            [SdkFeature.Find] = "FIND",
            [SdkFeature.WriteAsync] = "WRITE_ASYNC",
            [SdkFeature.ThreadSafeReference] = "THREAD_SAFE_REFERENCE",
            [SdkFeature.Add] = "ADD_UPDATE",
            [SdkFeature.ShouldCompactOnLaunch] = "SHOULD_COMPACT_ON_LAUNCH",
            [SdkFeature.MigrationCallback] = "MIGRATION_CALLBACK",
            [SdkFeature.RealmChanged] = "REALM_CHANGED",
            [SdkFeature.ListSubscribeForNotifications] = "LIST_SUBSCRIBE_FOR_NOTIFICATIONS",
            [SdkFeature.SetSubscribeForNotifications] = "SET_SUBSCRIBE_FOR_NOTIFICATIONS",
            [SdkFeature.DictionarySubscribeForNotifications] = "DICTIONARY_SUBSCRIBE_FOR_NOTIFICATIONS",
            [SdkFeature.ResultSubscribeForNotifications] = "RESULT_SUBSCRIBE_FOR_NOTIFICATIONS",
            [SdkFeature.PropertyChanged] = "PROPERTY_CHANGED",
            [SdkFeature.RecoverOrDiscardUnsyncedChangesHandler] = "RECOVER_OR_DISCARD_UNSYNCED_CHANGES_HANDLER",
            [SdkFeature.RecoverUnsyncedChangesHandler] = "RECOVER_UNSYNCED_CHANGES_HANDLER",
            [SdkFeature.DiscardUnsyncedChangesHandler] = "DISCARD_UNSYNCED_CHANGES_HANDLER",
            [SdkFeature.ManualRecoveryHandler] = "MANUAL_RECOVERY_HANDLER",
            [SdkFeature.GetProgressObservable] = "GET_PROGRESS_OBSERVABLE",
            [SdkFeature.PartitionSyncConfiguration] = "PARTITION_SYNC_CONFIGURATION",
            [SdkFeature.FlexibleSyncConfiguration] = "FLEXIBLE_SYNC_CONFIGURATION",
            [SdkFeature.Anonymous] = "ANONYMOUS",
            [SdkFeature.EmailPassword] = "EMAIL_PASSWORD",
            [SdkFeature.Facebook] = "FACEBOOK",
            [SdkFeature.Google] = "GOOGLE",
            [SdkFeature.Apple] = "APPLE",
            [SdkFeature.JWT] = "JWT",
            [SdkFeature.ApiKey] = "API_KEY",
            [SdkFeature.ServerApiKey] = "SERVER_API_KEY",
            [SdkFeature.Function] = "FUNCTION",
            [SdkFeature.CallAsync] = "CALL_ASYNC",
            [SdkFeature.GetMongoClient] = "GET_MONGO_CLIENT",
            [SdkFeature.DynamicApi] = "DYNAMIC_API",
        };

        private static readonly Lazy<string[]> _frameworks = new Lazy<string[]>(() =>
        {
            var targetProject = Path.Combine(_analyticsAssemblyLocation.Value, "AnalyticsAssembly.csproj");
            var doc = XDocument.Parse(File.ReadAllText(targetProject));
            return doc.Descendants("TargetFrameworks").Single().Value.Split(';');
        });

        private static readonly Lazy<string> _analyticsAssemblyLocation = new Lazy<string>(() =>
        {
            var folder = Directory.GetCurrentDirectory();
            while (folder != null && !Directory.GetFiles(folder, "Realm.sln").Any())
            {
                folder = Path.GetDirectoryName(folder);
            }

            return Path.Combine(folder, "Tests", "Weaver", "AnalyticsAssembly");
        });

        // TODO andrea: a bunch of issues with this test:
        // 1. Unfortunately on CI we only run the weaver on Windows so other platforms aren't tested. So for this test
        //    to makes sense, we'd need to run it on multiple platforms when in CI
        // 2. If we want to keep this test, shouldn't we then have more tests around the environment?
        // 3. All this test is trying to do is: grab a method we know works (Environment.OSVersion.Platform)
        //    and keep comparing with the outcome of the collection. Currently the collection logic looks for the same
        //    information. My assumption was that this may not hold true in the future anymore, but I'm not convinced
        //    about this point: if we change the way in the collection, it may well be because this method is unreliable
        //    in which case we'd need to update this test to use the same method. Effectively, just mirroring what the collection
        //    does.
        [Test]
        public void ValidateCorrectPlatform()
        {
            try
            {
                CompileAnalyticsProject(_featureMap.FirstOrDefault().Value);

                var osType = Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => Metric.OperatingSystem.Windows,
                    PlatformID.MacOSX => Metric.OperatingSystem.MacOS,
                    PlatformID.Unix => Metric.OperatingSystem.Linux,
                    PlatformID id => $"{id} is an unsupported OS."
                };

                ValidateAnalyticsPayload(SdkFeature.HostOsType, osType);
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception: {e.Message}");
            }
        }

        [Test]
        public void ValidateFeatureUsage()
        {
            foreach (var kvp in _featureMap)
            {
                try
                {
                    CompileAnalyticsProject(kvp.Value);
                    ValidateSdkApiAnalyticsPayload(kvp.Key);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Exception: {e.Message}");
                }
            }
        }

        [Test]
        public void Submit_WhenDisabled_PayloadIsEmpty()
        {
            CompileAnalyticsProject();

            foreach (var framework in _frameworks.Value)
            {
                var response = WeaveRealm(framework, "Disabled");
                Assert.That(response, Is.EqualTo("Analytics disabled"), $"Analytics was not reported as disabled for framework {framework}.");
            }
        }

        private void ValidateAnalyticsPayload<T>(string featureName, T expectedResult)
        {
            foreach (var framework in _frameworks.Value)
            {
                try
                {
                    var response = WeaveRealm(framework, "DryRun");
                    var payload = BsonSerializer.Deserialize<BsonDocument>(response).AsBsonDocument;
                    Assert.That(payload[featureName].AsString, Is.EqualTo(expectedResult.ToString()), $"Feature {featureName} was not reported as used: {expectedResult} in {framework}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"For framework {framework} an exception was raised:\n{ex.Message}");
                }
            }
        }

        private void ValidateSdkApiAnalyticsPayload(string featureName, byte expectedUsed = 1)
        {
            ValidateAnalyticsPayload(featureName, expectedUsed);
        }

        private string WeaveRealm(string framework, string collectionType)
        {
            try
            {
                var assemblyPath = Path.Combine(_analyticsAssemblyLocation.Value, "bin", "Release", framework, "AnalyticsAssembly.dll");

                var payloadPath = Path.GetTempFileName();
                WeaveRealm(assemblyPath, XElement.Parse($"<Realm AnalyticsCollection=\"{collectionType}\" AnalyticsLogPath=\"{payloadPath}\"/>"));

                return File.ReadAllText(payloadPath);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception while weaving: {ex.Message}");
            }

            return string.Empty;
        }

        private static void CompileAnalyticsProject(params string[] constants)
        {
            var binPath = Path.Combine(_analyticsAssemblyLocation.Value, "bin");
            if (Directory.Exists(binPath))
            {
                Directory.Delete(binPath, recursive: true);
            }

            var targetProject = Path.Combine(_analyticsAssemblyLocation.Value, "AnalyticsAssembly.csproj");

            RunCommand("dotnet", $"build {targetProject} -p:AnalyticsConstants={string.Join(";", constants)} --configuration=Release");
        }

        private static void RunCommand(string command, string arguments)
        {
            var process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;

            // this, together with b) is only for debugging
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.RedirectStandardError = true;
            
            process.Start();

            // *** b)
            //var output = process.StandardOutput.ReadToEnd();
            //Console.WriteLine(output);
            //var err = process.StandardError.ReadToEnd();
            //Console.WriteLine(err);m

            process.WaitForExit();
        }
    }
}
