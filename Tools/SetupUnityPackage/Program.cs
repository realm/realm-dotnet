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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ILRepacking;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace SetupUnityPackage
{
    internal static class Program
    {
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

            var extractedPackagePath = CreateTempDirectory("realm-package");
            var realmDllPath = Path.Combine(Helpers.SolutionFolder, "Realm", "Realm.Unity", "Runtime", "Realm.dll");
            if (opts.RealmPackage == null)
            {
                // If the user didn't provide a .tgz package, we should assume a local install
                opts.RealmPackage = "../../../Realm/Realm.Unity";
            }
            else
            {
                // If we are building against a specific Realm package, we should untar it somewhere and reference it in the build
                using var fileStream = File.OpenRead(opts.RealmPackage);
                using var gzipStream = new GZipInputStream(fileStream);
                using var archive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                archive.ExtractContents(extractedPackagePath);
                realmDllPath = Path.Combine(extractedPackagePath, "package", "Runtime", "Realm.dll");
            }

            var testsProjectFolder = Path.Combine(Helpers.SolutionFolder, "Tests", "Realm.Tests");
            RunTool("dotnet", $"pack {testsProjectFolder} -p:UnityBuild=true -p:PackageOutputPath={Helpers.PackagesFolder} -p:RealmDllPath={realmDllPath} -c Release --include-symbols", Helpers.SolutionFolder);

            var testsSearchDirectory = Path.Combine(testsProjectFolder, "bin", "Release", "netstandard2.0");
            await CopyPackages(Helpers.PackagesFolder, opts, testsSearchDirectory);

            var manifestPath = Path.Combine(Helpers.SolutionFolder, "Tests", "Tests.Unity", "Packages", "manifest.json");
            var relativePackagePath = Path.GetRelativePath(Path.GetDirectoryName(manifestPath)!, opts.RealmPackage);

            UpdateManifestJson(manifestPath, "io.realm.unity", relativePackagePath);

            Directory.Delete(extractedPackagePath, recursive: true);
        }

        private static async Task Run(RealmOptions opts)
        {
            var unityPackageVersion = await CopyPackages(opts.PackagesPath, opts);

            CopyDocumentation(opts.PackageBasePath);

            if (opts.Pack)
            {
                Console.WriteLine("Preparing npm package...");

                RunTool("npm", $"version {unityPackageVersion} --allow-same-version", opts.PackageBasePath);
                RunTool("npm", $"pack", opts.PackageBasePath);

                Console.WriteLine("Preparing Asset Store project...");

                var targetFolder = Path.Combine(Helpers.SolutionFolder, "Tools", "AssetStorePublisher", "Assets", "Realm");
                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, recursive: true);
                }

                Helpers.CopyFiles(opts.PackageBasePath, targetFolder, file => !file.StartsWith("package.json", StringComparison.OrdinalIgnoreCase) && !file.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase));

                Console.WriteLine($"Copied package files to {targetFolder}. Open the project with Unity and upload to asset store manually.");
            }
        }

        private static async Task<string> CopyPackages(string packagesPath, OptionsBase opts, params string[] searchDirectories)
        {
            string? unityPackageVersion = null;

            foreach (var info in opts.Files)
            {
                Console.WriteLine($"Including {info.Id} binaries");
                var (version, deps, extractedFiles) = await CopyBinaries(packagesPath, info.Id, info.GetFilesToExtract(opts.PackageBasePath));

                unityPackageVersion ??= version;

                Console.WriteLine($"Included {extractedFiles} files from {info.Id}@{version}");

                if (info.Dependencies == null)
                {
                    continue;
                }

                var dependencies = new Queue<PackageDependency>();
                dependencies.EnqueueRange(deps);

                Console.WriteLine($"Package {info.Id}@{version} has {dependencies.Count} dependencies. Including them in project...");

                await CopyDependencies(opts, info, dependencies, searchDirectories);
            }

            return unityPackageVersion!;
        }

        private static async Task CopyDependencies(OptionsBase opts, PackageInfo info, Queue<PackageDependency> dependencies, params string[] searchDirectories)
        {
            Console.WriteLine("Including dependencies");

            var tempPath = CreateTempDirectory("dependencies");
            var referencePath = CreateTempDirectory("references");

            while (dependencies.TryDequeue(out var package))
            {
                var packageVersion = package.VersionRange.MinVersion.ToNormalizedString();
                var depInfo = info.Dependencies?.SingleOrDefault(i => i.Id == package.Id);
                if (opts.IgnoredDependencies.Contains(package.Id))
                {
                    Console.WriteLine($"Skipping {package.Id}@{packageVersion} because it is ignored.");
                }
                else if (depInfo != null)
                {
                    Console.WriteLine($"Including {package.Id}@{packageVersion}");
                    var path = await DownloadPackage(package.Id, packageVersion);

                    var (_, newDeps, extractedFiles) = await CopyBinaries(path, depInfo.Id, depInfo.GetFilesToExtract(depInfo.ReferenceOnly ? referencePath : tempPath));

                    dependencies.EnqueueRange(newDeps);

                    Console.WriteLine($"Included {extractedFiles} files from {package.Id}@{packageVersion}");
                }
                else
                {
                    Console.WriteLine($"Error: package {package.Id} was not found either in {nameof(OptionsBase.Files)} or {nameof(OptionsBase.IgnoredDependencies)}.");
                    Environment.Exit(1);
                }
            }

            var relinq = Path.Combine(tempPath, "Remotion.Linq.dll");
            if (File.Exists(relinq))
            {
                const string UsedImplicitlyAttribute = "JetBrains.Annotations.UsedImplicitlyAttribute";
                Console.WriteLine($"Found Remotion.Linq dependency, stripping {UsedImplicitlyAttribute}");
                using var stream = File.Open(relinq, FileMode.Open, FileAccess.ReadWrite);
                using var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(stream);
                var module = assembly.MainModule;

                var usedImplicitly = module.Types.Single(t => t.FullName == UsedImplicitlyAttribute);
                module.Types.Remove(usedImplicitly);

                assembly.Write();
            }

            var mainPackagePath = Path.Combine(opts.PackageBasePath, info.MainPackagePath);
            if (opts.NoRepack)
            {
                var targetFolder = Path.Combine(Path.GetDirectoryName(mainPackagePath)!, "Dependencies");

                Helpers.CopyFiles(tempPath, targetFolder);
            }
            else
            {
                var assembliesToRepack = new List<string> { mainPackagePath };
                assembliesToRepack.AddRange(Directory.EnumerateFiles(tempPath));

                var repack = new ILRepack(new RepackOptions
                {
                    InputAssemblies = assembliesToRepack.ToArray(),
                    Internalize = true,
                    OutputFile = mainPackagePath,
                    SearchDirectories = searchDirectories.Concat(new[] { referencePath }),
                    XmlDocumentation = false,
                    ExcludeInternalizeMatches = { new Regex("MongoDB\\.Bson") }
                });

                repack.Repack();
            }

            Directory.Delete(tempPath, recursive: true);
            Directory.Delete(referencePath, recursive: true);
        }

        private static async Task<string> DownloadPackage(string packageId, string version)
        {
            Console.WriteLine($"  Downloading {packageId}@{version ?? "latest"}");
            var nugetRepo = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await nugetRepo.GetResourceAsync<FindPackageByIdResource>();

            using var cache = new SourceCacheContext();
            if (version == null)
            {
                Console.WriteLine("  Package version not specified, looking up latest version.");

                var versions = await resource.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, CancellationToken.None);
                version = versions.OrderByDescending(v => v).First().ToNormalizedString();

                Console.WriteLine($"  Found latest version: {version}.");
            }

            var tempPath = Path.Combine(Helpers.BuildFolder, "Downloaded Packages", $"{packageId}-{version}.nupkg");
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

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

        private static async Task<(string Version, IEnumerable<PackageDependency> Dependencies, int ExtractedFiles)> CopyBinaries(string path, string packageId, IEnumerable<(string PackagePath, string TargetPath)> paths)
        {
            if (!File.Exists(path))
            {
                // Try to find a .symbols.nupkg first - as that will contain both the dll and the pdbs.
                var regex = new Regex($"{packageId}\\.[0-9]+\\.[0-9\\-\\.A-Za-z]*?(?<symbols>\\.symbols)?\\.nupkg");
                path = Directory.GetFiles(path, "*.nupkg")
                    .Select(f => (File: f, Match: regex.Match(f)))
                    .Where(f => f.Match.Success)
                    .OrderByDescending(f => f.Match.Groups["symbols"]?.Success)
                    .Select(f => f.File)
                    .First();

                Console.WriteLine($"Inferred package path for {packageId} is {path}");
            }

            using var stream = File.OpenRead(path);
            using var packageReader = new PackageArchiveReader(stream);

            var extractedFiles = 0;
            foreach (var (packagePath, targetPath) in paths)
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                packageReader.ExtractFile(packagePath, targetPath, NullLogger.Instance);
                extractedFiles++;
            }

            var dependencies = await packageReader.GetPackageDependenciesAsync(CancellationToken.None);
            var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();
            var packages = dependencies.FirstOrDefault(d => d.TargetFramework.DotNetFrameworkName == ".NETStandard,Version=v2.0")?.Packages ?? Enumerable.Empty<PackageDependency>();
            return (version, packages, extractedFiles);
        }

        private static void UpdateManifestJson(string path, string packageName, string packagePath)
        {
            var contents = File.ReadAllText(path);
            var regex = new Regex($"\"{packageName}\": \"file:(?<package>[^\"]*)\"");
            var oldPackagePath = regex.Match(contents).Groups["package"].Value;
            File.WriteAllText(path, contents.Replace(oldPackagePath, packagePath.Replace("\\", "/")));
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

            Console.WriteLine($"{fileName} {arguments}");
#pragma warning disable CA1416
            var runner = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = packageBasePath,
                Arguments = arguments,
            });
#pragma warning restore CA1416

            runner?.WaitForExit();
        }

        private static string CreateTempDirectory(string name)
        {
            var tempPath = Path.Combine(Helpers.SolutionFolder, $"temp-{name}");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }

            Directory.CreateDirectory(tempPath);

            return tempPath;
        }
    }
}
