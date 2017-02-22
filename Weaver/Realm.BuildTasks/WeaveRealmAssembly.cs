////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace RealmBuildTasks
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public class WeaveRealmAssembly : Task
    {
        private const string NativeCallbackAttribute = "NativeCallbackAttribute";

        private DefaultAssemblyResolver _resolver;

        internal Action<string> LogDebug { get; set; }

        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string OutputDirectory { get; set; }

        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string AssemblyName { get; set; }

        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string IntermediateDirectory { get; set; }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public override bool Execute()
        {
            if (LogDebug == null)
            {
                LogDebug = message => BuildEngine?.LogMessageEvent(new BuildMessageEventArgs(
                                $"WeaveRealmAssembly: {message}",
                                string.Empty,
                                "BuildTasks",
                                MessageImportance.Normal));
            }

            AssemblyDefinition currentAssembly;
            _resolver = new DefaultAssemblyResolver();
            _resolver.AddSearchDirectory(IntermediateDirectory);
            _resolver.ResolveFailure += (sender, reference) =>
            {
                return _resolver.Resolve("mscorlib");
            };

            if (!TryReadAssembly(AssemblyName, out currentAssembly, isRealmAssembly: false))
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;

            WeaveAssembly("Realm", currentAssembly);
            WeaveAssembly("Realm.Sync", currentAssembly);

            return true;
        }

        private void WeaveAssembly(string name, AssemblyDefinition currentAssembly)
        {
            AssemblyDefinition assemblyToWeave;
            if (!currentAssembly.MainModule.AssemblyReferences.Any(a => a.Name == name) ||
                !TryReadAssembly(name, out assemblyToWeave))
            {
                if (currentAssembly.MainModule.AssemblyReferences.Any(r => r.Name == name))
                {
                    // Assembly not found, not weaving it will produce subtle runtime bugs, so it's safer to throw.
                    throw new Exception($"{name} is referenced by your project but was not found in {OutputDirectory}. Make sure that CopyLocal is set to True and try again.");
                }

                LogDebug($"Current assembly doesn't reference {name}. Skipping.");
                return;
            }

            LogDebug($"Weaving {name}");

            var assemblyToWeave = AssemblyDefinition.ReadAssembly(path);

            var targetFramework = currentAssembly.GetAttribute(typeof(TargetFrameworkAttribute).Name);
            var frameworkName = new FrameworkName((string)targetFramework.ConstructorArguments.Single().Value);

            var hasWovenChanges = false;
            switch (frameworkName.Identifier)
            {
                case "Xamarin.iOS":
                    WeaveMonoPInvoke(currentAssembly.MainModule, assemblyToWeave);
                    WeaveDllImport(assemblyToWeave, "__Internal");
                    hasWovenChanges = true;
                    break;
            }

            if (hasWovenChanges)
            {
                assemblyToWeave.Write(assemblyToWeave.MainModule.FullyQualifiedName);
                LogDebug($"Woven {assemblyToWeave.Name} for {frameworkName.Identifier}");
            }
            else
            {
                LogDebug($"No changes woven for {assemblyToWeave.Name}");
            }
        }

        private void WeaveMonoPInvoke(ModuleDefinition mainModule, AssemblyDefinition assemblyToWeave)
        {
            var xamariniOSAssemlby = mainModule.AssemblyReferences.Single(r => r.Name == "Xamarin.iOS");
            var monoPInvokeCallbackAttribute = new TypeReference("ObjCRuntime", "MonoPInvokeCallbackAttribute", mainModule, xamariniOSAssemlby);
            var system_Type = new TypeReference("System", "Type", mainModule, mainModule.TypeSystem.Corlib);
            var monoPInvokeAttribute_Constructor = new MethodReference(".ctor", mainModule.TypeSystem.Void, monoPInvokeCallbackAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(system_Type) }
            };

            var monoPInvokeAttribute_ConstructorRef = assemblyToWeave.MainModule.Import(monoPInvokeAttribute_Constructor);

            var classes = assemblyToWeave.MainModule.GetTypes();
            var callbackMethods = classes.Select(c => c.Methods.Where(m => m.HasAttribute(NativeCallbackAttribute)))
                                         .SelectMany(m => m);

            foreach (var method in callbackMethods)
            {
                var nativeCallbackAttribute = method.GetAttribute(NativeCallbackAttribute);
                var actualNativeCallbackAttribute = new CustomAttribute(monoPInvokeAttribute_ConstructorRef);
                actualNativeCallbackAttribute.ConstructorArguments.Add(nativeCallbackAttribute.ConstructorArguments[0]);

                method.CustomAttributes.Add(actualNativeCallbackAttribute);
                method.CustomAttributes.Remove(nativeCallbackAttribute);
            }
        }

        private void WeaveDllImport(AssemblyDefinition assemblyToWeave, string dllName)
        {
            var dllImportModule = assemblyToWeave.MainModule.ModuleReferences.SingleOrDefault(r => r.Name == "realm-wrappers");
            if (dllImportModule != null)
            {
                dllImportModule.Name = dllName;
            }
        }

        private bool TryReadAssembly(string name, out AssemblyDefinition assembly, bool isRealmAssembly = true)
        {
            var path = Path.Combine(isRealmAssembly ? OutputDirectory : IntermediateDirectory, $"{name}.{(isRealmAssembly ? "dll" : "exe")}");
            if (File.Exists(path))
            {
                assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters
                {
                    AssemblyResolver = _resolver
                });

                return true;
            }

            assembly = null;
            return false;
        }

        private string GetAssemblyPath(string name, bool isRealmAssembly = true)
        {
            return Path.Combine(isRealmAssembly ? OutputDirectory : IntermediateDirectory, $"{name}.{(isRealmAssembly ? "dll" : "exe")}");
        }

        // Invoked when looking for Cecil
        private Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            LogDebug("Resolving: " + args.Name);

            var location = typeof(WeaveRealmAssembly).Assembly.Location;
            var folder = Path.GetDirectoryName(location);

            try
            {
                return Assembly.LoadFile(Path.Combine(folder, args.Name + ".dll"));
            }
            catch
            {
                return null;
            }
        }
    }
}
