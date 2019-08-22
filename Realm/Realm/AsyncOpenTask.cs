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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Sync;

namespace Realms
{
    public class AsyncOpenTask
    {
        private readonly AsyncOpenTaskHandle _handle;
        private readonly Task<Realm> _getRealmTask;
        private readonly CancellationTokenSource _getRealmCancellationSource;

        internal AsyncOpenTask(AsyncOpenTaskHandle handle, TaskCompletionSource<ThreadSafeReferenceHandle> tcs, Func<SharedRealmHandle, Realm> getRealmFunc)
        {
            _handle = handle;
            var realmTcs = new TaskCompletionSource<Realm>();
            var cts = new CancellationTokenSource();
            cts.Token.Register(() => realmTcs.TrySetCanceled());
            _getRealmCancellationSource = cts;

            if (AsyncHelper.TryGetScheduler(out var scheduler))
            {
                tcs.Task.ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        realmTcs.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        realmTcs.TrySetCanceled();
                    }
                    else
                    {
                        SetResult(task);
                    }
                }, scheduler);
            }
            else
            {
                // Just block the current thread.
                SetResult(tcs.Task);
            }

            _getRealmTask = realmTcs.Task;

            void SetResult(Task<ThreadSafeReferenceHandle> t)
            {
                try
                {
                    using (var tsr = t.Result)
                    {
                        var realmPtr = SharedRealmHandle.ResolveFromReference(tsr);
                        var sharedRealmHandle = new SharedRealmHandle(realmPtr);
                        realmTcs.TrySetResult(getRealmFunc(sharedRealmHandle));
                    }
                }
                catch (Exception ex)
                {
                    realmTcs.TrySetException(ex);
                }
            }
        }

        internal AsyncOpenTask(Realm realm) : this(Task.FromResult(realm), null)
        {
        }

        internal AsyncOpenTask(Task<Realm> getRealmTask, CancellationTokenSource cts)
        {
            _getRealmTask = getRealmTask;
            _getRealmCancellationSource = cts;
        }

        public TaskAwaiter<Realm> GetAwaiter()
        {
            return _getRealmTask.GetAwaiter();
        }

        public Task<Realm> GetRealmAsync()
        {
            return _getRealmTask;
        }

        public void Cancel()
        {
            _handle.Cancel();
            _getRealmCancellationSource?.Cancel();
        }

        public IObservable<SyncProgress> GetProgressObservable()
        {
            return new SyncProgressObservable(
                register: (ptr) =>
                {
                    return _handle.RegisterProgressNotifier(ptr);
                },
                unregister: (nativeToken) =>
                {
                    _handle.UnregisterProgressNotifier(nativeToken);
                });
        }
    }
}