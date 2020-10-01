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
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Realms.Exceptions;
using Realms.Helpers;
using Realms.Native;

namespace Realms.Sync
{
    /// <summary>
    /// This class represents a user on the Realm Object Server. The credentials are provided by various 3rd party providers (Facebook, Google, etc.).
    /// A user can log in to the Realm Object Server, and if access is granted, it is possible to synchronize the local and the remote Realm. Moreover, synchronization is halted when the user is logged out.
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
        /// <value>A document containing the user data.</value>
        /// <seealso href="https://docs.mongodb.com/realm/users/enable-custom-user-data/"/>
        public BsonDocument CustomData
        {
            get
            {
                var serialized = Handle.GetCustomData();
                if (string.IsNullOrEmpty(serialized) || !BsonDocument.TryParse(serialized, out var doc))
                {
                    return null;
                }

                return doc;
            }
        }

        /// <summary>
        /// Gets a <see cref="ApiKeyApi"/> instance that exposes functionality about managing user API keys.
        /// </summary>
        /// <value>A <see cref="ApiKeyApi"/> instance scoped to this <see cref="User"/>.</value>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/api-key/"/>
        public ApiKeyApi ApiKeys { get; }

        /// <summary>
        /// Gets a <see cref="FunctionsApi"/> instance that exposes functionality about calling remote MongoDB Realm functions.
        /// </summary>
        /// <value>A <see cref="FunctionsApi"/> instance scoped to this <see cref="User"/>.</value>
        /// <seealso href="https://docs.mongodb.com/realm/functions/"/>
        public FunctionsApi Functions { get; }

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
            ApiKeys = new ApiKeyApi(this);
            Functions = new FunctionsApi(this);
        }

        /// <summary>
        /// Logs out the user from the Realm Object Server. Once the Object Server has confirmed the logout the user credentials will be deleted from this device.
        /// </summary>
        /// <returns>An awaitable Task, that, upon completion indicates that the user has been logged out both locally and on the server.</returns>
        public Task LogOutAsync()
        {
            Handle.LogOut();

            // V10TODO: native logout must be async
            return Task.CompletedTask;
        }

        /// <summary>
        /// Re-fetch the user's custom data from the server.
        /// </summary>
        /// <returns>
        /// An awaitable Task, that, upon completion returns the updated user custom data. The <see cref="CustomData"/>
        /// property will also be updated with the new information.
        /// </returns>
        public async Task<BsonDocument> RefreshCustomDataAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            Handle.RefreshCustomData(tcs);
            await tcs.Task;

            return CustomData;
        }

        /// <summary>
        /// Gets a <see cref="MongoClient"/> instance for accessing documents in the MongoDB.
        /// </summary>
        /// <param name="serviceName">The name of the service as configured on the server.</param>
        /// <returns>A <see cref="MongoClient"/> instance that can interact with the databases exposed in the remote service.</returns>
        public MongoClient GetMongoClient(string serviceName) => new MongoClient(this, serviceName);

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
        public class ApiKeyApi
        {
            private readonly User _user;

            internal ApiKeyApi(User user)
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
            /// A <see cref="Task{ApiKey}"/> representing the asynchronous operation. Successful completion indicates
            /// that the <see cref="ApiKey"/> has been created on the server and its <see cref="ApiKey.Value"/> can
            /// be used to create <see cref="Credentials.ApiKey(string)"/>.
            /// </returns>
            public async Task<ApiKey> CreateAsync(string name)
            {
                Argument.NotNullOrEmpty(name, nameof(name));

                var tcs = new TaskCompletionSource<UserApiKey[]>();
                _user.Handle.CreateApiKey(_user.App.AppHandle, name, tcs);
                var apiKeys = await tcs.Task;

                Debug.Assert(apiKeys.Length == 1, "The result of Create should be exactly 1 ApiKey.");

                return new ApiKey(apiKeys.Single());
            }

            /// <summary>
            /// Fetches a specific user API key by id.
            /// </summary>
            /// <param name="id">The id of the key to fetch.</param>
            /// <returns>
            /// A <see cref="Task{ApiKey}"/> representing the asynchronous lookup operation.
            /// </returns>
            public async Task<ApiKey> FetchAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<UserApiKey[]>();
                _user.Handle.FetchApiKey(_user.App.AppHandle, id, tcs);
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
                _user.Handle.FetchAllApiKeys(_user.App.AppHandle, tcs);
                var apiKeys = await tcs.Task;

                return apiKeys.Select(k => new ApiKey(k)).ToArray();
            }

            /// <summary>
            /// Deletes an API key by id.
            /// </summary>
            /// <param name="id">The id of the key to delete.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
            public Task DeleteAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.DeleteApiKey(_user.App.AppHandle, id, tcs);

                return Handle404(tcs);
            }

            /// <summary>
            /// Disables an API key by id.
            /// </summary>
            /// <param name="id">The id of the key to disable.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous disable operation.</returns>
            /// <seealso cref="EnableAsync(ObjectId)"/>
            public Task DisableAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.DisableApiKey(_user.App.AppHandle, id, tcs);

                return Handle404(tcs, id, shouldThrow: true);
            }

            /// <summary>
            /// Enables an API key by id.
            /// </summary>
            /// <param name="id">The id of the key to enable.</param>
            /// <returns>A <see cref="Task"/> representing the asynchrounous enable operation.</returns>
            /// <seealso cref="DisableAsync(ObjectId)"/>
            public Task EnableAsync(ObjectId id)
            {
                var tcs = new TaskCompletionSource<object>();
                _user.Handle.EnableApiKey(_user.App.AppHandle, id, tcs);

                return Handle404(tcs, id, shouldThrow: true);
            }

            private static async Task<T> Handle404<T>(TaskCompletionSource<T> tcs, ObjectId? id = null, bool shouldThrow = false)
            {
                try
                {
                    return await tcs.Task;
                }
                catch (AppException ex) when (ex.ErrorCode == (int)AppException.AppErrorCodes.ApiKeyNotFound)
                {
                    if (shouldThrow)
                    {
                        throw new AppException($"Failed to execute operation because ApiKey with Id: {id} doesn't exist.", 404);
                    }

                    return default;
                }
            }
        }

        /// <summary>
        /// A class exposing functionality for calling remote MongoDB Realm functions.
        /// </summary>
        /// <seealso href="https://docs.mongodb.com/realm/functions/"/>
        public class FunctionsApi
        {
            private static readonly JsonWriterSettings _jsonSettings = new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.CanonicalExtendedJson,
            };

            private readonly User _user;

            internal FunctionsApi(User user)
            {
                _user = user;
            }

            /// <summary>
            /// Calls a remote function with the supplied arguments.
            /// </summary>
            /// <param name="name">Name of the Realm function to call.</param>
            /// <param name="args">Arguments that will be sent to the Realm function. They have to be json serializable values.</param>
            /// <returns>
            /// A <see cref="Task{BsonValue}"/> wrapping the asynchronous call function operation. The result of the task is
            /// the value returned by the function.
            /// </returns>
            public async Task<BsonValue> CallAsync(string name, params object[] args)
            {
                var response = await CallCoreAsync(name, args);
                return response.GetValue();
            }

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
            /// camelCase properties to PascalCase, you can regiseter a
            /// <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/reference/bson/mapping/conventions/">ConventionPack</see>.
            /// </remarks>
            /// <typeparam name="T">The type that the response will be decoded to.</typeparam>
            /// <param name="name">Name of the Realm function to call.</param>
            /// <param name="args">Arguments that will be sent to the Realm function. They have to be json serializable values.</param>
            /// <returns>
            /// A <see cref="Task{T}"/> wrapping the asynchronous call function operation. The result of the task is
            /// the value returned by the function decoded as <typeparamref name="T"/>.
            /// </returns>
            public async Task<T> CallAsync<T>(string name, params object[] args)
            {
                var response = await CallCoreAsync(name, args);
                return response.GetValue<T>();
            }

            private Task<BsonPayload> CallCoreAsync(string name, params object[] args)
            {
                Argument.NotNullOrEmpty(name, nameof(name));

                var tcs = new TaskCompletionSource<BsonPayload>();

                _user.Handle.CallFunction(_user.App.AppHandle, name, args.ToJson(_jsonSettings), tcs);

                return tcs.Task;
            }
        }
    }
}