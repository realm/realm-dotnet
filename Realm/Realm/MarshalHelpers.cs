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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Realms.Exceptions;

namespace Realms
{
    internal class MarshalHelpers
    {
        public delegate IntPtr NativeCollectionGetter(IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex);

        public delegate void NativeCollectionSetter(IntPtr buffer, IntPtr bufferLength, bool hasValue, out NativeException ex);

        public static string GetString(NativeCollectionGetter getter)
        {
            // TODO: rework to use GetCollection
            var bufferSize = 128;

            // First alloc this thread
            var stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSize * sizeof(char)));

            try
            {
                // try to read
                var bytesRead = (int)getter(stringGetBuffer, (IntPtr)bufferSize, out var isNull, out var nativeException);
                nativeException.ThrowIfNecessary();

                if (bytesRead == -1)
                {
                    throw new RealmInvalidDatabaseException("Corrupted string data");
                }

                // need a bigger buffer
                if (bytesRead > bufferSize)
                {
                    bufferSize = bytesRead;

                    Marshal.FreeHGlobal(stringGetBuffer);
                    stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSize * sizeof(char)));

                    // try to read with big buffer
                    bytesRead = (int)getter(stringGetBuffer, (IntPtr)bufferSize, out isNull, out nativeException);
                    nativeException.ThrowIfNecessary();

                    // bad UTF-8 in full string
                    if (bytesRead == -1)
                    {
                        throw new RealmInvalidDatabaseException("Corrupted string data");
                    }

                    Debug.Assert(bytesRead <= bufferSize, "Buffer must have overflowed.");
                } // needed re-read with expanded buffer

                return bytesRead != 0 ? Marshal.PtrToStringUni(stringGetBuffer, bytesRead) : (isNull ? null : string.Empty);
            }
            finally
            {
                Marshal.FreeHGlobal(stringGetBuffer);
            }
        }

        public delegate IntPtr NativeCollectionGetter<T>(T[] buffer, IntPtr bufferLength, out NativeException ex)
            where T : struct;

        public static unsafe byte[] GetByteArray(NativeCollectionGetter getter, int size = 0)
        {
            // Initially called with size = 0, we make a native call just to get the size of the buffer.
            var bytes = new byte[size];
            bool isNull;
            NativeException nativeException;

            int actualSize;
            fixed (byte* buffer = bytes)
            {
                actualSize = (int)getter((IntPtr)buffer, (IntPtr)size, out isNull, out nativeException);
            }

            nativeException.ThrowIfNecessary();

            if (isNull)
            {
                return null;
            }

            if (actualSize > size)
            {
                return GetByteArray(getter, actualSize);
            }

            return bytes;
        }

        public static unsafe void SetByteArray(byte[] bytes, NativeCollectionSetter setter)
        {
            NativeException nativeException;
            if (bytes == null)
            {
                setter(IntPtr.Zero, IntPtr.Zero, false, out nativeException);
            }
            else if (bytes.Length == 0)
            {
                // empty byte arrays are expressed in terms of a BinaryData object with a dummy pointer and zero size
                // that's how core differentiates between empty and null buffers
                setter((IntPtr)0x1, IntPtr.Zero, true, out nativeException);
            }
            else
            {
                fixed (byte* buffer = bytes)
                {
                    setter((IntPtr)buffer, (IntPtr)bytes.Length, true, out nativeException);
                }
            }

            nativeException.ThrowIfNecessary();
        }

        public static T[] GetCollection<T>(NativeCollectionGetter<T> getter, int bufferSize)
            where T : struct
        {
            var buffer = new T[bufferSize];

            var itemsRead = (int)getter(buffer, (IntPtr)bufferSize, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (itemsRead > bufferSize)
            {
                bufferSize = itemsRead;
                buffer = new T[bufferSize];

                itemsRead = (int)getter(buffer, (IntPtr)bufferSize, out nativeException);
                nativeException.ThrowIfNecessary();

                Debug.Assert(itemsRead <= bufferSize, "Buffer must have overflowed.");
            }

            Array.Resize(ref buffer, itemsRead);
            return buffer;
        }
    }
}
