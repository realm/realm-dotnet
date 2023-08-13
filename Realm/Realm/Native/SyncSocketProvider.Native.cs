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
using System.Net.WebSockets;
using System.Runtime.InteropServices;

namespace Realms.Native
{
    internal partial class SyncSocketProvider
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

        private static class NativeMethods
        {
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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_run_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern void delete_callback(IntPtr native_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_connected_handler", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_connected_handler(IntPtr observer, StringValue protocol);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_error_handler", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_error_handler(IntPtr observer);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_binary_message_received", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_binary_message_received(IntPtr observer, BinaryValue data);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_websocket_observer_closed_handler", CallingConvention = CallingConvention.Cdecl)]
            public static extern void observer_closed_handler(IntPtr observer, NativeBool was_clean, WebSocketCloseStatus status, StringValue reason);
        }
    }
}
