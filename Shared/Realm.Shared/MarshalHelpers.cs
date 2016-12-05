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
using System.Linq;
using System.Runtime.InteropServices;
using DotNetCross.Memory;

namespace Realms
{
    internal class MarshalHelpers
    {
        public static IntPtr BoolToIntPtr(bool value)
        {
            return value ? (IntPtr)1 : (IntPtr)0;
        }

        public static bool IntPtrToBool(IntPtr value)
        {
            return (IntPtr)1 == value;
        }

        public static IntPtr RealmColType(Type columnType)
        {
            // values correspond to core/data_type.hpp enum DataType
            // ordered in decreasing likelihood of type
            if (columnType == typeof(string))
            {
                return (IntPtr)2;  // type_String
            }

            if (columnType == typeof(char) || columnType == typeof(byte) || columnType == typeof(short) || columnType == typeof(int) || columnType == typeof(long) ||
                columnType == typeof(char?) || columnType == typeof(byte?) || columnType == typeof(short?) || columnType == typeof(int?) || columnType == typeof(long?))
            {
                return (IntPtr)0;  // type_Int
            }

            if (columnType == typeof(float) || columnType == typeof(float?))
            {
                return (IntPtr)9;  // type_Float
            }

            if (columnType == typeof(double) || columnType == typeof(double?))
            {
                return (IntPtr)10; // type_Double
            }

            if (columnType == typeof(DateTimeOffset) || columnType == typeof(DateTimeOffset?))
            {
                return (IntPtr)8;  // type_Timestamp
            }

            if (columnType == typeof(bool) || columnType == typeof(bool?))
            {
                return (IntPtr)1;  // type_Bool
            }

            if (columnType == typeof(byte[]))
            {
                return (IntPtr)4; // type_Data
            }

            if (columnType.BaseType == typeof(RealmObject))
            {
                return (IntPtr)12;  // type_Link
            }

            if (columnType.IsGenericType)
            {
                var type = columnType.GetGenericTypeDefinition();
                if (type == typeof(RealmList<>) || type == typeof(IList<>))
                {
                    return (IntPtr)13;  // type_LinkList 
                }
            }

            /*
            TODO
                    Table = 5, // type_Table for sub-tables, not relatinoships????
                    Mixed = 6, // type_Mixed

            */
            throw new NotImplementedException("Type " + columnType.FullName + " not supported");
        }

        public static string GetString(NativeCollectionGetter getter)
        {
            return GetCollection<string>(getter, 128, sizeof(char), (buffer, size, isNull) =>
            {
                if (size == 0)
                {
                    return isNull ? null : string.Empty;
                }

                return Marshal.PtrToStringUni(buffer, size);
            });
        }

        public static unsafe IntPtr[] GetArray(NativeCollectionGetter getter, int bufferSize)
        {
            var intPtrSize = Unsafe.SizeOf<IntPtr>();

            return GetCollection(getter, bufferSize, intPtrSize, (buffer, size, isNull) =>
            {
                if (size == 0)
                {
                    return isNull ? null : new IntPtr[0];
                }

                return Enumerable.Range(0, size)
                                 .Select(i => MarshalElement<IntPtr>(i, buffer, intPtrSize))
                                 .ToArray();
            });
        }

        public delegate IntPtr NativeCollectionGetter(IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex);

        private delegate TCollection Marshaller<TCollection>(IntPtr buffer, int size, bool isNull);

        private static TCollection GetCollection<TCollection>(NativeCollectionGetter getter, int bufferSize, int structSize, Marshaller<TCollection> marshaller) 
        {
            var buffer = Marshal.AllocHGlobal((IntPtr)(bufferSize * structSize));

            bool isNull;
            NativeException nativeException;

            var itemsRead = (int)getter(buffer, (IntPtr)bufferSize, out isNull, out nativeException);
            nativeException.ThrowIfNecessary();
            if (itemsRead == -1)
            {
                throw new RealmInvalidDatabaseException("Corrupted data");
            }

            if (itemsRead > bufferSize)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = Marshal.AllocHGlobal((IntPtr)(itemsRead * structSize));
                bufferSize = itemsRead;

                itemsRead = (int)getter(buffer, (IntPtr)bufferSize, out isNull, out nativeException);
                nativeException.ThrowIfNecessary();

                if (itemsRead == -1)
                {
                    throw new RealmInvalidDatabaseException("Corrupted data");
                }

                Debug.Assert(itemsRead <= bufferSize, "Buffer must have overflowed.");
            }

            return marshaller(buffer, itemsRead, isNull);
        }

        private static unsafe T MarshalElement<T>(int elementIndex, IntPtr buffer, int structSize)
        {
            var @struct = default(T);
            Unsafe.CopyBlock(Unsafe.AsPointer(ref @struct), IntPtr.Add(buffer, elementIndex * structSize).ToPointer(), (uint)structSize);
            return @struct;
        }
    }
}
