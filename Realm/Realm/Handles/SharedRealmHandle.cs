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

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void OnBindingContextDestructedCallback(IntPtr handle);

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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern TableKey get_table_key(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string tableName, IntPtr tableNameLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema_version", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong get_schema_version(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_compact", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool compact(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_reference(SharedRealmHandle sharedRealm, ThreadSafeReferenceHandle referenceHandle, ThreadSafeReference.Type type, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_resolve_realm_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr resolve_realm_reference(ThreadSafeReferenceHandle referenceHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_write_copy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void write_copy(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, byte[] encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object(SharedRealmHandle sharedRealm, TableKey table_key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object_unique(SharedRealmHandle sharedRealm, TableKey table_key, PrimitiveValue value,
                                                             [MarshalAs(UnmanagedType.U1)] bool update,
                                                             [MarshalAs(UnmanagedType.U1)] out bool is_new, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_schema(SharedRealmHandle sharedRealm, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(NotifyRealmCallback notifyRealmCallback, GetNativeSchemaCallback nativeSchemaCallback, OpenRealmCallback openCallback, OnBindingContextDestructedCallback contextDestructedCallback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_has_changed", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool has_changed(SharedRealmHandle sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_object_for_primary_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_primary_key(SharedRealmHandle realmHandle, TableKey table_key, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(SharedRealmHandle sharedRealm, TableKey table_key, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
        }

        static unsafe SharedRealmHandle()
        {
            NativeCommon.Initialize();

            NativeMethods.NotifyRealmCallback notifyRealm = NotifyRealmChanged;
            NativeMethods.GetNativeSchemaCallback getNativeSchema = GetNativeSchema;
            NativeMethods.OpenRealmCallback openRealm = HandleOpenRealmCallback;
            NativeMethods.OnBindingContextDestructedCallback onBindingContextDestructed = OnBindingContextDestructed;

            GCHandle.Alloc(notifyRealm);
            GCHandle.Alloc(getNativeSchema);
            GCHandle.Alloc(openRealm);
            GCHandle.Alloc(onBindingContextDestructed);

            NativeMethods.install_callbacks(notifyRealm, getNativeSchema, openRealm, onBindingContextDestructed);
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

        public TableKey GetTableKey(string tableName)
        {
            var tableKey = NativeMethods.get_table_key(this, tableName, (IntPtr)tableName.Length, out var nativeException);
            nativeException.ThrowIfNecessary();
            return tableKey;
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

            var result = NativeMethods.resolve_reference(this, reference.Handle, reference.ReferenceType, out var nativeException);
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

        public ObjectHandle CreateObject(TableKey tableKey)
        {
            var result = NativeMethods.create_object(this, tableKey, out NativeException ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
        }

        public unsafe ObjectHandle CreateObjectWithPrimaryKey(Property pkProperty, object primaryKey, TableKey tableKey, string parentType, bool update, out bool isNew)
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
            var result = NativeMethods.create_object_unique(this, tableKey, primitiveValue, update, out isNew, out var ex);
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

        public unsafe bool TryFindObject(TableKey tableKey, in RealmValue id, out ObjectHandle objectHandle)
        {
            var (primitiveValue, handles) = id.ToNative();
            var result = NativeMethods.get_object_for_primary_key(this, tableKey, primitiveValue, out var ex);
            handles?.Dispose();
            ex.ThrowIfNecessary();

            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(this, result);
            return true;
        }

        public ResultsHandle CreateResults(TableKey tableKey)
        {
            var result = NativeMethods.create_results(this, tableKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(this, result);
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

        [MonoPInvokeCallbackAttribute(typeof(NativeMethods.OnBindingContextDestructedCallback))]
        public static void OnBindingContextDestructed(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                var gch = GCHandle.FromIntPtr(handle);
                ((Realm.State)gch.Target).Dispose();
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
