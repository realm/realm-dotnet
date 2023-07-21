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
using System.Runtime.InteropServices;
using System.Threading;
using Realms.Helpers;
using Realms.Logging;
using Realms.Native;
using Realms.Sync;

namespace Realms
{
    internal static class NativeCommon
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void delete_pointer(void* pointer);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "_realm_flip_guid_for_testing", CallingConvention = CallingConvention.Cdecl)]
        public static extern void flip_guid_for_testing([In, Out] byte[] guid_bytes);

        private static int _isInitialized;

        internal static void Initialize()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
#if NET5_0_OR_GREATER
                if (OperatingSystem.IsIOS() || OperatingSystem.IsTvOS())
                {
                    NativeLibrary.SetDllImportResolver(typeof(NativeCommon).Assembly, (libraryName, assembly, searchPath) =>
                    {
                        if (libraryName == InteropConfig.DLL_NAME)
                        {
                            libraryName = "@rpath/realm-wrappers.framework/realm-wrappers";
                        }

                        return NativeLibrary.Load(libraryName, assembly, searchPath);
                    });
                }
#else
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // This is the path for regular windows apps using NuGet.
                    AddWindowsWrappersToPath("lib\\win32");

                    // This is the path for Unity apps built as standalone.
                    AddWindowsWrappersToPath("..\\Plugins", isUnityTarget: true);

                    // This is the path in the Unity package - it is what the Editor uses.
                    AddWindowsWrappersToPath("Windows", isUnityTarget: true);
                }
#endif

                SynchronizationContextScheduler.Initialize();
                SharedRealmHandle.Initialize();
                SessionHandle.Initialize();
                HttpClientTransport.Initialize();
                AppHandle.Initialize();
                SubscriptionSetHandle.Initialize();

                SerializationHelper.Initialize();
            }
        }

        /// <summary>
        /// **WARNING**: This will close all native Realm instances and AppHandles. This method is extremely unsafe
        /// to call in any circumstance where the user might be accessing anything Realm-related. The only places
        /// where we do call it is in DomainUnload and Application.quitting on Unity. We expect that at this point
        /// the Application/Domain is being torn down and the user should not be interacting with Realm.
        /// </summary>
        public static void CleanupNativeResources(string reason)
        {
            try
            {
                if (Interlocked.CompareExchange(ref _isInitialized, 0, 1) == 1)
                {
                    Logger.LogDefault(LogLevel.Info, $"Realm: Force closing all native instances: {reason}");

                    var sw = new Stopwatch();
                    sw.Start();

                    AppHandle.ForceCloseHandles();
                    AsyncOpenTaskHandle.CancelInFlightTasks();
                    SharedRealmHandle.ForceCloseNativeRealms();

                    sw.Stop();
                    Logger.LogDefault(LogLevel.Info, $"Realm: Closed all native instances in {sw.ElapsedMilliseconds} ms.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogDefault(LogLevel.Error, $"Realm: Failed to close all native instances. You may need to restart your app. Error: {ex}");
            }
        }

#if !NET5_0_OR_GREATER

        private static void AddWindowsWrappersToPath(string relativePath, bool isUnityTarget = false)
        {
            try
            {
                var assemblyLocation = Path.GetDirectoryName(typeof(NativeCommon).Assembly.Location)!;

                // Unity doesn't support arm/arm64 builds for windows - only through UWP, so we're not
                // special-casing the naming there.
                var architecture = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "x86",
                    Architecture.X64 => isUnityTarget ? "x86_64" : "x64",
                    Architecture.Arm64 => "arm64",
                    _ => throw new NotSupportedException($"Unknown architecture: {RuntimeInformation.ProcessArchitecture}"),
                };

                var expectedFilePath = Path.GetFullPath(Path.Combine(assemblyLocation, relativePath, architecture));
                var path = expectedFilePath + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            }
            catch
            {
            }
        }
#endif
    }
}
