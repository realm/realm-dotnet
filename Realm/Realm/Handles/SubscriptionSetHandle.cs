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
using Realms.Native;

namespace Realms.Sync
{
    internal class SubscriptionSetHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GetSubscriptionCallback(Native.Subscription subscription, IntPtr managed_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(
                GetSubscriptionCallback get_subscription_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_count(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern SubscriptionSetState get_state(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_subscriptionset_get_at_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_at_index(SubscriptionSetHandle handle, IntPtr index, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_subscriptionset_find_by_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_by_name(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_subscriptionset_find_by_query", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_by_query(SubscriptionSetHandle handle, ResultsHandle results, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_subscriptionset_add", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add(SubscriptionSetHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string type, IntPtr type_len,
                [MarshalAs(UnmanagedType.LPWStr)] string query, IntPtr query_len,
                [MarshalAs(UnmanagedType.LPArray), In] PrimitiveValue[] arguments, IntPtr args_count,
                [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len,
                [MarshalAs(UnmanagedType.I1)] bool update_existing, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_subscriptionset_add_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add(SubscriptionSetHandle handle, ResultsHandle results,
                [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr name_len,
                [MarshalAs(UnmanagedType.I1)] bool update_existing, IntPtr callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_by_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove_by_type(SubscriptionSetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string type, IntPtr type_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_remove_all", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr remove_all(SubscriptionSetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscriptionset_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);
        }

        private delegate void GetSubscriptionBase(IntPtr callback, out NativeException ex);

        public static void Initialize()
        {
            NativeMethods.GetSubscriptionCallback getSubscription = OnGetSubscription;

            GCHandle.Alloc(getSubscription);

            NativeMethods.install_callbacks(getSubscription);
        }

        [Preserve]
        public SubscriptionSetHandle(IntPtr handle) : base(null, handle)
        {
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

        public Subscription GetAtIndex(int index) => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.get_at_index(this, (IntPtr)index, callback, out ex));

        public Subscription Find(string name) => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.find_by_name(this, name, name.IntPtrLength(), callback, out ex));

        public Subscription Find(ResultsHandle results) => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.find_by_query(this, results, callback, out ex));

        public Subscription Add(string type, string query, RealmValue[] arguments, SubscriptionOptions options)
        {
            return GetSubscriptionCore((IntPtr callback, out NativeException ex) =>
            {
                var (primitiveValues, handles) = arguments.ToPrimitiveValues();
                NativeMethods.add(this,
                                type, type.IntPtrLength(),
                                query, query.IntPtrLength(),
                                primitiveValues, (IntPtr)arguments.Length,
                                options.Name, options.Name.IntPtrLength(),
                                options.UpdateExisting, callback, out ex);

                handles.Dispose();
            });
        }

        public Subscription Add(ResultsHandle results, SubscriptionOptions options)
            => GetSubscriptionCore((IntPtr callback, out NativeException ex) => NativeMethods.add(this, results, options.Name, options.Name.IntPtrLength(), options.UpdateExisting, callback, out ex));

        public int Remove(string type)
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
        private static void OnGetSubscription(Native.Subscription subscription, IntPtr managedCallbackPtr)
        {
            var handle = GCHandle.FromIntPtr(managedCallbackPtr);
            var callback = (Action<Native.Subscription>)handle.Target;
            callback(subscription);
        }
    }
}
