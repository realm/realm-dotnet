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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal static class TaskExtensions
{
    public static Task<T> Timeout<T>(this Task<T> task, int millisecondTimeout, string? detail = null)
    {
        return Task.WhenAny(task, Task.Delay(millisecondTimeout)).ContinueWith(t =>
        {
            if (t.Result != task)
            {
                var errorMessage = $"The operation has timed out after {millisecondTimeout} ms.";
                if (detail != null)
                {
                    errorMessage += $" {detail}";
                }

                throw new TimeoutException(errorMessage);
            }

            if (task.IsFaulted)
            {
                throw task.Exception!.InnerException ?? task.Exception;
            }

            return task.Result;
        });
    }

    public static async Task Timeout(this Task task, int millisecondTimeout, Task? errorTask = null, string? detail = null)
    {
        var tasks = new List<Task> { task, Task.Delay(millisecondTimeout) };
        if (errorTask != null)
        {
            tasks.Add(errorTask);
        }

        var completed = await Task.WhenAny(tasks);
        if (completed != task && completed != errorTask)
        {
            var errorMessage = $"The operation has timed out after {millisecondTimeout} ms.";
            if (detail != null)
            {
                errorMessage += $" {detail}";
            }

            throw new TimeoutException(errorMessage);
        }

        await completed;
    }

    public static async Task AddCancellation(this Task task, CancellationToken? token)
    {
        if (token == null)
        {
            await task;
        }
        else
        {
            using var cancelTask = new CancellationTask(token.Value);
            await (await Task.WhenAny(task, cancelTask.Task));
        }
    }

    private class CancellationTask : IDisposable
    {
        private readonly IDisposable? _registration;

        public CancellationTask(CancellationToken token)
        {
            var tcs = new TaskCompletionSource();
            if (token.IsCancellationRequested)
            {
                tcs.TrySetCanceled();
            }
            else
            {
                _registration = token.Register(() => 
                    tcs.TrySetCanceled());
            }

            Task = tcs.Task;
        }

        public Task Task { get; }

        public void Dispose()
        {
            _registration?.Dispose();
        }
    }
}
