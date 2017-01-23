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
            var token = new ProgressNotificationToken(_session, observer);
            token.Token = _session.Handle.RegisterProgressNotifier(GCHandle.ToIntPtr(token.GCHandle), _direction, _mode);
            return token;
        }

        public class ProgressNotificationToken : IDisposable
        {
            private bool isDisposed;
            private Session _session;
            private IObserver<SyncProgress> _observer;

            public ulong Token { get; set; }

            public GCHandle GCHandle { get; }

            public ProgressNotificationToken(Session session, IObserver<SyncProgress> observer)
            {
                _session = session;
                _observer = observer;
                GCHandle = GCHandle.Alloc(this);
            }

            public void Notify(ulong transferredBytes, ulong transferrableBytes)
            {
                Task.Run(() =>
                {
                    _observer.OnNext(new SyncProgress(transferredBytes, transferrableBytes));
                });
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;

                    _session.Handle.UnregisterProgressNotifier(Token);
                    GCHandle.Free();
                    _session = null;
                    _observer = null;
                }
            }
        }
    }
}