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
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Versioning;
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
    // long long time. Since Realm is a free product without an email sign-up, we
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
      ""Sync Enabled"": ""%SYNC_ENABLED%"",
      ""Realm Version"": ""%REALM_VERSION%"",
      ""Host OS Type"": ""%OS_TYPE%"",
      ""Host OS Version"": ""%OS_VERSION%"",
      ""Target OS Type"": ""%TARGET_OS%"",
      ""Target OS Version"": ""%TARGET_OS_VERSION%""
   }
}";

        private readonly FrameworkName _frameworkName;
        private readonly bool _isSyncEnabled;
        private readonly string _anonymizedAppID;

        private static string AnonymizedUserID
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

        private static byte[] GenerateComputerIdentifier()
        {
            // Assume OS X if not Windows.
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(n => n.Name == "en0")
                                   .Select(n => n.GetPhysicalAddress().GetAddressBytes())
                                   .FirstOrDefault();
        }

        private string JsonPayload
        {
            get
            {
                ComputeHostOSNameAndVersion(out var osName, out var osVersion);
                ComputeTargetOSNameAndVersion(out var targetName, out var targetVersion);
                return JsonTemplate
                    .Replace("%EVENT%", "Run")
                    .Replace("%TOKEN%", MixPanelToken)
                    .Replace("%USER_ID%", AnonymizedUserID)
                    .Replace("%APP_ID%", _anonymizedAppID)

                    .Replace("%SYNC_ENABLED%", _isSyncEnabled.ToString())

                    // Version of weaver is expected to match that of the library.
                    .Replace("%REALM_VERSION%", Assembly.GetExecutingAssembly().GetName().Version.ToString())

                    .Replace("%OS_TYPE%", osName)
                    .Replace("%OS_VERSION%", osVersion)
                    .Replace("%TARGET_OS%", targetName)
                    .Replace("%TARGET_OS_VERSION%", targetVersion);
            }
        }

        internal Analytics(FrameworkName frameworkName, string moduleName, bool isUsingSync)
        {
            _anonymizedAppID = SHA256Hash(Encoding.UTF8.GetBytes(moduleName));
            _frameworkName = frameworkName;
            _isSyncEnabled = isUsingSync;
        }

        internal string SubmitAnalytics()
        {
            var payload = JsonPayload;

            // uncomment next line to inspect the payload under Windows VS build
            // Debugger.Launch();
#if !DEBUG
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            // this will need to go when mixPanel won't be used anymore
            SendRequest(
                "https://api.mixpanel.com/track/?data=",
                base64Payload,
                "&ip=1");

            SendRequest(
                "https://webhooks.mongodb-realm.com/api/client/v2.0/app/realmsdkmetrics-zmhtm/service/metric_webhook/incoming_webhook/metric?data=",
                base64Payload,
                string.Empty);
#endif

            return payload;
        }

        private static void SendRequest(string prefixAddr, string payload, string suffixAddr)
        {
            var request = System.Net.HttpWebRequest.CreateHttp(new Uri(prefixAddr + payload + suffixAddr));
            request.Method = "GET";
            request.Timeout = 4000;
            request.ReadWriteTimeout = 2000;
            request.GetResponse();
        }

        private void ComputeTargetOSNameAndVersion(out string name, out string version)
        {
            version = "UNKNOWN";

            // Default to windows for backward compatibility
            name = "windows";

            try
            {
                // Legacy reporting used ios, osx, and android
                switch (_frameworkName.Identifier)
                {
                    case "Xamarin.iOS":
                        name = "ios";
                        break;
                    case "Xamarin.Mac":
                        name = "osx";
                        break;
                    case "MonoAndroid":
                    case "Mono.Android":
                        name = "android";
                        break;
                    default:
                        name = _frameworkName.Identifier;
                        break;
                }

                version = _frameworkName.Version.ToString();
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
                    name = "osx";  //// proved "windows" detected so default to "osx" for now
                                   //// name = Environment.OSVersion.Platform.ToString();
                    break;
            }

            version = Environment.OSVersion.Version.ToString();
        }
    }
}