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

namespace Realms.Native
{
    internal class BufferPool : IDisposable
    {
        private readonly Dictionary<int, List<Slab>> _slabs = new();
        private bool _disposed;

        public unsafe Buffer<T> Rent<T>(int count = 1)
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

            if (!_slabs.TryGetValue(sizeof(T), out var bucket))
            {
                _slabs.Add(sizeof(T), bucket = new List<Slab>());
            }

            Slab? slab = null;
            foreach (var candidate in bucket)
            {
                if (candidate.Fits(count))
                {
                    slab = candidate;
                    break;
                }
            }

            if (slab == null)
            {
                bucket.Add(slab = new Slab(sizeof(T), count));
            }

            Debug.Assert(slab.ElementSize == sizeof(T), "Trying to append to slab with the wrong element size");

            return new Buffer<T>((T*)slab.Grab(count), count);
        }

        ~BufferPool()
        {
            Debug.Assert(_disposed, "BufferPool finalized without explicit disposal");
        }

        public void Dispose()
        {
            _disposed = true;

            foreach (var kvp in _slabs)
            {
                kvp.Value.ForEach(x => x.Dispose());
            }

            _slabs.Clear();
        }

        public readonly struct Buffer<T>
            where T : unmanaged
        {
            public unsafe T* Data { get; }

            public int Length { get; }

            internal unsafe Buffer(T* data, int length)
            {
                Data = data;
                Length = length;
            }
        }

        private class Slab : IDisposable
        {
            public static readonly int Size = Environment.SystemPageSize;

            public IntPtr Buffer { get; }

            public int ElementSize { get; }

            public int MaxCount { get; }

            public int Count { get; private set; }

            public Slab(int elementSize, int minimumCount)
            {
                ElementSize = elementSize;
                Count = 0;

                var elementsPerPage = Environment.SystemPageSize / elementSize;
                if (minimumCount <= elementsPerPage)
                {
                    // the minimum required allocation can fit on one page
                    MaxCount = elementsPerPage;
                }
                else
                {
                    // the minimum required allocation doesn't fit on a single page
                    // so allocate enough pages to hold it
                    MaxCount = (int)Math.Ceiling(minimumCount / (double)elementsPerPage) * elementsPerPage;
                }

                Buffer = Marshal.AllocHGlobal(MaxCount * elementSize);
            }

            public bool Fits(int count) => Count + count <= MaxCount;

            public IntPtr Grab(int count)
            {
                if (count > MaxCount)
                {
                    throw new InvalidOperationException($"Can't fit {count} items in a slab that can hold {MaxCount} items at most");
                }

                Debug.Assert(Fits(count), "Can't grab more from the slab than it can fit");

                var start = Buffer + (Count * ElementSize);
                Count += count;
                return start;
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(Buffer);
            }
        }
    }
}
