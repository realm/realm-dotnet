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

namespace Realms.Native;

internal partial class SyncSocketProvider
{
    private class Timer
    {
        private readonly CancellationTokenSource _cts = new();

        internal Timer(TimeSpan delay, IntPtr nativeCallback, ChannelWriter<IWork> workQueue)
        {
            Logger.LogDefault(LogLevel.Trace, $"Creating timer with delay {delay} and target {nativeCallback}.");
            var cancellationToken = _cts.Token;
            Task.Delay(delay, cancellationToken).ContinueWith(async _ =>
            {
                await workQueue.WriteAsync(new Work(nativeCallback, cancellationToken));
            });
        }

        internal void Cancel()
        {
            Logger.LogDefault(LogLevel.Trace, $"Canceling timer.");
            _cts.Cancel();
            _cts.Dispose();
        }

        private class Work : IWork
        {
            private readonly IntPtr _nativeCallback;
            private readonly CancellationToken _cancellationToken;

            public Work(IntPtr nativeCallback, CancellationToken cancellationToken)
            {
                _nativeCallback = nativeCallback;
                _cancellationToken = cancellationToken;
            }

            public void Execute()
            {
                var status = Status.OK;
                if (_cancellationToken.IsCancellationRequested)
                {
                    status = new(ErrorCode.OperationAborted, "Timer canceled");
                }

                RunCallback(_nativeCallback, status);
            }
        }
    }

    private class EventLoopWork : IWork
    {
        private readonly IntPtr _nativeCallback;
        private readonly Status _status;

        // Belongs to SyncSocketProvider. When Native destroys the Provider we need to stop executing
        // enqueued work, but we need to release all the callbacks we copied on the heap.
        private readonly CancellationToken _cancellationToken;

        public EventLoopWork(IntPtr nativeCallback, Status status, CancellationToken cancellationToken)
        {
            _nativeCallback = nativeCallback;
            _status = status;
            _cancellationToken = cancellationToken;
        }

        public void Execute()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                Logger.LogDefault(LogLevel.Trace, "Deleting EventLoopWork callback only because event loop was cancelled.");
                NativeMethods.delete_callback(_nativeCallback);
                return;
            }

            RunCallback(_nativeCallback, _status);
        }
    }

    private static void RunCallback(IntPtr nativeCallback, Status status)
    {
        Logger.LogDefault(LogLevel.Trace, $"SyncSocketProvider running native callback {nativeCallback} with status {status.Code} \"{status.Reason}\".");

        using var arena = new Arena();
        NativeMethods.run_callback(nativeCallback, status.Code, StringValue.AllocateFrom(status.Reason, arena));
    }

    private async Task PostWorkAsync(IntPtr nativeCallback)
    {
        Logger.LogDefault(LogLevel.Trace, "Posting work to SyncSocketProvider event loop.");
        await _workQueue.Writer.WriteAsync(new EventLoopWork(nativeCallback, Status.OK, _cts.Token));
    }

    private async partial Task WorkThread()
    {
        Logger.LogDefault(LogLevel.Trace, "Starting SyncSocketProvider event loop.");
        try
        {
            while (await _workQueue.Reader.WaitToReadAsync())
            {
                while (_workQueue.Reader.TryRead(out var work))
                {
                    work.Execute();
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogDefault(LogLevel.Error, $"Error occurred in SyncSocketProvider event loop {e.GetType().FullName}: {e.Message}");
            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                Logger.LogDefault(LogLevel.Trace, e.StackTrace);
            }

            throw;
        }

        Logger.LogDefault(LogLevel.Trace, "Exiting SyncSocketProvider event loop.");
    }
}
