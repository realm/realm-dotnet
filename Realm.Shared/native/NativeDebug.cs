using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Realms
{
    public static class NativeDebug
    {
        public static void Initialize()
        {
            bind_debug_log(DebugLog);
        }

        unsafe private static void DebugLog(IntPtr utf8String, IntPtr stringLen)
        {
            var message = new String((sbyte*)utf8String, 0 /* start offset */, (int)stringLen, Encoding.UTF8);
            Console.WriteLine(message);
        }

        public delegate void DebugLogCallback (IntPtr utf8String, IntPtr stringLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "bind_debug_log", CallingConvention = CallingConvention.Cdecl)]
        public static extern void bind_debug_log(DebugLogCallback callback);
    }
}

