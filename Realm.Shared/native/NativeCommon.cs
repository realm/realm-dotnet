/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
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

namespace Realms {

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

            case RealmExceptionCodes.RealmMismatchedConfig:
                throw new RealmMismatchedConfigException(message);

            case RealmExceptionCodes.RealmFormatUpgradeRequired :
                throw new RealmMigrationNeededException(message);

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


        
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "fake_a_native_exception", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fake_a_native_exception(IntPtr errorCode);

    }
}  // namespace Realms
