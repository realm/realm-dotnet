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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Exceptions;
using Realms.Native;
using Realms.Schema;
using Realms.Sync.Exceptions;

namespace Realms
{
    internal class SharedRealmHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void NotifyRealmCallback(IntPtr stateHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GetNativeSchemaCallback(Native.Schema schema, IntPtr managed_callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void OpenRealmCallback(IntPtr task_completion_source, IntPtr shared_realm, int error_code, byte* message_buf, IntPtr message_len);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open(Configuration configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync(Configuration configuration, Sync.Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync_async", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync_async(Configuration configuration, Sync.Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[] encryptionKey,
                IntPtr task_completion_source,
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
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_in_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool refresh(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_table(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string tableName, IntPtr tableNameLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema_version", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong get_schema_version(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_compact", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool compact(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_object_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_object_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_list_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_list_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_query_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_query_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_set_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_set_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_realm_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_realm_reference(ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_write_copy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void write_copy(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, byte[] encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object(SharedRealmHandle sharedRealm, TableHandle table, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object_unique(SharedRealmHandle sharedRealm, TableHandle table, PrimitiveValue value,
                                                             [MarshalAs(UnmanagedType.U1)] bool update,
                                                             [MarshalAs(UnmanagedType.U1)] out bool is_new, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_schema(SharedRealmHandle sharedRealm, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(NotifyRealmCallback notifyRealmCallback, GetNativeSchemaCallback nativeSchemaCallback, OpenRealmCallback open_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_has_changed", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool has_changed(SharedRealmHandle sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(SharedRealmHandle sharedRealm, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
        }

        static unsafe SharedRealmHandle()
        {
            NativeCommon.Initialize();

            NativeMethods.NotifyRealmCallback notifyRealm = NotifyRealmChanged;
            NativeMethods.GetNativeSchemaCallback getNativeSchema = GetNativeSchema;
            NativeMethods.OpenRealmCallback openRealm = HandleOpenRealmCallback;

            GCHandle.Alloc(notifyRealm);
            GCHandle.Alloc(getNativeSchema);
            GCHandle.Alloc(openRealm);

            NativeMethods.install_callbacks(notifyRealm, getNativeSchema, openRealm);
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

        public static SharedRealmHandle OpenWithSync(Configuration configuration, Sync.Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey)
        {
            var marshaledSchema = new SchemaMarshaler(schema);

            var result = NativeMethods.open_with_sync(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new SharedRealmHandle(result);
        }

        public static AsyncOpenTaskHandle OpenWithSyncAsync(Configuration configuration, Sync.Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey, GCHandle tcsHandle)
        {
            var marshaledSchema = new SchemaMarshaler(schema);

            var asyncTaskPtr = NativeMethods.open_with_sync_async(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, GCHandle.ToIntPtr(tcsHandle), out var nativeException);
            nativeException.ThrowIfNecessary();
            var asyncTaskHandle = new AsyncOpenTaskHandle(asyncTaskPtr);
            return asyncTaskHandle;
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
            var result = reference.ReferenceType switch
            {
                ThreadSafeReference.Type.Object => NativeMethods.resolve_object_reference(this, reference.Handle, out nativeException),
                ThreadSafeReference.Type.List => NativeMethods.resolve_list_reference(this, reference.Handle, out nativeException),
                ThreadSafeReference.Type.Query => NativeMethods.resolve_query_reference(this, reference.Handle, out nativeException),
                ThreadSafeReference.Type.Set => NativeMethods.resolve_set_reference(this, reference.Handle, out nativeException),
                _ => throw new NotSupportedException(),
            };
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

        public unsafe ObjectHandle CreateObjectWithPrimaryKey(Property pkProperty, object primaryKey, TableHandle table, string parentType, bool update, out bool isNew)
        {
            if (primaryKey == null && !pkProperty.Type.IsNullable())
            {
                throw new ArgumentException($"{parentType}'s primary key is defined as non-nullable, but the value passed is null");
            }

            RealmValue pkValue = pkProperty.Type.ToRealmValueType() switch
            {
                RealmValueType.String => (string)primaryKey,
                RealmValueType.Int => primaryKey == null ? (long?)null : Convert.ToInt64(primaryKey),
                RealmValueType.ObjectId => (ObjectId?)primaryKey,
                RealmValueType.Guid => (Guid?)primaryKey,
                _ => throw new NotSupportedException($"Primary key of type {pkProperty.Type} is not supported"),
            };

            var (primitiveValue, handles) = pkValue.ToNative();
            var result = NativeMethods.create_object_unique(this, table, primitiveValue, update, out isNew, out var ex);
            handles?.Dispose();
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
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

        [MonoPInvokeCallback(typeof(NativeMethods.OpenRealmCallback))]
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The task awaiter will own the ThreadSafeReference handle.")]
        private static unsafe void HandleOpenRealmCallback(IntPtr taskCompletionSource, IntPtr realm_reference, int error_code, byte* messageBuffer, IntPtr messageLength)
        {
            var handle = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource<ThreadSafeReferenceHandle>)handle.Target;

            if (error_code == 0)
            {
                tcs.TrySetResult(new ThreadSafeReferenceHandle(realm_reference, isRealmReference: true));
            }
            else
            {
                var inner = new SessionException(Encoding.UTF8.GetString(messageBuffer, (int)messageLength), (ErrorCode)error_code);
                const string OuterMessage = "A system error occurred while opening a Realm. See InnerException for more details";
                tcs.TrySetException(new RealmException(OuterMessage, inner));
            }
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
                        properties_end = properties.Count,
                        is_embedded = @object.IsEmbedded,
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
