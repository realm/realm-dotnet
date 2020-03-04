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
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr tableHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(TableHandle handle, SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_name(TableHandle handle, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_column_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_column_name(TableHandle table, ColumnKey column_key, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(TableHandle table, SharedRealmHandle realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object_for_string_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_string_primarykey(TableHandle handle, SharedRealmHandle realmHandle,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object_for_int_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_int_primarykey(TableHandle handle, SharedRealmHandle realmHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object_for_null_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_null_primarykey(TableHandle handle, SharedRealmHandle realmHandle, out NativeException ex);
        }

        [Preserve]
        public TableHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public ResultsHandle CreateResults(SharedRealmHandle sharedRealmHandle)
        {
            var result = NativeMethods.create_results(this, sharedRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(sharedRealmHandle, result);
        }

        public ObjectHandle Get(SharedRealmHandle realmHandle, ObjectKey objectKey)
        {
            var result = NativeMethods.get_object(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (result == IntPtr.Zero)
            {
                return null;
            }

            return new ObjectHandle(realmHandle, result);
        }

        public string GetName()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_name(this, buffer, length, out ex);
            });
        }

        public string GetColumnName(ColumnKey columnKey)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_column_name(this, columnKey, buffer, length, out ex);
            });
        }

        public bool TryFind(SharedRealmHandle realmHandle, string id, out ObjectHandle objectHandle)
        {
            NativeException nativeException;
            IntPtr result;
            if (id == null)
            {
                result = NativeMethods.get_object_for_null_primarykey(this, realmHandle, out nativeException);
            }
            else
            {
                result = NativeMethods.get_object_for_string_primarykey(this, realmHandle, id, (IntPtr)id.Length, out nativeException);
            }

            nativeException.ThrowIfNecessary();
            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(realmHandle, result);
            return true;
        }

        public bool TryFind(SharedRealmHandle realmHandle, long? id, out ObjectHandle objectHandle)
        {
            NativeException nativeException;
            IntPtr result;
            if (id.HasValue)
            {
                result = NativeMethods.get_object_for_int_primarykey(this, realmHandle, id.Value, out nativeException);
            }
            else
            {
                result = NativeMethods.get_object_for_null_primarykey(this, realmHandle, out nativeException);
            }

            nativeException.ThrowIfNecessary();
            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(realmHandle, result);
            return true;
        }
    }
}