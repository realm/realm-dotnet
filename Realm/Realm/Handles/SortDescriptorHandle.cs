////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using Realms.Native;

namespace Realms
{
    internal class SortDescriptorHandle : RealmHandle
    {
        // This is a delegate type meant to represent one of the "query operator" methods such as float_less and bool_equal
        internal delegate void Operation<T>(QueryHandle queryPtr, ColumnKey columnKey, T value);

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sort_descriptor_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr queryHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sort_descriptor_add_clause", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_clause(SortDescriptorHandle descriptor, TableHandle query, SharedRealmHandle realm,
                [MarshalAs(UnmanagedType.LPArray), In] ColumnKey[] column_key_chain, IntPtr column_keys_count,
                [MarshalAs(UnmanagedType.I1)] bool ascending,
                out NativeException ex);
        }

        public SortDescriptorHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public void AddClause(TableHandle table, SharedRealmHandle realm, ColumnKey[] columnKeyChain, bool ascending)
        {
            NativeMethods.add_clause(this, table, realm, columnKeyChain, (IntPtr)columnKeyChain.Length, ascending, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
