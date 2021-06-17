using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
    public static class Program
    {
        private const string RealmPackagaName = "io.realm.unity";
        private const string RealmBundlePackageName = "io.realm.unity-bundled";

        private static readonly IReadOnlyDictionary<string, string> DocumentationFiles = new Dictionary<string, string>
        {
            { "LICENSE", "LICENSE.md" },
            { "CHANGELOG.md", "CHANGELOG.md" }
        };

        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<RealmOptions, TestOptions>(args)
                .WithParsedAsync(r =>
                {
                    return r switch
                    {
                        RealmOptions ropts => Run(ropts),
                        TestOptions topts => Run(topts),
                        _ => throw new NotSupportedException($"Invalid options: {r.GetType().Name}"),
                    };
                });
        }

        private static async Task Run(TestOptions opts)
        {
            Console.WriteLine("Packaging Realm.Tests");

            foreach (var file in Directory.EnumerateFiles(Helpers.PackagesFolder, "Realm.Tests.*.nupkg"))
            {
                File.Delete(file);
            }

            var testsProjectFolder = Path.Combine(Helpers.SolutionFolder, "Tests", "Realm.Tests");
            RunTool("dotnet", $"pack {testsProjectFolder} -p:UnityBuild=true -p:PackageOutputPath={Helpers.PackagesFolder} --include-symbols", Helpers.SolutionFolder);

            var (_, dependencies) = await CopyMainPackages(Helpers.PackagesFolder, opts);

            await CopyDependencies(opts, dependencies);
        }

        private static async Task Run(RealmOptions opts)
        {
            var (unityPackageVersion, dependencies) = await CopyMainPackages(opts.PackagesPath, opts);

            if (opts.IncludeDependencies)
            {
                await CopyDependencies(opts, dependencies);
                CopyMetaFiles(opts);
            }

            UpdatePackageJson(opts.IncludeDependencies, opts.PackageBasePath);

            CopyDocumentation(opts.PackageBasePath);

            if (opts.Pack)
            {
                Console.WriteLine("Preparing npm package...");

                RunTool("npm", $"version {unityPackageVersion} --allow-same-version", opts.PackageBasePath);
                RunTool("npm", $"pack", opts.PackageBasePath);
            }
        }

        private static async Task<(string Version, Queue<PackageDependency> Dependencies)> CopyMainPackages(string packagesPath, OptionsBase opts)
        {
            var dependencies = new Queue<PackageDependency>();
            string unityPackageVersion = null;

            foreach (var info in opts.Files.Where(i => i.DependencyMode != DependencyMode.IsDependency))
            {
                Console.WriteLine($"Including {info.Id} binaries");
                var (version, deps, extractedFiles) = await CopyBinaries(packagesPath, info, opts.PackageBasePath);

                unityPackageVersion ??= version;

                if (info.DependencyMode == DependencyMode.IncludeIfRequested)
                {
                    dependencies.EnqueueRange(deps);
                }

                Console.WriteLine($"Included {extractedFiles} files from {info.Id}@{version}");
            }

            return (unityPackageVersion, dependencies);
        }

        private static async Task CopyDependencies(OptionsBase opts, Queue<PackageDependency> dependencies)
        {
            Console.WriteLine("Including dependencies");

            while (dependencies.TryDequeue(out var package))
            {
                var packageVersion = package.VersionRange.MinVersion.ToNormalizedString();
                var info = opts.Files.SingleOrDefault(i => i.Id == package.Id || i.Id == "*");
                if (opts.IgnoredDependencies.Contains(package.Id))
                {
                    Console.WriteLine($"Skipping {package.Id}@{packageVersion} because it is ignored.");
                }
                else if (info != null)
                {
                    Console.WriteLine($"Including {package.Id}@{packageVersion}");
                    var path = await DownloadPackage(package.Id, packageVersion);
                    var (_, newDeps, extractedFiles) = await CopyBinaries(path, info, opts.PackageBasePath);

                    dependencies.EnqueueRange(newDeps);

                    Console.WriteLine($"Included {extractedFiles} files from {package.Id}@{packageVersion}");
                }
                else
                {
                    Console.WriteLine($"Error: package {package.Id} was not found either in {nameof(OptionsBase.Files)} or {nameof(OptionsBase.IgnoredDependencies)}.");
                    Environment.Exit(1);
                }
            }
        }

        private static void CopyMetaFiles(RealmOptions opts)
        {
            var metaFilesPath = Path.Combine(Helpers.BuildFolder, "MetaFiles", "Realm");

            Console.WriteLine("Copying meta files");
            foreach (var file in Directory.EnumerateFiles(metaFilesPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(file, Path.Combine(opts.PackageBasePath, Path.GetRelativePath(metaFilesPath, file)), overwrite: true);
            }

            if (!opts.SkipValidation)
            {
                var relativeDependenciesPath = Directory.EnumerateDirectories(metaFilesPath, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length).First();
                var dependenciesPath = Path.Combine(opts.PackageBasePath, Path.GetRelativePath(metaFilesPath, relativeDependenciesPath));
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

            var tempPath = Path.Combine(Helpers.BuildFolder, "Downloaded Packages", $"{packageId}-{version}.nupkg");
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

        private static async Task<(string Version, IEnumerable<PackageDependency> Dependencies, int ExtractedFiles)> CopyBinaries(string path, PackageInfo info, string packageBasePath)
        {
            if (!File.Exists(path))
            {
                // Try to find a .symbols.nupkg first - as that will contain both the dll and the pdbs.
                var regex = new Regex($"{info.Id}\\.[0-9]+\\.[0-9\\-\\.A-Za-z]*?(?<symbols>\\.symbols)?\\.nupkg");
                path = Directory.GetFiles(path, "*.nupkg")
                    .Select(f => (File: f, Match: regex.Match(f)))
                    .Where(f => f.Match.Success)
                    .OrderByDescending(f => f.Match.Groups["symbols"]?.Success)
                    .Select(f => f.File)
                    .First();

                Console.WriteLine($"Inferred package path for {info.Id} is {path}");
            }

            using var stream = File.OpenRead(path);
            using var packageReader = new PackageArchiveReader(stream);

            var extractedFiles = 0;
            foreach (var (PackagePath, OnDiskPath) in info.GetFilesToExtract(packageReader))
            {
                var targetPath = Path.Combine(packageBasePath, OnDiskPath);

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                packageReader.ExtractFile(PackagePath, targetPath, NullLogger.Instance);
                extractedFiles++;
            }

            var dependencies = await packageReader.GetPackageDependenciesAsync(CancellationToken.None);
            var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();
            var packages = dependencies.FirstOrDefault(d => d.TargetFramework.DotNetFrameworkName == ".NETStandard,Version=v2.0")?.Packages;
            return (version, packages, extractedFiles);
        }

        private static void UpdatePackageJson(bool includesDependencies, string packageBasePath)
        {
            var packageJsonPath = Path.Combine(packageBasePath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                return;
            }

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

        private static void CopyDocumentation(string packageBasePath)
        {
            foreach (var kvp in DocumentationFiles)
            {
                var source = Path.Combine(Helpers.SolutionFolder, kvp.Key);
                var target = Path.Combine(packageBasePath, kvp.Value);
                File.Copy(source, target, overwrite: true);
            }
        }

        private static void RunTool(string tool, string command, string packageBasePath)
        {
            string fileName;
            string arguments;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "cmd";
                arguments = $"/c {tool} {command}";
            }
            else
            {
                fileName = tool;
                arguments = command;
            }

            var runner = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = packageBasePath,
                Arguments = arguments,
            });

            runner.WaitForExit();
        }
    }
}
