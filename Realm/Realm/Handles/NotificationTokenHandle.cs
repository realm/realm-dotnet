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

using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class NotificationTokenHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy_notificationtoken", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr destroy_notificationtoken(IntPtr token, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
        }

        public NotificationTokenHandle(NotifiableObjectHandleBase root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            var managedObjectHandle = NativeMethods.destroy_notificationtoken(handle, out var nativeException);
            nativeException.ThrowIfNecessary();
            GCHandle.FromIntPtr(managedObjectHandle).Free();
        }
    }
}