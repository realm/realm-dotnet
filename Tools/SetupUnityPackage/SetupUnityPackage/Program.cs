using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using UnityUtils;

namespace SetupUnityPackage
{
    public class Options
    {
        [Option("path", Required = false, HelpText = "Use a local NuGet package.")]
        public string Path { get; set; }

        [Option("nuget-version", Required = false, HelpText = "Specify an explicit version of the package to use.")]
        public string Version { get; set; }

        [Option("include-dependencies", Default = false, Required = false, HelpText = "Specify whether dependencies should be bundled too.")]
        public bool IncludeDependencies { get; set; }

        [Option("pack", Default = false, Required = false, HelpText = "Specify whether to invoke npm version + npm pack to produce a .tgz")]
        public bool Pack { get; set; }

        [Option("skip-validation", Default = false, Required = false, HelpText = "Specify whether to skip validating the list of dependencies. Only used when include-dependencies is true.")]
        public bool SkipValidation { get; set; }
    }

    public static class Program
    {
        private const string RealmPackageId = "Realm";
        private const string RealmPackagaName = "realm.unity";
        private const string RealmBundlePackageName = "realm.unity.bundle";

        private static readonly string _buildFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        private static readonly IDictionary<string, IDictionary<string, string>> _packageMaps = new Dictionary<string, IDictionary<string, string>>
        {
            [RealmPackageId] = new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.dll", "Realm.dll" },
                { "native/ios/Realm.dll.config", "Realm.dll.config" },
                { "native/ios/universal/realm-wrappers.framework/realm-wrappers", "iOS/realm-wrappers.framework/realm-wrappers" },
                { "native/ios/universal/realm-wrappers.framework/Info.plist", "iOS/realm-wrappers.framework/Info.plist" },
                { "runtimes/osx-x64/native/librealm-wrappers.dylib", "macOS/librealm-wrappers.dylib" },
                { "runtimes/linux-x64/native/librealm-wrappers.so", "Linux/librealm-wrappers.so" },
                { "native/android/armeabi-v7a/librealm-wrappers.so", "Android/armeabi-v7a/librealm-wrappers.so" },
                { "native/android/arm64-v8a/librealm-wrappers.so", "Android/arm64-v8a/librealm-wrappers.so" },
                { "native/android/x86_64/librealm-wrappers.so", "Android/x86_64/librealm-wrappers.so" },
                { "runtimes/win-x64/native/realm-wrappers.dll", "Windows/x86_64/realm-wrappers.dll" },
                { "runtimes/win-x86/native/realm-wrappers.dll", "Windows/x86/realm-wrappers.dll" },
                { "runtimes/win10-arm/nativeassets/uap10.0/realm-wrappers.dll", "UWP/ARM/realm-wrappers.dll" },
                { "runtimes/win10-x64/nativeassets/uap10.0/realm-wrappers.dll", "UWP/x86_64/realm-wrappers.dll" },
                { "runtimes/win10-x86/nativeassets/uap10.0/realm-wrappers.dll", "UWP/x86/realm-wrappers.dll" },
            },
            ["MongoDB.Bson"] = new Dictionary<string, string>
            {
                { "lib/netstandard2.0/MongoDB.Bson.dll", "Dependencies/MongoDB.Bson.dll" },
            },
            ["Remotion.Linq"] = new Dictionary<string, string>
            {
                { "lib/netstandard1.0/Remotion.Linq.dll", "Dependencies/Remotion.Linq.dll" },
            },
            ["System.Runtime.CompilerServices.Unsafe"] = new Dictionary<string, string>
            {
                { "lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll", "Dependencies/System.Runtime.CompilerServices.Unsafe.dll" },
            },
            ["System.Buffers"] = new Dictionary<string, string>
            {
                { "lib/netstandard2.0/System.Buffers.dll", "Dependencies/System.Buffers.dll" },
            },
        };

        private static readonly ISet<string> _ignoredDependencies = new HashSet<string>
        {
            "Microsoft.CSharp",
            "Realm.Fody",
            "Fody",
            "System.Dynamic.Runtime",
        };

        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(Run);
        }

        private static async Task Run(Options opts)
        {
            if (opts.Path == null)
            {
                Console.WriteLine("Local path not specified, using NuGet to download package.");
                opts.Path = await DownloadPackage(RealmPackageId, opts.Version);
            }

            Console.WriteLine("Including Realm binaries");

            var (version, realmDependencies) = await CopyBinaries(opts.Path, _packageMaps[RealmPackageId]);

            Console.WriteLine($"Included {_packageMaps[RealmPackageId].Count} files from {RealmPackageId}@{version}");

            Console.WriteLine("Inluding UnityUtils");

            var targetPath = Path.Combine(GetUnityPackagePath(), "UnityUtils.dll");
            File.Copy(GetUnityUtilsPath(), targetPath, overwrite: true);
            Console.WriteLine($"Included 1 file from UnityUtils@{typeof(FileHelper).Assembly.GetName().Version.ToString(3)}");

            if (opts.IncludeDependencies)
            {
                Console.WriteLine("Including dependencies");

                foreach (var package in realmDependencies)
                {
                    var packageVersion = package.VersionRange.MinVersion.ToNormalizedString();
                    if (_packageMaps.TryGetValue(package.Id, out var fileMap))
                    {
                        Console.WriteLine($"Including {package.Id}@{packageVersion}");
                        var path = await DownloadPackage(package.Id, packageVersion);
                        await CopyBinaries(path, fileMap);

                        Console.WriteLine($"Included {fileMap.Count} files from {package.Id}@{packageVersion}");
                    }
                    else if (_ignoredDependencies.Contains(package.Id))
                    {
                        Console.WriteLine($"Skipping {package.Id}@{packageVersion} because it's not included in {nameof(_packageMaps)}");
                    }
                    else if (!opts.SkipValidation)
                    {
                        Console.WriteLine($"Error: package {package.Id} was not found either in {nameof(_packageMaps)} or {nameof(_ignoredDependencies)}. Suppress error with -skip-validation.");
                        Environment.Exit(1);
                    }
                }

                Console.WriteLine("Copying meta files");

                var metaFilesPath = Path.Combine(_buildFolder, "MetaFiles");
                var targetBasePath = GetUnityPackagePath();
                foreach (var file in Directory.EnumerateFiles(metaFilesPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(file, Path.Combine(targetBasePath, Path.GetRelativePath(metaFilesPath, file)), overwrite: true);
                }

                if (!opts.SkipValidation)
                {
                    var dependenciesPath = Path.Combine(targetBasePath, "Dependencies");
                    var expectedFiles = Directory.EnumerateFiles(dependenciesPath, "*.dll")
                        .Select(f => $"{f}.meta")
                        .ToHashSet();

                    expectedFiles.SymmetricExceptWith(Directory.EnumerateFiles(dependenciesPath, "*.dll.meta"));

                    if (expectedFiles.Any())
                    {
                        Console.WriteLine($"Error: the .dll or the .dll meta for the following files was missing in {dependenciesPath}:");

                        foreach (var file in expectedFiles)
                        {
                            Console.WriteLine($"\t{Path.GetFileNameWithoutExtension(file)}");
                        }

                        Console.WriteLine("Suppress error with -skip-validation.");
                        Environment.Exit(2);
                    }
                }
            }

            UpdatePackageJson(opts.IncludeDependencies);

            if (opts.Pack)
            {
                Console.WriteLine("Preparing npm package...");

                RunNpm($"version {version} --allow-same-version");
                RunNpm($"pack");
            }
        }

        private static async Task<string> DownloadPackage(string packageId, string version)
        {
            Console.WriteLine($"  Downloading {packageId}@{version ?? "latest"}");
            var nugetRepo = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await nugetRepo.GetResourceAsync<FindPackageByIdResource>();

            var cache = new SourceCacheContext();
            if (version == null)
            {
                Console.WriteLine("  Package version not specified, looking up latest version.");

                var versions = await resource.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, CancellationToken.None);
                version = versions.OrderByDescending(v => v).First().ToNormalizedString();

                Console.WriteLine($"  Found latest version: {version}.");
            }

            var tempPath = Path.Combine(_buildFolder, "Downloaded Packages", $"{packageId}-{version}.nupkg");
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath));

            if (File.Exists(tempPath))
            {
                Console.WriteLine($"  Skipping download of {packageId} {version} because it exists at {tempPath}");
            }
            else
            {
                Console.WriteLine($"  Downloading {packageId} {version} to {tempPath}.");

                var nugetVersion = new NuGetVersion(version);
                using (var fileStream = File.OpenWrite(tempPath))
                {
                    await resource.CopyNupkgToStreamAsync(packageId, nugetVersion, fileStream, cache, NullLogger.Instance, CancellationToken.None);
                }

                Console.WriteLine("  Download complete.");
            }

            return tempPath;
        }

        private static async Task<(string, IEnumerable<PackageDependency>)> CopyBinaries(string path, IDictionary<string, string> fileMap)
        {
            using (var stream = File.OpenRead(path))
            using (var packageReader = new PackageArchiveReader(stream))
            {
                var unityPath = GetUnityPackagePath();
                foreach (var kvp in fileMap)
                {
                    var targetPath = Path.Combine(unityPath, kvp.Value);
                    File.Delete(targetPath);
                    packageReader.ExtractFile(kvp.Key, targetPath, NullLogger.Instance);
                }

                var dependencies = await packageReader.GetPackageDependenciesAsync(CancellationToken.None);
                var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();
                var packages = dependencies.FirstOrDefault(d => d.TargetFramework.DotNetFrameworkName == ".NETStandard,Version=v2.0")?.Packages;
                return (version, packages);
            }
        }

        private static string GetSolutionFolder()
        {
            var pattern = Path.Combine("Tools", "SetupUnityPackage", "SetupUnityPackage");
            return _buildFolder.Substring(0, _buildFolder.IndexOf(pattern));
        }

        private static string GetUnityPackagePath() => Path.Combine(GetSolutionFolder(), "Realm", "Realm.Unity", "Runtime");

        private static string GetUnityUtilsPath()
        {
#if DEBUG
            var targetFolder = "Debug";
#else
            var targetFolder = "Release";
#endif

            return Path.Combine(GetSolutionFolder(), "Tools", "SetupUnityPackage", "UnityUtils", "bin", targetFolder, "netstandard2.0", "UnityUtils.dll");
        }

        private static void UpdatePackageJson(bool includesDependencies)
        {
            var packageJsonPath = Path.GetFullPath(Path.Combine(GetUnityPackagePath(), "..", "package.json"));
            var contents = File.ReadAllText(packageJsonPath);

            string maybeSourceName;
            string targetName;

            if (includesDependencies)
            {
                targetName = RealmBundlePackageName;
                maybeSourceName = RealmPackagaName;
            }
            else
            {
                targetName = RealmPackagaName;
                maybeSourceName = RealmBundlePackageName;
            }

            contents = contents.Replace($"\"name\": \"{maybeSourceName}\"", $"\"name\": \"{targetName}\"");
            File.WriteAllText(packageJsonPath, contents);
        }

        private static void RunNpm(string command)
        {
            string fileName;
            string arguments;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "cmd";
                arguments = $"/c npm {command}";
            }
            else
            {
                fileName = "npm";
                arguments = command;
            }

            var npmRunner = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = Path.GetFullPath(Path.Combine(GetUnityPackagePath(), "..")),
                Arguments = arguments,
            });

            npmRunner.WaitForExit();
        }
    }
}
