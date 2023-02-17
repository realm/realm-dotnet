////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SetupUnityPackage
{
    [Verb("realm", HelpText = "Setup the Realm package")]
    internal class RealmOptions : OptionsBase
    {
        [Option("packages-path", Required = true, HelpText = "Path to the folder containing Realm.nupkg, Realm.UnityUtils.nupkg, and Realm.UnityWeaver.nupkg to use.")]
        public string PackagesPath { get; set; } = null!;

        [Option("pack", Default = false, Required = false, HelpText = "Specify whether to invoke npm version + npm pack to produce a .tgz")]
        public bool Pack { get; set; }

        [Option("skip-validation", Default = false, Required = false, HelpText = "Specify whether to skip validating the list of dependencies. Only used when include-dependencies is true.")]
        public bool SkipValidation { get; set; }

        public override string PackageBasePath => Path.Combine(Helpers.SolutionFolder, "Realm", "Realm.Unity");

        public override ISet<string> IgnoredDependencies { get; } = new HashSet<string>
        {
            "Realm.SourceGenerator",
            "Microsoft.CSharp",
            "Realm.Fody",
            "Fody",
            "System.Dynamic.Runtime",
        };

        private static readonly IEnumerable<DependencyInfo> _realmDependencies = new[]
        {
            new DependencyInfo("MongoDB.Bson", "lib/netstandard2.0/MongoDB.Bson.dll"),
            new DependencyInfo("Remotion.Linq", "lib/netstandard1.0/Remotion.Linq.dll"),
            new DependencyInfo("System.Runtime.CompilerServices.Unsafe", "lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll"),
            new DependencyInfo("System.Buffers", "lib/netstandard2.0/System.Buffers.dll"),
        };

        private static readonly IEnumerable<DependencyInfo> _weaverDependencies = new[]
        {
            new DependencyInfo("Mono.Cecil", "lib/netstandard2.0/Mono.Cecil.dll", "lib/netstandard2.0/Mono.Cecil.Pdb.dll", "lib/netstandard2.0/Mono.Cecil.Rocks.dll"),
            new DependencyInfo("Unity3D.SDK", referenceOnly: true, "lib/UnityEditor.dll", "lib/UnityEngine.dll"),
        };

        public override PackageInfo[] Files { get; } = new[]
        {
            new PackageInfo("Realm", new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.dll", "Runtime/Realm.dll" },
                { "runtimes/ios/native/realm-wrappers.framework/Info.plist", "Runtime/iOS/Device/realm-wrappers.framework/Info.plist" },
                { "runtimes/ios/native/realm-wrappers.framework/realm-wrappers", "Runtime/iOS/Device/realm-wrappers.framework/realm-wrappers" },
                { "runtimes/iossimulator/native/realm-wrappers.framework/Info.plist", "Runtime/iOS/Simulator/realm-wrappers.framework/Info.plist" },
                { "runtimes/iossimulator/native/realm-wrappers.framework/realm-wrappers", "Runtime/iOS/Simulator/realm-wrappers.framework/realm-wrappers" },
                { "runtimes/iossimulator/native/realm-wrappers.framework/_CodeSignature/CodeResources", "Runtime/iOS/Simulator/realm-wrappers.framework/_CodeSignature/CodeResources" },
                { "runtimes/tvos/native/realm-wrappers.framework/Info.plist", "Runtime/tvOS/Device/realm-wrappers.framework/Info.plist" },
                { "runtimes/tvos/native/realm-wrappers.framework/realm-wrappers", "Runtime/tvOS/Device/realm-wrappers.framework/realm-wrappers" },
                { "runtimes/tvossimulator/native/realm-wrappers.framework/Info.plist", "Runtime/tvOS/Simulator/realm-wrappers.framework/Info.plist" },
                { "runtimes/tvossimulator/native/realm-wrappers.framework/realm-wrappers", "Runtime/tvOS/Simulator/realm-wrappers.framework/realm-wrappers" },
                { "runtimes/tvossimulator/native/realm-wrappers.framework/_CodeSignature/CodeResources", "Runtime/tvOS/Simulator/realm-wrappers.framework/_CodeSignature/CodeResources" },
                { "runtimes/osx/native/librealm-wrappers.dylib", "Runtime/macOS/librealm-wrappers.dylib" },
                { "runtimes/linux-x64/native/librealm-wrappers.so", "Runtime/Linux/librealm-wrappers.so" },
                { "runtimes/android-arm/native/librealm-wrappers.so", "Runtime/Android/armeabi-v7a/librealm-wrappers.so" },
                { "runtimes/android-arm64/native/librealm-wrappers.so", "Runtime/Android/arm64-v8a/librealm-wrappers.so" },
                { "runtimes/android-x64/native/librealm-wrappers.so", "Runtime/Android/x86_64/librealm-wrappers.so" },
                { "runtimes/android-x86/native/librealm-wrappers.so", "Runtime/Android/x86/librealm-wrappers.so" },
                { "runtimes/win-x64/native/realm-wrappers.dll", "Runtime/Windows/x86_64/realm-wrappers.dll" },
                { "runtimes/win-x86/native/realm-wrappers.dll", "Runtime/Windows/x86/realm-wrappers.dll" },
                { "runtimes/win10-arm/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/ARM/realm-wrappers.dll" },
                { "runtimes/win10-arm64/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/ARM64/realm-wrappers.dll" },
                { "runtimes/win10-x64/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/x86_64/realm-wrappers.dll" },
                { "runtimes/win10-x86/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/x86/realm-wrappers.dll" },
            }, _realmDependencies),
            new PackageInfo("Realm.SourceGenerator", new Dictionary<string, string>
            {
                { "analyzers/dotnet/cs/Realm.SourceGenerator.dll", "Editor/Realm.SourceGenerator.dll" },
            }),
            new PackageInfo("Realm.UnityUtils", new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.UnityUtils.dll", "Runtime/Realm.UnityUtils.dll" },
            }),
            new PackageInfo("Realm.UnityWeaver", new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.UnityWeaver.dll", "Editor/Realm.UnityWeaver.dll" },
            }, _weaverDependencies),
        };
    }
}
