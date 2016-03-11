using System;
using System.Runtime.InteropServices;
using System.Text;
using Android.OS;
using System.Collections.Generic;
using System.Threading;

namespace Realms
{
    internal static class NativeSetup
    {
        // declare the type for the MonoPInvokeCallback
        public delegate IntPtr CreateHandlerFunction (IntPtr realmPtr);
        public delegate void NotifyHandlerFunction (IntPtr handler);
        public delegate void DestroyHandlerFunction (IntPtr handler);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "bind_handler_functions", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void bind_handler_functions(CreateHandlerFunction createHandlerFunction, 
            NotifyHandlerFunction notifyHandlerFunction,
            DestroyHandlerFunction destroyHandlerFunction);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "notify_realm", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void notify_realm(IntPtr realmPtr);

    }

    public class MyHandler : Handler
    {
        static int Id = 0;
        IntPtr _realmPtr;
        int id;

        public MyHandler(IntPtr realmPtr)
        {
            id = Id++;
            _realmPtr = realmPtr;

            Console.WriteLine("[" + id + "] realmPtr=" + _realmPtr + ", thread id: " + Thread.CurrentThread.ManagedThreadId + " -- Handle constructed");
        }

        public override void HandleMessage(Message msg)
        {
            Console.WriteLine("[" + id + "] realmPtr=" + _realmPtr + ", thread id: " + Thread.CurrentThread.ManagedThreadId + " -- Notified");
            NativeSetup.notify_realm(_realmPtr);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                NativeSharedRealm.destroy(_realmPtr);

            base.Dispose(disposing);
        }
    }

    internal static class Platform
    {
        public static void Initialize()
        {
            NativeSetup.bind_handler_functions(CreateHandler, NotifyHandler, DestroyHandler);
        }

        private static Dictionary<IntPtr, Handler> handlers = new Dictionary<IntPtr, Handler>();

        private static IntPtr CreateHandler(IntPtr realmPtr) 
        {
            if (Looper.MyLooper() == null)
            {
                Console.WriteLine("No looper exists. Cannot create handler");
                return IntPtr.Zero;
            }

            var h = new MyHandler(realmPtr);
            handlers[h.Handle] = h;

            return h.Handle;
        }

        private static void NotifyHandler(IntPtr handlerHandle)
        {
            if (handlerHandle == IntPtr.Zero)
                return;

            var h = handlers[handlerHandle];

            h.SendEmptyMessage(13);
        }

        private static void DestroyHandler(IntPtr handlerHandle)
        {
            if (handlerHandle == IntPtr.Zero)
                return;

            var h = handlers[handlerHandle];

            h.Dispose();
        }
    }
}

