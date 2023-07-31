////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Realms.Native;

namespace Realms
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe readonly struct MarshaledVector<T>
        where T : unmanaged
    {
        private readonly T* items;

        public readonly nint Count;

        public MarshaledVector(T* items, nint count)
        {
            this.items = items;
            Count = count;
        }

        public ref readonly T this[nint index]
        {
            get
            {
                if (index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return ref Unsafe.Add(ref *items, index);
            }
        }

        public ref struct Enumerator
        {
            private readonly MarshaledVector<T> _vector;
            private nint _index;

            public Enumerator(MarshaledVector<T> vector)
            {
                _vector = vector;
                _index = -1;
            }

            public ref readonly T Current => ref _vector[_index];

            public bool MoveNext()
            {
                var index = _index + 1;
                if (index < _vector.Count)
                {
                    _index = index;
                    return true;
                }

                return false;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public IEnumerable<T> ToEnumerable()
        {
            for (nint index = 0; index < Count; index++)
            {
                yield return this[index];
            }
        }

        public T[] ToArray()
        {
            var ret = new T[Count];
            fixed(T* destination = ret)
            {
                var byteSize = sizeof(T) * Count;
                Buffer.MemoryCopy(items, destination, byteSize, byteSize);
            }

            return ret;
        }

        public static MarshaledVector<T> AllocateEmpty(int capacity, Arena arena)
        {
            var buffer = arena.Allocate<T>(capacity);
            Unsafe.InitBlock(buffer.Data, 0, (uint)(sizeof(T) * capacity));
            return new MarshaledVector<T>(buffer.Data, capacity);
        }

        public static unsafe MarshaledVector<T> AllocateFrom(IReadOnlyCollection<T>? collection, Arena arena)
        {
            if (collection == null || collection.Count == 0)
            {
                return new MarshaledVector<T>(null, 0);
            }

            var buffer = arena.Allocate<T>(collection.Count);
            var i = 0;
            foreach (var item in collection)
            {
                buffer.Data[i++] = item;
            }

            return new MarshaledVector<T>(buffer.Data, buffer.Length);
        }
    }
}
