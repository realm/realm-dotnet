﻿////////////////////////////////////////////////////////////////////////////
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
using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// An <see cref="App"/> is the main client-side entry point for interacting with a MongoDB Realm App.
    /// </summary>
    /// <remarks>
    /// The App can be used to:
    /// <br/>
    /// <list type="bullet">
    /// <item><description>
    /// Register uses and perform various user-related operations through authentication providers (e.g. <see cref="User.ApiKeys"/>, <see cref="EmailPasswordAuth"/>).
    /// </description></item>
    /// <item><description>
    /// Synchronize data between the local device and a remote Realm App with Synchronized Realms (using <see cref="SyncConfiguration"/>).
    /// </description></item>
    /// <item><description>
    /// Invoke Realm App functions with Functions (using <see cref="User.Functions"/>).
    /// </description></item>
    /// <item><description>
    /// Access remote data from MongoDB databases with a <see cref="MongoClient"/> (using <see cref="User.GetMongoClient"/>).
    /// </description></item>
    /// </list>
    /// <br/>
    /// To create an app that is linked with a remote Realm App initialize Realm and configure the App as shown below:
    /// <code>
    /// var appConfig = new AppConfiguration("my-realm-app-id")
    /// {
    ///     LocalAppName = "My amazing iOS app",
    ///     LocalAppVersion = "1.2.3"
    /// };
    ///
    /// var app = new App(appConfig);
    /// </code>
    /// After configuring the App you can start managing users, configure Synchronized Realms, call remote Realm Functions and access remote data through Mongo Collections.
    /// <br/>
    /// To register a new user and/or login with an existing user do as shown below:
    /// <code>
    /// await app.EmailPassword.RegisterUserAsync("foo@bar.com", "password");
    /// // Login with existing user
    /// var user = app.LoginAsync(Credentials.EmailPassword("foo@bar.com", "password");
    /// </code>
    /// With an authorized user you can synchronize data between the local device and the remote Realm App by opening a Realm with a <see cref="SyncConfiguration"/> as indicated below:
    /// <code>
    /// var syncConfig = new SyncConfiguration("some-partition-value", user);
    /// using var realm = await Realm.GetInstanceAsync(syncConfig);
    ///
    /// realm.Write(() =>
    /// {
    ///     realm.Add(...);
    /// });
    ///
    /// await realm.GetSession().WaitForUploadAsync();
    /// </code>
    /// You can call remote Realm functions as shown below:
    /// <code>
    /// user.Functions.Call("sum", 1, 2, 3, 4, 5);
    /// </code>
    /// And access collections from the remote Realm App as shown here:
    /// <code>
    /// var client = user.GetMongoClient("atlas-service");
    /// var db = client.GetDatabase("my-db");
    /// var collection = db.GetCollection("foos");
    /// var foosCount = await collection.CountAsync();
    /// </code>
    /// </remarks>
    /// <seealso cref="AppConfiguration"/>
    public class App
    {
        internal readonly AppHandle AppHandle;

        /// <summary>
        /// Gets a <see cref="SyncApi"/> instance that exposes API for interacting with the synchronization client for this <see cref="App"/>.
        /// </summary>
        /// <value>A <see cref="SyncApi"/> instance scoped to this <see cref="App"/>.</value>
        public SyncApi Sync { get; }

        /// <summary>
        /// Gets a <see cref="EmailPasswordApi"/> instance that exposes functionality related to users either being created or logged in using
        /// the <see cref="Credentials.AuthProvider.EmailPassword"/> provider.
        /// </summary>
        /// <value>An <see cref="EmailPasswordApi"/> instance scoped to this <see cref="App"/>.</value>
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

        internal App(AppHandle handle)
        {
            AppHandle = handle;
            Sync = new SyncApi(this);
            EmailPasswordAuth = new EmailPasswordApi(this);
        }

        /// <summary>
        /// A factory method for creating an app with a particular <see cref="AppConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="AppConfiguration"/>, specifying key parameters for the app behavior.</param>
        /// <returns>An <see cref="App"/> instance can now be used to login users, call functions, or open synchronized Realms.</returns>
        public static App Create(AppConfiguration config)
        {
            Argument.NotNull(config, nameof(config));

            if (config.MetadataPersistenceMode.HasValue)
            {
                if (config.MetadataPersistenceMode == MetadataPersistenceMode.Encrypted && config.MetadataEncryptionKey == null)
                {
                    throw new ArgumentException($"{nameof(AppConfiguration.MetadataEncryptionKey)} must be set when {nameof(AppConfiguration.MetadataPersistenceMode)} is set to {nameof(MetadataPersistenceMode.Encrypted)}.");
                }

                if (config.MetadataPersistenceMode != MetadataPersistenceMode.Encrypted && config.MetadataEncryptionKey != null)
                {
                    throw new ArgumentException($"{nameof(AppConfiguration.MetadataPersistenceMode)} must be set to {nameof(MetadataPersistenceMode.Encrypted)} when {nameof(AppConfiguration.MetadataEncryptionKey)} is set.");
                }
            }

            var nativeConfig = new Native.AppConfiguration
            {
                AppId = config.AppId,
                BaseFilePath = config.BaseFilePath ?? InteropConfig.DefaultStorageFolder,
                BaseUrl = config.BaseUri?.ToString().TrimEnd('/'),
                LocalAppName = config.LocalAppName,
                LocalAppVersion = config.LocalAppVersion,
                MetadataPersistence = config.MetadataPersistenceMode,
                default_request_timeout_ms = (ulong?)config.DefaultRequestTimeout?.TotalMilliseconds ?? 0,
                log_level = config.LogLevel,
            };

            if (config.CustomLogger != null)
            {
                // TODO: should we free this eventually?
                var logHandle = GCHandle.Alloc(config.CustomLogger);
                nativeConfig.managed_log_callback = GCHandle.ToIntPtr(logHandle);
            }

            var handle = AppHandle.CreateApp(nativeConfig, config.MetadataEncryptionKey);
            return new App(handle);
        }

        /// <summary>
        /// A factory method for creating an app with a particular <see cref="AppConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// This is a convenience method that creates an <see cref="AppConfiguration"/> with the default parameters and the provided <paramref name="appId"/>
        /// and invokes <see cref="App.Create(AppConfiguration)"/>.
        /// </remarks>
        /// <param name="appId">The application id of the MongoDB Realm Application.</param>
        /// <returns>An <see cref="App"/> instance can now be used to login users, call functions, or open synchronized Realms.</returns>
        public static App Create(string appId) => Create(new AppConfiguration(appId));

        /// <summary>
        /// Logs in as a user with the given credentials associated with an authentication provider.
        /// </summary>
        /// <remarks>
        /// The last logged in user will be saved as <see cref="CurrentUser"/>. If there was already a current user,
        /// that user is still logged in and can be found in the list returned by <see cref="AllUsers"/>. It is also
        /// possible to switch between which user is considered the current user by using <see cref="SwitchUser(User)"/>.
        /// </remarks>
        /// <param name="credentials">The <see cref="Credentials"/> representing the type of login.</param>
        /// <returns>
        /// A <see cref="Task{User}"/> that represents the asynchronous LogIn operation.</returns>
        public async Task<User> LogInAsync(Credentials credentials)
        {
            Argument.NotNull(credentials, nameof(credentials));

            var tcs = new TaskCompletionSource<SyncUserHandle>();
            AppHandle.LogIn(credentials.ToNative(), tcs);
            var handle = await tcs.Task;
            return new User(handle, this);
        }

        /// <summary>
        /// Switches the <see cref="CurrentUser"/> to the one specified in <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The new current user.</param>
        public void SwitchUser(User user)
        {
            Argument.NotNull(user, nameof(user));

            AppHandle.SwitchUser(user.Handle);
        }

        /// <summary>
        /// Removes a user and their local data from the device. If the user is logged in, they will be logged out in the process.
        /// </summary>
        /// <remarks>
        /// This is client operation and will not delete any data stored on the server for that user.
        /// </remarks>
        /// <param name="user">The user to log out and remove.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous RemoveUser operation. Successful completion indicates that the user has been logged out,
        /// their local data - removed, and the user's <see cref="User.RefreshToken"/> - revoked on the server.
        /// </returns>
        public async Task RemoveUserAsync(User user)
        {
            Argument.NotNull(user, nameof(user));

            var tcs = new TaskCompletionSource<object>();
            AppHandle.Remove(user.Handle, tcs);
            await tcs.Task;
        }

        /// <summary>
        /// A sync manager, handling synchronization of local Realm with remote MongoDB Realm apps. It is always scoped to a
        /// particular app and can only be accessed via <see cref="Sync"/>.
        /// </summary>
        public class SyncApi
        {
            private readonly App _app;

            internal SyncApi(App app)
            {
                _app = app;
            }

            /// <summary>
            /// Attempt to reconnect all <see cref="Session">Sync sessions</see> for the app.
            /// </summary>
            /// <remarks>
            /// Realm will automatically detect when a device gets connectivity after being offline and resume syncing.
            /// However, some of these checks are performed using incremental backoff, which means that there are cases
            /// when automatic reconnection doesn't happen immediately. In those cases, it can be beneficial to call
            /// this method manually, which will force all sessions to attempt to reconnect and in the process, reset
            /// any timers, that are used for incremental backoff.
            /// </remarks>
            public void Reconnect()
            {
                _app.AppHandle.Reconnect();
            }
        }

        /// <summary>
        /// A class, encapsulating functionality for users, logged in with the <see cref="Credentials.AuthProvider.EmailPassword"/> provider.
        /// It is always scoped to a particular app and can only be accessed via <see cref="EmailPasswordAuth"/>.
        /// </summary>
        public class EmailPasswordApi
        {
            private readonly App _app;

            internal EmailPasswordApi(App app)
            {
                _app = app;
            }

            /// <summary>
            /// Registers a new user with the given email and password.
            /// </summary>
            /// <param name="email">
            /// The email to register with. This will be the user's username and, if user confirmation is enabled, this will be the address for
            /// the confirmation email.
            /// </param>
            /// <param name="password">The password to associate with the email. The password must be between 6 and 128 characters long.</param>
            /// <returns>
            /// A <see cref="Task"/> representing the asynchronous RegisterUser operation. Successful completion indicates that the user has been
            /// created on the server and can now be logged in calling <see cref="LogInAsync(Credentials)"/> with <see cref="Credentials.EmailPassword(string, string)"/>.
            /// </returns>
            public Task RegisterUserAsync(string email, string password)
            {
                Argument.NotNullOrEmpty(email, nameof(email));
                Argument.NotNullOrEmpty(password, nameof(password));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.RegisterUser(email, password, tcs);
                return tcs.Task;
            }

            /// <summary>
            /// Confirms a user with the given token and token id. These are typically included in the email the user received
            /// after registering.
            /// </summary>
            /// <remarks>
            /// While confirmation typically happens in a web app, mobile applications that have deep linking enabled can intercept the url
            /// and complete the user confirmation flow in the app itself.
            /// </remarks>
            /// <param name="token">The confirmation token.</param>
            /// <param name="tokenId">The id of the confirmation token.</param>
            /// <returns>
            /// A <see cref="Task"/> representing the asynchronous ConfirmUser operation. Successful completion indicates that the user has been
            /// confirmed on the server.
            /// </returns>
            public Task ConfirmUserAsync(string token, string tokenId)
            {
                Argument.NotNullOrEmpty(token, nameof(token));
                Argument.NotNullOrEmpty(tokenId, nameof(tokenId));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.ConfirmUser(token, tokenId, tcs);
                return tcs.Task;
            }

            /// <summary>
            /// Resends the confirmation email for a user to the given email.
            /// </summary>
            /// <param name="email">The email of the user.</param>
            /// <returns>
            /// A <see cref="Task"/> representing the asynchronous request to the server that a confirmation email is sent. Successful
            /// completion indicates that the server has accepted the request and will send a confirmation email to the specified address
            /// if a user with that email exists.
            /// </returns>
            public Task ResendConfirmationEmailAsync(string email)
            {
                Argument.NotNullOrEmpty(email, nameof(email));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.ResendConfirmationEmail(email, tcs);
                return tcs.Task;
            }

            /// <summary>
            /// Sends a password reset email to the specified address.
            /// </summary>
            /// <param name="email">the email of the user.</param>
            /// <returns>
            /// A <see cref="Task"/> representing the asynchronous request to the server that a reset password email is sent. Successful
            /// completion indicates that the server has accepted the request and will send a password reset email to the specified
            /// address if a user with that email exists.
            /// </returns>
            public Task SendResetPasswordEmailAsync(string email)
            {
                Argument.NotNullOrEmpty(email, nameof(email));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.SendResetPasswordEmail(email, tcs);
                return tcs.Task;
            }

            /// <summary>
            /// Completes the reset password flow by providing the desired new password.
            /// </summary>
            /// <remarks>
            /// While the reset password flow is typically completed in the web app, mobile applications that have deep linking enabled can intercept the url
            /// and complete the password reset flow in the app itself.
            /// </remarks>
            /// <param name="password">The new password for the user.</param>
            /// <param name="token">The password reset token that was sent to the user's email address.</param>
            /// <param name="tokenId">The password reset token id that was sent together with the <paramref name="token"/> to the user's email address.</param>
            /// <returns>
            /// A <see cref="Task"/> representing the asynchronous request that a user's password is reset. Successful completion indicates that the user's password has been
            /// reset and they can now use the new password to create <see cref="Credentials.EmailPassword"/> credentials and call <see cref="LogInAsync(Credentials)"/> to login.
            /// </returns>
            public Task ResetPasswordAsync(string password, string token, string tokenId)
            {
                Argument.NotNullOrEmpty(token, nameof(token));
                Argument.NotNullOrEmpty(tokenId, nameof(tokenId));
                Argument.NotNullOrEmpty(password, nameof(password));

                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.ResetPassword(password, token, tokenId, tcs);
                return tcs.Task;
            }

            /// <summary>
            /// Calls the reset password function, configured on the server.
            /// </summary>
            /// <param name="email">The email of the user.</param>
            /// <param name="password">The new password of the user.</param>
            /// <param name="functionArgs">
            /// Any additional arguments provided to the reset function. All arguments must be able to be converted to JSON
            /// compatible values.
            /// </param>
            /// <returns>
            /// A <see cref="Task"/> representing the asynchronous request to call a password reset function. Successful completion indicates
            /// that the user's password has been change and they can now use the new password to create <see cref="Credentials.EmailPassword"/>
            /// credentials and call <see cref="LogInAsync(Credentials)"/> to login.
            /// </returns>
            public Task CallResetPasswordFunctionAsync(string email, string password, params object[] functionArgs)
            {
                Argument.NotNullOrEmpty(email, nameof(email));
                Argument.NotNullOrEmpty(password, nameof(password));

                var bsonArgs = new BsonArray(functionArgs);
                var tcs = new TaskCompletionSource<object>();
                _app.AppHandle.EmailPassword.CallResetPasswordFunction(email, password, bsonArgs.ToString(), tcs);
                return tcs.Task;
            }
        }
    }
}
