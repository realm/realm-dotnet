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
using Realms.Exceptions;
using Realms.Native;

namespace Realms.Sync
{
    internal class SubscriptionSetHandle : RealmHandle
    {
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
                [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len,
                [MarshalAs(UnmanagedType.I1)] bool update_existing, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool remove(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_by_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove_by_type(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string type, IntPtr type_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_all", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove_all(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_wait_for_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern void wait_for_state(SubscriptionSetHandle handle, IntPtr task_completion_source, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_begin_write", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr begin_write(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_commit_write", CallingConvention = CallingConvention.Cdecl)]
            public static extern void commit_write(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_error", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_error_message(SubscriptionSetHandle handle, IntPtr buffer, IntPtr buffer_length, [MarshalAs(UnmanagedType.U1)] out bool isNull, out NativeException ex);
        }

        private delegate void GetSubscriptionBase(IntPtr callback, out NativeException ex);

        public static void Initialize()
        {
            NativeMethods.GetSubscriptionCallback getSubscription = OnGetSubscription;
            NativeMethods.StateWaitCallback waitState = HandleStateWaitCallback;

            GCHandle.Alloc(getSubscription);
            GCHandle.Alloc(waitState);

            NativeMethods.install_callbacks(getSubscription, waitState);
        }

        public readonly bool IsReadonly;

        [Preserve]
        public SubscriptionSetHandle(IntPtr handle, bool isReadonly = true) : base(null, handle)
        {
            IsReadonly = isReadonly;
        }

        public int GetCount()
        {
            var result = NativeMethods.get_count(this, out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public SubscriptionSetState GetState()
        {
            var state = NativeMethods.get_state(this, out var ex);
            ex.ThrowIfNecessary();
            return state;
        }

        public string GetErrorMessage()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                NativeMethods.get_error_message(this, buffer, length, out isNull, out ex));
        }

        public SubscriptionSetHandle BeginWrite()
        {
            var result = NativeMethods.begin_write(this, out var ex);
            ex.ThrowIfNecessary();
            return new SubscriptionSetHandle(result, isReadonly: false);
        }

        public void CommitWrite()
        {
            NativeMethods.commit_write(this, out var ex);
            ex.ThrowIfNecessary();
        }

        public Subscription GetAtIndex(int index) => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.get_at_index(this, (IntPtr)index, callback, out ex));

        public Subscription Find(string name) => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.find_by_name(this, name, name.IntPtrLength(), callback, out ex));

        public Subscription Find(ResultsHandle results) => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.find_by_query(this, results, callback, out ex));

        public Subscription Add(ResultsHandle results, SubscriptionOptions options)
            => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.add(this, results, options.Name, options.Name.IntPtrLength(), options.UpdateExisting, callback, out ex));

        public bool Remove(string name)
        {
            var result = NativeMethods.remove(this, name, name.IntPtrLength(), out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public int RemoveAll(string type)
        {
            var result = NativeMethods.remove_by_type(this, type, type.IntPtrLength(), out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public int RemoveAll()
        {
            var result = NativeMethods.remove_all(this, out var ex);
            ex.ThrowIfNecessary();
            return (int)result;
        }

        public async Task<SubscriptionSetState> WaitForStateChangeAsync()
        {
            var tcs = new TaskCompletionSource<SubscriptionSetState>();
            var tcsHandle = GCHandle.Alloc(tcs);

            try
            {
                NativeMethods.wait_for_state(this, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();

                return await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        private static Subscription GetSubscriptionCore(GetSubscriptionBase getter)
        {
            Subscription result = null;
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

            return result;
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.GetSubscriptionCallback))]
        private static void OnGetSubscription(IntPtr managedCallbackPtr, Native.Subscription subscription)
        {
            var handle = GCHandle.FromIntPtr(managedCallbackPtr);
            var callback = (Action<Native.Subscription>)handle.Target;
            callback(subscription);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.StateWaitCallback))]
        private static void HandleStateWaitCallback(IntPtr taskCompletionSource, SubscriptionSetState state, PrimitiveValue message)
        {
            var handle = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource<SubscriptionSetState>)handle.Target;

            if (message.Type == RealmValueType.Null)
            {
                tcs.TrySetResult(state);
            }
            else
            {
                // TODO: new exception type
                var inner = new RealmException(message.AsString());
                const string OuterMessage = "A system error occurred while waiting for completion. See InnerException for more details";
                tcs.TrySetException(new RealmException(OuterMessage, inner));
            }
        }
    }
}
