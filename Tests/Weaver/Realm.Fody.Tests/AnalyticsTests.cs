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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using RealmWeaver;

namespace Analytics
{
    extern alias realm;
    using Metric = realm::RealmWeaver.Metric;

    [TestFixture]
    internal class Tests : WeaverTestBase
    {
        public static object[] Features = Metric.SdkFeatures.Keys.Select(f => new object[] { f, ConvertToUpperCase(f) }).ToArray();

        private static readonly Lazy<string[]> _frameworks = new(() =>
        {
            var targetProject = Path.Combine(_analyticsAssemblyLocation.Value, "AnalyticsAssembly.csproj");
            var doc = XDocument.Parse(File.ReadAllText(targetProject));
            return doc.Descendants("TargetFrameworks").Single().Value.Split(';');
        });

        private static readonly Lazy<string> _analyticsAssemblyLocation = new(() =>
        {
            var folder = Directory.GetCurrentDirectory();
            while (folder != null && !Directory.GetFiles(folder, "Realm.sln").Any())
            {
                folder = Path.GetDirectoryName(folder);
            }

            return Path.Combine(folder, "Tests", "Weaver", "AnalyticsAssembly");
        });

        private static string ConvertToUpperCase(string target)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append(target[0]);
            var charArray = target.ToCharArray(1, target.Length - 1);
            foreach (var c in charArray)
            {
                if (char.IsUpper(c))
                {
                    strBuilder.Append('_');
                }

                strBuilder.Append(char.ToUpper(c));
            }

            return strBuilder.ToString();
        }

        [TestCaseSource(nameof(Features))]
        public void ValidateFeatureUsage(string feature, string constant)
        {
            CompileAnalyticsProject(constant);
            ValidateAnalyticsPayloadAllFrameworks(new[] { (featureName: Metric.SdkFeatures[feature], expectedValue: "1") });
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

        [Test]
        public void ValidateHostOS()
        {
            var currentOs = Environment.OSVersion;
            CompileAnalyticsProject();
            ValidateAnalyticsPayloadAllFrameworks(new[] {
                (featureName: Metric.Environment.HostOsType, expectedValue: ConvertToMetricOS(currentOs.Platform)),
                (featureName: Metric.Environment.HostOsVersion, expectedValue: currentOs.Version.ToString()) });
        }

        [Test]
        public void ValidateEnvironmentMetrics()
        {
            // This test validates that all environment metrics (Metric.Environment) are set. It doesn't validate
            // the actual values, just that they exist in the payload.
            CompileAnalyticsProject();
            foreach (var framework in _frameworks.Value)
            {
                var response = WeaveRealm(framework, "DryRun");
                var payload = BsonSerializer.Deserialize<BsonDocument>(response).AsBsonDocument;

                var environmentMetrics = typeof(Metric.Environment).GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Select(f => (string)f.GetValue(null))
                    .ToArray();

                foreach (var metric in environmentMetrics)
                {
                    Assert.That(payload.Contains(metric), Is.True, $"Metric: {metric} not found in document: {response}");
                }
            }
        }

        [Test]
        public void Metirc_SdkFeatures_ContainsAllFeatureConstants()
        {
            var sdkFeatures = Metric.SdkFeatures;
            var featureConstants = typeof(Metric.Feature).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => (string)f.GetValue(null))
                .ToArray();

            foreach (var constant in featureConstants)
            {
                Assert.That(sdkFeatures.ContainsKey(constant), Is.True, $"Constant: {constant} is not present as a key in Metric.SdkFeatures");
            }
        }

        private static string ConvertToMetricOS(PlatformID platformID) =>
            platformID switch
            {
                PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.WinCE => Metric.OperatingSystem.Windows,
                PlatformID.MacOSX => Metric.OperatingSystem.MacOS,
                PlatformID.Unix => Metric.OperatingSystem.Linux,
                _ => platformID.ToString()
            };

        private void ValidateAnalyticsPayloadAllFrameworks((string featureName, string expectedValue)[] payloadFeatures)
        {
            foreach (var framework in _frameworks.Value)
            {
                var response = WeaveRealm(framework, "DryRun");
                var payload = BsonSerializer.Deserialize<BsonDocument>(response).AsBsonDocument;

                foreach (var (featureName, expectedValue) in payloadFeatures)
                {
                    Assert.That(payload[featureName].AsString, Is.EqualTo(expectedValue),
                        $"For framework {framework}, field \"{expectedValue}\" doesn't match the expected value \"{expectedValue}\"");
                }
            }
        }

        private string WeaveRealm(string framework, string collectionType)
        {
            try
            {
                var assemblyPath = Path.Combine(_analyticsAssemblyLocation.Value, "bin", "Release", framework, "AnalyticsAssembly.dll");

                var payloadPath = Path.GetTempFileName();
                WeaveRealm(assemblyPath, XElement.Parse($"<Realm AnalyticsCollection=\"{collectionType}\" AnalyticsLogPath=\"{payloadPath}\"/>"));

                var counter = 0;
                while (!File.Exists(payloadPath) || string.IsNullOrEmpty(File.ReadAllText(payloadPath)))
                {
                    if (counter++ > 5000)
                    {
                        throw new Exception($"File at {payloadPath} did not appear after 5000 ms");
                    }

                    Task.Delay(1).Wait();
                }

                // Make sure the file has been written completely.
                Task.Delay(10).Wait();

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
