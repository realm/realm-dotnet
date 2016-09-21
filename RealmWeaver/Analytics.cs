////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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
using System.Management;
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


    public interface IAnalyticsData
    {
        string MixPanelToken { get; }
        byte[] UserId { get; }
        string AppId { get; }
        string OsName { get; }
        string OsVersion { get; }
        string TargetOs { get; }
        string RealmVersion { get; }
    }

    public class AnalyticsData : IAnalyticsData
    {
        public string MixPanelToken => "ce0fac19508f6c8f20066d345d360fd0";
        private readonly ModuleDefinition _moduleDefinition;

        public byte[] UserId => GenerateComputerIdentifier();

        private static byte[] GenerateComputerIdentifier()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                    Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var scope = new ManagementScope("\\\\localhost\\root\\CIMV2", null);
                    scope.Connect();
                    var query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
                    var searcher = new ManagementObjectSearcher(scope, query);

                    var uuid = searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["UUID"] as string;
                    return string.IsNullOrEmpty(uuid) ? null : Encoding.UTF8.GetBytes(uuid);
                }
                else  // Assume OS X if not Windows.
                {
                    var macs = from nic in NetworkInterface.GetAllNetworkInterfaces()
                               where nic.Name == "en0"
                               select nic.GetPhysicalAddress().GetAddressBytes();
                    return macs.FirstOrDefault();
                }
            }
            catch
            {
                return null;
            }
        }

        public string AppId => _moduleDefinition.Name;

        public string OsName
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    // Mono completely messes up reporting the OS name and version for OS X, so...
                    /*   case PlatformID.MacOSX:
                    return "osx";
                case PlatformID.Unix:
                    return "linux";
                    */
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return "windows";
                    default:
                        return "osx";  // proved "windows" detected so default to "osx" for now
                }

            }
        }

        public string OsVersion => Environment.OSVersion.Version.ToString();

        public string TargetOs
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

        public string RealmVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        internal AnalyticsData(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
        }
    }

    public interface IAnalytics
    {
        void SubmitAnalytics(IAnalyticsData analyticsData, Action<Uri> makeRequest = null);
    }

    public class Analytics : IAnalytics
    {
        private static string SHA256Hash(byte[] bytes)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(bytes));
            }
        }

        private static string Anonymize(byte[] bytes)
        {
            return bytes != null ? SHA256Hash(bytes) : "UNKNOWN";
        }

        private static string Anonymize(string @string)
        {
            return SHA256Hash(Encoding.UTF8.GetBytes(@string));
        }

        private static string JsonTemplate => @"{
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

        private static string GetJsonPayload(IAnalyticsData analyticsData)
        {
            return JsonTemplate
                .Replace("%EVENT%", "Run")
                .Replace("%TOKEN%", analyticsData.MixPanelToken)
                .Replace("%USER_ID%", Anonymize(analyticsData.UserId))
                .Replace("%APP_ID%", Anonymize(analyticsData.AppId))

                // Version of weaver is expected to match that of the library.
                .Replace("%REALM_VERSION%", analyticsData.RealmVersion)
                .Replace("%OS_TYPE%", analyticsData.OsName)
                .Replace("%OS_VERSION%", analyticsData.OsVersion)
                .Replace("%TARGET_OS%", analyticsData.TargetOs);
        }

        public void SubmitAnalytics(IAnalyticsData analyticsData, Action<Uri> makeRequest = null)
        {
            if (makeRequest == null) makeRequest = MakeWebRequest;

            // uncomment next two lines to inspect the payload under Windows VS build
            //var load = JsonPayload;
            //Debugger.Launch();

            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(GetJsonPayload(analyticsData)));
            makeRequest(new Uri("https://api.mixpanel.com/track/?data=" + base64Payload + "&ip=1"));
        }

        private static void MakeWebRequest(Uri uri)
        {
            var request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            request.Timeout = 4000;
            request.ReadWriteTimeout = 2000;
            request.GetResponse();
        }
    }
}

