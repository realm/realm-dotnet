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
        public delegate IntPtr CreateHandlerFunction ();
        public delegate void NotifyHandlerFunction (IntPtr handler, IntPtr realm);
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
        [ThreadStatic]
        private static MyHandler currentHandler;

        internal static Handler Current
        {
            get
            {
                if (currentHandler == null) {
                    currentHandler = new MyHandler();
                }

                return currentHandler;
            }
        }

        public override void HandleMessage(Message msg)
        {
            var realmHandle = (IntPtr)(Int64)(Java.Lang.Long)msg.Obj;
            if (realmHandle == IntPtr.Zero)
                return;

            NativeSetup.notify_realm(realmHandle);
        }
    }

    internal static class Platform
    {
        public static void Initialize()
        {
            NativeSetup.bind_handler_functions(CreateHandler, NotifyHandler, DestroyHandler);
        }

        private static IntPtr CreateHandler() 
        {
            if (Looper.MyLooper() == null)
            {
                Console.WriteLine("No looper exists. Cannot create handler");
                return IntPtr.Zero;
            }

            var gch = GCHandle.ToIntPtr(GCHandle.Alloc(MyHandler.Current));
            return gch;
        }

        private static void NotifyHandler(IntPtr handlerHandle, IntPtr realmHandle)
        {
            if (realmHandle == IntPtr.Zero)
                return;

            var handler = (MyHandler)GCHandle.FromIntPtr(handlerHandle).Target;
            handler.SendMessage(new Message { Obj = new Java.Lang.Long(realmHandle.ToInt64()) });
        }

        private static void DestroyHandler(IntPtr handlerHandle)
        {
            if (handlerHandle == IntPtr.Zero)
                return;

            var handler = (MyHandler)GCHandle.FromIntPtr(handlerHandle).Target;
            GCHandle.FromIntPtr(handlerHandle).Free();
        }
    }
}