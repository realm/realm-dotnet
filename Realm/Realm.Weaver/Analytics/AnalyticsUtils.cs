// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Mono.Cecil;
using static RealmWeaver.Metric;

namespace RealmWeaver
{
    internal static class AnalyticsUtils
    {
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

        public static void ComputeHostOSNameAndVersion(out string osType, out string osVersion)
        {
            var platformRegex = new Regex("^(?<platform>[^0-9]*) (?<version>[^ ]*)", RegexOptions.Compiled);
            var osDescription = platformRegex.Match(RuntimeInformation.OSDescription);
            if (osDescription.Success)
            {
                osType = osDescription.Groups["platform"].Value;
                osVersion = osDescription.Groups["version"].Value;
            }
            else
            {
                osType = Environment.OSVersion.Platform.ToString();
                osVersion = Environment.OSVersion.VersionString;
            }

            osType = ConvertOsNameToMetricsVersion(osType);
        }

        public static string GetHostCpuArchitecture =>
            WrapInTryCatch(() => ConvertArchitectureToMetricsVersion(RuntimeInformation.ProcessArchitecture.ToString()));

        public static string GetTargetCpuArchitecture(ModuleDefinition module) =>

            // TODO andrea: module.Architecture reports "I386" which isn't a value I could find in the documentation of MS. Investigate
            WrapInTryCatch(() => ConvertArchitectureToMetricsVersion(module.Architecture.ToString()));

        public static byte[] GenerateComputerIdentifier =>

            // Assume OS X if not Windows.
            NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.Name == "en0" || (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                .FirstOrDefault();

        // TODO andrea: add here check on timestamp file, maybe
        public static bool ShouldRunAnalytics =>
            Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") == null &&
                Environment.GetEnvironmentVariable("CI") == null;

        private static string ConvertOsNameToMetricsVersion(string osName) =>
            WrapInTryCatch(() =>
            {
                switch (osName)
                {
                    case string s when s.ContainsIgnoreCase(Metric.OperatingSystem.Windows):
                        return Metric.OperatingSystem.Windows;
                    case string s when s.ContainsIgnoreCase(Metric.OperatingSystem.MacOS):
                        return Metric.OperatingSystem.MacOS;
                    case string s when s.ContainsIgnoreCase(Metric.OperatingSystem.Linux):
                        return Metric.OperatingSystem.Linux;
                    default:
                        return $"{osName} is an unknown operating system.";
                }
            });

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
                        throw new ArgumentException($"{RuntimeInformation.ProcessArchitecture} is an unknown architecture");
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
