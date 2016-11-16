////////////////////////////////////////////////////////////////////////////
//
// Copyright 2014 Realm Inc.
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
using System.Diagnostics;
using System.Linq;
using Realms;

namespace DrawXShared
{
    internal static class DrawXSettingsManager
    {
        private static Realm _realmLocalSettings;
        private static DrawXSettings _savedSettings;

        public static DrawXSettings Settings
        {
            get
            {
                if (_savedSettings == null)
                    _savedSettings = _realmLocalSettings.All<DrawXSettings>().FirstOrDefault();
                if (_savedSettings == null)
                {
                    _realmLocalSettings.Write(() =>
                    {
                        _savedSettings = _realmLocalSettings.CreateObject<DrawXSettings>();
                        _savedSettings.LastColorUsed = "Indigo";
                    });
                }
                return _savedSettings;
            }
        }


        internal static void InitLocalSettings()
        {
            var settingsConf = new RealmConfiguration("DrawXsettings.realm");
            settingsConf.ObjectClasses = new[] { typeof(DrawXSettings) };
            settingsConf.SchemaVersion = 2;  // set explicitly and bump as we add setting properties
            _realmLocalSettings = Realm.GetInstance(settingsConf);
        }

        internal static void Write(Action writer)
        {
            _realmLocalSettings.Write(writer);
        }


        internal static void UpdateCredentials(string serverIP, string username, string password)
        {
            if (!serverIP.Contains(":"))
            {
                // assume they forgot port so add standard port
                serverIP += ":9080";
            }
            _realmLocalSettings.Write(() =>
            {
                _savedSettings.ServerIP = serverIP;
                _savedSettings.Username = username;
                _savedSettings.Password = password;
            });
        }


        internal static bool HasCredentials()
        {
            return _savedSettings != null &&
                !string.IsNullOrEmpty(_savedSettings.Username) &&
                !string.IsNullOrEmpty(_savedSettings.Password) &&
                !string.IsNullOrEmpty(_savedSettings.ServerIP);
        }

    }
}
