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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace BuildTasks
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public class WeaveRealmAssembly : Task
    {
        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string OutputDirectory { get; set; }

        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string AssemblyName { get; set; }

        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string OutputType { get; set; }

        [Required]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public string IntermediateDirectory { get; set; }

        private string OutputExtension
        {
            get
            {
                switch (OutputType)
                {
                    case "Exe":
                        return "exe";
                    default:
                        return "dll";
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        public override bool Execute()
        {
            AssemblyDefinition currentAssembly;
            if (!TryReadAssembly(AssemblyName, out currentAssembly, OutputExtension, IntermediateDirectory))
            {
                return false;
            }

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
                // Assembly not found, nothing to weave
                return;
            }

            var targetFramework = currentAssembly.CustomAttributes.Single(a => a.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);
            var frameworkName = new FrameworkName((string)targetFramework.ConstructorArguments.Single().Value);
            switch (frameworkName.Identifier)
            {
                case "Xamarin.iOS":
                    if (OutputType == "exe")
                    {
                        // We only want to weave when building the application for Xamarin.iOS.
                        WeaveiOSAssembly(currentAssembly, assemblyToWeave);
                    }

                    break;
            }
        }

        private void WeaveiOSAssembly(AssemblyDefinition currentAssembly, AssemblyDefinition assemblyToWeave)
        {
            var xamariniOSAssemlby = currentAssembly.MainModule.AssemblyReferences.Single(r => r.Name == "Xamarin.iOS");
            var monoPInvokeCallbackAttribute = new TypeReference("ObjCRuntime", "MonoPInvokeCallbackAttribute", currentAssembly.MainModule, xamariniOSAssemlby);
            var system_Type = new TypeReference("System", "Type", currentAssembly.MainModule, currentAssembly.MainModule.TypeSystem.CoreLibrary);
            var monoPInvokeAttribute_Constructor = new MethodReference(".ctor", currentAssembly.MainModule.TypeSystem.Void, monoPInvokeCallbackAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(system_Type) }
            };

            var monoPInvokeAttribute_ConstructorRef = assemblyToWeave.MainModule.ImportReference(monoPInvokeAttribute_Constructor);

            var classes = assemblyToWeave.MainModule.GetTypes();
            var callbackMethods = classes.Select(c => c.Methods.Where(m => m.CustomAttributes.Any(a => a.AttributeType.Name == "NativeCallbackAttribute")))
                                         .SelectMany(m => m);

            foreach (var method in callbackMethods)
            {
                var nativeCallbackAttribute = method.CustomAttributes.Single(a => a.AttributeType.Name == "NativeCallbackAttribute");
                var actualNativeCallbackAttribute = new CustomAttribute(monoPInvokeAttribute_ConstructorRef);
                actualNativeCallbackAttribute.ConstructorArguments.Add(nativeCallbackAttribute.ConstructorArguments[0]);

                method.CustomAttributes.Add(actualNativeCallbackAttribute);
                method.CustomAttributes.Remove(nativeCallbackAttribute);
            }

            assemblyToWeave.Write(assemblyToWeave.MainModule.FullyQualifiedName);

            LogDebug($"Woven {assemblyToWeave.Name} for Xamarin.iOS");
        }

        private void LogDebug(string message)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                $"WeaveRealmAssembly: {message}",
                string.Empty,
                "BuildTasks",
                MessageImportance.Normal));
        }

        private bool TryReadAssembly(string name, out AssemblyDefinition assembly, string extension = "dll", string folder = null)
        {
            var path = Path.Combine(folder ?? OutputDirectory, $"{name}.{extension}");
            if (File.Exists(path))
            {
                assembly = AssemblyDefinition.ReadAssembly(path);
                return true;
            }

            assembly = null;
            return false;
        }
    }
}
