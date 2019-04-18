////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using NativeSyncConfiguration = Realms.Sync.Native.SyncConfiguration;

namespace Realms.Server
{
    internal class NotifierHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate bool ShouldHandleCallback(IntPtr managedNotifier, byte* path_buf, IntPtr path_len);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void EnqueueCalculationCallback(IntPtr managedNotifier, byte* path_buf, IntPtr path_len, IntPtr calculator_ptr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void StartCallback(IntPtr task_completion_source, int error_code, byte* message_buf, IntPtr message_len);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_server_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(
                ShouldHandleCallback should_handle_callback,
                EnqueueCalculationCallback enqueue_calculation_callback,
                StartCallback start_callback,
                NotifierNotificationHandle.CalculationCompleteCallback calculation_complete_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_server_create_global_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_notifier(IntPtr managedInstance,
                NativeSyncConfiguration sync_configuration,
                IntPtr task_completion_source,
                byte[] encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_server_global_notifier_get_realm_for_writing", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_realm_for_writing(SharedRealmHandle currentRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_server_global_notifier_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);
        }

        static unsafe NotifierHandle()
        {
            NativeCommon.Initialize();

            NativeMethods.ShouldHandleCallback shouldHandle = Notifier.ShouldHandle;
            NativeMethods.EnqueueCalculationCallback enqueueCalculation = Notifier.EnqueueCalculation;
            NativeMethods.StartCallback start = Notifier.OnStarted;
            NotifierNotificationHandle.CalculationCompleteCallback calculationComplete = Notifier.OnCalculationCompleted;

            GCHandle.Alloc(shouldHandle);
            GCHandle.Alloc(enqueueCalculation);
            GCHandle.Alloc(start);
            GCHandle.Alloc(calculationComplete);

            NativeMethods.install_callbacks(shouldHandle, enqueueCalculation, start, calculationComplete);
        }

        public static NotifierHandle CreateHandle(GCHandle managedNotifier, NotifierConfiguration configuration, TaskCompletionSource<object> tcs)
        {
            var nativeConfig = configuration.ToNative();
            var tcsHandle = GCHandle.Alloc(tcs);
            var result = NativeMethods.create_notifier(GCHandle.ToIntPtr(managedNotifier), nativeConfig, GCHandle.ToIntPtr(tcsHandle), configuration.EncryptionKey,
                                                       out var nativeException);
            nativeException.ThrowIfNecessary();

            return new NotifierHandle(result);
        }

        public static IntPtr GetRealmForWriting(SharedRealmHandle currentRealm)
        {
            var result = NativeMethods.get_realm_for_writing(currentRealm, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        protected NotifierHandle(IntPtr handle) : base(null, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
