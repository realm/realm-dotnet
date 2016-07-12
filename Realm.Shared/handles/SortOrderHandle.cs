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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class SortOrderHandle: RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sortorder_create_for_table", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_for_table(TableHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sortorder_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr sortHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sortorder_add_clause", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_clause(SortOrderHandle sortOrderHandle,
                [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen, IntPtr ascending, out NativeException ex);

        }

        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a SortOrderHandle when returning a size_t as a SortOrderHandle
        [Preserve]
        public SortOrderHandle()
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            return ReferenceEquals(this, p);
        }

        public void CreateForTable(TableHandle tableHandle)
        {
            NativeException nativeException;
            var sortOrderPtr = NativeMethods.create_for_table(tableHandle, out nativeException);
            nativeException.ThrowIfNecessary();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                SetHandle(sortOrderPtr);
            }
        }

        public void AddClause(string columnName, bool ascending)
        {
            NativeException nativeException;
            NativeMethods.add_clause(this, columnName, (IntPtr)columnName.Length, MarshalHelpers.BoolToIntPtr(ascending), out nativeException);
            nativeException.ThrowIfNecessary();
        }
    }
}
