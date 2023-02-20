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

#nullable enable

using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class AsyncOpenTaskHandle : StandaloneHandle
    {
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
        }

        public void Cancel()
        {
            NativeMethods.cancel(this, out var ex);
            ex.ThrowIfNecessary();
        }

        protected override void Unbind() => NativeMethods.destroy(handle);

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
