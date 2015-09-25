/**
@file NativeCommon.cs provides mappings to common functions that don't fit the Table classes etc.
*/
using System;
using System.Runtime.InteropServices;
using Interop.Config;
using System.Text;


#if __IOS__
using ObjCRuntime;
#endif

namespace RealmNet
{
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
            case RealmExceptionCodes.Exception_ClassNotFound :
                throw new RealmClassNotFoundException(message);
                break;

            case RealmExceptionCodes.Exception_NoSuchField :
                throw new RealmNoSuchFieldException(message);
                break;

            case RealmExceptionCodes.Exception_NoSuchMethod :
                throw new RealmNoSuchMethodException(message);
                break;

            case RealmExceptionCodes.Exception_IllegalArgument :
                throw new RealmIllegalArgumentException(message);
                break;

            case RealmExceptionCodes.Exception_IOFailed :
                throw new RealmIOFailedException(message);
                break;

            case RealmExceptionCodes.Exception_FileNotFound :
                throw new RealmFileNotFoundException(message);
                break;

            case RealmExceptionCodes.Exception_FileAccessError :
                throw new RealmFileAccessErrorException(message);
                break;

            case RealmExceptionCodes.Exception_IndexOutOfBounds :
                throw new RealmIndexOutOfBoundsException(message);
                break;

            case RealmExceptionCodes.Exception_TableInvalid :
                throw new RealmTableInvalidException(message);
                break;

            case RealmExceptionCodes.Exception_UnsupportedOperation :
                throw new RealmUnsupportedOperationException(message);
                break;

            case RealmExceptionCodes.Exception_OutOfMemory :
                throw new RealmOutOfMemoryException(message);
                break;

            case RealmExceptionCodes.Exception_FatalError :
                throw new RealmFatalErrorException(message);
                break;

            case RealmExceptionCodes.Exception_RuntimeError :
                throw new RealmRuntimeErrorException(message);
                break;

            case RealmExceptionCodes.Exception_RowInvalid :
                throw new RealmRowInvalidException(message);
                break;

            case RealmExceptionCodes.Exception_EncryptionNotSupported :
                throw new RealmEncryptionNotSupportedException(message);
                break;

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
}