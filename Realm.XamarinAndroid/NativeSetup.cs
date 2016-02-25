using System;
using System.Runtime.InteropServices;
using System.Text;
using Android.OS;
using System.Collections.Generic;

namespace Realms
{
    internal static class NativeSetup
    {
        // declare the type for the MonoPInvokeCallback
        public delegate IntPtr CreateHandlerFunction ();
        public delegate void NotifyHandlerFunction (IntPtr handler);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "bind_handler_functions", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void bind_handler_functions(CreateHandlerFunction createHandlerFunction, NotifyHandlerFunction notifyHandlerFunction);
        
    }

    internal static class Platform
    {
        public static void Initialize()
        {
            NativeSetup.bind_handler_functions(CreateHandler, NotifyHandler);
        }

        private static Dictionary<IntPtr, Handler> handlers = new Dictionary<IntPtr, Handler>();

        private static IntPtr CreateHandler() 
        {
            if (Looper.MainLooper == null)
            {
                Console.WriteLine("No looper exists. Cannot create handler");
                return IntPtr.Zero;
            }

            Console.WriteLine("Create handler... Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            var h = new Handler();
            handlers[h.Handle] = h;

            return h.Handle;
        }

        private static void NotifyHandler(IntPtr handlerHandle)
        {
            Console.WriteLine("Notifying handler... Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            var h = handlers[handlerHandle];

            h.Post(() => Console.WriteLine("Notified. Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId));

            //Console.WriteLine("Notify hander... Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " -- handler: " + handler.ToInt32());
        }
    }
}

