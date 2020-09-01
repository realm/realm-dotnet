////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    internal static class SynchronizationContextScheduler
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr get_context();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void post_on_context(IntPtr context, ContextPostHandler callback, IntPtr user_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void release_context(IntPtr context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool is_on_context(IntPtr context, IntPtr targetContext);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ContextPostHandler(IntPtr user_data);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_scheduler_callbacks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void install_scheduler_callbacks(get_context get, post_on_context post, release_context release, is_on_context is_on);

        private class Scheduler
        {
            private volatile bool _isReleased;
            private SynchronizationContext _context;

            internal Scheduler(SynchronizationContext context)
            {
                Argument.NotNull(context, nameof(context));

                _context = context;
            }

            internal void Post(ContextPostHandler callback, IntPtr user_data)
            {
                _context.Post(_ =>
                {
                    if (!_isReleased)
                    {
                        callback(user_data);
                    }
                }, null);
            }

            internal bool IsOnContext(Scheduler other) => (other?._context ?? SynchronizationContext.Current) == _context;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Invalidate()
            {
                _isReleased = true;
                _context = null;
            }
        }

        [MonoPInvokeCallback(typeof(get_context))]
        private static IntPtr GetCurrentSynchronizationContext()
        {
            var context = SynchronizationContext.Current;
            if (context == null)
            {
                return IntPtr.Zero;
            }

            return GCHandle.ToIntPtr(GCHandle.Alloc(new Scheduler(context)));
        }

        [MonoPInvokeCallback(typeof(post_on_context))]
        private static void PostOnSynchronizationContext(IntPtr context, ContextPostHandler callback, IntPtr user_data)
        {
            if (context != IntPtr.Zero)
            {
                var scheduler = (Scheduler)GCHandle.FromIntPtr(context).Target;
                scheduler.Post(callback, user_data);
            }
        }

        [MonoPInvokeCallback(typeof(release_context))]
        private static void ReleaseSynchronizationContext(IntPtr context)
        {
            if (context != IntPtr.Zero)
            {
                var gcHandle = GCHandle.FromIntPtr(context);
                ((Scheduler)gcHandle.Target).Invalidate();
                gcHandle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(is_on_context))]
        private static bool IsOnSynchronizationContext(IntPtr context, IntPtr targetContext)
        {
            if (context != IntPtr.Zero)
            {
                var scheduler = (Scheduler)GCHandle.FromIntPtr(context).Target;
                Scheduler targetScheduler = null;
                if (targetContext != IntPtr.Zero)
                {
                    targetScheduler = (Scheduler)GCHandle.FromIntPtr(targetContext).Target;
                }

                return scheduler.IsOnContext(targetScheduler);
            }

            return false;
        }

        internal static void Install()
        {
            get_context get = GetCurrentSynchronizationContext;
            post_on_context post = PostOnSynchronizationContext;
            release_context release = ReleaseSynchronizationContext;
            is_on_context is_on = IsOnSynchronizationContext;

            // prevent the delegates from ever being garbage collected
            GCHandle.Alloc(get);
            GCHandle.Alloc(post);
            GCHandle.Alloc(release);
            GCHandle.Alloc(is_on);

            install_scheduler_callbacks(get, post, release, is_on);
        }
    }
}
