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
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Mono.Cecil;
using static RealmWeaver.Analytics;
using static RealmWeaver.Metric;

namespace RealmWeaver
{
    internal static class AnalyticsUtils
    {
        // TODO andrea: https://github.com/realm/realm-dotnet/issues/2706#event-7877818404
        // add a stable ID not reliant on MACAddress
        public static string AnonymizedUserID
        {
            get
            {
                try
                {
                    var id = GenerateComputerIdentifier;
                    return id != null ? SHA256Hash(id) : "UNKNOWN";
                }
                catch
                {
                    return "UNKNOWN";
                }
            }
        }

        public static string GetTargetOsName(FrameworkName frameworkName) =>
            WrapInTryCatch(() =>
            {
                switch (frameworkName.Identifier)
                {
                    case string s when s.ContainsIgnoreCase("android"):
                        return Metric.OperatingSystem.Android;
                    case string s when s.ContainsIgnoreCase("ios"):
                        return Metric.OperatingSystem.Ios;
                    case string s when s.ContainsIgnoreCase("mac") ||
                            s.ContainsIgnoreCase("macos") ||
                            s.ContainsIgnoreCase("maccatalyst"):
                        return Metric.OperatingSystem.MacOS;
                    case string s when s.ContainsIgnoreCase("tvos"):
                        return Metric.OperatingSystem.TvOs;
                    case string s when s.ContainsIgnoreCase("linux"):
                        return Metric.OperatingSystem.Linux;
                    default:
                        return Metric.OperatingSystem.Windows;
                }
            });

        public static string SHA256Hash(byte[] bytes)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(bytes));
            }
        }

        public static string GetHostCpuArchitecture =>
            WrapInTryCatch(() => ConvertArchitectureToMetricsVersion(RuntimeInformation.OSArchitecture.ToString()));

        public static string GetTargetCpuArchitecture(ModuleDefinition module) =>
            WrapInTryCatch(() => ConvertArchitectureToMetricsVersion(module.Architecture.ToString()));

        public static byte[] GenerateComputerIdentifier =>
            NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.Name == "en0" || (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                .FirstOrDefault();

        public static string ConvertPlatformIdOsToMetricVersion(PlatformID platformID) =>
            WrapInTryCatch(() =>
            {
                switch (platformID)
                {
                    case PlatformID.Win32NT:
                        return Metric.OperatingSystem.Windows;
                    case PlatformID.MacOSX:
                        return Metric.OperatingSystem.MacOS;
                    case PlatformID.Unix:
                        return Metric.OperatingSystem.Linux;
                    default:
                        return $"{platformID} is an unsupported operating system.";
                }
            });

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
                var possibleFrameworks = new string[] { "Xamarin.Form", "Mono.Android", "Xamarin.iOS", "Microsoft.Maui.Sdk" };
                AssemblyNameReference frameworkUsedInConjunction = null;
                foreach (var toSearch in possibleFrameworks)
                {
                    frameworkUsedInConjunction = module.FindReference(toSearch);
                    if (frameworkUsedInConjunction != null)
                    {
                        break;
                    }
                }

                var framework = string.Empty;
                switch (frameworkUsedInConjunction?.Name)
                {
                    case string s when s.ContainsIgnoreCase("xamarin") || s.ContainsIgnoreCase("android"):
                        framework = Framework.Xamarin;
                        break;
                    case string s when s.ContainsIgnoreCase("maui"):
                        framework = Framework.Maui;
                        break;
                    default:
                        framework = "No framework of interest";
                        break;
                }

                return (framework, frameworkUsedInConjunction?.Version.ToString());
            }
        }

        // Unfortunately,
        // 1. `LangVersion` is never in the custom attributes.
        //    Even if I manage to find a way to read the msbuild properties,
        // 2. this approach needs manaul intervention every time that a new version of C# and .NET are released
        // 3. the weaver runs on each different target, which makes reporting not that useful as it'll just report the
        //    default lanaguage for the target framework. Making this as good as looking at the target framework.
        public static string GetLanguageVersion(ModuleDefinition module, string targetFramework)
        {
            var langVersion = string.Empty;
            /*
            if (module.Assembly.HasCustomAttributes)
            {
                // LangVersionAttribute would need to be created
                langVersion = module.Assembly.CustomAttributes.Where(a => a.AttributeType.FullName == typeof(LangVersionAttribute).FullName).SingleOrDefault().Value;
            }
            */

            if (langVersion.Length > 0)
            {
                return langVersion;
            }
            else
            {
                // lowest common denomitor (LCD) for target framework
                // order matters as the LCD determines the maximum usable version of the language
                // Values taken from https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version
                if (targetFramework.ContainsIgnoreCase("netcoreapp2") ||
                    targetFramework.ContainsIgnoreCase("netframework"))
                {
                    return "7.3";
                }
                else if (targetFramework.ContainsIgnoreCase("netstandard2.1") ||
                    targetFramework.ContainsIgnoreCase("netcoreapp3.1"))
                {
                    return "8";
                }
                else if (targetFramework.ContainsIgnoreCase("net5.0"))
                {
                    return "9";
                }
                else if (targetFramework.ContainsIgnoreCase("net6.0"))
                {
                    return "10";
                }
                else if (targetFramework.ContainsIgnoreCase("net7.0"))
                {
                    return "11";
                }
                else
                {
                    return "unknown version";
                }
            }
        }

        private static string ConvertArchitectureToMetricsVersion(string arch) =>
            WrapInTryCatch(() =>
            {
                switch (arch)
                {
                    case string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.Arm)):
                        return CpuArchitecture.Arm;
                    case string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.Arm64)):
                        return CpuArchitecture.Arm64;
                    case string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.X64)) || s.ContainsIgnoreCase("amd64"):
                        return CpuArchitecture.X64;
                    case string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.X86)) || s.ContainsIgnoreCase("i386"):
                        return CpuArchitecture.X86;
                    default:
                        throw new ArgumentException($"{arch} is an unknown architecture");
                }
            });

        private static bool ContainsIgnoreCase(this string @this, string strCompare) =>
            @this.IndexOf(strCompare, StringComparison.OrdinalIgnoreCase) > -1;

        private static string WrapInTryCatch(Func<string> func)
        {
            try
            {
                return func();
            }
            catch
            {
#if DEBUG
                // Make sure we get build failures and address the problem in debug,
                // but don't fail users' builds because of that.
                throw;
#else
                return string.Empty;
#endif
            }
        }
    }
}
