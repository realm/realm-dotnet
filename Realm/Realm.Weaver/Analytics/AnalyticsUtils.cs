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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using static RealmWeaver.Metric;
using OperatingSystem = RealmWeaver.Metric.OperatingSystem;

namespace RealmWeaver
{
    internal static class AnalyticsUtils
    {
        public static string GetTargetOsName(FrameworkName frameworkName)
        {
            var targetOs = frameworkName.Identifier;

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

            if (targetOs.ContainsIgnoreCase("win") || targetOs.ContainsIgnoreCase("net4"))
            {
                return OperatingSystem.Windows;
            }

            if (targetOs.ContainsIgnoreCase("core") ||
                targetOs.ContainsIgnoreCase("standard") ||
                targetOs.ContainsIgnoreCase("net"))
            {
                return OperatingSystem.CrossPlatform;
            }

            return Unknown(frameworkName.Identifier);
        }

        public static FrameworkInfo GetFrameworkAndVersion(ModuleDefinition module)
        {
            // the order in the array matters as we first need to look at the libraries (maui and forms)
            // and then at the frameworks (xamarin native, Catalyst and UWP)
            var possibleFrameworks = new Dictionary<string, string>
            {
                { "Microsoft.Maui", Framework.Maui },
                { "Xamarin.Forms.Core", Framework.XamarinForms },
                { "Xamarin.iOS", Framework.Xamarin },
                { "Xamarin.tvOS", Framework.Xamarin },
                { "Xamarin.Mac", Framework.Xamarin },
                { "Mono.Android", Framework.Xamarin },
                { "Microsoft.MacCatalyst", Framework.MacCatalyst },
                { "Windows.Foundation.UniversalApiContract", Framework.Uwp },
            };

            AssemblyNameReference frameworkUsedInConjunction = null;
            foreach (var kvp in possibleFrameworks)
            {
                frameworkUsedInConjunction = module.AssemblyReferences.Where(a => a.Name == kvp.Key).SingleOrDefault();
                if (frameworkUsedInConjunction != null)
                {
                    return new(kvp.Value, frameworkUsedInConjunction.Version.ToString());
                }
            }

            return new("No framework of interest", "0.0.0");
        }

        public static string SHA256Hash(byte[] bytes, bool useLegacyEncoding = false)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return useLegacyEncoding ? BitConverter.ToString(hash) : Convert.ToBase64String(hash);
        }

        public static string GetHostCpuArchitecture() => RuntimeInformation.OSArchitecture switch
        {
            Architecture.X86 => CpuArchitecture.X86,
            Architecture.Arm => CpuArchitecture.Arm,
            Architecture.Arm64 => CpuArchitecture.Arm64,
            Architecture.X64 => CpuArchitecture.X64,
            _ => Unknown(RuntimeInformation.OSArchitecture.ToString())
        };

        public static string GetHostOsName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OperatingSystem.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OperatingSystem.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OperatingSystem.MacOS;
            }

            return Unknown(System.Environment.OSVersion.Platform.ToString());
        }

        public static string InferLanguageVersion(string netFramework, string netFrameworkVersion)
        {
            // We don't have a reliable way to get the version in the weaver so we're using the default version
            // associated with the framework.
            // Values taken from https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version
            if (!netFramework.ContainsIgnoreCase("net"))
            {
                // Likely this isn't the model assembly, but the platform specific one
                return Unknown(netFramework);
            }

            if (netFrameworkVersion.ContainsIgnoreCase("2.0") ||
                netFrameworkVersion.ContainsIgnoreCase("4."))
            {
                return "7.3";
            }

            if (netFrameworkVersion.ContainsIgnoreCase("2.1") ||
                netFrameworkVersion.ContainsIgnoreCase("3.1"))
            {
                return "8";
            }

            if (netFrameworkVersion.ContainsIgnoreCase("5.0"))
            {
                return "9";
            }

            if (netFrameworkVersion.ContainsIgnoreCase("6.0"))
            {
                return "10";
            }

            if (netFrameworkVersion.ContainsIgnoreCase("7.0"))
            {
                return "11";
            }

            return Unknown();
        }

        // Knowledge on unique machine Ids for different OSes obtained from https://github.com/denisbrodbeck/machineid
        public static string GetAnonymizedUserId()
        {
            var id = string.Empty;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var machineIdToParse = RunProcess("reg", "QUERY HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography -v MachineGuid");
                    var regex = new Regex("\\s+MachineGuid\\s+\\w+\\s+((\\w+-?)+)", RegexOptions.Multiline);
                    var match = regex.Match(machineIdToParse);

                    if (match?.Groups.Count > 1)
                    {
                        id = match.Groups[1].Value;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var machineIdToParse = RunProcess("ioreg", "-rd1 -c IOPlatformExpertDevice");
                    var regex = new Regex(".*\\\"IOPlatformUUID\\\"\\s=\\s\\\"(.+)\\\"", RegexOptions.Multiline);
                    var match = regex.Match(machineIdToParse);

                    if (match?.Groups.Count > 1)
                    {
                        id = match.Groups[1].Value;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    id = File.ReadAllText("/etc/machine-id");
                }

                if (id.Length == 0)
                {
                    return Unknown();
                }

                // We're salting the id with an hardcoded byte array just to avoid that a machine is recognizable across
                // unrelated projects that use the same mechanics to obtain a machine's ID
                var salt = "realm is great";
                var saltedId = Encoding.UTF8.GetBytes(id + salt);
                return SHA256Hash(saltedId);
            }
            catch
            {
                return Unknown();
            }
        }

        public static string GetLegacyAnonymizedUserId()
        {
            try
            {
                var id = NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(n => n.Name == "en0" || (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                                   .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                                   .First();
                return SHA256Hash(id, useLegacyEncoding: true);
            }
            catch
            {
                return Unknown();
            }
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

        public readonly struct FrameworkInfo
        {
            public string Name { get; }

            public string Version { get; }

            public FrameworkInfo(string name, string version)
            {
                Name = name;
                Version = version;
            }
        }
    }
}
