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
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Realms.Native
{
    internal partial class SyncSocketProvider
    {
        private class Socket : IDisposable
        {
            private readonly ClientWebSocket _webSocket;
            private readonly IntPtr _observer;
            private readonly ChannelWriter<IWork> _workQueue;

            private readonly Uri _uri;
            private readonly Task _workerThread;

            private MemoryStream _receiveBuffer = new();

            internal Socket(ClientWebSocket webSocket, IntPtr observer, ChannelWriter<IWork> workQueue, Uri uri) 
            {
                _webSocket = webSocket;
                _observer = observer;
                _workQueue = workQueue;
                _uri = uri;
                _workerThread = Task.Factory.StartNew(ReadThread, creationOptions: TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach).Unwrap();
            }

            private async Task ReadThread()
            {
                try
                {
                    await _webSocket.ConnectAsync(_uri, default);
                    await _workQueue.WriteAsync(new WebSocketConnectedWork(_observer, _webSocket.SubProtocol));
                }
                catch (WebSocketException e)
                {
                    await _workQueue.WriteAsync(new WebSocketClosedWork(false, (WebSocketCloseStatus)NativeMethods.RLM_ERR_WEBSOCKET_CONNECTION_FAILED, e.Message, _observer));
                    return;
                }

                var buffer = new byte[32 * 1024];
                do
                {
                    try
                    {
                        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            await _receiveBuffer.WriteAsync(buffer, 0, result.Count);
                            if (result.EndOfMessage)
                            {
                                var current_buffer = _receiveBuffer;
                                _receiveBuffer = new MemoryStream();
                                await _workQueue.WriteAsync(new BinaryMessageReceivedWork(current_buffer, _observer));
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _workQueue.WriteAsync(new WebSocketClosedWork(clean: true, result.CloseStatus!.Value, result.CloseStatusDescription, _observer));
                        }
                    }
                    catch (WebSocketException e)
                    {
                        await _workQueue.WriteAsync(new WebSocketClosedWork(false, (WebSocketCloseStatus)NativeMethods.RLM_ERR_WEBSOCKET_READ_ERROR, e.Message, _observer));
                        return;
                    }
                } while (_webSocket.State == WebSocketState.Open);
            }

            public void Write(BinaryValue data, IntPtr native_callback)
            {
                var buffer = ArrayPool<byte>.Shared.Rent((int)data.size);
                unsafe
                {
                    Marshal.Copy((IntPtr)data.data, buffer, 0, (int)data.size);
                }

                _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, default).ContinueWith(async t =>
                {
                    ArrayPool<byte>.Shared.Return(buffer);

                    var status = Status.OK;
                    if (t.IsFaulted)
                    {
                        // TODO: The documentation for WebSocketObserver::async_write_binary() says the handler should be called with RuntimeError in case of errors
                        // but the default implementation always calls it with Ok. Which is it?
                        // status = new Status(NativeMethods.ErrorCode.RuntimeError, t.Exception.Message);
                        await _workQueue.WriteAsync(new WebSocketClosedWork(false, (WebSocketCloseStatus)NativeMethods.RLM_ERR_WEBSOCKET_WRITE_ERROR, t.Exception.Message, _observer));
                    }

                    await _workQueue.WriteAsync(new EventLoopWork(native_callback, status));
                });
            }

            public void Dispose()
            {
                _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default).ContinueWith(t =>
                {
                    _webSocket.Dispose();
                    _receiveBuffer.Dispose();
                });
            }
        }

        private abstract class WebSocketWork : IWork
        {
            private readonly IntPtr _observer;

            protected WebSocketWork(IntPtr observer)
            {
                _observer = observer;
            }

            protected abstract void Execute(IntPtr observer);

            void IWork.Execute()
            {
                Execute(_observer);
            }
        }

        private sealed class WebSocketConnectedWork : WebSocketWork
        {
            private readonly string _protocol;

            public WebSocketConnectedWork(IntPtr observer, string protocol)
            : base(observer)
            {
                _protocol = protocol;
            }

            protected unsafe override void Execute(IntPtr observer)
            {
                var bytes = Encoding.UTF8.GetBytes(_protocol);
                fixed (byte* data = bytes)
                {
                    NativeMethods.observer_connected_handler(observer, new() { data = data, size = bytes.Length });
                }
            }
        }

        private sealed class BinaryMessageReceivedWork : WebSocketWork
        {
            private MemoryStream _receiveBuffer;

            public BinaryMessageReceivedWork(MemoryStream receiveBuffer, IntPtr observer)
            : base(observer)
            {
                _receiveBuffer = receiveBuffer;
            }

            protected unsafe override void Execute(IntPtr observer)
            {
                using var buffer = _receiveBuffer;
                fixed (byte* data = buffer.GetBuffer())
                {
                    NativeMethods.observer_binary_message_received(observer, new() { data = data, size = (IntPtr)buffer.Length });
                }
            }
        }

        private sealed class WebSocketClosedWork : WebSocketWork
        {
            private bool _clean;
            private WebSocketCloseStatus _status;
            private string _description;

            public WebSocketClosedWork(bool clean, WebSocketCloseStatus status, string description, IntPtr observer)
            : base(observer)
            {
                _clean = clean;
                _status = status;
                _description = description;
            }

            protected unsafe override void Execute(IntPtr observer)
            {
                if (!_clean)
                {
                    NativeMethods.observer_error_handler(observer);
                }

                var bytes = Encoding.UTF8.GetBytes(_description);
                fixed(byte* data = bytes)
                {
                    NativeMethods.observer_closed_handler(observer, _clean, _status, new() { data = data, size = bytes.Length });
                }
            }
        }
    }
}
