﻿// ////////////////////////////////////////////////////////////////////////////
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
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Mono.Cecil;
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

        public static string ConvertOsNameToMetricsVersion(PlatformID platformID) =>
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
