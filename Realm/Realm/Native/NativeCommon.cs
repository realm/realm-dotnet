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

// file NativeCommon.cs provides mappings to common functions that don't fit the Table classes etc.
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Realms.Native;

namespace Realms
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
    internal static class NativeCommon
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NotifyRealmCallback(IntPtr stateHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool NotifyRealmObjectCallback(IntPtr realmObjectHandle, IntPtr propertyIndex);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeGCHandleCallback(IntPtr handle);

#if DEBUG
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void DebugLoggerCallback(byte* utf8String, IntPtr stringLen);

        [NativeCallback(typeof(DebugLoggerCallback))]
        private static unsafe void DebugLogger(byte* utf8String, IntPtr stringLen)
        {
            var message = Encoding.UTF8.GetString(utf8String, (int)stringLen);
            Console.WriteLine(message);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_debug_logger", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_debug_logger(DebugLoggerCallback callback);
#endif  // DEBUG

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "register_notify_realm_changed", CallingConvention = CallingConvention.Cdecl)]
        public static extern void register_notify_realm_changed(NotifyRealmCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void delete_pointer(void* pointer);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "register_notify_realm_object_changed", CallingConvention = CallingConvention.Cdecl)]
        public static extern void register_notify_realm_object_changed(NotifyRealmObjectCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_gchandle_deleter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void install_gchandle_deleter(FreeGCHandleCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_reset_for_testing", CallingConvention = CallingConvention.Cdecl)]
        public static extern void reset_for_testing();

        public static unsafe void Initialize()
        {
            if (RuntimeInformation.FrameworkDescription.Contains(".NET Framework") &&
                RuntimeInformation.OSDescription.Contains("Windows"))
            {
                var assemblyLocationPI = typeof(Assembly).GetProperty("Location", BindingFlags.Public | BindingFlags.Instance);
                if (assemblyLocationPI != null)
                {
                    var assemblyLocation = Path.GetDirectoryName((string)assemblyLocationPI.GetValue(typeof(NativeCommon).GetTypeInfo().Assembly));
                    var architecture = InteropConfig.Is64BitProcess ? "x64" : "x86";
                    var path = Path.Combine(assemblyLocation, "lib", "win32", architecture) + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
                    Environment.SetEnvironmentVariable("PATH", path);
                }
            }

#if DEBUG
            DebugLoggerCallback logger = DebugLogger;
            GCHandle.Alloc(logger);
            set_debug_logger(logger);
#endif

            FreeGCHandleCallback gchandleDeleter = FreeGCHandle;
            GCHandle.Alloc(gchandleDeleter);
            install_gchandle_deleter(gchandleDeleter);
        }

        [NativeCallback(typeof(FreeGCHandleCallback))]
        public static void FreeGCHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                GCHandle.FromIntPtr(handle).Free();
            }
        }
    }
}