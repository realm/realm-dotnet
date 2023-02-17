////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;

using static RealmWeaver.Analytics;
using static RealmWeaver.Metric;
using OperatingSystem = RealmWeaver.Metric.OperatingSystem;

namespace RealmWeaver
{
    internal static class AnalyticsUtils
    {
        public static string GetTargetOsName(FrameworkName frameworkName)
        {
            string targetOs = frameworkName.Identifier;

            if (targetOs.ContainsIgnoreCase("android"))
            {
                return OperatingSystem.Android;
            }

            if (targetOs.ContainsIgnoreCase("ios"))
            {
                return OperatingSystem.Ios;
            }

            if (targetOs.ContainsIgnoreCase("mac"))
            {
                return OperatingSystem.MacOS;
            }

            if (targetOs.ContainsIgnoreCase("tvos"))
            {
                return OperatingSystem.TvOs;
            }

            if (targetOs.ContainsIgnoreCase("linux"))
            {
                return OperatingSystem.Linux;
            }

            if (targetOs.ContainsIgnoreCase("win") || targetOs == ".NETFramework")
            {
                return OperatingSystem.Windows;
            }

            if (targetOs.ContainsIgnoreCase("uap"))
            {
                return OperatingSystem.Uwp;
            }

            return Unknown(frameworkName.Identifier);
        }

        public static string SHA256Hash(byte[] bytes)
        {
            using var sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(bytes));
        }

        public static string GetHostCpuArchitecture() => ConvertArchitectureToMetricsVersion(RuntimeInformation.OSArchitecture.ToString());

        public static string GetTargetCpuArchitecture(ModuleDefinition module) => ConvertArchitectureToMetricsVersion(module.Architecture.ToString());

        public static string GetHostOsName() =>
            System.Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.WinCE => OperatingSystem.Windows,
                PlatformID.MacOSX => OperatingSystem.MacOS,
                PlatformID.Unix => OperatingSystem.Linux,
                _ => Unknown(System.Environment.OSVersion.Platform.ToString())
            };

        public static (string Name, string Version) GetFrameworkAndVersion(ModuleDefinition module, Config config)
        {
            if (config.TargetFramework.ContainsIgnoreCase("unity"))
            {
                return config.TargetFramework.ContainsIgnoreCase("editor") ?
                    (Framework.UnityEditor, config.TargetFrameworkVersion) :
                    (Framework.Unity, config.TargetFrameworkVersion);
            }
            else
            {
                // TODO andrea: the correctness of these names need to be verified in projects that use each of the packages
                // I didn't have any handy one.
                var possibleFrameworks = new string[] { "Xamarin.Forms", "Mono.Android", "Xamarin.iOS", "Microsoft.Maui.Sdk" };
                AssemblyNameReference frameworkUsedInConjunction = null;
                foreach (var toSearch in possibleFrameworks)
                {
                    frameworkUsedInConjunction = module.FindReference(toSearch);
                    if (frameworkUsedInConjunction != null)
                    {
                        break;
                    }
                }

                var framework = "No framework of interest";
                if (frameworkUsedInConjunction != null)
                {
                    var name = frameworkUsedInConjunction.Name;
                    if (name.ContainsIgnoreCase("xamarin") || name.ContainsIgnoreCase("android"))
                    {
                        framework = Framework.Xamarin;
                    }
                    else if (name.ContainsIgnoreCase("maui"))
                    {
                        framework = Framework.Maui;
                    }
                }

                return (framework, frameworkUsedInConjunction?.Version.ToString());
            }
        }

        public static string GetLanguageVersion(string targetFramework)
        {
            // We don't have a reliable way to get the version in the weaver so we're using the default version
            // associated with the framework.
            // Values taken from https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version
            if (targetFramework.ContainsIgnoreCase("netcoreapp2") ||
                targetFramework.ContainsIgnoreCase("netframework"))
            {
                return "7.3";
            }

            if (targetFramework.ContainsIgnoreCase("netstandard2.1") ||
                targetFramework.ContainsIgnoreCase("netcoreapp3.1"))
            {
                return "8";
            }

            if (targetFramework.ContainsIgnoreCase("net5.0"))
            {
                return "9";
            }

            if (targetFramework.ContainsIgnoreCase("net6.0"))
            {
                return "10";
            }

            if (targetFramework.ContainsIgnoreCase("net7.0"))
            {
                return "11";
            }

            return Unknown();
        }

        // Knowledge on unique machine Ids for different OSes obtained from https://github.com/denisbrodbeck/machineid
        public static string GetAnonymizedUserId()
        {
            var id = string.Empty;
            var currentOs = System.Environment.OSVersion.Platform;
            try
            {
                switch (currentOs)
                {
                    case PlatformID.Win32S or PlatformID.Win32Windows or
                        PlatformID.Win32NT or PlatformID.WinCE:
                    {
                        var machineIdToParse = RunProcess("reg", "QUERY HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography -v MachineGuid");
                        var regex = new Regex("\\s+MachineGuid\\s+\\w+\\s+((\\w+-?)+)", RegexOptions.Multiline);
                        var match = regex.Match(machineIdToParse);

                        if (match?.Groups.Count > 1)
                        {
                            id = match.Groups[1].Value;
                        }

                        break;
                    }

                    case PlatformID.MacOSX:
                    {
                        var machineIdToParse = RunProcess("ioreg", "-rd1 -c IOPlatformExpertDevice");
                        var regex = new Regex(".*\\\"IOPlatformUUID\\\"\\s=\\s\\\"(.+)\\\"", RegexOptions.Multiline);
                        var match = regex.Match(machineIdToParse);

                        if (match?.Groups.Count > 1)
                        {
                            id = match.Groups[1].Value;
                        }

                        break;
                    }

                    case PlatformID.Unix:
                    {
                        id = File.ReadAllText("/etc/machine-id");
                        break;
                    }
                }

                if (id.Length == 0)
                {
                    return Unknown();
                }

                // We're salting the id with an hardcoded byte array just to avoid that a machine is recognizable across
                // unrelated projects that use the same mechanics to obtain a machine's ID
                var salt = new byte[] { 82, 101, 97, 108, 109, 32, 105, 115, 32, 103, 114, 101, 97, 116 };
                var byteId = Encoding.ASCII.GetBytes(id);
                var saltedId = new byte[byteId.Length + salt.Length];
                Buffer.BlockCopy(byteId, 0, saltedId, 0, byteId.Length);
                Buffer.BlockCopy(salt, 0, saltedId, byteId.Length, salt.Length);
                return SHA256Hash(saltedId);
            }
            catch
            {
                return Unknown();
            }
        }

        private static string ConvertArchitectureToMetricsVersion(string arch)
        {
            if (arch.ContainsIgnoreCase(nameof(CpuArchitecture.Arm)))
            {
                return CpuArchitecture.Arm;
            }

            if (arch.ContainsIgnoreCase(nameof(CpuArchitecture.Arm64)))
            {
                return CpuArchitecture.Arm64;
            }

            if (arch.ContainsIgnoreCase(nameof(CpuArchitecture.X64)) || arch.ContainsIgnoreCase("amd64"))
            {
                return CpuArchitecture.X64;
            }

            if (arch.ContainsIgnoreCase(nameof(CpuArchitecture.X86)) || arch.ContainsIgnoreCase("i386"))
            {
                return CpuArchitecture.X86;
            }

            return Unknown(arch);
        }

        private static bool ContainsIgnoreCase(this string @this, string strCompare) =>
            @this.IndexOf(strCompare, StringComparison.OrdinalIgnoreCase) > -1;

        private static string RunProcess(string filename, string arguments)
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
#if DEBUG
                    RedirectStandardError = true,
#endif
                }
            };

            proc.Start();

            var stdout = new StringBuilder();
            while (!proc.HasExited)
            {
                stdout.AppendLine(proc.StandardOutput.ReadToEnd());
#if DEBUG
                stdout.AppendLine(proc.StandardError.ReadToEnd());
#endif
            }

            stdout.AppendLine(proc.StandardOutput.ReadToEnd());
#if DEBUG
            stdout.AppendLine(proc.StandardError.ReadToEnd());
#endif

            return stdout.ToString();
        }
    }
}
