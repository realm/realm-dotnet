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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Realms.Sync
{
    internal partial class AppHandle : StandaloneHandle
    {
        private static class EmailNativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_register_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern void register_user(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string username, IntPtr username_len,
                [MarshalAs(UnmanagedType.LPWStr)] string password, IntPtr password_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_confirm_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern void confirm_user(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string token, IntPtr token_len,
                [MarshalAs(UnmanagedType.LPWStr)] string token_id, IntPtr token_id_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_resend_confirmation_email", CallingConvention = CallingConvention.Cdecl)]
            public static extern void resend_confirmation_email(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string email, IntPtr email_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_retry_custom_confirmation", CallingConvention = CallingConvention.Cdecl)]
            public static extern void retry_custom_comfirmation(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string email, IntPtr email_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_send_reset_password_email", CallingConvention = CallingConvention.Cdecl)]
            public static extern void send_reset_password_email(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string email, IntPtr email_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_reset_password", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reset_password(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string password, IntPtr password_len,
                [MarshalAs(UnmanagedType.LPWStr)] string token, IntPtr token_len,
                [MarshalAs(UnmanagedType.LPWStr)] string token_id, IntPtr token_id_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_email_call_reset_password_function", CallingConvention = CallingConvention.Cdecl)]
            public static extern void call_reset_password_function(AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string username, IntPtr username_len,
                [MarshalAs(UnmanagedType.LPWStr)] string password, IntPtr password_len,
                [MarshalAs(UnmanagedType.LPWStr)] string function_args, IntPtr function_args_len,
                IntPtr tcs_ptr, out NativeException ex);
        }

        public readonly EmailPasswordApi EmailPassword;

        public class EmailPasswordApi
        {
            private readonly AppHandle _appHandle;

            public EmailPasswordApi(AppHandle handle)
            {
                _appHandle = handle;
            }

            public async Task RegisterUserAsync(string username, string password)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.register_user(_appHandle, username, (IntPtr)username.Length, password, (IntPtr)password.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }

            public async Task ConfirmUserAsync(string token, string tokenId)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.confirm_user(_appHandle, token, (IntPtr)token.Length, tokenId, (IntPtr)tokenId.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }

            public async Task ResendConfirmationEmailAsync(string email)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.resend_confirmation_email(_appHandle, email, (IntPtr)email.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }

            public async Task RetryCustomConfirmationAsync(string email)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.resend_confirmation_email(_appHandle, email, (IntPtr)email.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }

            public async Task SendResetPasswordEmailAsync(string username)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.send_reset_password_email(_appHandle, username, (IntPtr)username.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }

            public async Task ResetPasswordAsync(string password, string token, string tokenId)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.reset_password(
                        _appHandle,
                        password, (IntPtr)password.Length,
                        token, (IntPtr)token.Length,
                        tokenId, (IntPtr)tokenId.Length,
                        GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }

            public async Task CallResetPasswordFunctionAsync(string username, string password, string functionArgs)
            {
                var tcs = new TaskCompletionSource();
                var tcsHandle = GCHandle.Alloc(tcs);

                try
                {
                    EmailNativeMethods.call_reset_password_function(_appHandle,
                        username, (IntPtr)username.Length,
                        password, (IntPtr)password.Length,
                        functionArgs, (IntPtr)functionArgs.Length,
                        GCHandle.ToIntPtr(tcsHandle), out var ex);
                    ex.ThrowIfNecessary();
                    await tcs.Task;
                }
                finally
                {
                    tcsHandle.Free();
                }
            }
        }
    }
}
