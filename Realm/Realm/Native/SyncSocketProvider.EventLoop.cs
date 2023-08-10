////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Realms.Logging;

namespace Realms.Native
{
    internal partial class SyncSocketProvider
    {
        private class Timer
        {
            private readonly CancellationTokenSource _cts = new();

            internal Timer(TimeSpan delay, IntPtr nativeCallback, ChannelWriter<IWork> workQueue)
            {
                Logger.LogDefault(LogLevel.Trace, $"Creating timer with delay {delay} and target {nativeCallback}.");
                Task.Delay(delay, _cts.Token).ContinueWith(t =>
                {
                    var status = Status.OK;
                    if (t.IsCanceled)
                    {
                        status = new(NativeMethods.ErrorCode.OperationAborted, "Timer canceled");
                    }

                    return workQueue.WriteAsync(new EventLoopWork(nativeCallback, status, default));
                });
            }

            internal void Cancel()
            {
                Logger.LogDefault(LogLevel.Trace, $"Canceling timer.");
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        private class EventLoopWork : IWork
        {
            private readonly IntPtr _nativeCallback;
            private readonly Status _status;
            private readonly CancellationToken _cancellationToken;

            public EventLoopWork(IntPtr nativeCallback, Status status, CancellationToken cancellationToken)
            {
                _nativeCallback = nativeCallback;
                _status = status;
                _cancellationToken = cancellationToken;

            }

            public unsafe void Execute()
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    NativeMethods.run_callback(_nativeCallback, NativeMethods.ErrorCode.Ok, StringValue.Null, delete_only: true);
                    return;
                }

                if (!string.IsNullOrEmpty(_status.Reason))
                {
                    var bytes = Encoding.UTF8.GetBytes(_status.Reason);
                    fixed (byte* data = bytes)
                    {
                        var reason = new StringValue { data = data, size = bytes.Length };
                        NativeMethods.run_callback(_nativeCallback, _status.Code, reason, false);
                    }
                }
                else
                {
                    NativeMethods.run_callback(_nativeCallback, _status.Code, StringValue.Null, false);
                }
            }
        }

        private partial async Task WorkThread()
        {
            while (await _workQueue.Reader.WaitToReadAsync())
            {
                while (_workQueue.Reader.TryRead(out var work))
                {
                    work.Execute();
                }
            }
        }
    }
}
