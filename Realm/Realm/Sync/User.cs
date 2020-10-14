////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Realms.Helpers;
using Realms.Native;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// This class represents a user in a MongoDB Realm app. The credentials are provided by various 3rd party providers (Facebook, Google, etc.).
    /// A user can log in to the server and, if access is granted, it is possible to synchronize the local and the remote Realm. Moreover, synchronization is halted when the user is logged out.
    /// It is possible to persist a user. By retrieving a user, there is no need to log in to the 3rd party provider again. Persisting a user between sessions, the user's credentials are stored locally on the device, and should be treated as sensitive data.
    /// </summary>
    public class User : IEquatable<User>
    {
        /// <summary>
        /// Gets this user's refresh token. This is the user's credential for accessing MongoDB Realm data and should be treated as sensitive information.
        /// </summary>
        /// <value>A unique string that can be used for refreshing the user's credentials.</value>
        public string RefreshToken
        {
            get => Handle.GetRefreshToken();
        }

        /// <summary>
        /// Gets this user's access token. This is the user's credential for accessing MongoDB Realm data and should be treated as sensitive information.
        /// </summary>
        /// <value>A unique string that can be used to represent this user before the server.</value>
        public string AccessToken
        {
            get => Handle.GetAccessToken();
        }

        /// <summary>
        /// Gets a unique identifier for the device the user logged in to.
        /// </summary>
        /// <value>A unique string that identifies the current device.</value>
        public string DeviceId
        {
            get => Handle.GetDeviceId();
        }

        /// <summary>
        /// Gets the Id of this user on MongoDB Realm.
        /// </summary>
        /// <value>A string that uniquely identifies that user.</value>
        public string Id => Handle.GetUserId();

        /// <summary>
        /// Gets the current state of the user.
        /// </summary>
        /// <value>A value indicating whether the user is active, logged out, or an error has occurred.</value>
        public UserState State => Handle.GetState();

        /// <summary>
        /// Gets a value indicating which <see cref="Credentials.AuthProvider"/> this user logged in with.
        /// </summary>
        /// <value>The <see cref="Credentials.AuthProvider"/> used to login the user.</value>
        public Credentials.AuthProvider Provider => Handle.GetProvider();

        /// <summary>
        /// Gets the app with which this user is associated.
        /// </summary>
        /// <value>An <see cref="App"/> instance that owns this user.</value>
        public App App { get; }

        /// <summary>
        /// Gets the profile information for that user.
        /// </summary>
        /// <value>A <see cref="UserProfile"/> object, containing information about the user's name, email, and so on.</value>
        public UserProfile Profile { get; }

        /// <summary>
        /// Gets the custom user data associated with this user in the Realm app.
        /// </summary>
        /// <remarks>
        /// The data is only refreshed when the user's access token is refreshed or when explicitly calling <see cref="RefreshCustomDataAsync"/>.
        /// </remarks>
        /// <returns>A document containing the user data.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/users/enable-custom-user-data/">Custom User Data Docs</seealso>
        public BsonDocument GetCustomData()
        {
            var serialized = Handle.GetCustomData();
            if (string.IsNullOrEmpty(serialized) || !BsonDocument.TryParse(serialized, out var doc))
            {
                return null;
            }

            return doc;
        }

        /// <summary>
        /// Gets the custom user data associated with this user in the Realm app and parses it to the specified type.
        /// </summary>
        /// <typeparam name="T">The managed type that matches the shape of the custom data documents.</typeparam>
        /// <remarks>
        /// The data is only refreshed when the user's access token is refreshed or when explicitly calling <see cref="RefreshCustomDataAsync"/>.
        /// </remarks>
        /// <returns>A document containing the user data.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/users/enable-custom-user-data/">Custom User Data Docs</seealso>
        public T GetCustomData<T>()
            where T : class
        {
            var customData = GetCustomData();
            if (customData == null)
            {
                return null;
            }

            return BsonSerializer.Deserialize<T>(customData);
        }

        /// <summary>
        /// Gets a collection of all identities associated with this user.
        /// </summary>
        /// <value>The user's identities across different <see cref="Credentials.AuthProvider"/>s.</value>
        public UserIdentity[] Identities
        {
            get
            {
                var serialized = Handle.GetIdentities();
                return BsonSerializer.Deserialize<UserIdentity[]>(serialized);
            }
        }

        /// <summary>
        /// Gets a <see cref="ApiKeyClient"/> instance that exposes functionality for managing user API keys.
        /// </summary>
        /// <value>A <see cref="ApiKeyClient"/> instance scoped to this <see cref="User"/>.</value>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/api-key/">API Keys Authentication Docs</seealso>
        public ApiKeyClient ApiKeys { get; }

        /// <summary>
        /// Gets a <see cref="FunctionsClient"/> instance that exposes functionality for calling remote MongoDB Realm functions.
        /// </summary>
        /// <value>A <see cref="FunctionsClient"/> instance scoped to this <see cref="User"/>.</value>
        /// <seealso href="https://docs.mongodb.com/realm/functions/">Functions Docs</seealso>
        public FunctionsClient Functions { get; }

        internal readonly SyncUserHandle Handle;

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The App instance will own its handle.")]
        internal User(SyncUserHandle handle, App app = null)
        {
            if (app == null && handle.TryGetApp(out var appHandle))
            {
                app = new App(appHandle);
            }

            App = app;
            Handle = handle;
            Profile = new UserProfile(this);
            ApiKeys = new ApiKeyClient(this);
            Functions = new FunctionsClient(this);
        }

        /// <summary>
        /// Removes the user's local credentials and attempts to invalidate their refresh token from the server.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/> that represents the remote logout operation.</returns>
        public Task LogOutAsync() => App.RemoveUserAsync(this);

        /// <summary>
        /// Re-fetch the user's custom data from the server.
        /// </summary>
        /// <returns>
        /// An awaitable <see cref="Task{T}"/> that represents the remote refresh operation. The result is a <see cref="BsonDocument"/>
        /// containing the updated custom user data. The value returned by <see cref="GetCustomData"/> will also be updated with the new information.
        /// </returns>
        public async Task<BsonDocument> RefreshCustomDataAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            Handle.RefreshCustomData(tcs);
            await tcs.Task;

            return GetCustomData();
        }

        /// <summary>
        /// Re-fetch the user's custom data from the server.
        /// </summary>
        /// <typeparam name="T">The managed type that matches the shape of the custom data documents.</typeparam>
        /// <returns>
        /// An awaitable <see cref="Task{T}"/> that represents the remote refresh operation. The result is an object
        /// containing the updated custom user data. The value returned by <see cref="GetCustomData{T}"/> will also be updated with the new information.
        /// </returns>
        public async Task<T> RefreshCustomDataAsync<T>()
            where T : class
        {
            var result = await RefreshCustomDataAsync();
            if (result == null)
            {
                return null;
            }

            return BsonSerializer.Deserialize<T>(result);
        }

        /// <summary>
        /// Gets a <see cref="MongoClient"/> instance for accessing documents in a MongoDB database.
        /// </summary>
        /// <param name="serviceName">The name of the service as configured on the server.</param>
        /// <returns>A <see cref="MongoClient"/> instance that can interact with the databases exposed in the remote service.</returns>
        public MongoClient GetMongoClient(string serviceName) => new MongoClient(this, serviceName);

        /// <summary>
        /// Gets a client for interacting the with Firebase Cloud Messaging service exposed in MongoDB Realm.
        /// </summary>
        /// <remarks>
        /// The FCM service needs to be configured and enabled in the MongodB Realm UI before devices can register
        /// and receive push notifications.
        /// </remarks>
        /// <param name="serviceName">The name of the service as configured in the MongoDB Realm UI.</param>
        /// <returns>A client that exposes API to register/deregister push notification tokens.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/services/send-mobile-push-notifications/index.html#send-a-push-notification">Send Mobile Push Notifications Docs</seealso>
        public PushClient GetPushClient(string serviceName) => new PushClient(this, serviceName);

        /// <summary>
        /// Links the current user with a new user identity represented by the given credentials.
        /// </summary>
        /// <remarks>
        /// Linking a user with more credentials, mean the user can login either of these credentials. It also
        /// makes it possible to "upgrade" an anonymous user by linking it with e.g. Email/Password credentials.
        /// <br/>
        /// Note: It is not possible to link two existing users of MongoDB Realm. The provided credentials must not have been used by another user.
        /// <br/>
        /// Note for email/password auth: To link a user with a new set of <see cref="Credentials.EmailPassword"/> credentials, you will need to first
        /// register these credentials by calling <see cref="App.EmailPasswordClient.RegisterUserAsync"/>.
        /// </remarks>
        /// <example>
        /// The following snippet shows how to associate an email and password with an anonymous user
        /// allowing them to login on a different device.
        /// <code>
        /// var app = App.Create("app-id")
        /// var user = await app.LogInAsync(Credentials.Anonymous());
        ///
        /// // This step is only needed for email password auth - a password record must exist
        /// // before you can link a user to it.
        /// await app.EmailPasswordAuth.RegisterUserAsync("email", "password");
        /// await user.LinkCredentialsAsync(Credentials.EmailPassword("email", "password"));
        /// </code>
        /// </example>
        /// <param name="credentials">The credentials to link with the current user.</param>
        /// <returns>
        /// An awaitable <see cref="Task{T}"/> representing the remote link credentials operation. Upon successful completion, the task result
        /// will contain the user to which the credentials were linked.
        /// </returns>
        public async Task<User> LinkCredentialsAsync(Credentials credentials)
        {
            Argument.NotNull(credentials, nameof(credentials));

            var tcs = new TaskCompletionSource<SyncUserHandle>();
            Handle.LinkCredentials(App.Handle, credentials.ToNative(), tcs);
            var handle = await tcs.Task;

            return new User(handle, App);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        /// <summary>
        /// Determines whether this instance and another <see cref="User"/> instance are equal by comparing their identities.
        /// </summary>
        /// <param name="other">The <see cref="User"/> instance to compare with.</param>
        /// <returns>true if the two instances are equal; false otherwise.</returns>
        public bool Equals(User other)
        {
            return Id.Equals(other?.Id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// A class exposing functionality for users to manage API keys from the client. It is always scoped
        /// to a particular <see cref="User"/> and can only be accessed via <see cref="ApiKeys"/>.
        /// </summary>
        public class ApiKeyClient
        {
            private readonly User _user;

            internal ApiKeyClient(User user)
            {
                _user = user;
            }

            /// <summary>
            /// Creates an API key that can be used to authenticate as the user.
            /// </summary>
            /// <remarks>
            /// The value of the returned API key must be persisted at this time as this is the only
            /// time it is visible. The key is enabled when created. It can be disabled by calling
            /// <see cref="DisableAsync"/>.
            /// </remarks>
            /// <param name="name">The friendly name of the key.</param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the asynchronous operation. Successful completion indicates
            /// that the <see cref="ApiKey"/> has been created on the server and its <see cref="ApiKey.Value"/> can
            /// be used to create <see cref="Credentials.ApiKey(string)"/>.
            /// </returns>
            public async Task<ApiKey> CreateAsync(string name)
            {
                Argument.NotNullOrEmpty(name, nameof(name));

                var tcs = new TaskCompletionSource<UserApiKey[]>();
                _user.Handle.CreateApiKey(_user.App.Handle, name, tcs);
                var apiKeys = await tcs.Task;

                Debug.Assert(apiKeys.Length == 1, "The result of Create should be exactly 1 ApiKey.");

                return new ApiKey(apiKeys.Single());
            }

            /// <summary>
            /// Fetches a specific user API key by id.
            /// </summary>
            /// <param name="id">The id of the key to fetch.</param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the asynchronous lookup operation.
            /// </returns>
            public async Task<ApiKey> FetchAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<UserApiKey[]>();
                _user.Handle.FetchApiKey(_user.App.Handle, id, tcs);
                var apiKeys = await Handle404(tcs);

                Debug.Assert(apiKeys == null || apiKeys.Length <= 1, "The result of the fetch operation should be either null, or an array of 0 or 1 elements.");

                return apiKeys == null || apiKeys.Length == 0 ? null : new ApiKey(apiKeys.Single());
            }

            /// <summary>
            /// Fetches all API keys associated with the user.
            /// </summary>
            /// <returns>
            /// An awaitable task representing the asynchronous lookup operation. Upon completion, the result contains
            /// a collection of all API keys for that user.
            /// </returns>
            public async Task<IEnumerable<ApiKey>> FetchAllAsync()
            {
                var tcs = new TaskCompletionSource<UserApiKey[]>();
                _user.Handle.FetchAllApiKeys(_user.App.Handle, tcs);
                var apiKeys = await tcs.Task;

                return apiKeys.Select(k => new ApiKey(k)).ToArray();
            }

            /// <summary>
            /// Deletes an API key by id.
            /// </summary>
            /// <param name="id">The id of the key to delete.</param>
            /// <returns>An awaitable <see cref="Task"/> representing the asynchronous delete operation.</returns>
            public Task DeleteAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.DeleteApiKey(_user.App.Handle, id, tcs);

                return Handle404(tcs);
            }

            /// <summary>
            /// Disables an API key by id.
            /// </summary>
            /// <param name="id">The id of the key to disable.</param>
            /// <returns>An awaitable <see cref="Task"/> representing the asynchronous disable operation.</returns>
            /// <seealso cref="EnableAsync(ObjectId)"/>
            public Task DisableAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.DisableApiKey(_user.App.Handle, id, tcs);

                return Handle404(tcs, id, shouldThrow: true);
            }

            /// <summary>
            /// Enables an API key by id.
            /// </summary>
            /// <param name="id">The id of the key to enable.</param>
            /// <returns>An awaitable <see cref="Task"/> representing the asynchrounous enable operation.</returns>
            /// <seealso cref="DisableAsync(ObjectId)"/>
            public Task EnableAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.EnableApiKey(_user.App.Handle, id, tcs);

                return Handle404(tcs, id, shouldThrow: true);
            }

            private static async Task<T> Handle404<T>(TaskCompletionSource<T> tcs, ObjectId? id = null, bool shouldThrow = false)
            {
                try
                {
                    return await tcs.Task;
                }
                catch (AppException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (shouldThrow)
                    {
                        throw new AppException($"Failed to execute operation because ApiKey with Id: {id} doesn't exist.", ex.HelpLink, 404);
                    }

                    return default;
                }
            }
        }

        /// <summary>
        /// A class exposing functionality for calling remote MongoDB Realm functions.
        /// </summary>
        /// <seealso href="https://docs.mongodb.com/realm/functions/">Functions Docs</seealso>
        public class FunctionsClient
        {
            private readonly User _user;

            internal FunctionsClient(User user)
            {
                _user = user;
            }

            /// <summary>
            /// Calls a remote function with the supplied arguments.
            /// </summary>
            /// <param name="name">Name of the Realm function to call.</param>
            /// <param name="args">Arguments that will be sent to the Realm function. They have to be json serializable values.</param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> wrapping the asynchronous call function operation. The result of the task is
            /// the value returned by the function.
            /// </returns>
            public Task<BsonValue> CallAsync(string name, params object[] args) => CallAsync<BsonValue>(name, args);

            /// <summary>
            /// Calls a remote function with the supplied arguments.
            /// </summary>
            /// <remarks>
            /// The <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/">MongoDB Bson</see> library is used
            /// to decode the response. It will automatically handle most cases, but if you want to control the behavior
            /// of the deserializer, you can use the attributes in the
            /// <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/apidocs/html/N_MongoDB_Bson_Serialization_Attributes.htm">MongoDB.Bson.Serialization.Attributes</see>
            /// namespace.
            /// <br/>
            /// If you want to modify the global conventions used when deserializing the response, such as convert
            /// camelCase properties to PascalCase, you can register a
            /// <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/reference/bson/mapping/conventions/">ConventionPack</see>.
            /// </remarks>
            /// <typeparam name="T">The type that the response will be decoded to.</typeparam>
            /// <param name="name">Name of the Realm function to call.</param>
            /// <param name="args">Arguments that will be sent to the Realm function. They have to be json serializable values.</param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> wrapping the asynchronous call function operation. The result of the task is
            /// the value returned by the function decoded as <typeparamref name="T"/>.
            /// </returns>
            public async Task<T> CallAsync<T>(string name, params object[] args)
            {
                Argument.NotNullOrEmpty(name, nameof(name));

                var tcs = new TaskCompletionSource<BsonPayload>();

                _user.Handle.CallFunction(_user.App.Handle, name, args.ToNativeJson(), tcs);

                var response = await tcs.Task;

                return response.GetValue<T>();
            }
        }

        /// <summary>
        /// The Push client exposes an API to register/deregister for push notifications from a client app.
        /// </summary>
        public class PushClient
        {
            private readonly User _user;
            private readonly string _service;

            internal PushClient(User user, string service)
            {
                _user = user;
                _service = service;
            }

            /// <summary>
            /// Registers the given Firebase Cloud Messaging registration token with the user's device on MongoDB Realm.
            /// </summary>
            /// <param name="token">The FCM registration token.</param>
            /// <returns>
            /// An awaitable <see cref="Task"/> representing the remote operation. Successful completion indicates that the registration token was registered
            /// by the MongoDB Realm server and this device can now receive push notifications.
            /// </returns>
            public Task RegisterDeviceAsync(string token)
            {
                Argument.NotNullOrEmpty(token, nameof(token));
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.RegisterPushToken(_user.App.Handle, _service, token, tcs);

                return tcs.Task;
            }

            /// <summary>
            /// Deregister the user's device from Firebase Cloud Messaging.
            /// </summary>
            /// <returns>
            /// An awaitable <see cref="Task"/> representing the remote operation. Successful completion indicates that the devices registration token
            /// was removed from the MongoDB Realm server and it will no longer receive push notifications.
            /// </returns>
            public Task DeregisterDeviceAsync()
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.DeregisterPushToken(_user.App.Handle, _service, tcs);

                return tcs.Task;
            }
        }
    }
}