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

    [TestFixture]
    internal class Tests : WeaverTestBase
    {
        private static readonly Dictionary<string, string> _featureMap = new Dictionary<string, string>()
        {
            ["Sync Enabled"] = "CREATE_LEGACY_SYNC_CONFIG",
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

        [Test]
        public void ValidateFeatureUsage()
        {
            foreach (var kvp in _featureMap)
            {
                CompileAnalyticsProject(kvp.Value);
                ValidateAnalyticsPayload(kvp.Key);
            }
        }

        [Test]
        public void Submit_WhenDisabled_PayloadIsEmpty()
        {
            CompileAnalyticsProject();

            foreach (var framework in _frameworks.Value)
            {
                var response = WeaveRealm(framework, "Disabled");

                Assert.That(response, Is.EqualTo("Analytics disabled"));
            }
        }

        private void ValidateAnalyticsPayload(string featureName, bool expectedUsed = true)
        {
            foreach (var framework in _frameworks.Value)
            {
                var response = WeaveRealm(framework, "DryRun");

                var payload = BsonSerializer.Deserialize<BsonDocument>(response)["properties"].AsBsonDocument;

                Assert.That(payload[featureName].AsString, Is.EqualTo(expectedUsed.ToString()), $"Feature {featureName} was not reported as used: {expectedUsed} in {framework}");
            }
        }

        private string WeaveRealm(string framework, string collectionType)
        {
            var assemblyPath = Path.Combine(_analyticsAssemblyLocation.Value, "bin", "Release", framework, "AnalyticsAssembly.dll");

            var payloadPath = Path.GetTempFileName();
            WeaveRealm(assemblyPath, XElement.Parse($"<Realm AnalyticsCollection=\"{collectionType}\" AnalyticsLogPath=\"{payloadPath}\"/>"));

            return File.ReadAllText(payloadPath);
        }

        private static void CompileAnalyticsProject(params string[] constants)
        {
            Directory.Delete(Path.Combine(_analyticsAssemblyLocation.Value, "bin"), recursive: true);

            var targetProject = Path.Combine(_analyticsAssemblyLocation.Value, "AnalyticsAssembly.csproj");

            Process.Start("dotnet", $"build {targetProject} -p:AnalyticsConstants={string.Join(";", constants)} --configuration=Release").WaitForExit();
        }
    }
}
