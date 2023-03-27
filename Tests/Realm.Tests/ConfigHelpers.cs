////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Realms.Tests
{
    internal class ConfigHelpers
    {
        private static readonly Dictionary<string, string> _settings = new();

        static ConfigHelpers()
        {
            if (File.Exists("App.Local.config"))
            {
                var doc = new XmlDocument();
                doc.Load("App.Local.config");

                foreach (var child in doc["appSettings"]?.ChildNodes?.OfType<XmlElement>() ?? Enumerable.Empty<XmlElement>())
                {
                    if (child.Name == "add")
                    {
                        _settings[child.GetAttribute("key")] = child.GetAttribute("value");
                    }
                }
            }
        }

        public static string? GetSetting(string key)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
