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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Logging;

namespace Realms.Sync
{
    internal class ProgressNotificationToken : IDisposable
    {
        private readonly ulong _nativeToken;
        private readonly GCHandle _gcHandle;
        private readonly Action<SyncProgress> _observer;
        private readonly Action<ulong> _unregister;

        private bool _isDisposed;

        public ProgressNotificationToken(Action<SyncProgress> observer, Func<GCHandle, ulong> register, Action<ulong> unregister)
        {
            _observer = observer;
            _gcHandle = GCHandle.Alloc(this);
            _unregister = unregister;
            try
            {
                _nativeToken = register(_gcHandle);
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
                try
                {
                    _observer(new SyncProgress(transferredBytes, transferableBytes));
                }
                catch (Exception ex)
                {
                    Logger.Default.Log(LogLevel.Warn, $"An error occurred while reporting progress: {ex}");
                }
            });
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);

                _isDisposed = true;
                _unregister(_nativeToken);
                _gcHandle.Free();
            }
        }
    }
}
