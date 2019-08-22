////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms.Sync
{
    internal class SyncProgressObservable : IObservable<SyncProgress>
    {
        private readonly Func<IntPtr, ulong> _register;
        private readonly Action<ulong> _unregister;
        private readonly bool _isIndefinite;

        public SyncProgressObservable(Func<IntPtr, ulong> register, Action<ulong> unregister, bool isIndefinite = false)
        {
            _register = register;
            _unregister = unregister;
            _isIndefinite = isIndefinite;
        }

        public IDisposable Subscribe(IObserver<SyncProgress> observer)
        {
            return new ProgressNotificationToken(observer, _register, _unregister, _isIndefinite);
        }

        public class ProgressNotificationToken : IDisposable
        {
            private readonly ulong _nativeToken;
            private readonly GCHandle _gcHandle;
            private readonly IObserver<SyncProgress> _observer;
            private readonly Action<ulong> _unregister;
            private readonly bool _isIndefinite;

            private bool isDisposed;

            public ProgressNotificationToken(IObserver<SyncProgress> observer, Func<IntPtr, ulong> register, Action<ulong> unregister, bool isIndefinite)
            {
                _observer = observer;
                _gcHandle = GCHandle.Alloc(this);
                _unregister = unregister;
                _isIndefinite = isIndefinite;
                try
                {
                    _nativeToken = register(GCHandle.ToIntPtr(_gcHandle));
                }
                catch
                {
                    _gcHandle.Free();
                    throw;
                }
            }

            public void Notify(ulong transferredBytes, ulong transferableBytes)
            {
                Task.Run(() =>
                {
                    _observer.OnNext(new SyncProgress(transferredBytes, transferableBytes));
                    if (!_isIndefinite && transferredBytes == transferableBytes)
                    {
                        _observer.OnCompleted();
                    }
                });
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    GC.SuppressFinalize(this);

                    isDisposed = true;
                    _unregister(_nativeToken);
                    _gcHandle.Free();
                }
            }
        }
    }
}