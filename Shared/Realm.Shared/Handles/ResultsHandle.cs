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
using System.Runtime.InteropServices;

namespace Realms
{
    internal class ResultsHandle : CollectionHandleBase
    {
        public static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_is_same_internal_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr is_same_internal_results(ResultsHandle lhs, ResultsHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr resultsHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_row", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_row(ResultsHandle results, IntPtr index, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr count(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ResultsHandle results, SharedRealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ResultsHandle results, IntPtr managedResultsHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy_notificationtoken", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr destroy_notificationtoken(IntPtr token, out NativeException ex);
        }

        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a ResultsHandle when returning a size_t as a ResultsHandle
        [Preserve]
        public ResultsHandle()
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public override IntPtr GetObjectAtIndex(long index)
        {
            NativeException nativeException;
            var result = NativeMethods.get_row(this, (IntPtr)index, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public int Count()
        {
            NativeException nativeException;
            var result = NativeMethods.count(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public void Clear(SharedRealmHandle realmHandle)
        {
            NativeException nativeException;
            NativeMethods.clear(this, realmHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override IntPtr AddNotificationCallback(IntPtr managedCollectionHandle, NotificationCallbackDelegate callback)
        {
            NativeException nativeException;
            var result = NativeMethods.add_notification_callback(this, managedCollectionHandle, callback, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override IntPtr DestroyNotificationToken(IntPtr token)
        {
            NativeException nativeException;
            var result = NativeMethods.destroy_notificationtoken(token, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, p))
            {
                return true;
            }

            NativeException nativeException;
            var result = NativeMethods.is_same_internal_results(this, (ResultsHandle)p, out nativeException);
            nativeException.ThrowIfNecessary();
            return result != IntPtr.Zero;
        }
    }
}
