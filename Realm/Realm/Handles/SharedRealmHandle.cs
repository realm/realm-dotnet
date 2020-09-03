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
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Exceptions;
using Realms.Native;
using Realms.Schema;

namespace Realms
{
    internal class SharedRealmHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void NotifyRealmCallback(IntPtr stateHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GetNativeSchemaCallback(Native.Schema schema, IntPtr managed_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open(Configuration configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_set_managed_state_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_managed_state_handle(SharedRealmHandle sharedRealm, IntPtr managedStateHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_managed_state_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_managed_state_handle(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_close_realm", CallingConvention = CallingConvention.Cdecl)]
            public static extern void close_realm(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern void begin_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern void commit_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern void cancel_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_in_transaction", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool is_in_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool refresh(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_table(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string tableName, IntPtr tableNameLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema_version", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong get_schema_version(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_compact", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool compact(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_object_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_object_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_list_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_list_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_query_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_query_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_realm_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_realm_reference(ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_write_copy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void write_copy(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, byte[] encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object(SharedRealmHandle sharedRealm, TableHandle table, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object_int_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object_unique(SharedRealmHandle sharedRealm, TableHandle table, long key, [MarshalAs(UnmanagedType.I1)] bool has_value,
                                                             [MarshalAs(UnmanagedType.I1)] bool is_nullable,
                                                             [MarshalAs(UnmanagedType.I1)] bool update,
                                                             [MarshalAs(UnmanagedType.I1)] out bool is_new, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object_string_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object_unique(SharedRealmHandle sharedRealm, TableHandle table,
                                                             [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen,
                                                             [MarshalAs(UnmanagedType.I1)] bool update,
                                                             [MarshalAs(UnmanagedType.I1)] out bool is_new, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_schema(SharedRealmHandle sharedRealm, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(NotifyRealmCallback notifyRealmCallback, GetNativeSchemaCallback nativeSchemaCallback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_has_changed", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool has_changed(SharedRealmHandle sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_frozen(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(SharedRealmHandle sharedRealm, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        static SharedRealmHandle()
        {
            NativeCommon.Initialize();

            NativeMethods.NotifyRealmCallback notifyRealm = NotifyRealmChanged;
            NativeMethods.GetNativeSchemaCallback getNativeSchema = GetNativeSchema;

            GCHandle.Alloc(notifyRealm);
            GCHandle.Alloc(getNativeSchema);

            NativeMethods.install_callbacks(notifyRealm, getNativeSchema);
        }

        [Preserve]
        public SharedRealmHandle(IntPtr handle) : base(null, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public static IntPtr Open(Configuration configuration, RealmSchema schema, byte[] encryptionKey)
        {
            var marshaledSchema = new SchemaMarshaler(schema);

            var result = NativeMethods.open(configuration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public static IntPtr ResolveFromReference(ThreadSafeReferenceHandle referenceHandle)
        {
            var result = NativeMethods.resolve_realm_reference(referenceHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void CloseRealm()
        {
            NativeMethods.close_realm(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool IsFrozen
        {
            get
            {
                var result = NativeMethods.get_is_frozen(this, out var nativeException);
                nativeException.ThrowIfNecessary();
                return result;
            }
        }

        public void SetManagedStateHandle(IntPtr managedStateHandle)
        {
            NativeMethods.set_managed_state_handle(this, managedStateHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public IntPtr GetManagedStateHandle()
        {
            var result = NativeMethods.get_managed_state_handle(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void BeginTransaction()
        {
            NativeMethods.begin_transaction(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void CommitTransaction()
        {
            NativeMethods.commit_transaction(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void CancelTransaction()
        {
            NativeMethods.cancel_transaction(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool IsInTransaction()
        {
            var result = NativeMethods.is_in_transaction(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Refresh()
        {
            var result = NativeMethods.refresh(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public TableHandle GetTable(string tableName)
        {
            var result = NativeMethods.get_table(this, tableName, (IntPtr)tableName.Length, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new TableHandle(this, result);
        }

        public bool IsSameInstance(SharedRealmHandle other)
        {
            var result = NativeMethods.is_same_instance(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public ulong GetSchemaVersion()
        {
            var result = NativeMethods.get_schema_version(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Compact()
        {
            var result = NativeMethods.compact(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr ResolveReference(ThreadSafeReference reference)
        {
            if (reference.Handle.IsClosed)
            {
                throw new RealmException("Can only resolve a thread safe reference once.");
            }

            NativeException nativeException;
            IntPtr result;
            switch (reference.ReferenceType)
            {
                case ThreadSafeReference.Type.Object:
                    result = NativeMethods.resolve_object_reference(this, reference.Handle, out nativeException);
                    break;

                case ThreadSafeReference.Type.List:
                    result = NativeMethods.resolve_list_reference(this, reference.Handle, out nativeException);
                    break;

                case ThreadSafeReference.Type.Query:
                    result = NativeMethods.resolve_query_reference(this, reference.Handle, out nativeException);
                    break;

                default:
                    throw new NotSupportedException();
            }

            nativeException.ThrowIfNecessary();

            reference.Handle.Close();

            return result;
        }

        public void WriteCopy(string path, byte[] encryptionKey)
        {
            NativeMethods.write_copy(this, path, (IntPtr)path.Length, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GetSchema(Action<Native.Schema> callback)
        {
            var handle = GCHandle.Alloc(callback);
            NativeMethods.get_schema(this, GCHandle.ToIntPtr(handle), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle CreateObject(TableHandle table)
        {
            var result = NativeMethods.create_object(this, table, out NativeException ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
        }

        public ObjectHandle CreateObjectWithPrimaryKey(Property pkProperty, object primaryKey, TableHandle table, string parentType, bool update, out bool isNew)
        {
            if (primaryKey == null && !pkProperty.Type.IsNullable())
            {
                throw new ArgumentException($"{parentType}'s primary key is defined as non-nullable, but the value passed is null");
            }

            switch (pkProperty.Type.UnderlyingType())
            {
                case PropertyType.String:
                    var stringKey = (string)primaryKey;
                    return CreateObjectWithPrimaryKey(table, stringKey, update, out isNew);

                case PropertyType.Int:
                    var longKey = primaryKey == null ? (long?)null : Convert.ToInt64(primaryKey);
                    return CreateObjectWithPrimaryKey(table, longKey, pkProperty.Type.IsNullable(), update, out isNew);

                default:
                    throw new NotSupportedException($"Unexpected primary key of type: {pkProperty.Type}");
            }
        }

        public bool HasChanged()
        {
            return NativeMethods.has_changed(this);
        }

        public SharedRealmHandle Freeze()
        {
            var result = NativeMethods.freeze(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new SharedRealmHandle(result);
        }

        private ObjectHandle CreateObjectWithPrimaryKey(TableHandle table, long? key, bool isNullable, bool update, out bool isNew)
        {
            var result = NativeMethods.create_object_unique(this, table, key ?? 0, key.HasValue, isNullable, update, out isNew, out var ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
        }

        private ObjectHandle CreateObjectWithPrimaryKey(TableHandle table, string key, bool update, out bool isNew)
        {
            var result = NativeMethods.create_object_unique(this, table, key, (IntPtr)(key?.Length ?? 0), update, out isNew, out var ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.GetNativeSchemaCallback))]
        private static void GetNativeSchema(Native.Schema schema, IntPtr managedCallbackPtr)
        {
            var handle = GCHandle.FromIntPtr(managedCallbackPtr);
            var callback = (Action<Native.Schema>)handle.Target;
            callback(schema);
            handle.Free();
        }

        [MonoPInvokeCallback(typeof(NativeMethods.NotifyRealmCallback))]
        public static void NotifyRealmChanged(IntPtr stateHandle)
        {
            var gch = GCHandle.FromIntPtr(stateHandle);
            ((Realm.State)gch.Target).NotifyChanged(EventArgs.Empty);
        }

        public class SchemaMarshaler
        {
            public readonly SchemaObject[] Objects;
            public readonly SchemaProperty[] Properties;

            public SchemaMarshaler(RealmSchema schema)
            {
                var properties = new List<SchemaProperty>();

                Objects = schema.Select(@object =>
                {
                    var start = properties.Count;

                    properties.AddRange(@object.Select(ForMarshalling));

                    return new SchemaObject
                    {
                        name = @object.Name,
                        properties_start = start,
                        properties_end = properties.Count
                    };
                }).ToArray();
                Properties = properties.ToArray();
            }

            public static SchemaProperty ForMarshalling(Property property)
            {
                return new SchemaProperty
                {
                    name = property.Name,
                    type = property.Type,
                    object_type = property.ObjectType,
                    link_origin_property_name = property.LinkOriginPropertyName,
                    is_indexed = property.IsIndexed,
                    is_primary = property.IsPrimaryKey
                };
            }
        }
    }
}
