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
using Realms.Native;

namespace Realms
{
    internal class TableHandle : RealmHandle
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias")]
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_empty_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_empty_object(TableHandle tableHandle, SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_count_all", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 count_all(TableHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_unbind", CallingConvention = CallingConvention.Cdecl)]
            public static extern void unbind(IntPtr tableHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_column_index(TableHandle tablehandle,
                [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(TableHandle handle, SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_sorted_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_sorted_results(TableHandle handle, SharedRealmHandle sharedRealm,
                    [MarshalAs(UnmanagedType.LPArray), In]SortDescriptorBuilder.Clause.Marshalable[] sortClauses, IntPtr clauseCount,
                    [MarshalAs(UnmanagedType.LPArray), In]IntPtr[] flattenedColumnIndices,
                    out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_for_string_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr object_for_string_primarykey(TableHandle handle, SharedRealmHandle realmHandle,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_for_int_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr object_for_int_primarykey(TableHandle handle, SharedRealmHandle realmHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_for_null_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr object_for_null_primarykey(TableHandle handle, SharedRealmHandle realmHandle, out NativeException ex);
        }

        private TableHandle(RealmHandle root) : base(root)
        {
        }

        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public TableHandle()
        {
        }

        protected override void Unbind()
        {
            NativeException nativeException;
            NativeMethods.unbind(handle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        // if root is null the this tablehandle is responsible for cleaning up the tableView and its children
        // if root is something else, it is the this tablehandles root, and that root should also manage the tableview and its children
        // note that IgnoreUnbind is set to false, the tableview should be unbound
        /*
            private TableViewHandle RootedTableViewHandle()
            {
                return (Root == null) ?
                    new TableViewHandle(this):
                    new TableViewHandle(Root);
            }
        */

        // call with a parent, will set the correct root (parent if parent.root=null, or parent.root otherwise)
        // if You want a RootedTableHandle with is self-rooted, call with no parameter
        internal static TableHandle RootedTableHandle(RealmHandle parent)
        {
            return new TableHandle(parent.Root ?? parent);
        }

        public IntPtr AddEmptyObject(SharedRealmHandle sharedRealm)
        {
            NativeException nativeException;
            var result = NativeMethods.add_empty_object(this, sharedRealm, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long CountAll()
        {
            NativeException nativeException;
            var result = NativeMethods.count_all(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        // returns -1 if the column string does not match a column index
        public IntPtr GetColumnIndex(string name)
        {
            NativeException nativeException;
            var result = NativeMethods.get_column_index(this, name, (IntPtr)name.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr CreateResults(SharedRealmHandle sharedRealmHandle)
        {
            NativeException nativeException;
            var result = NativeMethods.create_results(this, sharedRealmHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr CreateSortedResults(SharedRealmHandle sharedRealmHandle, SortDescriptorBuilder sortDescriptorBuilder)
        {
            NativeException nativeException;
            var marshaledValues = sortDescriptorBuilder.Flatten();
            var result = NativeMethods.create_sorted_results(this, sharedRealmHandle, marshaledValues.Item2, (IntPtr)marshaledValues.Item2.Length, marshaledValues.Item1, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        internal IntPtr Find(SharedRealmHandle realmHandle, string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            NativeException nativeException;
            var result = NativeMethods.object_for_string_primarykey(this, realmHandle, id, (IntPtr)id.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        internal IntPtr Find(SharedRealmHandle realmHandle, long? id)
        {
            NativeException nativeException;
            IntPtr result;
            if (id.HasValue)
            {
                result = NativeMethods.object_for_int_primarykey(this, realmHandle, id.Value, out nativeException);
            }
            else
            {
                result = NativeMethods.object_for_null_primarykey(this, realmHandle, out nativeException);
            }

            nativeException.ThrowIfNecessary();
            return result;
        }
    }
}

/*sample implementation of two-tiered handle instantiation , from http://blogs.msdn.com/b/bclteam/archive/2005/03/15/396335.aspx
 * 
 * //Best practice to avoid object allocation inside CER.
MySafeHandle mySafeHandle = new MySafeHandle(0, true);
IntPtr myHandle;
IntPtr invalidHandle = new IntPtr(-1));
Int32 ret;
 
       // The creation of myHandle and assignment to mySafeHandle should be done inside a CER
RuntimeHelpers.PrepareConstrainedRegions();
try {// Begin CER
}
        finally {
ret = MyNativeMethods.CreateHandle(out myHandle);
              if (ret ==0 && !myHandle.IsNull() && myHandle != invalidHandle)
            mySafeHandle.SetHandle(myHandle);
        }// End CER
*/