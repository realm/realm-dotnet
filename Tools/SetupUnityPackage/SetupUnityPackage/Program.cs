using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace SetupUnityPackage
{
    public class Options
    {
        [Option('p', "path", Required = false, HelpText = "Use a local NuGet package.")]
        public string Path { get; set; }

        [Option('v', "nuget-version", Required = false, HelpText = "Specify an explicit version of the package to use.")]
        public string Version { get; set; }

        [Option('f', "include-dependencies", Default = false, Required = false, HelpText = "Specify whether dependencies should be bundled too.")]
        public bool IncludeDependencies { get; set; }
    }

    public class Program
    {
        private const string RealmPackageId = "Realm";

        private static readonly string _buildFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        private static readonly IDictionary<string, IDictionary<string, string>> _packageMaps = new Dictionary<string, IDictionary<string, string>>
        {
            [RealmPackageId] = new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.dll", "Realm.dll" },
                { "native/ios/Realm.dll.config", "iOS/Realm.dll.config" },
                { "native/ios/universal/realm-wrappers.framework/realm-wrappers", "iOS/realm-wrappers.framework/realm-wrappers" },
                { "native/ios/universal/realm-wrappers.framework/Info.plist", "iOS/realm-wrappers.framework/info.plist" },
                { "runtimes/osx-x64/native/librealm-wrappers.dylib", "macOS/librealm-wrappers.dylib" },
                { "runtimes/linux-x64/native/librealm-wrappers.so", "Linux/librealm-wrappers.so" },
                { "native/android/armeabi-v7a/librealm-wrappers.so", "Android/armeabi-v7a/librealm-wrappers.so" },
                { "native/android/arm64-v8a/librealm-wrappers.so", "Android/arm64-v8a/librealm-wrappers.so" },
                { "native/android/x86_64/librealm-wrappers.so", "Android/x86_64/librealm-wrappers.so" },
                { "runtimes/win10-x64/nativeassets/uap10.0/realm-wrappers.dll", "Windows/x64/realm-wrappers.dll" },
                { "runtimes/win10-x86/nativeassets/uap10.0/realm-wrappers.dll", "Windows/x86/realm-wrappers.dll" },
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

            if (opts.IncludeDependencies)
            {
                Console.WriteLine("Including dependencies");

                foreach (var package in realmDependencies)
                {
                    if (_packageMaps.TryGetValue(package.Id, out var fileMap))
                    {
                        var packageVersion = package.VersionRange.MinVersion.ToNormalizedString();
                        Console.WriteLine($"Including {package.Id}@{packageVersion}");
                        var path = await DownloadPackage(package.Id, packageVersion);
                        await CopyBinaries(path, fileMap);

                        Console.WriteLine($"Included {fileMap.Count} files from {package.Id}@{packageVersion}");
                    }
                }

                Console.WriteLine("Copying meta files");

                var metaFilesPath = Path.Combine(_buildFolder, "MetaFiles");
                var targetBasePath = GetUnityPackagePath();
                foreach (var file in Directory.EnumerateFiles(metaFilesPath))
                {
                    File.Copy(file, Path.Combine(targetBasePath, Path.GetRelativePath(metaFilesPath, file)), overwrite: true);
                }
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
                    packageReader.ExtractFile(kvp.Key, Path.Combine(unityPath, kvp.Value), NullLogger.Instance);
                }

                var dependencies = await packageReader.GetPackageDependenciesAsync(CancellationToken.None);
                var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();
                var packages = dependencies.FirstOrDefault(d => d.TargetFramework.DotNetFrameworkName == ".NETStandard,Version=v2.0")?.Packages;
                return (version, packages);
            }
        }

        private static string GetUnityPackagePath()
        {
            var pattern = Path.Combine("Tools", "SetupUnityPackage", "SetupUnityPackage");

            var basePath = _buildFolder.Substring(0, _buildFolder.IndexOf(pattern));

            return Path.Combine(basePath, "Realm", "Realm.Unity", "Runtime");
        }
    }
}
