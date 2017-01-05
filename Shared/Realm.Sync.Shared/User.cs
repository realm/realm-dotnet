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
using System.IO;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// This class represents a user on the Realm Object Server. The credentials are provided by various 3rd party providers (Facebook, Google, etc.).
    /// A user can log in to the Realm Object Server, and if access is granted, it is possible to synchronize the local and the remote Realm. Moreover, synchronization is halted when the user is logged out.
    /// It is possible to persist a user. By retrieving a user, there is no need to log in to the 3rd party provider again. Persisting a user between sessions, the user's credentials are stored locally on the device, and should be treated as sensitive data.
    /// </summary>
    public class User : IEquatable<User>
    {
        #region static

        /// <summary>
        /// Gets the currently logged-in user. If none exists, null is returned. 
        /// If more than one user is currently logged in, an exception is thrown.
        /// </summary>
        /// <value>Valid user or <c>null</c> to indicate nobody logged in.</value>
        /// <exception cref="RealmException">Thrown if there are more than one users logged in.</exception>
        public static User Current
        {
            get
            {
                SharedRealmHandleExtensions.DoInitialFileSystemConfiguration();

                var handle = SyncUserHandle.GetCurrentUser();
                if (handle == null)
                {
                    return null;
                }

                return new User(handle);
            }
        }

        /// <summary>
        /// Gets all currently logged in users.
        /// </summary>
        /// <value>An array of valid logged in users.</value>
        public static User[] AllLoggedIn
        {
            get
            {
                SharedRealmHandleExtensions.DoInitialFileSystemConfiguration();
                return SyncUserHandle.GetAllLoggedInUsers()
                                     .Select(handle => new User(handle))
                                     .ToArray();
            }
        }

        /// <summary>
        /// Logs the user in to the Realm Object Server.
        /// </summary>
        /// <param name="credentials">The credentials to use for authentication.</param>
        /// <param name="serverUrl">The URI of the server that the user is authenticated against.</param>
        /// <returns>An awaitable Task, that, upon completion, contains the logged in user.</returns>
        public static async Task<User> LoginAsync(Credentials credentials, Uri serverUrl)
        {
            SharedRealmHandleExtensions.DoInitialFileSystemConfiguration();

            if (credentials.IdentityProvider == Credentials.Providers.AccessToken)
            {
                var identity = (string)credentials.UserInfo[Credentials.Keys.Identity];
                var isAdmin = (bool)credentials.UserInfo[Credentials.Keys.IsAdmin];
                return new User(SyncUserHandle.GetSyncUser(identity, credentials.Token, serverUrl?.AbsoluteUri, isAdmin));
            }

            var result = await MakeAuthRequestAsync(serverUrl, credentials.ToJson(), TimeSpan.FromSeconds(30)).ConfigureAwait(continueOnCapturedContext: false);
            var refresh_token = result["refresh_token"];

            return new User(SyncUserHandle.GetSyncUser((string)refresh_token["token_data"]["identity"], (string)refresh_token["token"], serverUrl.AbsoluteUri, false));
        }

        /// <summary>
        /// Configures user persistence. If you need to call this, be sure to do so before accessing any other Realm API.
        /// </summary>
        /// <param name="mode">The persistence mode.</param>
        /// <param name="encryptionKey">The key to encrypt the persistent user store with.</param>
        /// <param name="resetOnError">If set to <c>true</c> reset the persistent user store on error.</param>
        /// <remarks>
        /// Users are persisted in a realm file within the application's sandbox.
        /// <para>
        /// By default <see cref="User"/> objects are persisted and are additionaly protected with an encryption key stored
        /// in the iOS Keychain when running on an iOS device (but not on a Simulator).
        /// On Android users are persisted in plaintext, because the AndroidKeyStore API is only supported on API level 18 and up.
        /// You might want to provide your own encryption key on Android or disable persistence for security reasons.
        /// </para>
        /// </remarks>
        public static void ConfigurePersistence(UserPersistenceMode mode, byte[] encryptionKey = null, bool resetOnError = false)
        {
            if (mode == UserPersistenceMode.Encrypted && encryptionKey != null && encryptionKey.Length != 64)
            {
                throw new ArgumentException("The encryption key must be 64 bytes long", nameof(encryptionKey));
            }

            SharedRealmHandleExtensions.ConfigureFileSystem(mode, encryptionKey, resetOnError);
        }

        #endregion

        private const int ErrorContentTruncationLimit = 256 * 1024;

        private static readonly MediaTypeHeaderValue _applicationJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private static readonly MediaTypeHeaderValue _applicationProblemJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/problem+json; charset=utf-8");

        /// <summary>
        /// Gets this user's refresh token. This is the user's credential for accessing the Realm Object Server and should be treated as sensitive data.
        /// </summary>
        /// <value>A unique string that can be used for refreshing the user's credentials.</value>
        public string RefreshToken => Handle.GetRefreshToken();

        /// <summary>
        /// Gets the identity of this user on the Realm Object Server. The identity is a guaranteed to be unique among all users on the Realm Object Server.
        /// </summary>
        /// <value>A string that uniquely identifies that user in Realm Object Server.</value>
        public string Identity => Handle.GetIdentity();

        /// <summary>
        /// Gets the server URI that was used for authentication.
        /// </summary>
        /// <value>The <see cref="Uri"/> used to connect to the authentication service.</value>
        public Uri ServerUri
        {
            get
            {
                var serverUrl = Handle.GetServerUrl();
                if (string.IsNullOrEmpty(serverUrl))
                {
                    return null;
                }

                return new Uri(serverUrl);
            }
        }

        /// <summary>
        /// Gets the current state of the user.
        /// </summary>
        /// <value>A value indicating whether the user is active, logged out, or an error has occurred.</value>
        public UserState State => Handle.GetState();

        internal readonly SyncUserHandle Handle;

        internal User(SyncUserHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Logs out the user from the Realm Object Server. Once the Object Server has confirmed the logout the user credentials will be deleted from this device.
        /// </summary>
        public void LogOut()
        {
            Handle.LogOut();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        /// <inheritdoc />
        public bool Equals(User other)
        {
            return Identity.Equals(other?.Identity);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        // returns a tuple of access token and resolved realm path
        internal async Task<Tuple<string, string>> RefreshAccessToken(string path)
        {
            var json = new JsonObject
            {
                ["data"] = RefreshToken,
                ["path"] = path,
                ["provider"] = "realm"
            };

            var result = await MakeAuthRequestAsync(ServerUri, json, TimeSpan.FromSeconds(30)).ConfigureAwait(continueOnCapturedContext: false);
            var access_token = result["access_token"];
            return Tuple.Create<string, string>((string)access_token["token"], (string)access_token["token_data"]["path"]);
        }

        private static async Task<JsonValue> MakeAuthRequestAsync(Uri serverUri, JsonObject body, TimeSpan timeout)
        {
            body["app_id"] = string.Empty; // FIXME

            string requestBody;
            using (var writer = new StringWriter())
            {
                body.Save(writer);
                requestBody = writer.ToString();
            }

            using (var client = new HttpClient { Timeout = timeout })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(serverUri, "auth"));
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Accept.ParseAdd(_applicationJsonUtf8MediaType.MediaType);
                request.Headers.Accept.ParseAdd(_applicationProblemJsonUtf8MediaType.MediaType);

                var response = await client.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
                if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.Equals(_applicationJsonUtf8MediaType))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                    {
                        return JsonValue.Load(stream);
                    }
                }

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (response.Content.Headers.ContentType.Equals(_applicationProblemJsonUtf8MediaType))
                    {
                        var problem = JsonValue.Load(stream);

                        var code = ErrorCodeHelper.GetErrorCode((int)problem["code"]) ?? ErrorCode.Unknown;

                        throw new AuthenticationException(code, response.StatusCode, response.ReasonPhrase, problem.ToString(), (string)problem["title"]);
                    }

                    var content = ReadContent(stream, ErrorContentTruncationLimit, $"Response too long. Truncated to first {ErrorContentTruncationLimit} characters:{Environment.NewLine}");
                    throw new HttpException(response.StatusCode, response.ReasonPhrase, content);
                }
            }
        }

        private static string ReadContent(Stream stream, int maxNumberOfCharacters, string prefixIfExceeded)
        {
            using (var sr = new StreamReader(stream))
            {
                var sb = new StringBuilder();

                int current;
                while ((current = sr.Read()) > 0)
                {
                    sb.Append((char)current);
                    if (sb.Length > maxNumberOfCharacters - 1)
                    {
                        sb.Insert(0, prefixIfExceeded);
                        break;
                    }
                }

                return sb.ToString();
            }
        }
    }
}