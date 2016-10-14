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
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

internal static class RealmAssemblyWeaver
{
    internal static void PrepareRealmAssemblyForIos(AssemblyDefinition realmAssembly, IAssemblyResolver assemblyResolver)
    {
        UpdateWrappersReference(realmAssembly, "__Internal");

        var iOSModule = assemblyResolver.Resolve("Xamarin.iOS").MainModule;
        var linkWithAttributeType = iOSModule.GetTypes().First(t => t.Name == "LinkWithAttribute");

        var ctorDefinition = linkWithAttributeType.GetConstructors().Single(c => c.Parameters.Count == 3);
        var ctorReference = realmAssembly.MainModule.ImportReference(ctorDefinition);
        realmAssembly.MainModule.ImportReference(ctorReference);
        var linkWithAttribute = new CustomAttribute(ctorReference);
        linkWithAttribute.ConstructorArguments.Add(new CustomAttributeArgument(iOSModule.TypeSystem.String, "libwrappers.a"));

        var linkTargetDefinition = iOSModule.GetTypes().Single(t => t.Name == "LinkTarget");
        var linkTargetType = iOSModule.ImportReference(linkTargetDefinition);

        var linkTargetValue = 0;
        foreach (var platform in new[] { "ArmV7", "ArmV7s", "Arm64", "Simulator", "Simulator64" })
        {
            linkTargetValue |= (int)linkTargetDefinition.Fields.Single(f => f.Name == platform).Constant;
        }

        linkWithAttribute.ConstructorArguments.Add(new CustomAttributeArgument(iOSModule.TypeSystem.Int32, linkTargetValue));
        linkWithAttribute.ConstructorArguments.Add(new CustomAttributeArgument(iOSModule.TypeSystem.String, "-lstdc++ -lz"));

        // TODO: linkWith.SmartLink = true

        realmAssembly.CustomAttributes.Add(linkWithAttribute);

        var callbackAttributeType = iOSModule.GetTypes().Single(t => t.Name == "MonoPInvokeCallbackAttribute");
        var monoPInvokeCallbackConstructor = callbackAttributeType.GetConstructors().First();
        var monoPInvokeCallbackConstructorRef = realmAssembly.MainModule.ImportReference(monoPInvokeCallbackConstructor);

        var classes = realmAssembly.MainModule.GetTypes();
        foreach (var method in classes.Select(c => c.Methods.Where(m => m.CustomAttributes.Any(a => a.AttributeType.Name == "NativeCallbackAttribute"))).SelectMany(methods => methods))
        {
            var monoPInvokeCallbackAttribute = new CustomAttribute(monoPInvokeCallbackConstructorRef);
            monoPInvokeCallbackAttribute.ConstructorArguments.Add(method.CustomAttributes.Single(a => a.AttributeType.Name == "NativeCallbackAttribute").ConstructorArguments[0]);
            method.CustomAttributes.Add(monoPInvokeCallbackAttribute);
        }

        var outputFolder = System.IO.Path.GetDirectoryName(realmAssembly.MainModule.FullyQualifiedName);
        var wrappersPath = System.IO.Path.Combine(outputFolder, "libwrappers.a");

        var wrappersLibBinary = System.IO.File.ReadAllBytes(wrappersPath);
        var wrappersLib = new EmbeddedResource("libwrappers.a", ManifestResourceAttributes.Public, wrappersLibBinary);
        realmAssembly.MainModule.Resources.Add(wrappersLib);

        realmAssembly.Write(realmAssembly.MainModule.FullyQualifiedName);
    }

    internal static void PrepareRealmAssemblyForOsx(AssemblyDefinition realmAssembly)
    {
    }

    internal static void PrepareRealmAssemblyForAndroid(AssemblyDefinition realmAssembly)
    {
        UpdateWrappersReference(realmAssembly, "wrappers");

        realmAssembly.Write(realmAssembly.MainModule.FullyQualifiedName);
    }

    internal static void PrepareRealmAssemblyForWindows(AssemblyDefinition realmAssembly)
    {
        var is64Bit = false;
        var isDebug = true;

        UpdateWrappersReference(realmAssembly, "wrappers" + (is64Bit ? "x64" : "x86") + "-" + (isDebug ? "Debug" : "Release"));

        realmAssembly.Write(realmAssembly.MainModule.FullyQualifiedName);
    }

    private static void UpdateWrappersReference(AssemblyDefinition assemblyToUpdate, string wrappersName)
    {
        var wrappers = new ModuleReference(wrappersName);
        assemblyToUpdate.MainModule.ModuleReferences.Add(wrappers);

        assemblyToUpdate.MainModule.ModuleReferences.Add(wrappers);
        var classes = assemblyToUpdate.MainModule.GetTypes(); ////.Where(t => t.Name == "NativeMethods");
        foreach (var method in classes.Select(c => c.Methods.Where(m => m.HasPInvokeInfo)).SelectMany(methods => methods))
        {
            method.PInvokeInfo.Module = wrappers;
        }
    }
}
