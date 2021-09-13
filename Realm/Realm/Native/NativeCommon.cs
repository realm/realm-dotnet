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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Realms.Logging;
using Realms.Native;
using Realms.Sync;

namespace Realms
{
    internal static class NativeCommon
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void delete_pointer(void* pointer);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        private static int _isInitialized;

        internal static void Initialize()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                var platform = Environment.OSVersion.Platform;

                if (platform == PlatformID.Win32NT)
                {
                    _ = TryLoadWindowsWrappers("lib\\win32") || // This is the path for regular windows apps using NuGet.
                        TryLoadWindowsWrappers("..\\Plugins", isUnityTarget: true) || // This is the path for Unity apps built as standalone.
                        TryLoadWindowsWrappers("Windows", isUnityTarget: true); // This is the path in the Unity package - it is what the Editor uses.
                }

                SynchronizationContextScheduler.Install();
                SharedRealmHandle.Initialize();
                SessionHandle.InstallCallbacks();
                HttpClientTransport.Install();
                AppHandle.Initialize();
            }
        }

        /// <summary>
        /// **WARNING**: This will close all native Realm instances and AppHandles. This method is extremely unsafe
        /// to call in any circumstance where the user might be accessing anything Realm-related. The only places
        /// where we do call it is in DomainUnload and Application.quitting on Unity. We expect that at this point
        /// the Application/Domain is being torn down and the user should not be interracting with Realm.
        /// </summary>
        public static void CleanupNativeResources(string reason)
        {
            try
            {
                Logger.LogDefault(LogLevel.Info, $"Realm: Force closing all native instances: {reason}");

                var sw = new Stopwatch();
                sw.Start();

                AppHandle.ForceCloseHandles();
                SharedRealmHandle.ForceCloseNativeRealms();

                sw.Stop();
                Logger.LogDefault(LogLevel.Info, $"Realm: Closed all native instances in {sw.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                Logger.LogDefault(LogLevel.Error, $"Realm: Failed to close all native instances. You may need to restart your app. Error: {ex}");
            }
        }

        private static bool TryLoadWindowsWrappers(string relativePath, bool isUnityTarget = false)
        {
            try
            {
                var assemblyLocation = Path.GetDirectoryName(typeof(NativeCommon).GetTypeInfo().Assembly.Location);

                var expectedFilePath = Path.GetFullPath(Path.Combine(assemblyLocation, relativePath, getArchitecture()));
                if (Directory.Exists(expectedFilePath))
                {
                    SetDllDirectory(expectedFilePath);

                    return true;
                }
            }
            catch
            {
            }

            return false;

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
