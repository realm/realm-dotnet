// ***********************************************************************
// Copyright (c) 2016 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;

using System.Threading.Tasks;
using Xamarin.Forms;
using NUnit.Runner.Messages;
using System.Threading;

#if NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif

namespace NUnit.Runner.Services
{
    /// <summary>
    /// Redirects output to a Tcp connection
    /// </summary>
    class TcpWriter : TextWriter
    {
#if NETFX_CORE
        StreamSocket _socket;
#endif
        StreamWriter _writer;
        readonly TcpWriterInfo _info;

        public TcpWriter(TcpWriterInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _info = info;
        }

        public async Task Connect()
        {
#if NETFX_CORE
            try
            {
                _socket = new StreamSocket();
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(_info.Timeout));

                await _socket.ConnectAsync(new HostName(_info.Hostname), _info.Port.ToString()).AsTask();
                _writer = new StreamWriter(_socket.OutputStream.AsStreamForWrite());
            }
            catch (TaskCanceledException)
            {
                MessagingCenter.Send(new ErrorMessage($"Timeout connecting to {_info} after {_info.Timeout} seconds.\n\nIs your server running?"), ErrorMessage.Name);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send(new ErrorMessage(ex.Message), ErrorMessage.Name);
            }
#else
            try
            {
                TcpClient client = new TcpClient();
                Task connect = client.ConnectAsync(_info.Hostname, _info.Port);
                Task timeout = Task.Delay(TimeSpan.FromSeconds(_info.Timeout));
                if(await Task.WhenAny(connect, timeout) == timeout)
                {
                    throw new TimeoutException();
                }
                NetworkStream stream = client.GetStream();
                _writer = new StreamWriter(stream);
            }
            catch (TimeoutException)
            {
                MessagingCenter.Send(new ErrorMessage($"Timeout connecting to {_info} after {_info.Timeout} seconds.\n\nIs your server running?"), ErrorMessage.Name);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send(new ErrorMessage(ex.Message), ErrorMessage.Name);
            }
#endif
        }

        public override void Write(char value)
        {
            _writer?.Write(value);
        }

        public override void Write(string value)
        {
            _writer?.Write(value);
        }

        public override void WriteLine(string value)
        {
            _writer?.WriteLine(value);
            _writer?.Flush();
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        protected override void Dispose(bool disposing)
        {
            _writer?.Dispose();
#if NETFX_CORE
            _socket?.Dispose();
#endif
        }
    }
}