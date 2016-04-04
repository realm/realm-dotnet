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

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void shared_realm_delete(IntPtr realmPtr);

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
            Console.WriteLine("[" + id + "] realmPtr=" + _realmPtr + ", thread id: " + Thread.CurrentThread.ManagedThreadId + " -- Destroyed");
            if (disposing)
                NativeSetup.shared_realm_delete(_realmPtr);

            base.Dispose(disposing);
        }
    }

    internal static class Platform
    {
        public static void Initialize()
        {
            NativeSetup.bind_handler_functions(CreateHandler, NotifyHandler, DestroyHandler);
        }

        private static IntPtr CreateHandler(IntPtr realmPtr) 
        {
            if (Looper.MyLooper() == null)
            {
                Console.WriteLine("No looper exists. Cannot create handler");
                return IntPtr.Zero;
            }

            var h = new MyHandler(realmPtr);
            var gch = GCHandle.ToIntPtr(GCHandle.Alloc(h));

            Console.WriteLine("CreateHandler(" + gch + ")");

            return gch;
        }

        private static void NotifyHandler(IntPtr handlerHandle)
        {
            Console.WriteLine("NotifyHandler(" + handlerHandle + ")");

            if (handlerHandle == IntPtr.Zero)
                return;

            Console.WriteLine("Notify handler..");

            var h = (MyHandler)GCHandle.FromIntPtr(handlerHandle).Target;

            h.SendEmptyMessage(13);
        }

        private static void DestroyHandler(IntPtr handlerHandle)
        {
            Console.WriteLine("DestroyHandler(" + handlerHandle + ")");

            if (handlerHandle == IntPtr.Zero)
                return;

            var h = (MyHandler)GCHandle.FromIntPtr(handlerHandle).Target;

            h.Dispose();
        }
    }
}