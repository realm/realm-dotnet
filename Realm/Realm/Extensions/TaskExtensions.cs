﻿////////////////////////////////////////////////////////////////////////////
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

            throw new TimeoutException($"The operation has timed out after {millisecondTimeout} ms.");
        });
    }

    public static async Task Timeout(this Task task, int millisecondTimeout, Task errorTask = null)
    {
        var tasks = new List<Task> { task, Task.Delay(millisecondTimeout) };
        if (errorTask != null)
        {
            tasks.Add(errorTask);
        }

        var completed = await Task.WhenAny(tasks);
        if (completed != task && completed != errorTask)
        {
            throw new TimeoutException($"The operation has timed out after {millisecondTimeout} ms.");
        }

        await completed;
    }
}
