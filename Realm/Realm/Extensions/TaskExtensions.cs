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
using System.Threading.Tasks;

internal static class TaskExtensions
{
    public static Task<T> Timeout<T>(this Task<T> task, int millisecondTimeout)
    {
        return Task.WhenAny(task, Task.Delay(millisecondTimeout)).ContinueWith(t =>
        {
            if (t.Result == task)
            {
                if (task.IsFaulted)
                {
                    throw task.Exception.InnerException;
                }

                return task.Result;
            }

            throw new TimeoutException("The operation has timed out.");
        });
    }

    public static async Task Timeout(this Task task, int millisecondTimeout)
    {
        var completed = await Task.WhenAny(task, Task.Delay(millisecondTimeout));
        if (completed != task)
        {
            throw new TimeoutException("The operation has timed out.");
        }

        await completed;
    }
}