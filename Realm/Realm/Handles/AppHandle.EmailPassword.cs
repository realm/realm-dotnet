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
    internal partial class AppHandle : RealmHandle
    {
        private static class EmailNativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

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

#pragma warning restore IDE1006 // Naming Styles
        }

        public readonly EmailPasswordApi EmailPassword;

        public class EmailPasswordApi
        {
            private readonly AppHandle _appHandle;

            public EmailPasswordApi(AppHandle handle)
            {
                _appHandle = handle;
            }

            public void RegisterUser(string username, string password, TaskCompletionSource<object> tcs)
            {
                var tcsHandle = GCHandle.Alloc(tcs);
                EmailNativeMethods.register_user(_appHandle, username, (IntPtr)username.Length, password, (IntPtr)password.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
            }

            public void ConfirmUser(string token, string tokenId, TaskCompletionSource<object> tcs)
            {
                var tcsHandle = GCHandle.Alloc(tcs);
                EmailNativeMethods.confirm_user(_appHandle, token, (IntPtr)token.Length, tokenId, (IntPtr)tokenId.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
            }

            public void ResendConfirmationEmail(string email, TaskCompletionSource<object> tcs)
            {
                var tcsHandle = GCHandle.Alloc(tcs);
                EmailNativeMethods.resend_confirmation_email(_appHandle, email, (IntPtr)email.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
            }

            public void SendResetPasswordEmail(string username, TaskCompletionSource<object> tcs)
            {
                var tcsHandle = GCHandle.Alloc(tcs);
                EmailNativeMethods.send_reset_password_email(_appHandle, username, (IntPtr)username.Length, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
            }

            public void ResetPassword(string password, string token, string tokenId, TaskCompletionSource<object> tcs)
            {
                var tcsHandle = GCHandle.Alloc(tcs);
                EmailNativeMethods.reset_password(
                    _appHandle,
                    password, (IntPtr)password.Length,
                    token, (IntPtr)token.Length,
                    tokenId, (IntPtr)tokenId.Length,
                    GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
            }

            public void CallResetPasswordFunction(string username, string password, string functionArgs, TaskCompletionSource<object> tcs)
            {
                var tcsHandle = GCHandle.Alloc(tcs);
                EmailNativeMethods.call_reset_password_function(_appHandle,
                    username, (IntPtr)username.Length,
                    password, (IntPtr)password.Length,
                    functionArgs, (IntPtr)functionArgs.Length,
                    GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
            }
        }
    }
}
