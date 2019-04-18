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
using Realms.Server.Native;

namespace Realms.Server
{
    internal class NotifierNotificationHandle : RealmHandle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CalculationCompleteCallback(IntPtr details, IntPtr managedCallbackPtr);

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_server_global_notifier_notification_get_changes", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_changes(NotifierNotificationHandle handle, IntPtr callback, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_server_global_notifier_notification_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);
        }

        public void GetChanges(Action<NativeChangeDetails?> callback)
        {
            var handle = GCHandle.Alloc(callback);
            NativeMethods.get_changes(this, GCHandle.ToIntPtr(handle), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        internal NotifierNotificationHandle(IntPtr handle) : base(null, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
