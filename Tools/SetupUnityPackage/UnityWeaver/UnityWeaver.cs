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
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;

namespace RealmWeaver
{
    // Heavily influenced by https://github.com/ExtendRealityLtd/Malimbe and https://github.com/fody/fody
    public class UnityWeaver : IPostBuildPlayerScriptDLLs
    {
        public int callbackOrder => 0;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            CompilationPipeline.assemblyCompilationFinished += (string assemblyPath, CompilerMessage[] _) =>
            {
                if (string.IsNullOrEmpty(assemblyPath))
                {
                    return;
                }

                var assembly = CompilationPipeline.GetAssemblies(AssembliesType.Player)
                                      .FirstOrDefault(p => p.outputPath == assemblyPath);

                if (assembly == null)
                {
                    return;
                }

                WeaveAssemblyCore(assemblyPath, assembly.allReferences);
            };

            WeaveAssembliesOnEditorLaunch();
        }

        [MenuItem("Realm/Weave Assemblies")]
        public static async void WeaveAllAssembliesMenuItem()
        {
            var assembliesWoven = await WeaveAllAssemblies();
            UnityLogger.Instance.Info($"Weaving completed. {assembliesWoven} assemblies needed weaving.");
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

                var weavingTasks = CompilationPipeline.GetAssemblies(AssembliesType.Player)
                    .Select(assembly => Task.Run(() =>
                    {
                        if (!WeaveAssemblyCore(assembly.outputPath, assembly.allReferences))
                        {
                            return null;
                        }

                        string sourceFilePath = assembly.sourceFiles.FirstOrDefault();
                        if (sourceFilePath == null)
                        {
                            return null;
                        }

                        return sourceFilePath;
                    }));

                var weaveResults = await Task.WhenAll(weavingTasks);

                foreach (var result in weaveResults.Where(r => r != null))
                {
                    AssetDatabase.ImportAsset(result, ImportAssetOptions.ForceUpdate);
                    assembliesWoven++;
                }
            }
            catch (Exception ex)
            {
                UnityLogger.Instance.Error($"Failed to weave assemblies: {ex}");
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

        private static bool WeaveAssemblyCore(string assemblyPath, IEnumerable<string> references)
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
                    // using NETStandard2, so we just hardcode this.
                    var weaver = new Weaver(resolutionResult.Module, UnityLogger.Instance, new FrameworkName(".NETStandard,Version=v2.0"));
                    var results = weaver.Execute();

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
                UnityLogger.Instance.Warning($"[{name}] Weaving failed: {ex}");
            }

            return false;
        }

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            // This is a bit hacky - we need actual references, not directories, containing references, so we pass folder/dummy.dll
            // knowing that dummy.dll will be stripped.
            var systemAssemblies = CompilationPipeline.GetSystemAssemblyDirectories(ApiCompatibilityLevel.NET_Standard_2_0).Select(d => Path.Combine(d, "dummy.dll"));
            var referencePaths = systemAssemblies
                .Concat(report.files.Select(f => f.path))
                .ToArray();

            var assembliesToWeave = report.files.Where(f => f.role == "ManagedLibrary");
            foreach (var file in assembliesToWeave)
            {
                WeaveAssemblyCore(file.path, referencePaths);
            }

            if (report.summary.platform == BuildTarget.iOS)
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
