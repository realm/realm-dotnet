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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Realms.Native;

namespace Realms
{
    internal static class NativeCommon
    {
#if DEBUG
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void DebugLoggerCallback(byte* utf8String, IntPtr stringLen);

        [MonoPInvokeCallback(typeof(DebugLoggerCallback))]
        private static unsafe void DebugLogger(byte* utf8String, IntPtr stringLen)
        {
            var message = Encoding.UTF8.GetString(utf8String, (int)stringLen);
            Console.WriteLine(message);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_debug_logger", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_debug_logger(DebugLoggerCallback callback);
#endif  // DEBUG

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void delete_pointer(void* pointer);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void delete_pointer(IntPtr pointer);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_reset_for_testing", CallingConvention = CallingConvention.Cdecl)]
        public static extern void reset_for_testing();

        private static int _isInitialized;

        internal static unsafe void Initialize()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                var platform = Environment.OSVersion.Platform;

                if (platform == PlatformID.Win32NT)
                {
                    // This is the path for regular windows apps using NuGet.
                    AddWindowsWrappersToPath("lib\\win32");

                    // This is the path for Unity apps built as standalone.
                    AddWindowsWrappersToPath("..\\Plugins", isUnityTarget: true);

                    // This is the path in the Unity package - it is what the Editor uses.
                    AddWindowsWrappersToPath("Windows", isUnityTarget: true);
                }

#if DEBUG
                DebugLoggerCallback logger = DebugLogger;
                GCHandle.Alloc(logger);
                set_debug_logger(logger);
#endif

                SynchronizationContextScheduler.Install();
            }
        }

        private static void AddWindowsWrappersToPath(string relativePath, bool isUnityTarget = false)
        {
            try
            {
                var assemblyLocation = Path.GetDirectoryName(typeof(NativeCommon).GetTypeInfo().Assembly.Location);

                var expectedFilePath = Path.GetFullPath(Path.Combine(assemblyLocation, relativePath, getArchitecture()));
                var path = expectedFilePath + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            }
            catch
            {
            }

            string getArchitecture()
            {
                if (!Environment.Is64BitProcess)
                {
                    return "x86";
                }

                return isUnityTarget ? "x86_64" : "x64";
            }
        }
    }
}