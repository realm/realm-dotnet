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
        private static class NativeMethods
        {
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sort_descriptor_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr queryHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sort_descriptor_add_clause", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_clause(SortDescriptorHandle descriptor, UInt32 table_key, SharedRealmHandle realm,
                [MarshalAs(UnmanagedType.LPArray), In] IntPtr[] property_index_chain, IntPtr column_keys_count,
                [MarshalAs(UnmanagedType.U1)] bool ascending, [MarshalAs(UnmanagedType.U1)] bool replacing,
                out NativeException ex);

#pragma warning restore SA1121 // Use built-in type alias
        }

        public SortDescriptorHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public void AddClause(TableKey tableKey, IntPtr[] propertyIndexChain, bool ascending, bool replacing)
        {
            EnsureIsOpen();

            NativeMethods.add_clause(this, tableKey.Value, Root!, propertyIndexChain, (IntPtr)propertyIndexChain.Length, ascending, replacing, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override void Unbind() => NativeMethods.destroy(handle);
    }
}
