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
            return new ProgressNotificationToken(progress =>
            {
                observer.OnNext(progress);
                if (_mode == ProgressMode.ForCurrentlyOutstandingWork && progress.IsComplete)
                {
                    observer.OnCompleted();
                }
            }, handle => _session.Handle.RegisterProgressNotifier(handle, _direction, _mode), _session.Handle.UnregisterProgressNotifier);
        }
    }
}
