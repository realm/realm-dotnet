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
using System.Runtime.InteropServices;
using System.Text;

#if __IOS__
using ObjCRuntime;
#endif

namespace Realms {
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
        public static extern void set_debug_logger(DebugLoggerCallback callback);
        #endif  // DEBUG

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "register_notify_realm_changed", CallingConvention = CallingConvention.Cdecl)]
        public static extern void register_notify_realm_changed(NotifyRealmCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "delete_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void delete_pointer(void* pointer);

        public static void Initialize()
        {
            #if DEBUG
            set_debug_logger(DebugLogger);
            #endif
        }
    }
}  // namespace Realms
