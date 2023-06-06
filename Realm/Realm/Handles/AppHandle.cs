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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Native;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;

namespace Realms.Sync
{
    internal partial class AppHandle : StandaloneHandle
    {
        private static readonly List<WeakReference> _appHandles = new();

        private static class NativeMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void UserCallback(IntPtr tcs_ptr, IntPtr user_ptr, AppError error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void VoidTaskCallback(IntPtr tcs_ptr, AppError error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void StringCallback(IntPtr tcs_ptr, PrimitiveValue response, AppError error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ApiKeysCallback(IntPtr tcs_ptr, /* UserApiKey[] */ IntPtr api_keys, IntPtr api_keys_len, AppError error);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_initialize", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr initialize(
                [MarshalAs(UnmanagedType.LPWStr)] string framework, IntPtr framework_len,
                [MarshalAs(UnmanagedType.LPWStr)] string framework_version, IntPtr framework_version_len,
                [MarshalAs(UnmanagedType.LPWStr)] string sdk_version, IntPtr sdk_version_len,
                [MarshalAs(UnmanagedType.LPWStr)] string platform_version, IntPtr platform_version_len,
                [MarshalAs(UnmanagedType.LPWStr)] string device_name, IntPtr device_name_len,
                [MarshalAs(UnmanagedType.LPWStr)] string device_version, IntPtr device_version_len,
                UserCallback user_callback, VoidTaskCallback void_callback, StringCallback string_callback, ApiKeysCallback api_keys_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_create", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_app(Native.AppConfiguration app_config, byte[]? encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr syncuserHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_sync_immediately_run_file_actions", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool immediately_run_file_actions(AppHandle app, [MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_sync_reconnect", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reconnect(AppHandle app);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_get_current_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_current_user(AppHandle app, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_get_logged_in_users", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_logged_in_users(AppHandle app, [Out] IntPtr[] users, IntPtr bufsize, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_switch_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr switch_user(AppHandle app, SyncUserHandle user, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_login_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern void login_user(AppHandle app, Native.Credentials credentials, IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_remove_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern void remove_user(AppHandle app, SyncUserHandle user, IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_delete_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern void delete_user(AppHandle app, SyncUserHandle user, IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_reset_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reset_for_testing(AppHandle app);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_get_user_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_user_for_testing(
                AppHandle app,
                [MarshalAs(UnmanagedType.LPWStr)] string id_buf, IntPtr id_len,
                [MarshalAs(UnmanagedType.LPWStr)] string refresh_token_buf, IntPtr refresh_token_len,
                [MarshalAs(UnmanagedType.LPWStr)] string access_token_buf, IntPtr access_token_len,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_clear_cached_apps", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear_cached_apps(out NativeException ex);

            public static class EmailPassword
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
                public static extern void resent_confirmation_email(AppHandle app,
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
                    IntPtr tcs_ptr, out NativeException ex);
            }
        }

        static AppHandle()
        {
            NativeCommon.Initialize();
        }

        public static void Initialize()
        {
            NativeMethods.UserCallback userLogin = HandleUserCallback;
            NativeMethods.VoidTaskCallback taskCallback = HandleTaskCompletion;
            NativeMethods.StringCallback stringCallback = HandleStringCallback;
            NativeMethods.ApiKeysCallback apiKeysCallback = HandleApiKeysCallback;

            GCHandle.Alloc(userLogin);
            GCHandle.Alloc(taskCallback);
            GCHandle.Alloc(stringCallback);
            GCHandle.Alloc(apiKeysCallback);

            var frameworkName = InteropConfig.FrameworkName;
            var frameworkVersion = Environment.Version.ToString();

            // TODO: https://github.com/realm/realm-dotnet/issues/2218 this doesn't handle prerelease versions.
            var sdkVersion = InteropConfig.SDKVersion.ToString(3);

            var platformVersion = Environment.OSVersion.Version.ToString();

            if (!string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
            {
                platformVersion += $" {Environment.OSVersion.ServicePack}";
            }

            // TODO: try and infer device information as part of RNET-849
            var deviceName = "unknown";
            var deviceVersion = "unknown";

            NativeMethods.initialize(
                frameworkName, frameworkName.IntPtrLength(),
                frameworkVersion, frameworkVersion.IntPtrLength(),
                sdkVersion, sdkVersion.IntPtrLength(),
                platformVersion, platformVersion.IntPtrLength(),
                deviceName, deviceName.IntPtrLength(),
                deviceVersion, deviceVersion.IntPtrLength(),
                userLogin, taskCallback, stringCallback, apiKeysCallback);
        }

        internal AppHandle(IntPtr handle) : base(handle)
        {
            EmailPassword = new EmailPasswordApi(this);

            lock (_appHandles)
            {
                _appHandles.RemoveAll(a => !a.IsAlive);
                _appHandles.Add(new WeakReference(this));
            }
        }

        public static AppHandle CreateApp(Native.AppConfiguration config, byte[]? encryptionKey)
        {
            var handle = NativeMethods.create_app(config, encryptionKey, out var ex);
            ex.ThrowIfNecessary();
            return new AppHandle(handle);
        }

        public static void ForceCloseHandles(bool clearNativeCache = false)
        {
            lock (_appHandles)
            {
                foreach (var weakHandle in _appHandles)
                {
                    var appHandle = (AppHandle)weakHandle.Target!;
                    appHandle?.Close();
                }

                _appHandles.Clear();
            }

            if (clearNativeCache)
            {
                NativeMethods.clear_cached_apps(out var ex);
                ex.ThrowIfNecessary();
            }
        }

        public bool ImmediatelyRunFileActions(string path)
        {
            var result = NativeMethods.immediately_run_file_actions(this, path, (IntPtr)path.Length, out var ex);
            ex.ThrowIfNecessary();

            return result;
        }

        public void Reconnect()
        {
            NativeMethods.reconnect(this);
        }

        public bool TryGetCurrentUser([MaybeNullWhen(false)] out SyncUserHandle userHandle)
        {
            var userPtr = NativeMethods.get_current_user(this, out var ex);
            ex.ThrowIfNecessary();

            if (userPtr == IntPtr.Zero)
            {
                userHandle = null;
                return false;
            }

            userHandle = new SyncUserHandle(userPtr);
            return true;
        }

        public IEnumerable<SyncUserHandle> GetAllLoggedInUsers()
        {
            return MarshalHelpers.GetCollection((IntPtr[] buf, IntPtr len, out NativeException ex) => NativeMethods.get_logged_in_users(this, buf, len, out ex), bufferSize: 8)
                                 .Select(h => new SyncUserHandle(h));
        }

        public void SwitchUser(SyncUserHandle user)
        {
            NativeMethods.switch_user(this, user, out var ex);
            ex.ThrowIfNecessary();
        }

        public async Task<SyncUserHandle> LogInAsync(Native.Credentials credentials)
        {
            var tcs = new TaskCompletionSource<SyncUserHandle>();
            var tcsHandle = GCHandle.Alloc(tcs);

            try
            {
                NativeMethods.login_user(this, credentials, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();

                return await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        public async Task RemoveAsync(SyncUserHandle user)
        {
            var tcs = new TaskCompletionSource();
            var tcsHandle = GCHandle.Alloc(tcs);
            try
            {
                NativeMethods.remove_user(this, user, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
                await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        public async Task DeleteUserAsync(SyncUserHandle user)
        {
            var tcs = new TaskCompletionSource();
            var tcsHandle = GCHandle.Alloc(tcs);
            try
            {
                NativeMethods.delete_user(this, user, GCHandle.ToIntPtr(tcsHandle), out var ex);
                ex.ThrowIfNecessary();
                await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        public void ResetForTesting()
        {
            NativeMethods.reset_for_testing(this);
        }

        public SyncUserHandle GetUserForTesting(string id, string refreshToken, string accessToken)
        {
            var result = NativeMethods.get_user_for_testing(
                this,
                id, (IntPtr)id.Length,
                refreshToken, (IntPtr)refreshToken.Length,
                accessToken, (IntPtr)accessToken.Length,
                out var ex);
            ex.ThrowIfNecessary();
            return new SyncUserHandle(result);
        }

        protected override void Unbind() => NativeMethods.destroy(handle);

        [MonoPInvokeCallback(typeof(NativeMethods.UserCallback))]
        private static void HandleUserCallback(IntPtr tcs_ptr, IntPtr user_ptr, AppError error)
        {
            var tcsHandle = GCHandle.FromIntPtr(tcs_ptr);
            var tcs = (TaskCompletionSource<SyncUserHandle>)tcsHandle.Target!;
            if (error.is_null)
            {
                var userHandle = new SyncUserHandle(user_ptr);
                tcs.TrySetResult(userHandle);
            }
            else
            {
                tcs.TrySetException(new AppException(error));
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.VoidTaskCallback))]
        private static void HandleTaskCompletion(IntPtr tcs_ptr, AppError error)
        {
            var tcsHandle = GCHandle.FromIntPtr(tcs_ptr);
            var tcs = (TaskCompletionSource)tcsHandle.Target!;
            if (error.is_null)
            {
                tcs.TrySetResult();
            }
            else
            {
                tcs.TrySetException(new AppException(error));
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.StringCallback))]
        private static void HandleStringCallback(IntPtr tcs_ptr, PrimitiveValue response, AppError error)
        {
            var tcsHandle = GCHandle.FromIntPtr(tcs_ptr);
            var tcs = (TaskCompletionSource<string>)tcsHandle.Target!;
            if (error.is_null)
            {
                tcs.TrySetResult(response.AsString());
            }
            else
            {
                tcs.TrySetException(new AppException(error));
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.ApiKeysCallback))]
        private static void HandleApiKeysCallback(IntPtr tcs_ptr, IntPtr api_keys, IntPtr api_keys_len, AppError error)
        {
            var tcsHandle = GCHandle.FromIntPtr(tcs_ptr);
            var tcs = (TaskCompletionSource<ApiKey[]>)tcsHandle.Target!;
            if (error.is_null)
            {
                var result = new ApiKey[api_keys_len.ToInt32()];
                for (var i = 0; i < api_keys_len.ToInt32(); i++)
                {
                    var nativeKey = Marshal.PtrToStructure<UserApiKey>(IntPtr.Add(api_keys, i * UserApiKey.Size));
                    result[i] = new ApiKey(nativeKey);
                }

                tcs.TrySetResult(result);
            }
            else
            {
                tcs.TrySetException(new AppException(error));
            }
        }
    }
}
