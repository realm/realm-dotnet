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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RealmWeaver;
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
                    var id = GenerateComputerIdentifier();
                    return id != null ? SHA256Hash(id) : "UNKNOWN";
                }
                catch
                {
                    return "UNKNOWN";
                }
            }
        }

        /*
        private Analytics.Config GetAnalyticsConfig(FrameworkName frameworkName)
        {
            var config = new Analytics.Config
            {
                Framework = "xamarin", // This is for backwards compatibility
                RunAnalytics = !disableAnalytics,
            };

            config.FrameworkVersion = frameworkName.Version.ToString();
            config.TargetOSName = GetTargetOSName(frameworkName);

            // For backward compatibility
            config.TargetOSVersion = frameworkName.Version.ToString();

            return config;
        }
        */

        public static string GetTargetOSName(FrameworkName frameworkName)
        {
            try
            {
                // Legacy reporting used ios, osx, and android
                switch (frameworkName.Identifier)
                {
                    case "Xamarin.iOS":
                        return "ios";
                    case "Xamarin.Mac":
                        return "osx";
                    case "MonoAndroid":
                    case "Mono.Android":
                        return "android";
                }

                if (frameworkName.Identifier.EndsWith("-android", StringComparison.OrdinalIgnoreCase))
                {
                    return "android";
                }

                if (frameworkName.Identifier.EndsWith("-ios", StringComparison.OrdinalIgnoreCase))
                {
                    return "ios";
                }

                if (frameworkName.Identifier.EndsWith("-maccatalyst", StringComparison.OrdinalIgnoreCase))
                {
                    return "osx";
                }
            }
            catch
            {
#if DEBUG
                // Make sure we get build failures and address the problem in debug,
                // but don't fail users' builds because of that.
                throw;
#endif
            }

            return "windows";
        }

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

            switch (osType)
            {
                case string n when n.Contains(Metric.OperatingSystem.Windows):
                    osType = Metric.OperatingSystem.Windows;
                    break;
                case string n when n.Contains(Metric.OperatingSystem.MacOS):
                    osType = Metric.OperatingSystem.MacOS;
                    break;
                case string n when n.Contains(Metric.OperatingSystem.Linux):
                    osType = Metric.OperatingSystem.Linux;
                    break;
                default:
                    DebugLog($"{osType} is not an operating system that we recognize.");
                    break;
            }
        }

        public static byte[] GenerateComputerIdentifier()
        {
            // Assume OS X if not Windows.
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(n => n.Name == "en0" || (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                                   .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                                   .FirstOrDefault();
        }

        // TODO andrea: add here check on timestamp file, maybe
        public static bool ShouldRunAnalytics =>
            Environment.GetEnvironmentVariable("REALM_DISABLE_ANALYTICS") == null &&
                Environment.GetEnvironmentVariable("CI") == null;

        public static void DebugLog(string message)
        {
            Debug.WriteLine($"** Analytics: {message}");
        }

        public static void ErrorLog(string message)
        {
            Console.WriteLine($"** Analytics, Error: {message}");
        }

        public static void WarningLog(string message)
        {
            Console.WriteLine($"** Analytics, Warning: {message}");
        }

        public static void InfoLog(string message)
        {
            Console.WriteLine($"** Analytics, Info: {message}");
        }
    }
}
