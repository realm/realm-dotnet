////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Native;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    internal class SubscriptionSetHandle : RealmHandle
    {
#pragma warning disable IDE0049 // Use built-in type alias
#pragma warning disable SA1121 // Use built-in type alias

        private static class NativeMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void StateWaitCallback(IntPtr task_completion_source, SubscriptionSetState new_state, PrimitiveValue message);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GetSubscriptionCallback(IntPtr managed_callback, Native.Subscription subscription);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(
                GetSubscriptionCallback get_subscription_callback,
                StateWaitCallback state_wait_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_count(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_version", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_version(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern SubscriptionSetState get_state(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_at_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_at_index(SubscriptionSetHandle handle, IntPtr index, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_find_by_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_by_name(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_find_by_query", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_by_query(SubscriptionSetHandle handle, ResultsHandle results, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_add_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add(SubscriptionSetHandle handle, ResultsHandle results,
                [MarshalAs(UnmanagedType.LPWStr)] string? name, IntPtr name_len,
                [MarshalAs(UnmanagedType.I1)] bool update_existing, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool remove(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_by_id", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool remove(SubscriptionSetHandle handle, PrimitiveValue id, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_by_query", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove(SubscriptionSetHandle handle, ResultsHandle results, [MarshalAs(UnmanagedType.I1)] bool remove_named, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_by_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove_by_type(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string type, IntPtr type_len, [MarshalAs(UnmanagedType.I1)] bool remove_named, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_all", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove_all(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.I1)] bool remove_named, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_destroy_mutable", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy_mutable(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_wait_for_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern void wait_for_state(SubscriptionSetHandle handle, IntPtr task_completion_source, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_begin_write", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr begin_write(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_commit_write", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr commit_write(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_error", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_error_message(SubscriptionSetHandle handle, IntPtr buffer, IntPtr buffer_length, [MarshalAs(UnmanagedType.U1)] out bool isNull, out NativeException ex);
        }

#pragma warning restore IDE0049 // Use built-in type alias
#pragma warning restore SA1121 // Use built-in type alias

        private delegate void GetSubscriptionBase(IntPtr callback, out NativeException ex);

        public override bool ForceRootOwnership => true;

        public static void Initialize()
        {
            NativeMethods.GetSubscriptionCallback getSubscription = OnGetSubscription;
            NativeMethods.StateWaitCallback waitState = HandleStateWaitCallback;

            GCHandle.Alloc(getSubscription);
            GCHandle.Alloc(waitState);

            NativeMethods.install_callbacks(getSubscription, waitState);
        }

        public bool IsReadonly { get; }

        public SubscriptionSetHandle(SharedRealmHandle root, IntPtr handle, bool isReadonly = true) : base(root, handle)
        {
            IsReadonly = isReadonly;
        }

        public int GetCount()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_count(this, out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public SubscriptionSetState GetState()
        {
            EnsureIsOpen();

            var state = NativeMethods.get_state(this, out var ex);
            ex.ThrowIfNecessary();
            return state;
        }

        public long GetVersion()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_version(this, out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public string? GetErrorMessage()
        {
            EnsureIsOpen();

            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                NativeMethods.get_error_message(this, buffer, length, out isNull, out ex));
        }

        public SubscriptionSetHandle BeginWrite()
        {
            EnsureIsOpen();

            var result = NativeMethods.begin_write(this, out var ex);
            ex.ThrowIfNecessary();
            return new SubscriptionSetHandle(Root!, result, isReadonly: false);
        }

        public SubscriptionSetHandle CommitWrite()
        {
            EnsureIsOpen();

            var result = NativeMethods.commit_write(this, out var ex);
            ex.ThrowIfNecessary();

            return new SubscriptionSetHandle(Root!, result, isReadonly: true);
        }

        public Subscription GetAtIndex(int index)
        {
            EnsureIsOpen();

            return GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.get_at_index(this, (IntPtr)index, callback, out ex));
        }

        public Subscription Find(string name)
        {
            EnsureIsOpen();

            return GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.find_by_name(this, name, name.IntPtrLength(), callback, out ex));
        }

        public Subscription Find(ResultsHandle results)
        {
            EnsureIsOpen();

            return GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.find_by_query(this, results, callback, out ex));
        }

        public Subscription Add(ResultsHandle results, SubscriptionOptions options)
        {
            EnsureIsOpen();

            return GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.add(this, results, options.Name, options.Name.IntPtrLength(), options.UpdateExisting, callback, out ex));
        }

        public bool Remove(string name)
        {
            EnsureIsOpen();

            var result = NativeMethods.remove(this, name, name.IntPtrLength(), out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public bool Remove(ObjectId id)
        {
            EnsureIsOpen();

            var subId = PrimitiveValue.ObjectId(id);
            var result = NativeMethods.remove(this, subId, out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public int Remove(ResultsHandle results, bool removeNamed)
        {
            EnsureIsOpen();

            var result = NativeMethods.remove(this, results, removeNamed, out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public int RemoveAll(string type, bool removeNamed)
        {
            EnsureIsOpen();

            var result = NativeMethods.remove_by_type(this, type, type.IntPtrLength(), removeNamed, out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public int RemoveAll(bool removeNamed)
        {
            EnsureIsOpen();

            var result = NativeMethods.remove_all(this, removeNamed, out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public async Task<SubscriptionSetState> WaitForStateChangeAsync()
        {
            EnsureIsOpen();

            var tcs = new TaskCompletionSource<SubscriptionSetState>();
            var tcsHandle = GCHandle.Alloc(tcs);

            try
            {
                NativeMethods.wait_for_state(this, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();

                return await tcs.Task;
            }
            catch (Exception ex) when (ex.Message == "Active SubscriptionSet without a SubscriptionStore")
            {
                throw new TaskCanceledException("The SubscriptionSet was closed before the wait could complete. This is likely because the Realm it belongs to was disposed.");
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        private static Subscription GetSubscriptionCore(GetSubscriptionBase getter)
        {
            Subscription? result = null;
            Action<Native.Subscription> callback = sub => result = sub.ManagedSubscription;
            var callbackHandle = GCHandle.Alloc(callback);
            try
            {
                getter(GCHandle.ToIntPtr(callbackHandle), out var ex);
                ex.ThrowIfNecessary();
            }
            finally
            {
                callbackHandle.Free();
            }

            return result!;
        }

        public override void Unbind()
        {
            if (IsReadonly)
            {
                NativeMethods.destroy(handle);
            }
            else
            {
                NativeMethods.destroy_mutable(handle);
            }

            handle = IntPtr.Zero;
        }

        [MonoPInvokeCallback(typeof(NativeMethods.GetSubscriptionCallback))]
        private static void OnGetSubscription(IntPtr managedCallbackPtr, Native.Subscription subscription)
        {
            var handle = GCHandle.FromIntPtr(managedCallbackPtr);
            var callback = (Action<Native.Subscription>)handle.Target!;
            callback(subscription);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.StateWaitCallback))]
        private static void HandleStateWaitCallback(IntPtr taskCompletionSource, SubscriptionSetState state, PrimitiveValue message)
        {
            var handle = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource<SubscriptionSetState>)handle.Target!;

            switch (message.Type)
            {
                case RealmValueType.Null:
                    tcs.TrySetResult(state);
                    break;
                case RealmValueType.Int when message.AsInt() == -1:
                    tcs.TrySetException(new TaskCanceledException("The SubscriptionSet was closed before the wait could complete. This is likely because the Realm it belongs to was disposed."));
                    break;
                default:
                    tcs.TrySetException(new SubscriptionException(message.AsString()));
                    break;
            }
        }
    }
}
