﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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

namespace Realms.Sync
{
    internal class SubscriptionHandle : RealmHandle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SubscriptionCallbackDelegate(IntPtr managedHandle);

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_create", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr subscribe(
                ResultsHandle results,
                [MarshalAs(UnmanagedType.LPWStr)] string name,
                IntPtr name_len,
                [MarshalAs(UnmanagedType.I1)] bool has_name,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern int get_state(SubscriptionHandle subscription, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_get_error", CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeException get_error(SubscriptionHandle subscription);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(SubscriptionHandle subscription, IntPtr managedSubscriptionHandle, SubscriptionCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_destroy_notification_token", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr destroy_notificationtoken(IntPtr token, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_subscription_unsubscribe", CallingConvention = CallingConvention.Cdecl)]
            public static extern void unsubscribe(SubscriptionHandle subscription, out NativeException ex);
        }

        [Preserve]
        private SubscriptionHandle(IntPtr handle) : base(null, handle)
        {
        }

        public static SubscriptionHandle Create(ResultsHandle results, string name)
        {
            var handle = NativeMethods.subscribe(results, name, (IntPtr)(name?.Length ?? 0), name != null, out var ex);
            ex.ThrowIfNecessary();

            return new SubscriptionHandle(handle);
        }

        public SubscriptionState GetState()
        {
            var result = NativeMethods.get_state(this, out var ex);
            ex.ThrowIfNecessary();
            return (SubscriptionState)result;
        }

        public Exception GetError()
        {
            var result = NativeMethods.get_error(this);
            if (result.type != RealmExceptionCodes.NoError)
            {
                return result.Convert();
            }

            return null;
        }

        public SubscriptionTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, SubscriptionCallbackDelegate callback)
        {
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out var ex);
            ex.ThrowIfNecessary();
            return new SubscriptionTokenHandle(this, result);
        }

        public IntPtr DestroyNotificationToken(IntPtr token)
        {
            var result = NativeMethods.destroy_notificationtoken(token, out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public void Unsubscribe()
        {
            NativeMethods.unsubscribe(this, out var ex);
            ex.ThrowIfNecessary();
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
