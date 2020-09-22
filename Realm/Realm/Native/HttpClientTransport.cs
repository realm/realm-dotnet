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
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Realms.Sync.Native;

namespace Realms.Native
{
    internal static class HttpClientTransport
    {
        private enum NativeHttpMethod
        {
            get,
            post,
            patch,
            put,
            del
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HttpClientRequest
        {
            public NativeHttpMethod method;

            [MarshalAs(UnmanagedType.LPStr)]
            public string url;

            public UInt64 timeout_ms;

            public IntPtr /* StringStringPair[] */ headers;
            public int headers_len;

            [MarshalAs(UnmanagedType.LPStr)]
            public string body;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HttpClientResponse
        {
            public Int32 http_status_code;

            [MarshalAs(UnmanagedType.LPWStr)]
            private string body;
            private IntPtr body_len;

            public string Body
            {
                set
                {
                    body = value;
                    body_len = (IntPtr)value.Length;
                }
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void execute_request(HttpClientRequest request, IntPtr callback_ptr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_http_transport_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void install_callbacks(execute_request execute);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_http_transport_respond", CallingConvention = CallingConvention.Cdecl)]
        private static extern void respond(
            HttpClientResponse response,
            [MarshalAs(UnmanagedType.LPArray), In] StringStringPair[] headers, int headers_len,
            IntPtr callback_ptr);

        private static readonly HttpClient _httpClient = new HttpClient();

        internal static void Install()
        {
            execute_request execute = ExecuteRequest;

            GCHandle.Alloc(execute);

            install_callbacks(execute);
        }

        [MonoPInvokeCallback(typeof(execute_request))]
        private static async void ExecuteRequest(HttpClientRequest request, IntPtr callback)
        {
            try
            {
                using var message = new HttpRequestMessage(request.method.ToHttpMethod(), request.url);
                foreach (var header in StringStringPair.UnmarshalDictionary(request.headers, request.headers_len))
                {
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                if (request.method != NativeHttpMethod.get)
                {
                    message.Content = new StringContent(request.body);
                }
                using var cts = new CancellationTokenSource();
                cts.CancelAfter((int)request.timeout_ms);

                var response = await _httpClient.SendAsync(message, cts.Token);
                var headers = new List<StringStringPair>(response.Headers.Count());
                foreach (var header in response.Headers)
                {
                    headers.Add(new StringStringPair
                    {
                        Key = header.Key,
                        Value = header.Value.FirstOrDefault()
                    });
                }

                var nativeResponse = new HttpClientResponse
                {
                    http_status_code = (int)response.StatusCode,
                    Body = await response.Content.ReadAsStringAsync(),
                };

                respond(nativeResponse, headers.ToArray(), headers.Count, callback);
            }
            catch (HttpRequestException ex)
            {
                // V10TODO: implement me
                throw;
            }
            catch (TaskCanceledException)
            {
                // V10TODO: implement me
                throw;
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
