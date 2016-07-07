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
    unsafe struct NativeException
    {
        IntPtr type;
        sbyte* messageBytes;
        IntPtr messageLength;
        public bool WasThrown => type != (IntPtr) 1000;

        unsafe internal Exception Convert()
        {
            var message = (messageLength != IntPtr.Zero) ?
                new string(messageBytes, 0 /* start offset */, (int)messageLength, Encoding.UTF8)
                : "no detail on exception";

            return RealmException.Create((RealmExceptionCodes)type, message);
        }
    }

    internal static class NativeCommon
    {
        // declare the type for the MonoPInvokeCallback
        public delegate void ExceptionThrowerCallback (NativeException exception);

        public delegate void NotifyRealmCallback (IntPtr realmHandle);

        #if DEBUG
        public delegate void DebugLoggerCallback (IntPtr utf8String, IntPtr stringLen);
        #endif

        #if __IOS__
        [MonoPInvokeCallback (typeof (ExceptionThrowerCallback))]
        #endif
        public static void ExceptionThrower(NativeException exception)
        {
            throw exception.Convert();
        }

        // once-off setup of a function pointer in the DLL which will be used later to throw managed exceptions       
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_exception_thrower", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_exception_thrower(ExceptionThrowerCallback callback);

        #if DEBUG

        #if __IOS__
        [MonoPInvokeCallback (typeof (DebugLoggerCallback))]
        #endif
        unsafe private static void DebugLogger(IntPtr utf8String, IntPtr stringLen)
        {
            var message = new String((sbyte*)utf8String, 0 /* start offset */, (int)stringLen, Encoding.UTF8);
            Console.WriteLine(message);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_debug_logger", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_debug_logger(DebugLoggerCallback callback);
        #endif  // DEBUG

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "register_notify_realm_changed", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void register_notify_realm_changed(NotifyRealmCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "fake_a_native_exception", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fake_a_native_exception(IntPtr errorCode);

        public static void Initialize()
        {
            set_exception_thrower (ExceptionThrower);

            #if DEBUG
            set_debug_logger(DebugLogger);
            #endif
        }
    }
}  // namespace Realms
