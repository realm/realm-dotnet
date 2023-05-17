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
using System.Runtime.InteropServices;
using ObjCRuntime;
using static Realms.PlatformHelpers.Platform;

namespace Realms.PlatformHelpers
{
    internal class DeviceInfo : IDeviceInfo
    {
        [DllImport(Constants.SystemLibrary, EntryPoint = "sysctlbyname")]
        internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        internal static string GetSystemLibraryProperty(string property)
        {
            var lengthPtr = Marshal.AllocHGlobal(sizeof(int));
            SysctlByName(property, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0);

            var propertyLength = Marshal.ReadInt32(lengthPtr);

            if (propertyLength == 0)
            {
                Marshal.FreeHGlobal(lengthPtr);
                throw new InvalidOperationException("Unable to read length of property.");
            }

            var valuePtr = Marshal.AllocHGlobal(propertyLength);
            SysctlByName(property, valuePtr, lengthPtr, IntPtr.Zero, 0);

            var returnValue = Marshal.PtrToStringAnsi(valuePtr);

            Marshal.FreeHGlobal(lengthPtr);
            Marshal.FreeHGlobal(valuePtr);

            return returnValue!;
        }

        private static readonly Lazy<string> _version = new(FindVersion);

        private static string FindVersion()
        {
            try
            {
                return GetSystemLibraryProperty("hw.machine")!;
            }
            catch
            {
            }

            return Unknown;
        }

        public string DeviceName => UIKit.UIDevice.CurrentDevice.Model;

        public string DeviceVersion => _version.Value;
    }
}
