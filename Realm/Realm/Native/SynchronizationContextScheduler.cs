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
using System.Reflection;
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
        private delegate void post_on_context(IntPtr context, IntPtr function_ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void release_context(IntPtr context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool is_on_context(IntPtr context, IntPtr targetContext);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_scheduler_invoke_function", CallingConvention = CallingConvention.Cdecl)]
        private static extern void scheduler_invoke_function(IntPtr function_ptr, [MarshalAs(UnmanagedType.I1)] bool can_execute);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_scheduler_callbacks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void install_scheduler_callbacks(get_context get, post_on_context post, release_context release, is_on_context is_on);

        private class Scheduler
        {
            private static readonly Lazy<FieldInfo?> XunitInnerContext = new(() =>
            {
                try
                {
                    var type = Type.GetType("Xunit.Sdk.AsyncTestSyncContext, xunit.execution.dotnet");
                    return type?.GetField("innerContext", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                catch
                {
                    return null;
                }
            });

            private readonly int _threadId;

            private volatile bool _isReleased;
            private SynchronizationContext? _context;

            internal Scheduler(SynchronizationContext context)
            {
                Argument.NotNull(context, nameof(context));

                _context = context;
                _threadId = Environment.CurrentManagedThreadId;
            }

            internal void Post(IntPtr function_ptr)
            {
                _context?.Post(f_ptr =>
                {
                    scheduler_invoke_function((IntPtr)f_ptr!, !_isReleased);
                }, function_ptr);
            }

            internal bool IsOnContext(Scheduler? other) => AreContextsEqual(_context, other?._context ?? SynchronizationContext.Current) || _threadId == (other?._threadId ?? Environment.CurrentManagedThreadId);

            private static bool AreContextsEqual(SynchronizationContext? first, SynchronizationContext? second)
            {
                if (first == second)
                {
                    return true;
                }

                return TryGetInnerContext(first) == TryGetInnerContext(second);
            }

            // xUnit has a peculiar behavior with regard to synchronization contexts. The test runs with AsyncTestSyncContext
            // which points to a MaxConcurrencySyncContext via its innerContext field. When `Post` is invoked on the `AsyncTestSyncContext`,
            // it will dispatch that to the innerContext, which means that inside the `Post` callback, `SynchronizationContext.Current` will
            // return `MaxConcurrencySyncContext` instead of `AsyncTestSyncContext`, meaning IsOnContext returns false inside the callback.
            // This roughly translates into:
            // capturedContext.Post(() =>
            // {
            //     // Failing here because current is capturedContext.innerContext
            //     Assert.That(capturedContext == SynchronizationContext.Current);
            // });
            // See: https://github.com/xunit/xunit/blob/c27a91f8cbcee37cb45699a3a81287bca225e876/src/xunit.v3.core/Sdk/MaxConcurrencySyncContext.cs
            // See: https://github.com/xunit/xunit/blob/c27a91f8cbcee37cb45699a3a81287bca225e876/src/xunit.v3.core/Sdk/AsyncTestSyncContext.cs
            private static SynchronizationContext? TryGetInnerContext(SynchronizationContext? outer)
            {
                if (outer != null &&
                    XunitInnerContext.Value != null &&
                    outer.GetType() == XunitInnerContext.Value.DeclaringType &&
                    XunitInnerContext.Value.GetValue(outer) is SynchronizationContext inner)
                {
                    return inner;
                }

                return outer;
            }

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
            if (context is null)
            {
                return IntPtr.Zero;
            }

            return GCHandle.ToIntPtr(GCHandle.Alloc(new Scheduler(context)));
        }

        [MonoPInvokeCallback(typeof(post_on_context))]
        private static void PostOnSynchronizationContext(IntPtr context, IntPtr user_data)
        {
            if (context != IntPtr.Zero)
            {
                var scheduler = (Scheduler)GCHandle.FromIntPtr(context).Target!;
                scheduler.Post(user_data);
            }
        }

        [MonoPInvokeCallback(typeof(release_context))]
        private static void ReleaseSynchronizationContext(IntPtr context)
        {
            if (context != IntPtr.Zero)
            {
                var gcHandle = GCHandle.FromIntPtr(context);
                ((Scheduler)gcHandle.Target!).Invalidate();
                gcHandle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(is_on_context))]
        private static bool IsOnSynchronizationContext(IntPtr context, IntPtr targetContext)
        {
            if (context != IntPtr.Zero)
            {
                var scheduler = (Scheduler)GCHandle.FromIntPtr(context).Target!;
                Scheduler? targetScheduler = null;
                if (targetContext != IntPtr.Zero)
                {
                    targetScheduler = (Scheduler)GCHandle.FromIntPtr(targetContext).Target!;
                }

                return scheduler.IsOnContext(targetScheduler);
            }

            return false;
        }

        internal static void Initialize()
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
