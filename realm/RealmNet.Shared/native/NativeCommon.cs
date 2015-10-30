/*
 * Copyright 2015 Realm Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
/**
@file NativeCommon.cs provides mappings to common functions that don't fit the Table classes etc.
*/
using System;
using System.Runtime.InteropServices;
using System.Text;


#if __IOS__
using ObjCRuntime;
#endif

namespace RealmNet {

    internal static class NativeCommon
    {
        // declare the type for the MonoPInvokeCallback
        public delegate void ExceptionThrowerCallback (IntPtr exceptionCode, IntPtr utf8String, IntPtr stringLen);


        #if __IOS__
        [MonoPInvokeCallback (typeof (ExceptionThrowerCallback))]
        #endif
        unsafe public static void ExceptionThrower(IntPtr exceptionCode, IntPtr utf8String, IntPtr stringLen)
        {
            String message = ((Int64)stringLen > 0) ?
                new String((sbyte*)utf8String, 0 /* start offset */, (int)stringLen, Encoding.UTF8)
                : "no detail on exception";

            // these are increasing enum value order
            switch ((RealmExceptionCodes)exceptionCode) {
            case RealmExceptionCodes.RealmError:
                throw new RealmException(message);

            case RealmExceptionCodes.RealmFileAccessError:
                throw new RealmFileAccessErrorException(message);

            case RealmExceptionCodes.RealmDecryptionFailed:
                throw new RealmDecryptionFailedException(message);

            case RealmExceptionCodes.RealmFileExists:
                throw new RealmFileExistsException(message);

            case RealmExceptionCodes.RealmFileNotFound :
                throw new RealmFileNotFoundException(message);

            case RealmExceptionCodes.RealmInvalidDatabase :
                throw new RealmInvalidDatabaseException(message);

            case RealmExceptionCodes.RealmOutOfMemory :
                throw new RealmOutOfMemoryException(message);

            case RealmExceptionCodes.RealmPermissionDenied :
                throw new RealmPermissionDeniedException(message);

            case RealmExceptionCodes.StdArgumentOutOfRange :
                throw new ArgumentOutOfRangeException(message);

            case RealmExceptionCodes.StdIndexOutOfRange :
                throw new IndexOutOfRangeException(message);

            case RealmExceptionCodes.StdInvalidOperation :
                throw new InvalidOperationException(message);

            default:
                throw new Exception(message);
            }
        }


        // once-off setup of a function pointer in the DLL which will be used later to throw managed exceptions       
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_exception_thrower", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_exception_thrower(ExceptionThrowerCallback callback);
        public static void SetupExceptionThrower()
        {
            set_exception_thrower (ExceptionThrower);
        }

    }
}  // namespace RealmNet
