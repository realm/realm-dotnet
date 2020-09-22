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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Native;

namespace Realms.Sync
{
    internal class AppHandle : RealmHandle
    {
        private static readonly Regex _platformRegex = new Regex("^(?<platform>[^0-9]*) (?<version>[^ ]*)", RegexOptions.Compiled);

        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void LogMessageCallback(IntPtr managed_handler, byte* message_buf, IntPtr message_len, LogLevel logLevel);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void UserLoginCallback(IntPtr tcs_ptr, IntPtr user_ptr, byte* message_buf, IntPtr message_len, byte* category_buf, IntPtr category_len, int error_code);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_initialize", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr initialize(
                [MarshalAs(UnmanagedType.LPWStr)] string platform, IntPtr platform_len,
                [MarshalAs(UnmanagedType.LPWStr)] string platform_version, IntPtr platform_version_len,
                [MarshalAs(UnmanagedType.LPWStr)] string sdk_version, IntPtr sdk_version_len,
                UserLoginCallback user_login_callback, LogMessageCallback log_message_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_create", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_app(Native.AppConfiguration app_config, byte[] encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr syncuserHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_sync_get_session_from_path", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_session(AppHandle app, SharedRealmHandle realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_sync_get_path_for_realm", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_path_for_realm(AppHandle app, SyncUserHandle user, [MarshalAs(UnmanagedType.LPWStr)] string partition, IntPtr partition_len, IntPtr buffer, IntPtr bufsize, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_sync_immediately_run_file_actions", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool immediately_run_file_actions(AppHandle app, [MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_sync_reconnect", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reconnect(AppHandle app);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_get_current_sync_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_current_user(AppHandle app, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_get_logged_in_users", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_logged_in_users(AppHandle app, [Out] IntPtr[] users, IntPtr bufsize, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_switch_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr switch_user(AppHandle app, SyncUserHandle user, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_login_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern void login_user(AppHandle app, Native.Credentials credentials, IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_app_reset_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reset_for_testing(AppHandle app);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        static unsafe AppHandle()
        {
            NativeCommon.Initialize();
            SessionHandle.InstallCallbacks();

            NativeMethods.LogMessageCallback logMessage = HandleLogMessage;
            NativeMethods.UserLoginCallback userLogin = HandleUserLogin;

            GCHandle.Alloc(logMessage);
            GCHandle.Alloc(userLogin);

            string platform;
            string platformVersion;
            var osDescription = _platformRegex.Match(RuntimeInformation.OSDescription);
            if (osDescription.Success)
            {
                platform = osDescription.Groups["platform"].Value;
                platformVersion = osDescription.Groups["version"].Value;
            }
            else
            {
                platform = Environment.OSVersion.Platform.ToString();
                platformVersion = Environment.OSVersion.VersionString;
            }

            var sdkVersion = typeof(AppHandle).GetTypeInfo().Assembly.GetName().Version.ToString();
            NativeMethods.initialize(
                platform, (IntPtr)platform.Length,
                platformVersion, (IntPtr)platformVersion.Length,
                sdkVersion, (IntPtr)sdkVersion.Length,
                userLogin, logMessage);

            HttpClientTransport.Install();
        }

        internal AppHandle(IntPtr handle) : base(null, handle)
        {
        }

        public static AppHandle CreateApp(Native.AppConfiguration config, byte[] encryptionKey)
        {
            var handle = NativeMethods.create_app(config, encryptionKey, out var ex);
            ex.ThrowIfNecessary();
            return new AppHandle(handle);
        }

        public string GetRealmPath(User user, string partition)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_path_for_realm(this, user.Handle, partition, (IntPtr)partition.Length, buffer, bufferLength, out ex);
            });
        }

        public SessionHandle GetSessionForPath(SharedRealmHandle realm)
        {
            var ptr = NativeMethods.get_session(this, realm, out var ex);
            ex.ThrowIfNecessary();
            return new SessionHandle(ptr);
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

        public bool TryGetCurrentUser(out SyncUserHandle handle)
        {
            var userPtr = NativeMethods.get_current_user(this, out var ex);
            ex.ThrowIfNecessary();

            if (userPtr == IntPtr.Zero)
            {
                handle = null;
                return false;
            }

            handle = new SyncUserHandle(userPtr);
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

        public void LogIn(Native.Credentials credentials, TaskCompletionSource<SyncUserHandle> tcs)
        {
            var tcsHandle = GCHandle.Alloc(tcs);
            NativeMethods.login_user(this, credentials, GCHandle.ToIntPtr(tcsHandle), out var ex);
            ex.ThrowIfNecessary();
        }

        public void ResetForTesting()
        {
            NativeMethods.reset_for_testing(this);
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.LogMessageCallback))]
        private static unsafe void HandleLogMessage(IntPtr managedHandler, byte* messageBuffer, IntPtr messageLength, LogLevel level)
        {
            try
            {
                var message = Encoding.UTF8.GetString(messageBuffer, (int)messageLength);
                var logCallback = (Action<string, LogLevel>)GCHandle.FromIntPtr(managedHandler).Target;
                logCallback.Invoke(message, level);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying to log a message: {ex}";
                Console.Error.WriteLine(errorMessage);
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.LogMessageCallback))]
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The user will own its handle.")]
        private static unsafe void HandleUserLogin(IntPtr tcs_ptr, IntPtr user_ptr, byte* message_buf, IntPtr message_len, byte* error_category_buf, IntPtr error_category_len, int error_code)
        {
            var tcsHandle = GCHandle.FromIntPtr(tcs_ptr);
            try
            {
                var tcs = (TaskCompletionSource<SyncUserHandle>)tcsHandle.Target;
                if (user_ptr != IntPtr.Zero)
                {
                    var userHandle = new SyncUserHandle(user_ptr);
                    tcs.TrySetResult(userHandle);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(message_buf, (int)message_len);
                    var errorCategory = Encoding.UTF8.GetString(error_category_buf, (int)error_category_len);
                    tcs.TrySetException(new AppException(message, errorCategory, error_code));
                }
            }
            finally
            {
                tcsHandle.Free();
            }
        }
    }
}
