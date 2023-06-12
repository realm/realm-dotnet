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

namespace Realms.PlatformHelpers
{
    internal static class NativeHelpers
    {
        [DllImport(Constants.SystemLibrary, EntryPoint = "sysctlbyname")]
        internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        internal static string? GetSysctlProperty(string property)
        {
            var lengthPtr = Marshal.AllocHGlobal(sizeof(int));

            IntPtr? valuePtr = null;
            try
            {
                SysctlByName(property, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0);

                var propertyLength = Marshal.ReadInt32(lengthPtr);

                if (propertyLength > 0)
                {
                    valuePtr = Marshal.AllocHGlobal(propertyLength);
                    SysctlByName(property, valuePtr.Value, lengthPtr, IntPtr.Zero, 0);

                    return Marshal.PtrToStringAnsi(valuePtr.Value);
                }
            }
            catch
            {
            }
            finally
            {
                Marshal.FreeHGlobal(lengthPtr);

                if (valuePtr.HasValue)
                {
                    Marshal.FreeHGlobal(valuePtr.Value);
                }
            }

            return null;
        }
    }
}
