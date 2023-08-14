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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Realms.Logging;

namespace Realms.Native
{
    internal partial class SyncSocketProvider : IDisposable
    {
        private static void PostWork(IntPtr managed_provider, IntPtr native_callback)
        {
            var provider = (SyncSocketProvider)GCHandle.FromIntPtr(managed_provider).Target;
            _ = provider.PostWorkAsync(native_callback);
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
        private static IntPtr WebSocketConnect(IntPtr managed_provider, IntPtr observer, Endpoint endpoint)
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

        [MonoPInvokeCallback(typeof(NativeMethods.websocket_close))]
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
            internal ErrorCode Code;
            internal string? Reason;
            internal static readonly Status OperationAborted = new(ErrorCode.OperationAborted, "Operation canceled");
            internal static readonly Status OK = new() { Code = ErrorCode.Ok };

            public Status(ErrorCode code, string reason)
            {
                Code = code;
                Reason = reason;
            }
        }

        /// <summary>
        /// Basic unit of work for the provider's event loop.
        /// </summary>
        private interface IWork
        {
            /// <summary>
            /// Execute the outstanding work.
            /// </summary>
            void Execute();
        }

        private readonly Channel<IWork> _workQueue;
        private readonly Task _workThread;
        private readonly Action<ClientWebSocketOptions>? _onWebSocketConnection;
        private readonly CancellationTokenSource _cts = new();

        internal SyncSocketProvider(Action<ClientWebSocketOptions>? onWebSocketConnection)
        {
            Logger.LogDefault(LogLevel.Debug, "Creating SyncSocketProvider.");
            _onWebSocketConnection = onWebSocketConnection;
            _workQueue = Channel.CreateUnbounded<IWork>(new() { SingleReader = true });
            _workThread = Task.Run(WorkThread);
        }

        private partial Task WorkThread();

        public void Dispose()
        {
            Logger.LogDefault(LogLevel.Debug, "Destroying SyncSocketProvider.");
            _workQueue.Writer.Complete();
            _cts.Cancel();
            _cts.Dispose();
            _workThread.GetAwaiter().GetResult();
        }
    }
}
