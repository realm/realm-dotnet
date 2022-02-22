////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Realms.Tests
{
    public class EventHelper<TSender, TArgs>
    {
        private readonly ConcurrentQueue<TaskCompletionSource<(TSender, TArgs)>> _consumers = new ConcurrentQueue<TaskCompletionSource<(TSender, TArgs)>>();

        public void OnEvent(object sender, TArgs args)
        {
            if (_consumers.TryDequeue(out var tcs))
            {
                tcs.TrySetResult(((TSender)sender, args));
            }
        }

        public Task<(TSender Sender, TArgs Args)> GetNextEventAsync()
        {
            var tcs = new TaskCompletionSource<(TSender, TArgs)>();
            _consumers.Enqueue(tcs);
            return tcs.Task;
        }
    }
}
