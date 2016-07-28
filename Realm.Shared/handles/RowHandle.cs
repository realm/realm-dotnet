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
    internal class RowHandle: RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_get_row_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_row_index(RowHandle rowHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_get_is_attached",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_is_attached(RowHandle rowHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr rowHandle);

        }

        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public RowHandle(SharedRealmHandle sharedRealmHandle) : base(sharedRealmHandle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public IntPtr RowIndex
        {
            get
            {
                NativeException nativeException;
                var result = NativeMethods.get_row_index(this, out nativeException);
                nativeException.ThrowIfNecessary(type =>
                {
                    if (type == RealmExceptionCodes.RealmRowDetached)
                    {
                        if (Root.IsClosed)
                            return new RealmClosedException("This object belongs to a closed realm.");
                        else
                            return new RealmInvalidObjectException("This object is detached. Was it deleted from the realm?");
                    }
                    return null;
                });
                return result;
            }
        }

        public bool IsAttached
        {
            get
            {
                NativeException nativeException;
                var result = NativeMethods.get_is_attached(this, out nativeException);
                nativeException.ThrowIfNecessary();
                return result == (IntPtr) 1;  // inline equiv of IntPtrToBool
            }
        }

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, p))
            {
                return true;
            }

            return ((RowHandle) p).RowIndex == RowIndex;
        }
    }
}