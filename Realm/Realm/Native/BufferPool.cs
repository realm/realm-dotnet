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
using System.Runtime.InteropServices;

namespace Realms.Native
{
    internal class BufferPool : IDisposable
    {
        private readonly List<IDisposable> _buffers = new();
        private bool _disposed;

        public Buffer<T> Rent<T>(int count = 1)
            where T : unmanaged
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(BufferPool).Name);
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var buffer = new Buffer<T>(ArrayPool<T>.Shared, count);
            _buffers.Add(buffer);
            return buffer;
        }

        public void Dispose()
        {
            _disposed = true;

            foreach (var buffer in _buffers)
            {
                buffer.Dispose();
            }

            _buffers.Clear();
        }

        public readonly struct Buffer<T> : IDisposable
            where T : unmanaged
        {
            private readonly ArrayPool<T> _pool;
            private readonly T[] _buffer;
            private readonly GCHandle _handle;

            public unsafe T* Data => (T*)_handle.AddrOfPinnedObject();

            public int Length { get; }

            internal Buffer(ArrayPool<T> pool, int length)
            {
                _pool = pool;
                _buffer = pool.Rent(length);
                _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                Length = length;
            }

            void IDisposable.Dispose()
            {
                _handle.Free();
                _pool.Return(_buffer);

            }
        }
    }
}
