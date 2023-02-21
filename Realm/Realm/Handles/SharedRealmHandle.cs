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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Exceptions;
using Realms.Logging;
using Realms.Native;
using Realms.Schema;
using Realms.Sync;

namespace Realms
{
    internal class SharedRealmHandle : StandaloneHandle
    {
        protected readonly List<WeakReference<RealmHandle>> _weakChildren = new();

        private readonly object _unbindListLock = new(); // used to serialize calls to unbind between finalizer threads

        // list of owned handles that should be unbound as soon as possible by a user thread
        private readonly List<RealmHandle> _unbindList = new();

        // goes to true when we don't expect more calls from user threads on this handle
        // is set when we dispose a handle
        // used when unbinding owned classes, by not using the unbind list but just unbinding them at once (as we cannot interleave with user threads
        // as there are none left than can access the root class (and its owned classes)
        // it is important that children always have a reference path to their root for this to work
        private bool _noMoreUserThread;

        private static class NativeMethods
        {
#pragma warning disable IDE0049 // Use built-in type alias
#pragma warning disable SA1121 // Use built-in type alias

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void NotifyRealmCallback(IntPtr stateHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GetNativeSchemaCallback(Native.Schema schema, IntPtr managed_callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void OpenRealmCallback(IntPtr task_completion_source, IntPtr shared_realm, NativeException ex);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void DisposeGCHandleCallback(IntPtr handle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void LogMessageCallback(PrimitiveValue message, LogLevel level);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void HandleTaskCompletionCallback(IntPtr tcs_ptr, [MarshalAs(UnmanagedType.U1)] bool invoke_async, NativeException ex);

            // migrationSchema is a special schema that is used only in the context of a migration block.
            // It is a pointer because we need to be able to modify this schema in some migration methods directly in core.
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr MigrationCallback(IntPtr oldRealm, IntPtr newRealm, IntPtr migrationSchema, Native.Schema oldSchema, ulong schemaVersion, IntPtr managedMigrationHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr ShouldCompactCallback(IntPtr managedDelegate, ulong totalSize, ulong dataSize, [MarshalAs(UnmanagedType.U1)] ref bool should_compact);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr InitializationCallback(IntPtr managedInitializationDelegate, IntPtr realm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open(Configuration configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[]? encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync(Configuration configuration, Sync.Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[]? encryptionKey,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync_async", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync_async(Configuration configuration, Sync.Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[]? encryptionKey,
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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_delete_files", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern void delete_files([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_close_all_realms", CallingConvention = CallingConvention.Cdecl)]
            public static extern void close_all_realms(out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction_async", CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt32 begin_transaction_async(SharedRealmHandle sharedRealm, IntPtr tcsPtr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction_async", CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt32 commit_transaction_async(SharedRealmHandle sharedRealm, IntPtr tcsPtr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_async_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool cancel_async_transaction(SharedRealmHandle sharedRealm, UInt32 transaction_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern void begin_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern void commit_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_transaction", CallingConvention = CallingConvention.Cdecl)]
            public static extern void cancel_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_in_transaction", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_in_transaction(SharedRealmHandle sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool refresh(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt32 get_table_key(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string tableName, IntPtr tableNameLength, out NativeException ex);

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
            public static extern void write_copy(SharedRealmHandle sharedRealm, Configuration configuration, [MarshalAs(UnmanagedType.U1)] bool useSync, byte[]? encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object(SharedRealmHandle sharedRealm, UInt32 table_key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_object_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_object_unique(SharedRealmHandle sharedRealm, UInt32 table_key, PrimitiveValue value,
                                                             [MarshalAs(UnmanagedType.U1)] bool update,
                                                             [MarshalAs(UnmanagedType.U1)] out bool is_new, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_schema(SharedRealmHandle sharedRealm, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(
                NotifyRealmCallback notify_realm_callback,
                GetNativeSchemaCallback native_schema_callback,
                OpenRealmCallback open_callback,
                DisposeGCHandleCallback dispose_gchandle_callback,
                LogMessageCallback log_message_callback,
                NotifiableObjectHandleBase.NotificationCallback notify_object,
                DictionaryHandle.KeyNotificationCallback notify_dictionary,
                MigrationCallback migration_callback,
                ShouldCompactCallback should_compact_callback,
                HandleTaskCompletionCallback handle_task_completion,
                InitializationCallback initialization_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_object_for_primary_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_primary_key(SharedRealmHandle realmHandle, UInt32 table_key, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(SharedRealmHandle sharedRealm, UInt32 table_key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_rename_property", CallingConvention = CallingConvention.Cdecl)]
            public static extern void rename_property(SharedRealmHandle sharedRealm,
                [MarshalAs(UnmanagedType.LPWStr)] string typeName, IntPtr typeNameLength,
                [MarshalAs(UnmanagedType.LPWStr)] string oldName, IntPtr oldNameLength,
                [MarshalAs(UnmanagedType.LPWStr)] string newName, IntPtr newNameLength,
                IntPtr migrationSchema,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_remove_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool remove_type(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)] string typeName, IntPtr typeLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_remove_all", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool remove_all(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_sync_session", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_session(SharedRealmHandle realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_subscriptions", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_subscriptions(SharedRealmHandle realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_subscriptions_version", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_subscriptions_version(SharedRealmHandle realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh_async", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool refresh_async(SharedRealmHandle realm, IntPtr tcs_handle, out NativeException ex);

#pragma warning restore SA1121 // Use built-in type alias
#pragma warning restore IDE0049 // Use built-in type alias
        }

        static SharedRealmHandle()
        {
            NativeCommon.Initialize();
        }

        public static void Initialize()
        {
            NativeMethods.NotifyRealmCallback notifyRealm = NotifyRealmChanged;
            NativeMethods.GetNativeSchemaCallback getNativeSchema = GetNativeSchema;
            NativeMethods.OpenRealmCallback openRealm = HandleOpenRealmCallback;
            NativeMethods.DisposeGCHandleCallback disposeGCHandle = DisposeGCHandleCallback;
            NativeMethods.LogMessageCallback logMessage = LogMessage;
            NotifiableObjectHandleBase.NotificationCallback notifyObject = NotifiableObjectHandleBase.NotifyObjectChanged;
            DictionaryHandle.KeyNotificationCallback notifyDictionary = DictionaryHandle.NotifyDictionaryChanged;
            NativeMethods.MigrationCallback onMigration = OnMigration;
            NativeMethods.ShouldCompactCallback shouldCompact = ShouldCompactOnLaunchCallback;
            NativeMethods.HandleTaskCompletionCallback handleTaskCompletion = HandleTaskCompletionCallback;
            NativeMethods.InitializationCallback onInitialization = OnDataInitialization;

            GCHandle.Alloc(notifyRealm);
            GCHandle.Alloc(getNativeSchema);
            GCHandle.Alloc(openRealm);
            GCHandle.Alloc(disposeGCHandle);
            GCHandle.Alloc(logMessage);
            GCHandle.Alloc(notifyObject);
            GCHandle.Alloc(notifyDictionary);
            GCHandle.Alloc(onMigration);
            GCHandle.Alloc(shouldCompact);
            GCHandle.Alloc(handleTaskCompletion);
            GCHandle.Alloc(onInitialization);

            NativeMethods.install_callbacks(notifyRealm, getNativeSchema, openRealm, disposeGCHandle, logMessage, notifyObject, notifyDictionary, onMigration, shouldCompact, handleTaskCompletion, onInitialization);
        }

        [Preserve]
        public SharedRealmHandle(IntPtr handle) : base(handle)
        {
        }

        public virtual bool OwnsNativeRealm => true;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            try
            {
                Unbind();

                lock (_unbindListLock)
                {
                    _noMoreUserThread = true;

                    // this call could interleave with calls from finalizing children in other threads
                    // but they or we will wait because of the unbindlistlock taken above
                    UnbindLockedList();
                }

                foreach (var child in _weakChildren)
                {
                    if (child.TryGetTarget(out var childHandle) && !childHandle.IsClosed)
                    {
                        childHandle.Close();
                    }
                }

                return true;
            }
            catch
            {
                // it would be really bad if we got an exception in here. We must not pass it on, but have to return false
                return false;
            }
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        /// <summary>
        /// Called by children to this root, when they would like to
        /// be unbound, but are (possibly) running in a finalizer thread
        /// so it is (possibly) not safe to unbind then directly.
        /// </summary>
        /// <param name="handleToUnbind">The core handle that is not needed anymore and should be unbound.</param>
        public void RequestUnbind(RealmHandle handleToUnbind)
        {
            // You can lock a lock several times inside the same thread. The top-level-lock is the one that counts
            lock (_unbindListLock)
            {
                // If the Realm handle has been closed - either in the finalizer or when the Realm has been disposed,
                // we should just unbind the child handle immediately. This can happen if a child handle is garbage
                // collected just as the Realm instance gets disposed. ReleaseHandle will get called on the Realm instance
                // and we may end up here.
                if (_noMoreUserThread)
                {
                    handleToUnbind.Unbind();
                }
                else
                {
                    // Child handles are typically garbage collected, so we're likely in a finalizer thread. We transfer
                    // the child handle ownership to the SharedRealmHandle and we'll unbind it either when a new child
                    // handle gets added to the Realm or when the Realm itself gets disposed.
                    _unbindList.Add(handleToUnbind);
                }
            }
        }

        public virtual void AddChild(RealmHandle handle)
        {
            if (handle.ForceRootOwnership)
            {
                _weakChildren.Add(new(handle));
            }

            if (_unbindList.Count == 0)
            {
                return;
            }

            // outside the lock so we may get a really strange value here.
            // however. If we get 0 and the real value was something else, we will find out inside the lock in unbindlockedlist
            // if we get !=0 and the real value was in fact 0, then we will just skip and then catch up next time around.
            // however, doing things this way will save lots and lots of locks when the list is empty, which it should be if people have
            // been using the dispose pattern correctly, or at least have been eager at disposing as soon as they can
            // except of course dot notation users that cannot dispose cause they never get a reference in the first place
            lock (_unbindListLock)
            {
                UnbindLockedList();
            }
        }

        // only call inside a lock on UnbindListLock
        private void UnbindLockedList()
        {
            // put in here in order to save time otherwise spent looping and clearing an empty list
            if (_unbindList.Count > 0)
            {
                foreach (var realmHandle in _unbindList)
                {
                    realmHandle.Unbind();
                }

                _unbindList.Clear();
            }
        }

        public static SharedRealmHandle Open(Configuration configuration, RealmSchema schema, byte[]? encryptionKey)
        {
            var marshaledSchema = new SchemaMarshaler(schema);

            var result = NativeMethods.open(configuration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new SharedRealmHandle(result);
        }

        public static SharedRealmHandle OpenWithSync(Configuration configuration, Sync.Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[]? encryptionKey)
        {
            var marshaledSchema = new SchemaMarshaler(schema);

            var result = NativeMethods.open_with_sync(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new SharedRealmHandle(result);
        }

        public static AsyncOpenTaskHandle OpenWithSyncAsync(Configuration configuration, Sync.Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[]? encryptionKey, IntPtr tcsHandle)
        {
            var marshaledSchema = new SchemaMarshaler(schema);
            var asyncTaskPtr = NativeMethods.open_with_sync_async(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, tcsHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new AsyncOpenTaskHandle(asyncTaskPtr);
        }

        public static SharedRealmHandle ResolveFromReference(ThreadSafeReferenceHandle referenceHandle)
        {
            var result = NativeMethods.resolve_realm_reference(referenceHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new SharedRealmHandle(result);
        }

        public void CloseRealm()
        {
            NativeMethods.close_realm(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void DeleteFiles(string path)
        {
            NativeMethods.delete_files(path, (IntPtr)path.Length, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void ForceCloseNativeRealms()
        {
            NativeMethods.close_all_realms(out var nativeException);
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

        public void SetManagedStateHandle(Realm.State managedState)
        {
            // This is freed in OnBindingContextDestructed
            var stateHandle = GCHandle.Alloc(managedState);

            NativeMethods.set_managed_state_handle(this, GCHandle.ToIntPtr(stateHandle), out var nativeException);
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

        public async Task BeginTransactionAsync(SynchronizationContext synchronizationContext, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource();
            var tcsHandle = GCHandle.Alloc(tcs);
            uint? asyncTransactionHandle = null;
            ct.Register(() => OnTaskCancellation(asyncTransactionHandle, tcs, synchronizationContext));
            try
            {
                asyncTransactionHandle = NativeMethods.begin_transaction_async(this, GCHandle.ToIntPtr(tcsHandle), out var nativeException);
                nativeException.ThrowIfNecessary();

                // When starting an async operation, core internally queues a cb and returns a handle to it (CBH).
                // By requesting cancellation before obtaining a CBH, nothing is dequeued because nothing is there yet.
                // However, we now have a CBH, so we can dequeue the cb; otherwise we free the tcsHandle before core calls the cb.
                // If not done, under Mono referencing a null GCHandle results in a hard crash of the runtime.
                if (ct.IsCancellationRequested)
                {
                    OnTaskCancellation(asyncTransactionHandle, tcs, synchronizationContext);
                }

                await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        public void CommitTransaction()
        {
            NativeMethods.commit_transaction(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public async Task CommitTransactionAsync(SynchronizationContext synchronizationContext, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource();
            var tcsHandle = GCHandle.Alloc(tcs);
            uint? asyncTransactionHandle = null;
            ct.Register(() => OnTaskCancellation(asyncTransactionHandle, tcs, synchronizationContext));
            try
            {
                asyncTransactionHandle = NativeMethods.commit_transaction_async(this, GCHandle.ToIntPtr(tcsHandle), out var nativeException);
                nativeException.ThrowIfNecessary();

                // When starting an async operation, core internally queues a cb and returns a handle to it (CBH).
                // By requesting cancellation before obtaining a CBH, nothing is dequeued because nothing is there yet.
                // However, we now have a CBH, so we can dequeue the cb; otherwise we free the tcsHandle before core calls the cb.
                // If not done, under Mono referencing a null GCHandle results in a hard crash of the runtime.
                if (ct.IsCancellationRequested)
                {
                    OnTaskCancellation(asyncTransactionHandle, tcs, synchronizationContext);
                }

                await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        public void CancelTransaction()
        {
            NativeMethods.cancel_transaction(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool IsInTransaction() => NativeMethods.is_in_transaction(this);

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
            return new TableKey(tableKey);
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

        public void WriteCopy(RealmConfigurationBase config)
        {
            var useSync = config is SyncConfigurationBase;

            var nativeConfig = config.CreateNativeConfiguration();

            NativeMethods.write_copy(this, nativeConfig, useSync, config.EncryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public RealmSchema GetSchema()
        {
            RealmSchema? result = null;
            Action<Native.Schema> callback = schema => result = RealmSchema.CreateFromObjectStoreSchema(schema);
            var callbackHandle = GCHandle.Alloc(callback);
            try
            {
                NativeMethods.get_schema(this, GCHandle.ToIntPtr(callbackHandle), out var nativeException);
                nativeException.ThrowIfNecessary();
            }
            finally
            {
                callbackHandle.Free();
            }

            return result!;
        }

        public ObjectHandle CreateObject(TableKey tableKey)
        {
            var result = NativeMethods.create_object(this, tableKey.Value, out var ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
        }

        public ObjectHandle CreateObjectWithPrimaryKey(Property pkProperty, in RealmValue primaryKey, TableKey tableKey, string parentType, bool update, out bool isNew)
        {
            if (primaryKey.Type == RealmValueType.Null)
            {
                // If passed primary key value is null, validate that the property is nullable
                if (!pkProperty.Type.IsNullable())
                {
                    throw new ArgumentException($"{parentType}'s primary key is defined as non-nullable, but the value passed is null");
                }
            }
            else
            {
                // If passed primary key value is not null, we should validate that the types match
                if (primaryKey.Type != pkProperty.Type.ToRealmValueType())
                {
                    throw new ArgumentException($"{parentType}'s primary key is defined as {pkProperty.Type.ToRealmValueType()}, but the value passed is of type {pkProperty.Type}");
                }
            }

            var (primitiveValue, handles) = primaryKey.ToNative();
            var result = NativeMethods.create_object_unique(this, tableKey.Value, primitiveValue, update, out isNew, out var ex);
            handles?.Dispose();
            ex.ThrowIfNecessary();
            return new ObjectHandle(this, result);
        }

        public SharedRealmHandle Freeze()
        {
            var result = NativeMethods.freeze(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new SharedRealmHandle(result);
        }

        public bool TryFindObject(TableKey tableKey, in RealmValue id, [MaybeNullWhen(false)] out ObjectHandle objectHandle)
        {
            var (primitiveValue, handles) = id.ToNative();
            var result = NativeMethods.get_object_for_primary_key(this, tableKey.Value, primitiveValue, out var ex);
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

        public void RenameProperty(string typeName, string oldName, string newName, IntPtr migrationSchema)
        {
            NativeMethods.rename_property(this, typeName, (IntPtr)typeName.Length,
                oldName, (IntPtr)oldName.Length, newName, (IntPtr)newName.Length, migrationSchema, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool RemoveType(string typeName)
        {
            var result = NativeMethods.remove_type(this, typeName, (IntPtr)typeName.Length, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool RemoveAll()
        {
            var result = NativeMethods.remove_all(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public ResultsHandle CreateResults(TableKey tableKey)
        {
            var result = NativeMethods.create_results(this, tableKey.Value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(this, result);
        }

        public SessionHandle GetSession()
        {
            var ptr = NativeMethods.get_session(this, out var ex);
            ex.ThrowIfNecessary();
            return new SessionHandle(this, ptr);
        }

        public SubscriptionSetHandle GetSubscriptions()
        {
            var ptr = NativeMethods.get_subscriptions(this, out var ex);
            ex.ThrowIfNecessary();
            return new SubscriptionSetHandle(this, ptr);
        }

        public long GetSubscriptionsVersion()
        {
            var result = NativeMethods.get_subscriptions_version(this, out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public async Task<bool> RefreshAsync()
        {
            var tcs = new TaskCompletionSource();
            var tcsHandle = GCHandle.Alloc(tcs);

            try
            {
                var didRegister = NativeMethods.refresh_async(this, GCHandle.ToIntPtr(tcsHandle), out var ex);
                if (!didRegister)
                {
                    return false;
                }

                await tcs.Task;
                return true;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.GetNativeSchemaCallback))]
        private static void GetNativeSchema(Native.Schema schema, IntPtr managedCallbackPtr)
        {
            var handle = GCHandle.FromIntPtr(managedCallbackPtr);
            var callback = (Action<Native.Schema>)handle.Target!;
            callback(schema);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.NotifyRealmCallback))]
        public static void NotifyRealmChanged(IntPtr stateHandle)
        {
            var gch = GCHandle.FromIntPtr(stateHandle);
            ((Realm.State)gch.Target!).NotifyChanged(EventArgs.Empty);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.OpenRealmCallback))]
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Realm will be owned by the creator of the tcs")]
        private static void HandleOpenRealmCallback(IntPtr taskCompletionSource, IntPtr realm_reference, NativeException ex)
        {
            var handleTcs = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource<ThreadSafeReferenceHandle>)handleTcs.Target!;

            if (ex.type == RealmExceptionCodes.NoError)
            {
                tcs.TrySetResult(new(realm_reference));
            }
            else
            {
                var inner = ex.Convert();
                const string outerMessage = "A system error occurred while operating on a Realm. See InnerException for more details.";
                tcs.TrySetException(new RealmException(outerMessage, inner));
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.DisposeGCHandleCallback))]
        public static void DisposeGCHandleCallback(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                GCHandle.FromIntPtr(handle).Free();
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.LogMessageCallback))]
        private static void LogMessage(PrimitiveValue message, LogLevel level)
        {
            Logger.LogDefault(level, message.AsString());
        }

        [MonoPInvokeCallback(typeof(NativeMethods.MigrationCallback))]
        private static IntPtr OnMigration(IntPtr oldRealmPtr, IntPtr newRealmPtr, IntPtr migrationSchema, Native.Schema oldSchema, ulong schemaVersion, IntPtr managedConfigHandle)
        {
            Migration? migration = null;
            try
            {
                var configHandle = GCHandle.FromIntPtr(managedConfigHandle);
                var config = (RealmConfiguration)configHandle.Target!;

                var oldRealmHandle = new UnownedRealmHandle(oldRealmPtr);
                var oldConfiguration = new RealmConfiguration(config.DatabasePath)
                {
                    SchemaVersion = schemaVersion,
                    IsReadOnly = true,
                    EnableCache = false
                };
                using var oldRealm = new Realm(oldRealmHandle, oldConfiguration, RealmSchema.CreateFromObjectStoreSchema(oldSchema));

                var newRealmHandle = new UnownedRealmHandle(newRealmPtr);
                using var newRealm = new Realm(newRealmHandle, config, config.Schema, isInMigration: true);
                migration = new Migration(oldRealm, newRealm, migrationSchema);

                config.MigrationCallback!.Invoke(migration, schemaVersion);
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                var exHandle = GCHandle.Alloc(ex);
                return GCHandle.ToIntPtr(exHandle);
            }
            finally
            {
                migration?.Free();
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.ShouldCompactCallback))]
        private static IntPtr ShouldCompactOnLaunchCallback(IntPtr managedConfigHandle, ulong totalSize, ulong dataSize, ref bool shouldCompact)
        {
            try
            {
                var configHandle = GCHandle.FromIntPtr(managedConfigHandle);
                var config = (RealmConfiguration)configHandle.Target!;

                shouldCompact = config.ShouldCompactOnLaunch!.Invoke(totalSize, dataSize);
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                var exHandle = GCHandle.Alloc(ex);
                return GCHandle.ToIntPtr(exHandle);
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.HandleTaskCompletionCallback))]
        private static void HandleTaskCompletionCallback(IntPtr tcs_ptr, bool invoke_async, NativeException ex)
        {
            if (invoke_async)
            {
                // There are situations where we want to let the native function exit before dispatching the continuation.
                // One example is Realm::run_writes which needs to complete before we can start writing to the Realm.
                SynchronizationContext.Current!.Post(_ =>
                {
                    SetResult();
                }, null);
            }
            else
            {
                SetResult();
            }

            void SetResult()
            {
                var handleTcs = GCHandle.FromIntPtr(tcs_ptr);
                var tcs = (TaskCompletionSource)handleTcs.Target!;

                if (ex.type == RealmExceptionCodes.RLM_ERR_NONE)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    var inner = ex.Convert();
                    const string outerMessage = "A system error occurred while operating on a Realm. See InnerException for more details.";
                    tcs.TrySetException(new RealmException(outerMessage, inner));
                }
            }
        }

        private void OnTaskCancellation(uint? asyncTransactionHandle, TaskCompletionSource tcs, SynchronizationContext synchronizationContext)
        {
            // We need to post on the original SynchronizationContext where the lock was acquired because
            // cancel_async_transaction needs to be on that thread in order to be able to perform the cancellation
            synchronizationContext.Post(_ =>
            {
                // Since we're posting, execution can happen after core has dequeued the cb in order to execute it.
                // We can't cancel to avoid that the caller frees the handle to the tcs that the cb relies on.
                // Referencing a null GCHandle when in Mono results in a hard crash of the runtime.
                if (asyncTransactionHandle.HasValue &&
                    NativeMethods.cancel_async_transaction(this, asyncTransactionHandle.Value, out var innerNativeException))
                {
                    if (innerNativeException.code != RealmExceptionCodes.RLM_ERR_NONE)
                    {
                        tcs.TrySetException(innerNativeException.Convert());
                    }

                    tcs.TrySetCanceled();
                }
            }, null);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.InitializationCallback))]
        private static IntPtr OnDataInitialization(IntPtr managedConfigHandle, IntPtr realmPtr)
        {
            try
            {
                var configHandle = GCHandle.FromIntPtr(managedConfigHandle);
                var config = (RealmConfigurationBase)configHandle.Target!;

                var realmHandle = new UnownedRealmHandle(realmPtr);
                using var realm = config.GetRealm(realmHandle);
                config.PopulateInitialData!.Invoke(realm);
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                var exHandle = GCHandle.Alloc(ex);
                return GCHandle.ToIntPtr(exHandle);
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
                        table_type = @object.BaseType,
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
