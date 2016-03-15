/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Mono.Cecil;

namespace RealmWeaver
{
    internal class Analytics
    {
        private const string MixPanelToken = "ce0fac19508f6c8f20066d345d360fd0";
        private const string JsonTemplate = @"{
   ""event"": ""%EVENT%"",
   ""properties"": {
      ""token"": ""%TOKEN%"",
      ""distinct_id"": ""%USER_ID%"",
      ""Anonymized MAC Address"": ""%USER_ID%"",
      ""Anonymized Bundle ID"": ""%APP_ID%"",
      ""Binding"": ""c#"",
      ""Realm Version"": ""%REALM_VERSION%"",
      ""Host OS Type"": ""%OS_TYPE%"",
      ""Host OS Version"": ""%OS_VERSION%"",
      ""Target OS Type"": ""%TARGET_OS%""
   }
}";

        private readonly ModuleDefinition _moduleDefinition;

        private static string AnonymizedUserID
        {
            get
            {
                var macs = from nic in NetworkInterface.GetAllNetworkInterfaces()
                           where nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.OperationalStatus == OperationalStatus.Up
                           select nic.GetPhysicalAddress().GetAddressBytes();
                var mac = macs.FirstOrDefault();
                if (mac == null)
                {
                    throw new Exception("No network interface detected.");
                }

                return SHA256Hash(mac);
            }
        }

        private string AnonymizedAppID
        {
            get
            {
                return SHA256Hash(System.Text.Encoding.UTF8.GetBytes(_moduleDefinition.Name));
            }
        }

        private string TargetOS
        {
            get
            {
                if (_moduleDefinition.AssemblyReferences.Any(r => r.Name == "Xamarin.iOS"))
                    return "iOS";
                else if (_moduleDefinition.AssemblyReferences.Any(r => r.Name == "Xamarin.Mac"))
                    return "Mac OS X";
                else if (_moduleDefinition.AssemblyReferences.Any(r => r.Name == "Mono.Android"))
                    return "Android";
                else
                    return "Generic .net";
                // TODO: figure out a way to tell whether we're building for Windows, UWP, PCL, Unity(?), etc. if we wanted to
            }
        }

        private string JsonPayload
        {
            get
            {
                string osName, osVersion;
                ComputeHostOSNameAndVersion(out osName, out osVersion);

                return JsonTemplate
                    .Replace("%EVENT%", "Run")
                    .Replace("%TOKEN%", MixPanelToken)
                    .Replace("%USER_ID%", AnonymizedUserID)
                    .Replace("%APP_ID%", AnonymizedAppID)
                // TODO: figure out a better way to get the Realm version, cause this ain't it right now
                    .Replace("%REALM_VERSION%", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
                    .Replace("%OS_TYPE%", osName)
                    .Replace("%OS_VERSION%", osVersion)
                    .Replace("%TARGET_OS%", TargetOS);
            }
        }

        internal Analytics(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
        }

        internal void SubmitAnalytics()
        {
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonPayload));
            var request = HttpWebRequest.CreateHttp(new Uri("https://api.mixpanel.com/track/?data=" + base64Payload + "&ip=1"));
            request.Method = "GET";
            request.Timeout = 4000;
            request.ReadWriteTimeout = 2000;
            request.GetResponse();
        }

        private static string SHA256Hash(byte[] bytes)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(bytes));
            }

        }

        private static void ComputeHostOSNameAndVersion(out string name, out string version)
        {
            switch (Environment.OSVersion.Platform)
            {
            // Mono completely messes up reporting the OS name and version for OS X, so...
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    if (MacOSXVersion.ComputeSwVersionOSNameAndVersion(out name, out version))
                        return;
                // in case the above fails, falback to the generic method below
                    goto default;
                default:
                    name = Environment.OSVersion.Platform.ToString();
                    version = Environment.OSVersion.Version.ToString();
                    break;
            }
        }

        private static class MacOSXVersion
        {
            private static bool RunSwVersion(string argument, out string output)
            {
                var process = Process.Start(new ProcessStartInfo("sw_vers", argument)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    });
                output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return process.ExitCode == 0;
            }

            internal static bool ComputeSwVersionOSNameAndVersion(out string name, out string version)
            {
                version = null;
                return RunSwVersion("-productName", out name) && RunSwVersion("-productVersion", out version);
            }
        }
    }
}

