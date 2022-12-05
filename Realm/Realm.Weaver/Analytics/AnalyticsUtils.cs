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
                frameworkName.Identifier switch
                {
                    string s when s.ContainsIgnoreCase("android") => Metric.OperatingSystem.Android,
                    string s when s.ContainsIgnoreCase("ios") => Metric.OperatingSystem.Ios,
                    string s when s.ContainsIgnoreCase("mac") ||
                        s.ContainsIgnoreCase("macos") ||
                        s.ContainsIgnoreCase("maccatalyst") => Metric.OperatingSystem.MacOS,
                    string s when s.ContainsIgnoreCase("tvos") => Metric.OperatingSystem.TvOs,
                    _ => Metric.OperatingSystem.Windows,
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

        public static void DebugLog(string message) => Debug.WriteLine($"** Analytics: {message}");

        public static void ErrorLog(string message) => Console.WriteLine($"** Analytics, Error: {message}");

        public static void WarningLog(string message) => Console.WriteLine($"** Analytics, Warning: {message}");

        public static void InfoLog(string message) => Console.WriteLine($"** Analytics, Info: {message}");

        private static string ConvertOsNameToMetricsVersion(string osName) =>
            WrapInTryCatch(() =>
                osName switch
                {
                    string s when s.ContainsIgnoreCase(Metric.OperatingSystem.Windows) => Metric.OperatingSystem.Windows,
                    string s when s.ContainsIgnoreCase(Metric.OperatingSystem.MacOS) => Metric.OperatingSystem.MacOS,
                    string s when s.ContainsIgnoreCase(Metric.OperatingSystem.Linux) => Metric.OperatingSystem.Linux,
                    _ => $"{osName} is an unknown operating system."
                });

        private static string ConvertArchitectureToMetricsVersion(string arch) =>
            arch switch
            {
                string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.Arm)) => CpuArchitecture.Arm,
                string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.Arm64)) => CpuArchitecture.Arm64,
                string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.X64)) ||
                    s.ContainsIgnoreCase("amd64") => CpuArchitecture.X64,
                string s when s.ContainsIgnoreCase(nameof(CpuArchitecture.X86)) ||
                    s.ContainsIgnoreCase("i386") => CpuArchitecture.X86,
                _ => throw new ArgumentException($"{RuntimeInformation.ProcessArchitecture} is an unknown architecture")
            };

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
#endif
            }
        }
    }
}
