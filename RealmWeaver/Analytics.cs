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
    // Asynchronously submits build information to Realm when the assembly weaver
    // is running
    //
    // To be clear: this does *not* run when your app is in production or on
    // your end-user's devices; it will only run when you build your app from source.
    //
    // Why are we doing this? Because it helps us build a better product for you.
    // None of the data personally identifies you, your employer or your app, but it
    // *will* help us understand what Realm version you use, what host OS you use,
    // etc. Having this info will help with prioritizing our time, adding new
    // features and deprecating old features. Collecting an anonymized assembly name &
    // anonymized MAC is the only way for us to count actual usage of the other
    // metrics accurately. If we don't have a way to deduplicate the info reported,
    // it will be useless, as a single developer building their app on Windows ten
    // times would report 10 times more than a single developer that only builds
    // once from Mac OS X, making the data all but useless. No one likes sharing
    // data unless it's necessary, we get it, and we've debated adding this for a
    // long long time. Since Realm is a free product without an email signup, we
    // feel this is a necessary step so we can collect relevant data to build a
    // better product for you.
    //
    // Currently the following information is reported:
    // - What version of Realm is being used
    // - What OS you are running on
    // - What OS you are building for
    // - An anonymized MAC address and assembly name ID to aggregate the other information on.
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
      ""Binding"": ""dotnet"",
      ""Language"": ""c#"",
      ""Framework"": ""xamarin"",
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
                    return "ios";
                else if (_moduleDefinition.AssemblyReferences.Any(r => r.Name == "Xamarin.Mac"))
                    return "osx";
                else if (_moduleDefinition.AssemblyReferences.Any(r => r.Name == "Mono.Android"))
                    return "android";
                else
                    return "windows";  // in theory is generic .Net but Tim requested we use windows for now
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
        // uncomment next two lines to inspect the payload under Windows VS build
        //    var load = JsonPayload;
        //    Debugger.Launch();  

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
             /*   case PlatformID.MacOSX:
                    name = "osx";
                    break;
                case PlatformID.Unix:
                    name = "linux";
                    break; */
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    name = "windows";
                    break;
                default:
                    name = "osx";  // proved "windows" detected so default to "osx" for now
//                    name = Environment.OSVersion.Platform.ToString();
                    break;
            }
            version = Environment.OSVersion.Version.ToString();
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

