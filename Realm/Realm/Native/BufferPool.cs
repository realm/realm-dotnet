// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2023 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Realms.Native
{
    internal class BufferPool : IDisposable
    {
        private readonly List<IDisposable> _buffers = new();

        public static BufferPool? Current => MakeCurrentHelper.CurrentBufferPool;

        public BufferPool()
        {
        }

        public unsafe Buffer<T> Rent<T>(int count = 1)
            where T : unmanaged
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var buffer = new Buffer<T>((T*)Marshal.AllocHGlobal(count * sizeof(T)), count);
            _buffers.Add(buffer);
            return buffer;
        }

        public IDisposable MakeCurrent() => new MakeCurrentHelper(this);

        public void Dispose()
        {
            _buffers.ForEach(b => b.Dispose());
        }

        public unsafe class Buffer<T> : MemoryManager<T>
            where T : unmanaged
        {
            public T* Data { get; }

            public int Length { get; }

            internal Buffer(T* data, int length)
            {
                Data = data;
                Length = length;
            }

            public override Span<T> GetSpan() => new Span<T>(Data, Length);

            public override MemoryHandle Pin(int elementIndex = 0)
            {
                if (elementIndex < 0 || elementIndex >= Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(elementIndex));
                }

                return new MemoryHandle(Data + elementIndex);
            }

            public override void Unpin()
            {
            }

            protected override void Dispose(bool disposing)
            {
                Marshal.FreeHGlobal((IntPtr)Data);
            }
        }

        private class MakeCurrentHelper : IDisposable
        {
            private static readonly AsyncLocal<Stack<BufferPool>> _pools = new();
            private readonly Stack<BufferPool> _stack;
            private readonly BufferPool _parent;

            public static BufferPool? CurrentBufferPool => _pools.Value?.Count > 0 ? _pools.Value?.Peek() : null;

            public MakeCurrentHelper(BufferPool parent)
            {
                _stack = _pools.Value ??= new();
                _parent = parent;

                Debug.Assert(!_stack.Contains(parent), "BufferPool is not reentrant");

                _stack.Push(parent);
            }

            public void Dispose()
            {
                Debug.Assert(_stack == _pools.Value, "BufferPool expected to be disposed on the same async context as the pool was entered on.");
                var popped = _stack.Pop();
                Debug.Assert(_parent == popped, "Expected BufferPool to be at the top of the stack when disposed.");
            }
        }
    }
}
