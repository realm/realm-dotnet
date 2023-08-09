////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using System;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Realms.Native
{
    internal partial class SyncSocketProvider : IDisposable
    {
        private static class NativeMethods
        {
            public const int RLM_ERR_WEBSOCKET_CONNECTION_FAILED = 4401;
            public const int RLM_ERR_WEBSOCKET_READ_ERROR = 4402;
            public const int RLM_ERR_WEBSOCKET_WRITE_ERROR = 4403;

            // equivalent to ErrorCodes::Error in <realm/error_codes.hpp>
            public enum ErrorCode : int
            {
                Ok = 0,
                RuntimeError = 1000,
                OperationAborted = 1027
            }

            [StructLayout(LayoutKind.Sequential)]
            public readonly struct Endpoint
            {
                public readonly StringValue address;

                public readonly ushort port;

                public readonly StringValue path;

                public readonly MarshaledVector<StringValue> protocols;

                public readonly NativeBool is_ssl;
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void post_work(IntPtr socket_provider, IntPtr native_callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void provider_dispose(IntPtr managed_provider);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr create_timer(IntPtr socket_provider, UInt64 delay_miliseconds, IntPtr native_callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void cancel_timer(IntPtr timer);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr websocket_connect(IntPtr socket_provider, IntPtr observer, Endpoint endpoint);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void websocket_write(IntPtr managed_websocket, BinaryValue data, IntPtr native_callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void websocket_close(IntPtr managed_websocket);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_callbacks(post_work post, provider_dispose dispose, create_timer create_timer, cancel_timer cancel_timer, websocket_connect connect, websocket_write write, websocket_close close);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_run_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern void run_callback(IntPtr native_callback, ErrorCode result, StringValue reason);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_connected_handler", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_connected_handler(IntPtr observer, StringValue protocol);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_error_handler", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_error_handler(IntPtr observer);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_binary_message_received", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_binary_message_received(IntPtr observer, BinaryValue data);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_closed_handler", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_closed_handler(IntPtr observer, NativeBool was_clean, WebSocketCloseStatus status, StringValue reason);
        }

        private static void PostWork(IntPtr managed_provider, IntPtr nativeCallback)
        {
            var provider = (SyncSocketProvider)GCHandle.FromIntPtr(managed_provider).Target;
            provider._workQueue.Writer.TryWrite(new EventLoopWork(nativeCallback, Status.OK));
        }

        [MonoPInvokeCallback(typeof(NativeMethods.provider_dispose))]
        private static void ProviderDispose(IntPtr managed_provider)
        {
            var handle = GCHandle.FromIntPtr(managed_provider);
            ((SyncSocketProvider)handle.Target).Dispose();
            handle.Free();
        }

        [MonoPInvokeCallback(typeof(NativeMethods.create_timer))]
        private static IntPtr CreateTimer(IntPtr managed_provider, ulong delay_milliseconds, IntPtr native_callback)
        {
            var provider = (SyncSocketProvider)GCHandle.FromIntPtr(managed_provider).Target;
            var timer = new Timer(TimeSpan.FromMilliseconds(delay_milliseconds), native_callback, provider._workQueue);
            return GCHandle.ToIntPtr(GCHandle.Alloc(timer));
        }

        [MonoPInvokeCallback(typeof(NativeMethods.cancel_timer))]
        private static void CancelTimer(IntPtr managed_timer)
        {
            var handle = GCHandle.FromIntPtr(managed_timer);
            try
            {
                ((Timer)handle.Target).Cancel();
            }
            finally
            {
                handle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.websocket_connect))]
        private static IntPtr WebSocketConnect(IntPtr managed_provider, IntPtr observer, NativeMethods.Endpoint endpoint)
        {
            var provider = (SyncSocketProvider)GCHandle.FromIntPtr(managed_provider).Target;
            var webSocket = new ClientWebSocket();
            foreach (string? subProtocol in endpoint.protocols)
            {
                webSocket.Options.AddSubProtocol(subProtocol!);
            }

            provider._onWebSocketConnection?.Invoke(webSocket.Options);

            var builder = new UriBuilder();
            builder.Scheme = endpoint.is_ssl ? "wss" : "ws";
            builder.Host = endpoint.address;
            builder.Port = endpoint.port;
            if (endpoint.path)
            {
                var pathAndQuery = ((string)endpoint.path)!.Split('?');
                builder.Path = pathAndQuery.ElementAtOrDefault(0);
                builder.Query = pathAndQuery.ElementAtOrDefault(1);
            }

            var socket = new Socket(webSocket, observer, provider._workQueue, builder.Uri);
            return GCHandle.ToIntPtr(GCHandle.Alloc(socket));
        }

        [MonoPInvokeCallback(typeof(NativeMethods.websocket_write))]
        private static void WebSocketWrite(IntPtr managed_socket, BinaryValue data, IntPtr native_callback)
        {
            var socket = (Socket)GCHandle.FromIntPtr(managed_socket).Target;
            socket.Write(data, native_callback);
        }

        [MonoPInvokeCallback (typeof(NativeMethods.websocket_close))]
        private static void WebSocketClose(IntPtr managed_websocket)
        {
            var handle = GCHandle.FromIntPtr(managed_websocket);
            ((Socket)handle.Target).Dispose();
            handle.Free();
        }

        static SyncSocketProvider()
        {
            NativeMethods.post_work post = PostWork;
            NativeMethods.provider_dispose dispose = ProviderDispose;
            NativeMethods.create_timer create_timer = CreateTimer;
            NativeMethods.cancel_timer cancel_timer = CancelTimer;
            NativeMethods.websocket_connect websocket_connect = WebSocketConnect;
            NativeMethods.websocket_write websocket_write = WebSocketWrite;
            NativeMethods.websocket_close websocket_close = WebSocketClose;

            GCHandle.Alloc(post);
            GCHandle.Alloc(dispose);
            GCHandle.Alloc(create_timer);
            GCHandle.Alloc(cancel_timer);
            GCHandle.Alloc(websocket_connect);
            GCHandle.Alloc(websocket_write);
            GCHandle.Alloc(websocket_close);

            NativeMethods.install_callbacks(post, dispose, create_timer, cancel_timer, websocket_connect, websocket_write, websocket_close);
        }

        private struct Status
        {
            internal NativeMethods.ErrorCode Code;
            internal string? Reason;

            internal static readonly Status OK = new() { Code = NativeMethods.ErrorCode.Ok };

            public Status(NativeMethods.ErrorCode code, string reason)
            {
                Code = code;
                Reason = reason;
            }
        }

        private interface IWork
        {
            void Execute();
        }

        private readonly Channel<IWork> _workQueue;
        private readonly Task _workThread;
        private readonly Action<ClientWebSocketOptions>? _onWebSocketConnection;

        internal SyncSocketProvider(Action<ClientWebSocketOptions>? onWebSocketConnection)
        {
            _onWebSocketConnection = onWebSocketConnection;
            _workQueue = Channel.CreateUnbounded<IWork>(new() { SingleReader = true });
            _workThread = Task.Factory.StartNew(WorkThread, creationOptions: TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach).Unwrap();
        }

        private partial Task WorkThread();

        public void Dispose()
        {
            _workQueue.Writer.Complete();
        }
    }
}
