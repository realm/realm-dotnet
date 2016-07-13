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
    internal class SchemaHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_create", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe IntPtr create([MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                                                              [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                                                              [MarshalAs(UnmanagedType.LPArray), Out]IntPtr[] object_schema_handles, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_clone", CallingConvention = CallingConvention.Cdecl)]
            public static extern  IntPtr clone(SchemaHandle schema, [MarshalAs(UnmanagedType.LPArray), In, Out]IntPtr[] object_schema_handles, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr schema);
        }

        [Preserve]
        public SchemaHandle()
        {
        }

        public SchemaHandle(SharedRealmHandle parent) : base(parent)
        {
        }

        public  void Initialize(SchemaObject[] objects, int count, SchemaProperty[] properties, IntPtr[] objectSchemaHandles)
        {
            NativeException nativeException;
            var ptr = NativeMethods.create(objects, count, properties, objectSchemaHandles, out nativeException);
            nativeException.ThrowIfNecessary();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                SetHandle(ptr);
            }
        }

        public  void InitializeCloneFrom(SchemaHandle schemaHandle, IntPtr[] objectSchemaHandles)
        {
            NativeException nativeException;
            var ptr = NativeMethods.clone(schemaHandle, objectSchemaHandles, out nativeException);
            nativeException.ThrowIfNecessary();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                SetHandle(ptr);
            }
        }

        protected override void Unbind()
        {
            // only destroy this instance if it isn't owned by a Realm
            // Object Store's Realm class owns the Schema object
            if (Root == null)
            {
                NativeMethods.destroy(handle);
            }
        }
    }
}
