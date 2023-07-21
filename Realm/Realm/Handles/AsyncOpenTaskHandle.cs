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
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class AsyncOpenTaskHandle : StandaloneHandle
    {
        private static ConcurrentDictionary<AsyncOpenTaskHandle, bool> _handles = new();

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_asyncopentask_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr asyncTaskHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_asyncopentask_cancel", CallingConvention = CallingConvention.Cdecl)]
            public static extern void cancel(AsyncOpenTaskHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_asyncopentask_register_progress_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong register_progress_notifier(AsyncOpenTaskHandle handle, IntPtr token_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_asyncopentask_unregister_progress_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern void unregister_progress_notifier(AsyncOpenTaskHandle handle, ulong token, out NativeException ex);
        }

        public AsyncOpenTaskHandle(IntPtr handle) : base(handle)
        {
            _handles.TryAdd(this, true);
        }

        public void Cancel()
        {
            NativeMethods.cancel(this, out var ex);
            ex.ThrowIfNecessary();
        }

        protected override void Unbind()
        {
            _handles.TryRemove(this, out _);

            NativeMethods.destroy(handle);
        }

        /// <summary>
        /// Cancels all in-flight async open tasks. This should only be used when the domain is being torn down.
        /// The case this handles is:
        /// 1. GetInstanceAsync.
        /// 2. Domain Reload wipes all coordinator caches.
        /// 3. AsyncOpen completes, calls back into managed (because s_can_call_managed is true again).
        /// 4. Undefined behavior as the state from before the domain reload is no longer valid.
        /// </summary>
        /// <remarks>This fixes the issue reported in https://github.com/realm/realm-dotnet/issues/3344.</remarks>
        public static void CancelInFlightTasks()
        {
            var keys = _handles.Keys;
            foreach (var value in keys)
            {
                value.Cancel();
            }
        }

        public ulong RegisterProgressNotifier(GCHandle managedHandle)
        {
            var token = NativeMethods.register_progress_notifier(this, GCHandle.ToIntPtr(managedHandle), out var ex);
            ex.ThrowIfNecessary();
            return token;
        }

        public void UnregisterProgressNotifier(ulong token)
        {
            NativeMethods.unregister_progress_notifier(this, token, out var ex);
            ex.ThrowIfNecessary();
        }
    }
}
