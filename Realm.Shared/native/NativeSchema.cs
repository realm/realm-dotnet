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
    internal static class NativeSchema
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Property
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string name;

            [MarshalAs(UnmanagedType.I4)]
            internal Schema.PropertyType type;

            [MarshalAs(UnmanagedType.LPStr)]
            internal string objectType;

            [MarshalAs(UnmanagedType.I1)]
            internal bool is_nullable;

            [MarshalAs(UnmanagedType.I1)]
            internal bool is_primary;

            [MarshalAs(UnmanagedType.I1)]
            internal bool is_indexed;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Object
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string name;

            internal int properties_start;
            internal int properties_end;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_create", CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern IntPtr create([MarshalAs(UnmanagedType.LPArray), In] Object[] objects, int objects_length,
                                                          [MarshalAs(UnmanagedType.LPArray), In] Property[] properties,
                                                          IntPtr* object_schema_handles);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_clone", CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern IntPtr clone(SchemaHandle schema, IntPtr* object_schema_handles);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void destroy(IntPtr schema);
    }
}
