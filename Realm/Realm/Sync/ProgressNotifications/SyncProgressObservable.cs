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
        private readonly SessionHandle _sessionHandle;
        private readonly ProgressDirection _direction;
        private readonly ProgressMode _mode;

        public SyncProgressObservable(SessionHandle sessionHandle, ProgressDirection direction, ProgressMode mode)
        {
            _sessionHandle = sessionHandle;
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
            }, handle => _sessionHandle.RegisterProgressNotifier(handle, _direction, _mode), _sessionHandle.UnregisterProgressNotifier);
        }
    }
}
