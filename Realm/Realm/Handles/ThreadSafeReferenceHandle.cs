////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms
{
    internal class ThreadSafeReferenceHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "thread_safe_reference_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_thread_safe_reference_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy_realm_reference(IntPtr handle);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        private bool _isRealmReference;

        [Preserve]
        public ThreadSafeReferenceHandle(IntPtr handle, bool isRealmReference = false) : base(null, handle)
        {
            _isRealmReference = isRealmReference;
        }

        protected override unsafe void Unbind()
        {
            // This is a bit awkward because ThreadSafeReference<Realm> doesn't inherit from ThreadSafeReferenceBase
            if (_isRealmReference)
            {
                NativeMethods.destroy_realm_reference(handle);
            }
            else
            {
                NativeMethods.destroy(handle);
            }
        }
    }
}
