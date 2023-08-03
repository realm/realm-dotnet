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

using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Realms.PlatformHelpers
{
    internal static class Platform
    {
        public const string Unknown = "";

        private static IDeviceInfo? _deviceInfo;

        private static readonly Lazy<IDeviceInfo> _deviceInfoLazy = new(() => _deviceInfo ?? new DeviceInfo());

        public static IDeviceInfo DeviceInfo
        {
            get => _deviceInfoLazy.Value;
            set
            {
                if (_deviceInfoLazy.IsValueCreated)
                {
                    throw new Exception("DeviceInfo should only be configured once");
                }

                _deviceInfo = value;
            }
        }

        private static string? _bundleId;
        public static string BundleId
        {
            get
            {
                var bundleId = _bundleId ?? Assembly.GetEntryAssembly()?.GetName().Name;

                if (bundleId == null)
                {
                    // On Android, the entry assembly is null (there's no main() method), so we need to find
                    // the first assembly that has the ResourceDesignerAttribute with IsApplication = true.
                    var entryAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.CustomAttributes.Any(att =>
                            att.AttributeType.FullName == "Android.Runtime.ResourceDesignerAttribute" &&
                            att.NamedArguments?.FirstOrDefault(arg => arg.MemberName == "IsApplication").TypedValue.Value is true));

                    bundleId = entryAssembly?.GetName().Name;
                }

                return Sha256(bundleId);
            }
            set => _bundleId = value;
        }

        internal static string Sha256(string? value)
        {
            if (value == null)
            {
                return Unknown;
            }

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(hash);
        }
    }
}
