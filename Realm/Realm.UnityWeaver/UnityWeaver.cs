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
using System.Reflection;
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
using Assembly = UnityEditor.Compilation.Assembly;

// ReSharper disable once CheckNamespace
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

        private static readonly int UnityMajorVersion = int.Parse(Application.unityVersion.Split('.')[0]);
        private static readonly TaskCompletionSource<string> _installMethodTask = new();

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
        private static ListRequest? _listRequest;

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
            if (Application.isBatchMode)
            {
                // In batch mode, `update` won't get called until compilation is complete,
                // which means we'll deadlock when we block compilation on the tcs completing
                _installMethodTask.TrySetResult(Metric.Unknown());
            }

            // We need to call that again after the editor is initialized to ensure that we populate the checkmark correctly.
            EditorApplication.delayCall += () =>
            {
                _listRequest = Client.List();

                if (!Application.isBatchMode)
                {
                    EditorApplication.update += OnEditorApplicationUpdate;
                }

                AnalyticsEnabled = EditorPrefs.GetBool(EnableAnalyticsPref, defaultValue: true);
                WeaveEditorAssemblies = EditorPrefs.GetBool(WeaveEditorAssembliesPref, defaultValue: false);
                WeaverAssemblyResolver.ApplicationDataPath = Application.dataPath;
                WeaveAssembliesOnEditorLaunch();
            };

            CompilationPipeline.assemblyCompilationFinished += (assemblyPath, _) =>
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

                var config = GetAnalyticsConfig();
                WeaveAssemblyCore(assemblyPath, assembly.allReferences, config);
            };
        }

        private static void OnEditorApplicationUpdate()
        {
            if (_listRequest?.IsCompleted != true)
            {
                return;
            }

            EditorApplication.update -= OnEditorApplicationUpdate;

            var installMethod = Metric.Unknown();

            if (_listRequest.Status == StatusCode.Success)
            {
                var realmPackage = _listRequest.Result.FirstOrDefault(p => p.name == UnityPackageName);
                installMethod = realmPackage?.source switch
                {
                    PackageSource.LocalTarball => "Manual",
                    PackageSource.Registry => "NPM",
                    _ => Metric.Unknown(realmPackage?.source.ToString()),
                };
            }

            _installMethodTask.TrySetResult(installMethod);
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
            const string automaticWeavePrefKey = "realm_last_automatic_weave";
            var lastAutomaticWeave = EditorPrefs.GetInt(automaticWeavePrefKey, 0);
            var timeSinceLastWeave = (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(lastAutomaticWeave)).TotalSeconds;
            if (timeSinceLastWeave > EditorApplication.timeSinceStartup)
            {
                // We haven't executed the automatic weaver in this editor session
                _ = WeaveAllAssemblies();
                EditorPrefs.SetInt(automaticWeavePrefKey, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
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

                var config = GetAnalyticsConfig();

                await Task.Run(() =>
                {
                    foreach (var assembly in assembliesToWeave)
                    {
                        if (!WeaveAssemblyCore(assembly.outputPath, assembly.allReferences, config))
                        {
                            continue;
                        }

                        var sourceFilePath = assembly.sourceFiles.FirstOrDefault();
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

        private static bool WeaveAssemblyCore(string assemblyPath, IEnumerable<string> references, Config analyticsConfig)
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

            var files = GetFiles(report);

            // This is a bit hacky - we need actual references, not directories, containing references, so we pass folder/dummy.dll
            // knowing that dummy.dll will be stripped.
            var systemAssemblies = CompilationPipeline.GetSystemAssemblyDirectories(ApiCompatibilityLevel.NET_Standard_2_0).Select(d => Path.Combine(d, "dummy.dll"));
            var referencePaths = systemAssemblies
                .Concat(files.Select(f => f.path))
                .ToArray();

            var assembliesToWeave = files.Where(f => f.role == "ManagedLibrary");
            var config = GetAnalyticsConfig(report.summary.platform);

            foreach (var file in assembliesToWeave)
            {
                WeaveAssemblyCore(file.path, referencePaths, config);
            }

            if (report.summary.platform != BuildTarget.iOS && report.summary.platform != BuildTarget.tvOS)
            {
                return;
            }

            var realmAssemblyPath = files
                .SingleOrDefault(r => "Realm.dll".Equals(Path.GetFileName(r.path), StringComparison.OrdinalIgnoreCase))
                .path;

            var realmResolutionResult = WeaverAssemblyResolver.Resolve(realmAssemblyPath, referencePaths);
            if (realmResolutionResult == null)
            {
                return;
            }

            using (realmResolutionResult)
            {
                var wrappersReference = realmResolutionResult.Module.ModuleReferences.SingleOrDefault(r => r.Name == "realm-wrappers");
                if (wrappersReference == null)
                {
                    return;
                }

                wrappersReference.Name = "__Internal";
                realmResolutionResult.SaveModuleUpdates();
            }
        }

        public void OnPostprocessBuild(BuildReport? report)
        {
            if (UnityMajorVersion >= 6000) return;
            switch (report?.summary.platform)
            {
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                    UpdateiOSFrameworks(false, false, report.summary.platform);
                    break;
            }
        }

        public void OnPreprocessBuild(BuildReport? report)
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
        /// a somewhat hacky solution that will manually update the compatibility flag and the AddToEmbeddedBinaries
        /// flag just for the slice that is compatible with the current build target (simulator or device).
        /// </summary>
        private static void UpdateiOSFrameworks(bool enableForDevice, bool enableForSimulator, BuildTarget buildTarget)
        {
            const string errorMessage = "Failed to find the native Realm framework at '{0}'. " +
                "Please double check that you have imported Realm correctly and that the file exists. " +
                "Typically, it should be located at Packages/io.realm.unity/Runtime/{1}";
            const string simulatorPath = "Simulator";
            const string devicePath = "Device";

            var importers = PluginImporter.GetAllImporters();

            UpdateiOSFramework(simulatorPath, enableForSimulator);
            UpdateiOSFramework(devicePath, enableForDevice);

            void UpdateiOSFramework(string path, bool enabled)
            {
                path = $"{buildTarget}/{path}/realm-wrappers.framework";
                var frameworkImporter = importers.SingleOrDefault(i => i.assetPath.Contains(path));
                if (frameworkImporter == null)
                {
                    throw new Exception(string.Format(errorMessage, path, buildTarget));
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

        private static string GetTargetOSName(BuildTarget? target)
        {
            // target is null for editor builds - in that case, we return the current OS
            // as target.
            if (target == null)
            {
                return Application.platform switch
                {
                    RuntimePlatform.WindowsEditor => Metric.OperatingSystem.Windows,
                    RuntimePlatform.OSXEditor => Metric.OperatingSystem.MacOS,
                    RuntimePlatform.LinuxEditor => Metric.OperatingSystem.Linux,
                    _ => Metric.Unknown(Application.platform.ToString()),
                };
            }

            // These have to match Analytics.GetConfig(FrameworkName)
            return target switch
            {
                BuildTarget.StandaloneOSX => Metric.OperatingSystem.MacOS,
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 or BuildTarget.WSAPlayer => Metric.OperatingSystem.Windows,
                BuildTarget.iOS => Metric.OperatingSystem.Ios,
                BuildTarget.Android => Metric.OperatingSystem.Android,
                BuildTarget.StandaloneLinux64 => Metric.OperatingSystem.Linux,
                BuildTarget.tvOS => Metric.OperatingSystem.TvOs,
                BuildTarget.XboxOne => Metric.OperatingSystem.XboxOne,
                _ => Metric.Unknown(target.ToString()),
            };
        }

        private static string GetTargetOsVersion(BuildTarget? target)
        {
            // target is null for editor builds - in that case, we return the host OS
            // version.
            if (target == null)
            {
                return Environment.OSVersion.Version.ToString();
            }

            return target switch
            {
                BuildTarget.Android => ((int)PlayerSettings.Android.targetSdkVersion).ToString(),
                BuildTarget.iOS => PlayerSettings.iOS.targetOSVersionString,
                BuildTarget.tvOS => PlayerSettings.tvOS.targetOSVersionString,
                _ => Metric.Unknown(),
            };
        }

        private static BuildFile[] GetFiles(BuildReport report)
        {
            try
            {
                if (UnityMajorVersion < 2022)
                {
                    var getFilesPI = typeof(BuildReport).GetProperty("files", BindingFlags.Public | BindingFlags.Instance)!;
                    return (BuildFile[])getFilesPI.GetValue(report);
                }

                // Starting with 2022, BuildReport.files is replaced with BuildReport.GetFiles. This is a
                // bit hacky, but allows us to target both versions with the same assembly.
                var getFilesMI = typeof(BuildReport).GetMethod("GetFiles", BindingFlags.Public | BindingFlags.Instance)!;
                return (BuildFile[])getFilesMI.Invoke(report, null);
            }
            catch (Exception e)
            {
                UnityLogger.Instance.Error($"Failed to obtain list of files from report. Please report this error on https://github.com/realm/realm-dotnet/issues. Unity version: {Application.unityVersion}, error message: {e.Message}.");
                throw;
            }
        }

        private static string GetMinimumOsVersion(BuildTarget? target)
        {
            // target is null for editor builds - in that case, we return the host OS
            // version.
            if (target == null)
            {
                return Environment.OSVersion.Version.ToString();
            }

            return target switch
            {
                BuildTarget.Android => ((int)PlayerSettings.Android.minSdkVersion).ToString(),
                BuildTarget.iOS => PlayerSettings.iOS.targetOSVersionString,
                BuildTarget.tvOS => PlayerSettings.tvOS.targetOSVersionString,
                _ => Metric.Unknown(),
            };
        }

        private static Config GetAnalyticsConfig(BuildTarget? target = null)
        {
            var netFrameworkInfo = GetNetFrameworkInfo(target);
            var compiler = PlayerSettings.GetScriptingBackend(BuildPipeline.GetBuildTargetGroup(target ?? EditorUserBuildSettings.activeBuildTarget)).ToString();

            var analyticsEnabled = AnalyticsEnabled &&
                        Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") == null &&
                        Environment.GetEnvironmentVariable("CI") == null;

            return new(targetOSName: GetTargetOSName(target),
                compiler: compiler,
                netFrameworkTarget: netFrameworkInfo.Name,
                netFrameworkTargetVersion: netFrameworkInfo.Version,
                installationMethod: _installMethodTask.Task.Wait(1000) ? _installMethodTask.Task.Result : Metric.Unknown(),
                frameworkName: target == null ? Metric.Framework.UnityEditor : Metric.Framework.Unity,
                frameworkVersion: Application.unityVersion)
            {
                AnalyticsCollection = analyticsEnabled ? AnalyticsCollection.Full : AnalyticsCollection.Disabled,
                TargetArchitecture = GetCpuArchitecture(target),
                TargetOsVersion = GetTargetOsVersion(target),
                TargetOsMinimumVersion = GetMinimumOsVersion(target),
                ProjectId = PlayerSettings.productName,
            };
        }

        private static (string Name, string Version) GetNetFrameworkInfo(BuildTarget? buildTarget)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget ?? EditorUserBuildSettings.activeBuildTarget);
            var apiTarget = PlayerSettings.GetApiCompatibilityLevel(targetGroup);

            // these consts are exactly mapped to what .NET reports in any .NET application
            const string netStandardApi = ".NETStandard";
            const string netFrameworkApi = ".NETFramework";

            var unityVersion = new Version(Application.unityVersion.Substring(0, 6));

            // conversion necessary as after unity version 2021.1, entry NET_4_6 and NET_Standard_2_0
            // are actually representing .NET 4.8 and .NET Standard 2.1
            // https://github.com/Unity-Technologies/UnityCsReference/blob/664dfe30cee8ee2ef7dd8c5e9db6235915245ecb/Editor/Mono/PlayerSettings.bindings.cs#L158
            if (unityVersion >= new Version("2021.2"))
            {
                if (apiTarget == ApiCompatibilityLevel.NET_Standard_2_0)
                {
                    return (netStandardApi, "2.1");
                }

                if (apiTarget == ApiCompatibilityLevel.NET_4_6)
                {
                    return (netFrameworkApi, "4.8");
                }
            }

            if (apiTarget == ApiCompatibilityLevel.NET_Standard_2_0)
            {
                return (netStandardApi, "2.0");
            }

            if (apiTarget == ApiCompatibilityLevel.NET_4_6)
            {
                return (netFrameworkApi, "4.6");
            }

            // this should really never be the case
            return (apiTarget.ToString(), "");
        }

        private static string GetCpuArchitecture(BuildTarget? buildTarget)
        {
            // buildTarget is null when we're building for the editor
            if (buildTarget == null)
            {
                if (SystemInfo.processorType.IndexOf("ARM", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return Environment.Is64BitProcess ? Metric.CpuArchitecture.Arm64 : Metric.CpuArchitecture.Arm;
                }

                // Must be in the x86 family.
                return Environment.Is64BitProcess ? Metric.CpuArchitecture.X64 : Metric.CpuArchitecture.X86;
            }

            return buildTarget switch
            {
                BuildTarget.iOS or BuildTarget.tvOS => Metric.CpuArchitecture.Arm64,
                BuildTarget.StandaloneOSX => EditorUserBuildSettings.GetPlatformSettings(BuildPipeline.GetBuildTargetName(buildTarget.Value), "Architecture") switch
                {
                    "ARM64" => Metric.CpuArchitecture.Arm64,
                    "x64" => Metric.CpuArchitecture.X64,
                    _ => Metric.CpuArchitecture.Universal,
                },
                BuildTarget.StandaloneWindows => Metric.CpuArchitecture.X86,
                BuildTarget.Android => PlayerSettings.Android.targetArchitectures switch
                {
                    AndroidArchitecture.ARMv7 => Metric.CpuArchitecture.Arm,
                    AndroidArchitecture.ARM64 => Metric.CpuArchitecture.Arm64,

                    // These two don't have enum values in our Unity reference dll, but exist in newer versions
                    // See https://github.com/Unity-Technologies/UnityCsReference/blob/70abf502c521c169ee8a302aa48c5600fc7c39fc/Editor/Mono/PlayerSettingsAndroid.bindings.cs#L14
                    (AndroidArchitecture)(1 << 2) => Metric.CpuArchitecture.X86,
                    (AndroidArchitecture)(1 << 3) => Metric.CpuArchitecture.X64,
                    _ => Metric.CpuArchitecture.Universal,
                },
                BuildTarget.StandaloneWindows64 or BuildTarget.StandaloneLinux64 or BuildTarget.XboxOne => Metric.CpuArchitecture.X64,
                _ => Metric.Unknown(),
            };
        }

        private class UnityLogger : ILogger
        {
            public static UnityLogger Instance { get; } = new UnityLogger();

            public void Debug(string message)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }

            public void Error(string message, SequencePoint? sequencePoint = null)
            {
                UnityEngine.Debug.LogError(GetMessage(message, sequencePoint));
            }

            public void Info(string message)
            {
                UnityEngine.Debug.Log(message);
            }

            public void Warning(string message, SequencePoint? sequencePoint = null)
            {
                UnityEngine.Debug.LogWarning(GetMessage(message, sequencePoint));
            }

            private static string GetMessage(string message, SequencePoint? sp)
            {
                return sp == null ? message : $"{sp.Document.Url}({sp.StartLine}, {sp.StartColumn}): {message}";
            }
        }
    }
}
