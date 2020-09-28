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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Helpers;

namespace Realms.Sync
{
    public class App
    {
        private readonly GCHandle? _logHandle;
        internal readonly AppHandle AppHandle;

        public SyncApi Sync { get; }

        public EmailPasswordApi EmailPasswordAuth { get; }

        /// <summary>
        /// Gets the currently logged-in user. If none exists, null is returned.
        /// </summary>
        /// <value>Valid user or <c>null</c> to indicate nobody logged in.</value>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The User instance will own its handle.")]
        public User CurrentUser => AppHandle.TryGetCurrentUser(out var userHandle) ? new User(userHandle, this) : null;

        /// <summary>
        /// Gets all currently logged in users.
        /// </summary>
        /// <value>An array of valid logged in users.</value>
        public User[] AllUsers => AppHandle.GetAllLoggedInUsers()
                                            .Select(handle => new User(handle, this))
                                            .ToArray();

        internal App(AppHandle handle, GCHandle? logHandle = null)
        {
            AppHandle = handle;
            _logHandle = logHandle;
            Sync = new SyncApi(this);
            EmailPasswordAuth = new EmailPasswordApi(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="App"/> class.
        /// </summary>
        ~App()
        {
            if (_logHandle.HasValue)
            {
                _logHandle.Value.Free();
            }
        }

        public static App Create(AppConfiguration config)
        {
            Argument.NotNull(config, nameof(config));

            var nativeConfig = new Native.AppConfiguration
            {
                AppId = config.AppId,
                BaseFilePath = config.BaseFilePath ?? InteropConfig.DefaultStorageFolder,
                BaseUrl = config.BaseUri?.ToString().TrimEnd('/'),
                LocalAppName = config.LocalAppName,
                LocalAppVersion = config.LocalAppVersion,
                MetadataPersistence = config.MetadataPersistenceMode,
                reset_metadata_on_error = config.ResetMetadataOnError,
                default_request_timeout_ms = (ulong?)config.DefaultRequestTimeout?.TotalMilliseconds ?? 0,
                log_level = config.LogLevel,
            };

            GCHandle? logHandle = null;
            if (config.CustomLogger != null)
            {
                // V10TODO: free the handle
                logHandle = GCHandle.Alloc(config.CustomLogger);
                nativeConfig.managed_log_callback = GCHandle.ToIntPtr(logHandle.Value);
            }

            var handle = AppHandle.CreateApp(nativeConfig, config.MetadataEncryptionKey);
            return new App(handle);
        }

        public static App Create(string appId) => Create(new AppConfiguration(appId));

        public async Task<User> LogInAsync(Credentials credentials)
        {
            Argument.NotNull(credentials, nameof(credentials));

            var tcs = new TaskCompletionSource<SyncUserHandle>();
            AppHandle.LogIn(credentials.ToNative(), tcs);
            var handle = await tcs.Task;
            return new User(handle, this);
        }

        public void SwitchUser(User user)
        {
            Argument.NotNull(user, nameof(user));

            AppHandle.SwitchUser(user.Handle);
        }

        public async Task RemoveUserAsync(User user)
        {
            Argument.NotNull(user, nameof(user));

            var tcs = new TaskCompletionSource<object>();
            AppHandle.Remove(user.Handle, tcs);
            await tcs.Task;
        }

        public class SyncApi
        {
            private readonly App _app;

            internal SyncApi(App app)
            {
                _app = app;
            }

            public void Reconnect()
            {
                _app.AppHandle.Reconnect();
            }
        }

        public class EmailPasswordApi
        {
            private readonly App _app;

            internal EmailPasswordApi(App app)
            {
                _app = app;
            }

            public async Task RegisterUserAsync(string email, string password)
            {
                Argument.NotNullOrEmpty(email, nameof(email));
                Argument.NotNullOrEmpty(password, nameof(password));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.RegisterUser(email, password, tcs);
                await tcs.Task;
            }

            public async Task ConfirmUserAsync(string token, string tokenId)
            {
                Argument.NotNullOrEmpty(token, nameof(token));
                Argument.NotNullOrEmpty(tokenId, nameof(tokenId));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.ConfirmUser(token, tokenId, tcs);
                await tcs.Task;
            }

            public async Task ResendConfirmationEmailAsync(string email)
            {
                Argument.NotNullOrEmpty(email, nameof(email));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.ResendConfirmationEmail(email, tcs);
                await tcs.Task;
            }

            public async Task SendResetPasswordEmailAsync(string email)
            {
                Argument.NotNullOrEmpty(email, nameof(email));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.SendResetPasswordEmail(email, tcs);
                await tcs.Task;
            }

            public async Task ResetPasswordAsync(string password, string token, string tokenId)
            {
                Argument.NotNullOrEmpty(token, nameof(token));
                Argument.NotNullOrEmpty(tokenId, nameof(tokenId));
                Argument.NotNullOrEmpty(password, nameof(password));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.ResetPassword(password, token, tokenId, tcs);
                await tcs.Task;
            }

            public async Task CallResetPasswordFunctionAsync(string email, string password)
            {
                // V10TODO: need to pass in some BsonArray
                throw new NotImplementedException();

                Argument.NotNullOrEmpty(email, nameof(email));
                Argument.NotNullOrEmpty(password, nameof(password));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.RegisterUser(email, password, tcs);
                await tcs.Task;
            }
        }
    }
}
