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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Realms.Server
{
    internal class CalculationProcessor<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, object> _locks = new ConcurrentDictionary<TKey, object>();
        private readonly IDictionary<TKey, Queue<TValue>> _calculationDictionary = new Dictionary<TKey, Queue<TValue>>();

        private readonly Func<TValue, Task> _calculator;

        public CalculationProcessor(Func<TValue, Task> calculator)
        {
            _calculator = calculator;
        }

        public void Enqueue(TKey key, TValue value)
        {
            var locker = _locks.GetOrAdd(key, _ => new object());
            lock (locker)
            {
                if (_calculationDictionary.TryGetValue(key, out var queue))
                {
                    queue.Enqueue(value);
                }
                else
                {
                    queue = new Queue<TValue>(new[] { value });
                    _calculationDictionary.Add(key, queue);
                    ProcessQueue(key, queue);
                }
            }
        }

        private void ProcessQueue(TKey key, Queue<TValue> queue)
        {
            Task.Run(() => AsyncContext.Run(async () =>
            {
                if (!_locks.TryGetValue(key, out var locker))
                {
                            // TODO: figure out how to propagate this
                            throw new Exception("Should not be possible!");
                }

                while (TryDequeue(key, queue, out var value))
                {
                    await _calculator(value);
                }
            }));
        }

        private bool TryDequeue(TKey key, Queue<TValue> queue, out TValue value)
        {
            if (!_locks.TryGetValue(key, out var locker))
            {
                // TODO: figure out how to propagate this
                throw new Exception("Should not be possible!");
            }

            bool success;
            lock (locker)
            {
                if (success = queue.Count > 0)
                {
                    value = queue.Dequeue();
                }
                else
                {
                    value = default(TValue);
                    _calculationDictionary.Remove(key);
                }
            }

            return success;
        }
    }
}