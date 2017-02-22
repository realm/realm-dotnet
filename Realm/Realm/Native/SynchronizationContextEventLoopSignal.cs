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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Realms.Native;

namespace Realms
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
    internal static class SynchronizationContextEventLoopSignal
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr get_eventloop();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void post_on_event_loop(IntPtr eventloop, EventLoopPostHandler callback, IntPtr user_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void release_eventloop(IntPtr eventloop);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventLoopPostHandler(IntPtr user_data);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_eventloop_callbacks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void install_eventloop_callbacks(get_eventloop get, post_on_event_loop post, release_eventloop release);

        private class EventLoop
        {
            private SynchronizationContext _context;

            private volatile bool _isReleased = false;

            internal EventLoop(SynchronizationContext context)
            {
                Debug.Assert(context != null, "The SynchronizationContext EventLoopSignal implementation always needs a SynchronizationContext");
                _context = context; 
            }

            internal void Post(EventLoopPostHandler callback, IntPtr user_data)
            {
                _context.Post(_ =>
                {
                    if (!_isReleased)
                    {
                        callback(user_data);
                    }
                }, null);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Invalidate()
            {
                _isReleased = true;
                _context = null;
            }
        }

        [NativeCallback(typeof(get_eventloop))]
        private static IntPtr GetCurrentSynchronizationContext()
        {
            var context = SynchronizationContext.Current;
            if (context == null)
            {
                return IntPtr.Zero;
            }

            return GCHandle.ToIntPtr(GCHandle.Alloc(new EventLoop(context)));
        }

        [NativeCallback(typeof(post_on_event_loop))]
        private static void PostOnSynchronizationContext(IntPtr eventloop, EventLoopPostHandler callback, IntPtr user_data)
        {
            if (eventloop != IntPtr.Zero)
            {
                var context = (EventLoop)GCHandle.FromIntPtr(eventloop).Target;
                context.Post(callback, user_data);
            }
        }

        [NativeCallback(typeof(release_eventloop))]
        private static void ReleaseSynchronizationContext(IntPtr eventloop)
        {
            if (eventloop != IntPtr.Zero)
            {
                var gcHandle = GCHandle.FromIntPtr(eventloop);
                ((EventLoop)gcHandle.Target).Invalidate();
                gcHandle.Free();
            }
        }

        internal static void Install()
        {
            get_eventloop get = GetCurrentSynchronizationContext;
            post_on_event_loop post = PostOnSynchronizationContext;
            release_eventloop release = ReleaseSynchronizationContext;

            // prevent the delegates from ever being garbage collected
            GCHandle.Alloc(get);
            GCHandle.Alloc(post);
            GCHandle.Alloc(release);

            install_eventloop_callbacks(get, post, release);
        }
    }
}
