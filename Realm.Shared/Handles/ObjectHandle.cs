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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class ObjectHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_is_valid(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_row_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_row_index(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr objectHandle);
        }

        public bool IsValid
        {
            get
            {
                NativeException nativeException;
                var result = NativeMethods.get_is_valid(this, out nativeException);
                nativeException.ThrowIfNecessary();
                return result == (IntPtr)1;  // inline equiv of IntPtrToBool
            }
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
                        {
                            return new RealmClosedException("This object belongs to a closed realm.");
                        }

                        return new RealmInvalidObjectException("This object is detached. Was it deleted from the realm?");
                    }

                    return null;
                });

                return result;
            }
        }

        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public ObjectHandle(SharedRealmHandle sharedRealmHandle) : base(sharedRealmHandle)
        {
        }

        public override bool Equals(object obj)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return RowIndex == (obj as ObjectHandle)?.RowIndex;
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        // acquire a LinkListHandle from table_get_linklist And set root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal LinkListHandle TableLinkList(IntPtr columnIndex)
        {
            var listHandle = new LinkListHandle(Root ?? this);

            // At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            // a finalize at this point will not leak anything and the handle will not do anything

            // now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions(); // the following finally will run with no out-of-band exceptions
            try
            {
            }
            finally
            {
                listHandle.SetHandle(NativeTable.GetLinklist(this, columnIndex));
            } // at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly

            return listHandle;
        }
    }
}