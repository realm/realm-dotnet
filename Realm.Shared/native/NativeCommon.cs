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
 
/**
@file NativeCommon.cs provides mappings to common functions that don't fit the Table classes etc.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DotNetCross.Memory;

#if __IOS__
using ObjCRuntime;
#endif

namespace Realms {

    [StructLayout(LayoutKind.Sequential)]
    struct MarshalledVector<T> where T : struct
    {
        IntPtr items;
        IntPtr count;

        internal IEnumerable<T> AsEnumerable()
        {
            return Enumerable.Range(0, (int)count).Select(MarshalElement);
        }

        unsafe T MarshalElement(int elementIndex)
        {
            var @struct = default(T);
            Unsafe.CopyBlock(Unsafe.AsPointer(ref @struct), IntPtr.Add(items, elementIndex * Unsafe.SizeOf<T>()).ToPointer(), (uint)Unsafe.SizeOf<T>());
            return @struct;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct PtrTo<T> where T : struct
    {
        void* ptr;

        internal T? Value
        {
            get
            {
                if (ptr == null)
                {
                    return null;
                }

                var @struct = default(T);
                Unsafe.CopyBlock(Unsafe.AsPointer(ref @struct), ptr, (uint)Unsafe.SizeOf<T>());
                return @struct;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeException
    {
        public IntPtr type;
        public sbyte* messageBytes;
        public IntPtr messageLength;
    }

    public static class NativeExceptionExtensions
    {
        internal static unsafe Exception Convert(this NativeException @this)
        {
            var message = (@this.messageLength != IntPtr.Zero) ?
                new string(@this.messageBytes, 0 /* start offset */, (int)@this.messageLength, Encoding.UTF8)
                : "No further information available";
            NativeCommon.delete_pointer(@this.messageBytes);

            return RealmException.Create((RealmExceptionCodes)@this.type, message);
        }

        internal static void ThrowIfNecessary(this NativeException @this)
        {
            if (@this.type == (IntPtr)1000)
                return;

            throw @this.Convert();
        }
    }

    internal static class NativeCommon
    {
        public delegate void NotifyRealmCallback (IntPtr realmHandle);

        #if DEBUG
        public delegate void DebugLoggerCallback (IntPtr utf8String, IntPtr stringLen);

        #if __IOS__
        [MonoPInvokeCallback (typeof (DebugLoggerCallback))]
        #endif
        private static unsafe void DebugLogger(IntPtr utf8String, IntPtr stringLen)
        {
            var message = new String((sbyte*)utf8String, 0 /* start offset */, (int)stringLen, Encoding.UTF8);
            Console.WriteLine(message);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_debug_logger", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_debug_logger(DebugLoggerCallback callback);
        #endif  // DEBUG

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "register_notify_realm_changed", CallingConvention = CallingConvention.Cdecl)]
        private static extern void register_notify_realm_changed(NotifyRealmCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void delete_pointer(void* pointer);

        public static void Initialize()
        {
            #if DEBUG
            set_debug_logger(DebugLogger);
            #endif
        }
    }
}  // namespace Realms
