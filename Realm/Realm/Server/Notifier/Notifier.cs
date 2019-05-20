////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Realms.Helpers;
using Realms.Server.Exceptions;
using Realms.Server.Native;

namespace Realms.Server
{
    /// <summary>
    /// A factory class, used for creating <see cref="INotifier"/> instances.
    /// </summary>
    public static class Notifier
    {
        /// <summary>
        /// Creates a new <see cref="INotifier"/> with the supplied <see cref="NotifierConfiguration"/>.
        /// </summary>
        /// <param name="config">
        /// A <see cref="NotifierConfiguration"/> describing the various <see cref="INotifier"/> settings.
        /// </param>
        /// <returns>
        /// An awaitable task, that, upon completion, will contain the fully initialized <see cref="INotifier"/>
        /// instance.
        /// </returns>
        public static Task<INotifier> StartAsync(NotifierConfiguration config)
        {
            Argument.NotNull(config, nameof(config));
            Argument.Ensure(config.Handlers?.Any() == true, "The list of handlers cannot be empty.", nameof(config));
            return Impl.StartAsync(config);
        }

        internal static unsafe bool ShouldHandle(IntPtr notifierHandle, byte* pathBuffer, IntPtr pathLength)
        {
            if (GCHandle.FromIntPtr(notifierHandle).Target is Impl notifier)
            {
                var path = Encoding.UTF8.GetString(pathBuffer, (int)pathLength);
                return notifier.ShouldHandle(path);
            }

            return false;
        }

        internal static unsafe void EnqueueCalculation(IntPtr notifierHandle, byte* pathBuffer, IntPtr pathLength, IntPtr calculator_ptr)
        {
            if (GCHandle.FromIntPtr(notifierHandle).Target is Impl notifier)
            {
                var path = Encoding.UTF8.GetString(pathBuffer, (int)pathLength);
                notifier.CalculateChanges(path, calculator_ptr);
            }
        }

        internal static unsafe void OnStarted(IntPtr managedInstance, int errorCode, byte* messageBuffer, IntPtr messageLength)
        {
            var handle = GCHandle.FromIntPtr(managedInstance);
            var notifier = (Impl)handle.Target;
            if (errorCode == 0)
            {
                notifier.OnStarted(null);
            }
            else
            {
                var message = Encoding.UTF8.GetString(messageBuffer, (int)messageLength);
                notifier.OnStarted(new NotifierStartException(errorCode, message));
            }
        }

        internal static void OnCalculationCompleted(IntPtr details_ptr, IntPtr managedCallbackPtr)
        {
            var handle = GCHandle.FromIntPtr(managedCallbackPtr);
            var callback = (Action<NativeChangeDetails?>)handle.Target;
            var details = new PtrTo<NativeChangeDetails>(details_ptr);
            callback(details.Value);
            handle.Free();
        }

        internal class Impl : INotifier
        {
            private readonly INotificationHandler[] _handlers;
            private readonly AsyncContextThread _notificationsThread;
            private readonly CalculationProcessor<string, IntPtr> _processor;
            private readonly TaskCompletionSource<INotifier> _start;

            private readonly NotifierHandle _notifierHandle;
            private readonly GCHandle _gcHandle;

            public NotifierConfiguration Configuration { get; }

            private Impl(NotifierConfiguration config, AsyncContextThread notificationsThread)
            {
                _handlers = config.Handlers.ToArray();
                _notificationsThread = notificationsThread;
                _gcHandle = GCHandle.Alloc(this);
                Configuration = config;
                _start = new TaskCompletionSource<INotifier>();
                try
                {
                    _notifierHandle = NotifierHandle.CreateHandle(_gcHandle, config);
                }
                catch
                {
                    _gcHandle.Free();
                    throw;
                }

                _processor = new CalculationProcessor<string, IntPtr>(CalculateAsync);
            }

            private int _isDisposed;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            internal static Task<INotifier> StartAsync(NotifierConfiguration config)
            {
                Directory.CreateDirectory(config.WorkingDirectory);

                var thread = new AsyncContextThread();
                return thread.Factory.Run(() =>
                {
                    var notifier = new Impl(config, thread);
                    return notifier._start.Task;
                });
            }

            internal void OnStarted(NotifierStartException exception)
            {
                if (exception != null)
                {
                    _start.SetException(exception);
                }
                else
                {
                    _start.SetResult(this);
                }
            }

            internal bool ShouldHandle(string path) => _handlers.Any(h => h.ShouldHandle(path));

            internal void CalculateChanges(string path, IntPtr calculation_ptr)
            {
                if (_isDisposed == 0)
                {
                    _processor.Enqueue(path, calculation_ptr);
                }
            }

            private async Task CalculateAsync(IntPtr calculation_ptr)
            {
                using (var calculation = new NotifierNotificationHandle(calculation_ptr))
                {
                    ChangeDetails details = null;
                    calculation.GetChanges(nativeDetails =>
                    {
                        if (nativeDetails.HasValue)
                        {
                            var pathOnDisk = nativeDetails.Value.PathOnDisk;
                            Realm previousRealm = null;
                            if (nativeDetails.Value.previous_realm != IntPtr.Zero)
                            {
                                previousRealm = Realm.GetInstance(new NotifierRealmConfiguration(nativeDetails.Value.previous_realm, pathOnDisk));
                            }

                            var currentRealm = Realm.GetInstance(new NotifierRealmConfiguration(nativeDetails.Value.current_realm, pathOnDisk));

                            details = new ChangeDetails(nativeDetails.Value.Path, nativeDetails.Value.change_sets.AsEnumerable(), previousRealm, currentRealm);
                        }
                    });

                    if (details == null)
                    {
                        return;
                    }

                    try
                    {
                        foreach (var handler in _handlers)
                        {
                            try
                            {
                                if (handler.ShouldHandle(details.RealmPath))
                                {
                                    await handler.HandleChangeAsync(details);
                                }
                            }
                            catch
                            {
                                // Don't skip notifications because someone didn't do a good job at error handling
                                // TODO: figure out a way to propagate those.
                            }
                        }
                    }
                    finally
                    {
                        details.PreviousRealm?.Dispose();
                        details.CurrentRealm.Dispose();
                    }
                }
            }

            private void Dispose(bool disposing)
            {
                if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
                {
                    // Don't dispose twice.
                    return;
                }

                if (disposing)
                {
                    _notifierHandle.Dispose();
                    _notificationsThread.Join();
                    _notificationsThread.Dispose();
                    _gcHandle.Free();
                    foreach (var handler in _handlers.OfType<IDisposable>())
                    {
                        handler.Dispose();
                    }
                }
            }
        }
    }
}