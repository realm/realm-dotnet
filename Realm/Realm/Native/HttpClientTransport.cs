////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realms.Native
{
    internal static class HttpClientTransport
    {
        private enum CustomErrorCode
        {
            NoError = 0,
            HttpClientDisposed = 997,
            UnknownHttp = 998,
            Unknown = 999,
            Timeout = 1000,
        }

#pragma warning disable SA1300 // Element should begin with upper-case letter

        private enum NativeHttpMethod
        {
            get,
            post,
            patch,
            put,
            del
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct HttpClientRequest
        {
            public readonly NativeHttpMethod method;

            private readonly StringValue url;

            public readonly UInt64 timeout_ms;

            public readonly MarshaledVector<MarshaledPair<StringValue, StringValue>> headers;

            private readonly StringValue body;

            private readonly IntPtr managed_http_client;

            public string Url => url!;

            public string? Body => body;

            public HttpClient HttpClient
            {
                get
                {
                    if (managed_http_client != IntPtr.Zero &&
                        GCHandle.FromIntPtr(managed_http_client).Target is HttpClient client)
                    {
                        return client;
                    }

                    throw new ObjectDisposedException("HttpClient has been disposed, most likely because the app has been closed.");
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HttpClientResponse
        {
            public Int32 http_status_code;

            public CustomErrorCode custom_status_code;

            public MarshaledVector<MarshaledPair<StringValue, StringValue>> headers;

            public StringValue body;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void execute_request(HttpClientRequest request, IntPtr callback_ptr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_http_transport_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void install_callbacks(execute_request execute);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_http_transport_respond", CallingConvention = CallingConvention.Cdecl)]
        private static extern void respond(HttpClientResponse response, IntPtr callback_ptr);

#pragma warning restore SA1300 // Element should begin with upper-case letter

        internal static void Initialize()
        {
            execute_request execute = ExecuteRequest;

            GCHandle.Alloc(execute);

            install_callbacks(execute);
        }

        private static HttpRequestMessage BuildRequest(HttpClientRequest request)
        {
            var message = new HttpRequestMessage(request.method.ToHttpMethod(), request.Url);
            foreach (var header in request.headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key!, header.Value);
            }

            if (request.method != NativeHttpMethod.get)
            {
                message.Content = new StringContent(request.Body!, Encoding.UTF8, "application/json");
            }

            return message;
        }

        [MonoPInvokeCallback(typeof(execute_request))]
        private static async void ExecuteRequest(HttpClientRequest request, IntPtr callback)
        {
            try
            {
                using var arena = new Arena();
                try
                {
                    var httpClient = request.HttpClient;

                    using var message = BuildRequest(request);
                    using var cts = new CancellationTokenSource((int)request.timeout_ms);

                    var response = await httpClient.SendAsync(message, cts.Token).ConfigureAwait(false);

                    var headers = response.Headers.Concat(response.Content.Headers)
                        .Select(h => new MarshaledPair<StringValue, StringValue>(StringValue.AllocateFrom(h.Key, arena), StringValue.AllocateFrom(h.Value.FirstOrDefault(), arena)))
                        .ToArray();

                    var nativeResponse = new HttpClientResponse
                    {
                        http_status_code = (int)response.StatusCode,
                        headers = MarshaledVector<MarshaledPair<StringValue, StringValue>>.AllocateFrom(headers, arena),
                        body = StringValue.AllocateFrom(await response.Content.ReadAsStringAsync().ConfigureAwait(false), arena),
                    };

                    respond(nativeResponse, callback);
                }
                catch (HttpRequestException rex)
                {
                    var sb = new StringBuilder("An unexpected error occurred while sending the request");

                    // We're doing this because the message for the top-level exception is usually pretty useless.
                    // If there's inner exception, we want to skip it and directly go for the more specific messages.
                    var innerEx = rex.InnerException ?? rex;
                    while (innerEx != null)
                    {
                        sb.Append($": {innerEx.Message}");
                        innerEx = innerEx.InnerException;
                    }

                    var nativeResponse = new HttpClientResponse
                    {
                        custom_status_code = CustomErrorCode.UnknownHttp,
                        body = StringValue.AllocateFrom(sb.ToString(), arena),
                    };

                    respond(nativeResponse, callback);
                }
                catch (TaskCanceledException)
                {
                    var nativeResponse = new HttpClientResponse
                    {
                        custom_status_code = CustomErrorCode.Timeout,
                        body = StringValue.AllocateFrom($"Operation failed to complete within {request.timeout_ms} ms.", arena),
                    };

                    respond(nativeResponse, callback);
                }
                catch (ObjectDisposedException ode)
                {
                    var nativeResponse = new HttpClientResponse
                    {
                        custom_status_code = CustomErrorCode.HttpClientDisposed,
                        body = StringValue.AllocateFrom(ode.Message, arena),
                    };

                    respond(nativeResponse, callback);
                }
                catch (Exception ex)
                {
                    var nativeResponse = new HttpClientResponse
                    {
                        custom_status_code = CustomErrorCode.Unknown,
                        body = StringValue.AllocateFrom(ex.Message, arena),
                    };

                    respond(nativeResponse, callback);
                }
            }
            catch (Exception outerEx)
            {
                Debug.WriteLine($"Unexpected error occurred while trying to respond to a request: {outerEx}");
            }
        }

        private static HttpMethod ToHttpMethod(this NativeHttpMethod nativeMethod)
        {
            return nativeMethod switch
            {
                NativeHttpMethod.get => HttpMethod.Get,
                NativeHttpMethod.post => HttpMethod.Post,
                NativeHttpMethod.patch => new HttpMethod("PATCH"),
                NativeHttpMethod.put => HttpMethod.Put,
                NativeHttpMethod.del => HttpMethod.Delete,
                _ => throw new NotSupportedException($"Unsupported HTTP method: {nativeMethod}")
            };
        }
    }
}
