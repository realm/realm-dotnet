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
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Realms.Logging;

namespace Realms.Native
{
    internal partial class SyncSocketProvider
    {
        private class Socket : IDisposable
        {
            private readonly ClientWebSocket _webSocket;
            private readonly IntPtr _observer;
            private readonly ChannelWriter<IWork> _workQueue;
            private readonly CancellationTokenSource _cts = new();

            private readonly Uri _uri;
            private readonly Task _readThread;

            private MemoryStream _receiveBuffer = new();

            internal Socket(ClientWebSocket webSocket, IntPtr observer, ChannelWriter<IWork> workQueue, Uri uri)
            {
                Logger.LogDefault(LogLevel.Trace, $"Creating a WebSocket to {uri.GetLeftPart(UriPartial.Path)}");
                _webSocket = webSocket;
                _observer = observer;
                _workQueue = workQueue;
                _uri = uri;
                _readThread = Task.Factory.StartNew(ReadThread, creationOptions: TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, cancellationToken: _cts.Token, scheduler: TaskScheduler.Default).Unwrap();
            }

            private async Task ReadThread()
            {
                Logger.LogDefault(LogLevel.Trace, "Entering WebSocket event loop.");

                try
                {
                    await _webSocket.ConnectAsync(_uri, _cts.Token);
                    await _workQueue.WriteAsync(new WebSocketConnectedWork(_webSocket.SubProtocol!, _observer, _cts.Token));
                }
                catch (WebSocketException e)
                {
                    if (e.InnerException is not null)
                    {
                        Logger.LogDefault(LogLevel.Error, $"Error establishing WebSocket connection: {e.InnerException.Message}");
                        Logger.LogStackTrace(e.InnerException);
                    }

                    await _workQueue.WriteAsync(new WebSocketClosedWork(false, (WebSocketCloseStatus)RLM_ERR_WEBSOCKET_CONNECTION_FAILED, e.Message, _observer, _cts.Token));
                    return;
                }

                var buffer = new byte[32 * 1024];
                while (_webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        switch (result.MessageType)
                        {
                            case WebSocketMessageType.Binary:
                                await _receiveBuffer.WriteAsync(buffer, 0, result.Count);
                                if (result.EndOfMessage)
                                {
                                    var currentBuffer = _receiveBuffer;
                                    _receiveBuffer = new MemoryStream();
                                    await _workQueue.WriteAsync(new BinaryMessageReceivedWork(currentBuffer, _observer, _cts.Token));
                                }

                                break;
                            case WebSocketMessageType.Close:
                                Logger.LogDefault(LogLevel.Trace, $"WebSocket closed with status {result.CloseStatus!.Value} and description \"{result.CloseStatusDescription}\"");
                                await _workQueue.WriteAsync(new WebSocketClosedWork(clean: true, result.CloseStatus!.Value, result.CloseStatusDescription!, _observer, _cts.Token));
                                break;
                            default:
                                Logger.LogDefault(LogLevel.Trace, $"Received unexpected {result.MessageType} WebSocket message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                                break;
                        }
                    }
                    catch (WebSocketException e)
                    {
                        Logger.LogDefault(LogLevel.Error, $"Error reading from WebSocket: {e.Message}");
                        Logger.LogStackTrace(e);
                        await _workQueue.WriteAsync(new WebSocketClosedWork(false, (WebSocketCloseStatus)RLM_ERR_WEBSOCKET_READ_ERROR, e.Message, _observer, _cts.Token));
                        return;
                    }
                }
            }

            public async void Write(BinaryValue data, IntPtr native_callback)
            {
                if (_webSocket.State == WebSocketState.Aborted)
                {
                    NativeMethods.delete_callback(native_callback);
                    return;
                }

                var buffer = data.AsBytes(usePooledArray: true);

                var status = Status.OK;
                try
                {
                    await _webSocket.SendAsync(new(buffer), WebSocketMessageType.Binary, true, _cts.Token);
                }
                catch (Exception e)
                {
                    Logger.LogDefault(LogLevel.Error, $"Error writing to WebSocket {e.GetType().FullName}: {e.Message}");
                    Logger.LogStackTrace(e);

                    // TODO: The documentation for WebSocketObserver::async_write_binary() says the handler should be called with RuntimeError in case of errors
                    // but the default implementation always calls it with Ok. Which is it?
                    // status = new Status(NativeMethods.ErrorCode.RuntimeError, e.Message);
                    await _workQueue.WriteAsync(new WebSocketClosedWork(false, (WebSocketCloseStatus)RLM_ERR_WEBSOCKET_WRITE_ERROR, e.Message, _observer, _cts.Token));
                    NativeMethods.delete_callback(native_callback);
                    return;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                await _workQueue.WriteAsync(new EventLoopWork(native_callback, status, _cts.Token));
            }

            public async void Dispose()
            {
                _cts.Cancel();

                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default);
                }

                _webSocket.Dispose();
                _receiveBuffer.Dispose();
                _cts.Dispose();
                Logger.LogDefault(LogLevel.Trace, "Disposing WebSocket.");
            }
        }

        private abstract class WebSocketWork : IWork
        {
            private readonly IntPtr _observer;

            // Belongs to the Socket and canceled when Native destroys the socket.
            // If it's canceled we shouldn't call any observer methods.
            private readonly CancellationToken _cancellationToken;

            protected WebSocketWork(IntPtr observer, CancellationToken cancellationToken)
            {
                _observer = observer;
                _cancellationToken = cancellationToken;
            }

            protected abstract void Execute(IntPtr observer);

            void IWork.Execute()
            {
                if (!_cancellationToken.IsCancellationRequested)
                {
                    Execute(_observer);
                }
            }
        }

        private sealed class WebSocketConnectedWork : WebSocketWork
        {
            private readonly string _protocol;

            public WebSocketConnectedWork(string protocol, IntPtr observer, CancellationToken cancellationToken)
                : base(observer, cancellationToken)
            {
                _protocol = protocol;
            }

            protected override void Execute(IntPtr observer)
            {
                using var arena = new Arena();
                NativeMethods.observer_connected_handler(observer, StringValue.AllocateFrom(_protocol, arena));
            }
        }

        private sealed class BinaryMessageReceivedWork : WebSocketWork
        {
            private readonly MemoryStream _receiveBuffer;

            public BinaryMessageReceivedWork(MemoryStream receiveBuffer, IntPtr observer, CancellationToken cancellationToken)
                : base(observer, cancellationToken)
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
            private readonly bool _clean;
            private readonly WebSocketCloseStatus _status;
            private readonly string _description;

            public WebSocketClosedWork(bool clean, WebSocketCloseStatus status, string description, IntPtr observer, CancellationToken cancellationToken)
                : base(observer, cancellationToken)
            {
                _clean = clean;
                _status = status;
                _description = description;
            }

            protected override void Execute(IntPtr observer)
            {
                if (!_clean)
                {
                    NativeMethods.observer_error_handler(observer);
                }

                using var arena = new Arena();
                NativeMethods.observer_closed_handler(observer, _clean, _status, StringValue.AllocateFrom(_description, arena));
            }
        }
    }
}
