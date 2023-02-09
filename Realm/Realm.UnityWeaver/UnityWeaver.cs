////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static RealmWeaver.Analytics;
using OperatingSystem = RealmWeaver.Metric.OperatingSystem;

namespace RealmWeaver
{
    // Heavily influenced by https://github.com/ExtendRealityLtd/Malimbe and https://github.com/fody/fody
    public class UnityWeaver : IPostBuildPlayerScriptDLLs, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string EnableAnalyticsPref = "realm_enable_analytics";
        private const string EnableAnalyticsMenuItemPath = "Tools/Realm/Enable build-time analytics";

        private const string WeaveEditorAssembliesPref = "realm_weave_editor_assemblies";
        private const string WeaveEditorAssembliesMenuItemPath = "Tools/Realm/Process editor assemblies";
        private const string UnityPackageName = "io.realm.unity";

        private static bool _analyticsEnabled;

        private static bool AnalyticsEnabled
        {
            get => _analyticsEnabled;
            set
            {
                _analyticsEnabled = value;
                EditorPrefs.SetBool(EnableAnalyticsPref, value);
                Menu.SetChecked(EnableAnalyticsMenuItemPath, value);
            }
        }

        private static bool _weaveEditorAssemblies;
        private static ListRequest _listRequest;
        private static TaskCompletionSource<string> _installMethodTask;

        private static bool WeaveEditorAssemblies
        {
            get => _weaveEditorAssemblies;
            set
            {
                _weaveEditorAssemblies = value;
                EditorPrefs.SetBool(WeaveEditorAssembliesPref, value);
                Menu.SetChecked(WeaveEditorAssembliesMenuItemPath, value);
            }
        }

        public int callbackOrder => 0;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            // We need to call that again after the editor is initialized to ensure that we populate the checkmark correctly.
            EditorApplication.delayCall += () =>
            {
                _listRequest = Client.List();
                _installMethodTask = new TaskCompletionSource<string>();
                EditorApplication.update += OnEditorApplicationUpdate;

                AnalyticsEnabled = EditorPrefs.GetBool(EnableAnalyticsPref, defaultValue: true);
                WeaveEditorAssemblies = EditorPrefs.GetBool(WeaveEditorAssembliesPref, defaultValue: false);
                WeaverAssemblyResolver.ApplicationDataPath = Application.dataPath;
                WeaveAssembliesOnEditorLaunch();
            };

            CompilationPipeline.assemblyCompilationFinished += (string assemblyPath, CompilerMessage[] _) =>
            {
                if (string.IsNullOrEmpty(assemblyPath))
                {
                    return;
                }

                var assembly = GetAssemblies().FirstOrDefault(p => p.outputPath == assemblyPath);

                if (assembly == null)
                {
                    return;
                }

                WeaveAssemblyCore(assemblyPath, assembly.allReferences, "Unity Editor", GetTargetOSName(Application.platform));
            };
        }

        private static void OnEditorApplicationUpdate()
        {
            if (_listRequest.IsCompleted)
            {
                var installMethod = Metric.Unknown();

                if (_listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in _listRequest.Result)
                    {
                        if (package.name == UnityPackageName)
                        {
                            installMethod = package.source switch
                            {
                                PackageSource.LocalTarball => "Manual",
                                PackageSource.Registry => "NPM",
                                _ => Metric.Unknown(package.source.ToString()),
                            };

                            break;
                        }
                    }
                }

                _installMethodTask.SetResult(installMethod);
                EditorApplication.update -= OnEditorApplicationUpdate;
            }
        }

        [MenuItem("Tools/Realm/Weave Assemblies")]
        public static async void WeaveAllAssembliesMenuItem()
        {
            var assembliesWoven = await WeaveAllAssemblies();
            UnityLogger.Instance.Info($"Weaving completed. {assembliesWoven} assemblies needed weaving.");
        }

        [MenuItem(EnableAnalyticsMenuItemPath)]
        public static void DisableAnalyticsMenuItem()
        {
            AnalyticsEnabled = !AnalyticsEnabled;
        }

        [MenuItem(WeaveEditorAssembliesMenuItemPath)]
        public static void WeaveEditorAssembliesMenuItem()
        {
            WeaveEditorAssemblies = !WeaveEditorAssemblies;

            // If we're switching weaving of editor assemblies on, we should re-weave all assemblies
            // to pick up the editor assemblies as well.
            if (WeaveEditorAssemblies)
            {
                WeaveAllAssembliesMenuItem();
            }
        }

        private static void WeaveAssembliesOnEditorLaunch()
        {
            // This code is susceptible to the year 2038 problem. Refactor before 2037!
            const string AutomaticWeavePrefKey = "realm_last_automatic_weave";
            var lastAutomaticWeave = EditorPrefs.GetInt(AutomaticWeavePrefKey, 0);
            var timeSinceLastWeave = (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(lastAutomaticWeave)).TotalSeconds;
            if (timeSinceLastWeave > EditorApplication.timeSinceStartup)
            {
                // We haven't executed the automatic weaver in this editor session
                _ = WeaveAllAssemblies();
                EditorPrefs.SetInt(AutomaticWeavePrefKey, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
        }

        private static async Task<int> WeaveAllAssemblies()
        {
            var assembliesWoven = 0;

            try
            {
                EditorApplication.LockReloadAssemblies();
                var assembliesToWeave = GetAssemblies();
                var weaveResults = new List<string>();
                await Task.Run(() =>
                {
                    foreach (var assembly in assembliesToWeave)
                    {
                        if (!WeaveAssemblyCore(assembly.outputPath, assembly.allReferences, "Unity Editor", GetTargetOSName(Application.platform)))
                        {
                            continue;
                        }

                        string sourceFilePath = assembly.sourceFiles.FirstOrDefault();
                        if (sourceFilePath == null)
                        {
                            continue;
                        }

                        weaveResults.Add(sourceFilePath);
                    }
                });

                foreach (var result in weaveResults)
                {
                    AssetDatabase.ImportAsset(result, ImportAssetOptions.ForceUpdate);
                    assembliesWoven++;
                }
            }
            catch (Exception ex)
            {
                UnityLogger.Instance.Error($"[Realm] Failed to weave assemblies. If the error persists, please report it to https://github.com/realm/realm-dotnet/issues: {ex}");
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                if (assembliesWoven > 0)
                {
                    AssetDatabase.Refresh();
                }
            }

            return assembliesWoven;
        }

        private static bool WeaveAssemblyCore(string assemblyPath, IEnumerable<string> references, string framework, string targetOSName)
        {
            var name = Path.GetFileNameWithoutExtension(assemblyPath);

            try
            {
                var timer = new Stopwatch();
                timer.Start();

                var resolutionResult = WeaverAssemblyResolver.Resolve(assemblyPath, references);
                if (resolutionResult == null)
                {
                    return false;
                }

                using (resolutionResult)
                {
                    // Unity doesn't add the [TargetFramework] attribute when compiling the assembly. However, it's
                    // using Mono, so we just hardcode Unity which is treated as Mono/.NET Framework by the weaver.
                    var weaver = new Weaver(resolutionResult.Module, UnityLogger.Instance, "Unity");

                    var analyticsEnabled = AnalyticsEnabled &&
                        Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") == null &&
                        Environment.GetEnvironmentVariable("CI") == null;

                    var analyticsConfig = new Config
                    {
                        TargetOSName = targetOSName,
                        TargetFrameworkVersion = Application.unityVersion,
                        TargetFramework = framework,
                        AnalyticsCollection = analyticsEnabled ? AnalyticsCollection.Full : AnalyticsCollection.Disabled,
                        InstallationMethod = _installMethodTask.Task.Result
                    };

                    var results = weaver.Execute(analyticsConfig);

                    if (results.ErrorMessage != null)
                    {
                        UnityLogger.Instance.Error($"[{name}] Weaving failed: {results}");
                        return false;
                    }

                    // Unity creates an entry in the build console for each item, so let's not pollute it.
                    if (results.SkipReason == null)
                    {
                        resolutionResult.SaveModuleUpdates();
                        UnityLogger.Instance.Info($"[{name}] Weaving completed in {timer.ElapsedMilliseconds} ms.{Environment.NewLine}{results}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityLogger.Instance.Error($"[{name}] Failed to weave assembly. If the error persists, please report it to https://github.com/realm/realm-dotnet/issues: {ex}");
            }

            return false;
        }

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            if (report == null)
            {
                return;
            }

            // This is a bit hacky - we need actual references, not directories, containing references, so we pass folder/dummy.dll
            // knowing that dummy.dll will be stripped.
            var systemAssemblies = CompilationPipeline.GetSystemAssemblyDirectories(ApiCompatibilityLevel.NET_Standard_2_0).Select(d => Path.Combine(d, "dummy.dll"));
            var referencePaths = systemAssemblies
                .Concat(report.files.Select(f => f.path))
                .ToArray();

            var assembliesToWeave = report.files.Where(f => f.role == "ManagedLibrary");
            var targetOS = GetTargetOSName(report.summary.platform);
            foreach (var file in assembliesToWeave)
            {
                WeaveAssemblyCore(file.path, referencePaths, "Unity", targetOS);
            }

            if (report.summary.platform == BuildTarget.iOS || report.summary.platform == BuildTarget.tvOS)
            {
                var realmAssemblyPath = report.files
                    .SingleOrDefault(r => "Realm.dll".Equals(Path.GetFileName(r.path), StringComparison.OrdinalIgnoreCase))
                    .path;

                var realmResolutionResult = WeaverAssemblyResolver.Resolve(realmAssemblyPath, referencePaths);
                using (realmResolutionResult)
                {
                    var wrappersReference = realmResolutionResult.Module.ModuleReferences.SingleOrDefault(r => r.Name == "realm-wrappers");
                    if (wrappersReference != null)
                    {
                        wrappersReference.Name = "__Internal";
                        realmResolutionResult.SaveModuleUpdates();
                    }
                }
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            switch (report?.summary.platform)
            {
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                    UpdateiOSFrameworks(false, false, report.summary.platform);
                    break;
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            bool enableForDevice;
            bool enableForSimulator;
            switch (report?.summary.platform)
            {
                case BuildTarget.iOS:
                    enableForDevice = PlayerSettings.iOS.sdkVersion == iOSSdkVersion.DeviceSDK;
                    enableForSimulator = PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK;
                    break;
                case BuildTarget.tvOS:
                    enableForDevice = PlayerSettings.tvOS.sdkVersion == tvOSSdkVersion.Device;
                    enableForSimulator = PlayerSettings.tvOS.sdkVersion == tvOSSdkVersion.Simulator;
                    break;
                default:
                    return;
            }

            UpdateiOSFrameworks(enableForDevice, enableForSimulator, report.summary.platform);
        }

        /// <summary>
        /// Updates the native module import config for the wrappers framework. Unity doesn't support
        /// xcframework, which means that it won't correctly include it when building for iOS. This is
        /// a somewhat hacky solution that will manually update the compatibilify flag and the AddToEmbeddedBinaries
        /// flag just for the slice that is compatible with the current build target (simulator or device).
        /// </summary>
        private static void UpdateiOSFrameworks(bool enableForDevice, bool enableForSimulator, BuildTarget buildTarget)
        {
            const string ErrorMessage = "Failed to find the native Realm framework at '{0}'. " +
                "Please double check that you have imported Realm correctly and that the file exists. " +
                "Typically, it should be located at Packages/io.realm.unity/Runtime/{1}";
            const string SimulatorPath = "Simulator";
            const string DevicePath = "Device";

            var importers = PluginImporter.GetAllImporters();

            UpdateiOSFramework(SimulatorPath, enableForSimulator);
            UpdateiOSFramework(DevicePath, enableForDevice);

            void UpdateiOSFramework(string path, bool enabled)
            {
                path = $"{buildTarget}/{path}/realm-wrappers.framework";
                var frameworkImporter = importers.SingleOrDefault(i => i.assetPath.Contains(path));
                if (frameworkImporter == null)
                {
                    throw new Exception(string.Format(ErrorMessage, path, buildTarget));
                }

                frameworkImporter.SetCompatibleWithPlatform(buildTarget, enabled);
                frameworkImporter.SetPlatformData(buildTarget, "AddToEmbeddedBinaries", enabled.ToString().ToLower());
            }
        }

        private static Assembly[] GetAssemblies()
        {
            if (WeaveEditorAssemblies)
            {
                return CompilationPipeline.GetAssemblies();
            }

            return CompilationPipeline.GetAssemblies(AssembliesType.Player);
        }

        private static string GetTargetOSName(BuildTarget target)
        {
            // These have to match Analytics.GetConfig(FrameworkName)
            return target switch
            {
                BuildTarget.StandaloneOSX => OperatingSystem.MacOS,
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => OperatingSystem.Windows,
                BuildTarget.iOS => OperatingSystem.Ios,
                BuildTarget.Android => OperatingSystem.Android,
                BuildTarget.StandaloneLinux64 => OperatingSystem.Linux,
                BuildTarget.tvOS => OperatingSystem.TvOs,
                _ => Metric.Unknown(target.ToString()),
            };
        }

        private static string GetTargetOSName(RuntimePlatform target)
        {
            return target switch
            {
                RuntimePlatform.WindowsEditor => OperatingSystem.Windows,
                RuntimePlatform.OSXEditor => OperatingSystem.MacOS,
                RuntimePlatform.LinuxEditor => OperatingSystem.Linux,
                _ => Metric.Unknown(target.ToString()),
            };
        }

        private class UnityLogger : ILogger
        {
            public static UnityLogger Instance { get; } = new UnityLogger();

            public void Debug(string message)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }

            public void Error(string message, SequencePoint sequencePoint = null)
            {
                UnityEngine.Debug.LogError(GetMessage(message, sequencePoint));
            }

            public void Info(string message)
            {
                UnityEngine.Debug.Log(message);
            }

            public void Warning(string message, SequencePoint sequencePoint = null)
            {
                UnityEngine.Debug.LogWarning(GetMessage(message, sequencePoint));
            }

            private static string GetMessage(string message, SequencePoint sp)
            {
                if (sp == null)
                {
                    return message;
                }

                return $"{sp.Document.Url}({sp.StartLine}, {sp.StartColumn}): {message}";
            }
        }
    }
}
