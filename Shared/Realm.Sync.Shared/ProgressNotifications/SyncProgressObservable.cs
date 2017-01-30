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
        private readonly Session _session;
        private readonly ProgressDirection _direction;
        private readonly ProgressMode _mode;

        public SyncProgressObservable(Session session, ProgressDirection direction, ProgressMode mode)
        {
            _session = session;
            _direction = direction;
            _mode = mode;
        }

        public IDisposable Subscribe(IObserver<SyncProgress> observer)
        {
            return new ProgressNotificationToken(_session, observer, _direction, _mode);
        }

        public class ProgressNotificationToken : IDisposable
        {
            private readonly ulong _nativeToken;
            private readonly GCHandle _gcHandle;

            private bool isDisposed;
            private Session _session;
            private IObserver<SyncProgress> _observer;

            public ProgressNotificationToken(Session session, 
                                             IObserver<SyncProgress> observer, 
                                             ProgressDirection direction, 
                                             ProgressMode mode)
            {
                _session = session;
                _observer = observer;
                _gcHandle = GCHandle.Alloc(this);
                try
                {
                    _nativeToken = _session.Handle.RegisterProgressNotifier(GCHandle.ToIntPtr(_gcHandle), direction, mode);
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
                });
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;

                    _session.Handle.UnregisterProgressNotifier(_nativeToken);
                    _gcHandle.Free();
                    _session = null;
                    _observer = null;
                }
            }
        }
    }
}