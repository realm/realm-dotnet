using System;
using System.Runtime.InteropServices;
using Android.OS;

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

    public class RealmNotificationHandler : Handler
    {
        [ThreadStatic]
        private static RealmNotificationHandler _currentHandler;

        internal static Handler Current => _currentHandler ?? (_currentHandler = new RealmNotificationHandler());

        public override void HandleMessage(Message msg)
        {
            var realmHandle = (IntPtr)(long)(Java.Lang.Long)msg.Obj;
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

            var gch = GCHandle.ToIntPtr(GCHandle.Alloc(RealmNotificationHandler.Current));
            return gch;
        }

        private static void NotifyHandler(IntPtr handlerHandle, IntPtr realmHandle)
        {
            if (realmHandle == IntPtr.Zero)
                return;

            var handler = (RealmNotificationHandler)GCHandle.FromIntPtr(handlerHandle).Target;
            handler.SendMessage(new Message { Obj = new Java.Lang.Long(realmHandle.ToInt64()) });
        }

        private static void DestroyHandler(IntPtr handlerHandle)
        {
            if (handlerHandle == IntPtr.Zero)
                return;

            GCHandle.FromIntPtr(handlerHandle).Free();
        }
    }
}