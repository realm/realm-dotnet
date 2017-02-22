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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Schema;

namespace Realms
{
    internal class SharedRealmHandle : RealmHandle
    {
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias")]
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open(Native.Configuration configuration,
                [MarshalAs(UnmanagedType.LPArray), In] Native.SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] Native.SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_set_managed_state_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_managed_state_handle(SharedRealmHandle sharedRealm, IntPtr managedStateHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_managed_state_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_managed_state_handle(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_close_realm",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void close_realm(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void begin_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void commit_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void cancel_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_in_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr is_in_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr refresh(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_table(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)]string tableName, IntPtr tableNameLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema_version",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt64 get_schema_version(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_add_observed_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_observed_object(SharedRealmHandle sharedRealm, ObjectHandle objectHandle, IntPtr managedRealmObjectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_remove_observed_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void remove_observed_object(SharedRealmHandle sharedRealm, IntPtr managedRealmObjectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_compact", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool compact(SharedRealmHandle sharedRealm, out NativeException ex);
        }

        [Preserve]
        public SharedRealmHandle()
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public IntPtr Open(Native.Configuration configuration, RealmSchema schema, byte[] encryptionKey)
        {
            var marshaledSchema = new SchemaMarshaler(schema);

            NativeException nativeException;
            var result = NativeMethods.open(configuration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void CloseRealm()
        {
            NativeException nativeException;
            NativeMethods.close_realm(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetManagedStateHandle(IntPtr managedStateHandle)
        {
            NativeException nativeException;
            NativeMethods.set_managed_state_handle(this, managedStateHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public IntPtr GetManagedStateHandle()
        {
            NativeException nativeException;
            var result = NativeMethods.get_managed_state_handle(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void AddObservedObject(ObjectHandle objectHandle, IntPtr managedRealmObjectHandle)
        {
            NativeException nativeException;
            NativeMethods.add_observed_object(this, objectHandle, managedRealmObjectHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void RemoveObservedObject(IntPtr managedRealmObjectHandle)
        {
            NativeException nativeException;
            NativeMethods.remove_observed_object(this, managedRealmObjectHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BeginTransaction()
        {
            NativeException nativeException;
            NativeMethods.begin_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void CommitTransaction()
        {
            NativeException nativeException;
            NativeMethods.commit_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void CancelTransaction()
        {
            NativeException nativeException;
            NativeMethods.cancel_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool IsInTransaction()
        {
            NativeException nativeException;
            var result = NativeMethods.is_in_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public bool Refresh()
        {
            NativeException nativeException;
            var result = NativeMethods.refresh(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public IntPtr GetTable(string tableName)
        {
            NativeException nativeException;
            var result = NativeMethods.get_table(this, tableName, (IntPtr)tableName.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool IsSameInstance(SharedRealmHandle other)
        {
            NativeException nativeException;
            var result = NativeMethods.is_same_instance(this, other, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public ulong GetSchemaVersion()
        {
            NativeException nativeException;
            var result = NativeMethods.get_schema_version(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Compact()
        {
            NativeException nativeException;
            var result = NativeMethods.compact(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        internal class SchemaMarshaler
        {
            internal readonly Native.SchemaObject[] Objects;
            internal readonly Native.SchemaProperty[] Properties;

            internal SchemaMarshaler(RealmSchema schema)
            {
                var properties = new List<Native.SchemaProperty>();

                Objects = schema.Select(@object =>
                {
                    var start = properties.Count;

                    properties.AddRange(@object.Select(ForMarshalling));

                    return new Native.SchemaObject
                    {
                        name = @object.Name,
                        properties_start = start,
                        properties_end = properties.Count
                    };
                }).ToArray();
                Properties = properties.ToArray();
            }

            internal static Native.SchemaProperty ForMarshalling(Schema.Property property)
            {
                return new Native.SchemaProperty
                {
                    name = property.Name,
                    type = property.Type,
                    object_type = property.ObjectType,
                    link_origin_property_name = property.LinkOriginPropertyName,
                    is_nullable = property.IsNullable,
                    is_indexed = property.IsIndexed,
                    is_primary = property.IsPrimaryKey
                };
            }
        }
    }
}
