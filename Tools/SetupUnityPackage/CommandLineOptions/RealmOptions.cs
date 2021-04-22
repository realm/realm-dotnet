using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SetupUnityPackage
{
    [Verb("realm", HelpText = "Setup the Realm package")]
    public class RealmOptions : OptionsBase
    {
        [Option("packages-path", Required = true, HelpText = "Path to the folder containing Realm.nupkg, Realm.UnityUtils.nupkg, and Realm.UnityWeaver.nupkg to use.")]
        public string PackagesPath { get; set; }

        [Option("include-dependencies", Default = false, Required = false, HelpText = "Specify whether dependencies should be bundled too.")]
        public bool IncludeDependencies { get; set; }

        [Option("pack", Default = false, Required = false, HelpText = "Specify whether to invoke npm version + npm pack to produce a .tgz")]
        public bool Pack { get; set; }

        [Option("skip-validation", Default = false, Required = false, HelpText = "Specify whether to skip validating the list of dependencies. Only used when include-dependencies is true.")]
        public bool SkipValidation { get; set; }

        public override string PackageBasePath => Path.Combine(Helpers.SolutionFolder, "Realm", "Realm.Unity");

        public override ISet<string> IgnoredDependencies { get; } = new HashSet<string>
        {
            "Microsoft.CSharp",
            "Realm.Fody",
            "Fody",
            "System.Dynamic.Runtime",
        };

        public override PackageInfo[] Files { get; } = new[]
        {
            new PackageInfo("Realm", DependencyMode.IncludeIfRequested, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.dll", "Runtime/Realm.dll" },
                { "native/ios/universal/realm-wrappers.framework/realm-wrappers", "Runtime/iOS/realm-wrappers.framework/realm-wrappers" },
                { "native/ios/universal/realm-wrappers.framework/Info.plist", "Runtime/iOS/realm-wrappers.framework/Info.plist" },
                { "runtimes/osx-x64/native/librealm-wrappers.dylib", "Runtime/macOS/librealm-wrappers.dylib" },
                { "runtimes/linux-x64/native/librealm-wrappers.so", "Runtime/Linux/librealm-wrappers.so" },
                { "native/android/armeabi-v7a/librealm-wrappers.so", "Runtime/Android/armeabi-v7a/librealm-wrappers.so" },
                { "native/android/arm64-v8a/librealm-wrappers.so", "Runtime/Android/arm64-v8a/librealm-wrappers.so" },
                { "native/android/x86_64/librealm-wrappers.so", "Runtime/Android/x86_64/librealm-wrappers.so" },
                { "runtimes/win-x64/native/realm-wrappers.dll", "Runtime/Windows/x86_64/realm-wrappers.dll" },
                { "runtimes/win-x86/native/realm-wrappers.dll", "Runtime/Windows/x86/realm-wrappers.dll" },
                { "runtimes/win10-arm/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/ARM/realm-wrappers.dll" },
                { "runtimes/win10-x64/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/x86_64/realm-wrappers.dll" },
                { "runtimes/win10-x86/nativeassets/uap10.0/realm-wrappers.dll", "Runtime/UWP/x86/realm-wrappers.dll" },
            }),
            new PackageInfo("Realm.UnityUtils", DependencyMode.NeverInclude, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.UnityUtils.dll", "Runtime/Realm.UnityUtils.dll" },
            }),
            new PackageInfo("Realm.UnityWeaver", DependencyMode.NeverInclude, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.UnityWeaver.dll", "Editor/Realm.UnityWeaver.dll" },
            }),
            new PackageInfo("MongoDB.Bson", DependencyMode.IsDependency, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/MongoDB.Bson.dll", "Runtime/Dependencies/MongoDB.Bson.dll" },
            }),
            new PackageInfo("Remotion.Linq", DependencyMode.IsDependency, new Dictionary<string, string>
            {
                { "lib/netstandard1.0/Remotion.Linq.dll", "Runtime/Dependencies/Remotion.Linq.dll" },
            }),
            new PackageInfo("System.Runtime.CompilerServices.Unsafe", DependencyMode.IsDependency, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll", "Runtime/Dependencies/System.Runtime.CompilerServices.Unsafe.dll" },
            }),
            new PackageInfo("System.Buffers", DependencyMode.IsDependency, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/System.Buffers.dll", "Runtime/Dependencies/System.Buffers.dll" },
            }),
        };
    }
}
