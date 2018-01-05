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
using System.Linq;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Helpers;

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

                if (SyncUserHandle.TryGetCurrentUser(out var userHandle))
                {
                    return new User(userHandle);
                }

                return null;
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
            Argument.NotNull(credentials, nameof(credentials));
            Argument.NotNull(serverUrl, nameof(serverUrl));
            Argument.Ensure(serverUrl.Scheme.StartsWith("http"), "Unexpected protocol for login url. Expected http:// or https://.", nameof(serverUrl));

            SharedRealmHandleExtensions.DoInitialFileSystemConfiguration();

            if (credentials.IdentityProvider == Credentials.Provider.AdminToken)
            {
                return new User(SyncUserHandle.GetAdminTokenUser(serverUrl.AbsoluteUri, credentials.Token));
            }

            if (credentials.UserInfo.TryGetValue(Credentials.Keys.IsAnonymous, out var isAnonymous) &&
                (bool)isAnonymous)
            {
                var existing = AllLoggedIn.FirstOrDefault(u => u.ServerUri == serverUrl && u.Identity.StartsWith("anonymous_"));
                if (existing != null)
                {
                    return existing;
                }
            }

            var result = await AuthenticationHelper.LoginAsync(credentials, serverUrl);
            var handle = SyncUserHandle.GetSyncUser(result.UserId, serverUrl.AbsoluteUri, result.RefreshToken, result.IsAdmin);
            return new User(handle);
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
        /// By default <see cref="User"/> objects are persisted and are additionally protected with an encryption key stored
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

        /// <summary>
        /// Gets a logged in user with a specified identity.
        /// </summary>
        /// <returns>A user instance if a logged in user with that id exists, <c>null</c> otherwise.</returns>
        /// <param name="identity">The identity of the user.</param>
        /// <param name="serverUrl">The URI of the server that the user is authenticated against.</param>
        public static User GetLoggedInUser(string identity, Uri serverUrl)
        {
            SharedRealmHandleExtensions.DoInitialFileSystemConfiguration();

            if (SyncUserHandle.TryGetLoggedInUser(identity, serverUrl.AbsoluteUri, out var userHandle))
            {
                return new User(userHandle);
            }

            return null;
        }

        #endregion static

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
        /// Gets a value indicating whether this <see cref="User"/> is a Realm Object Server administrator user.
        /// </summary>
        /// <value><c>true</c> if the user is admin; otherwise, <c>false</c>.</value>
        public bool IsAdmin => Handle.GetIsAdmin();

        /// <summary>
        /// Gets the current state of the user.
        /// </summary>
        /// <value>A value indicating whether the user is active, logged out, or an error has occurred.</value>
        public UserState State => Handle.GetState();

        internal readonly SyncUserHandle Handle;

        internal User(SyncUserHandle handle)
        {
            Handle = handle;
            _permissionRealm = new Lazy<Realm>(() => GetSpecialPurposeRealm("__permission", typeof(Permission)));
            _managementRealm = new Lazy<Realm>(() => GetSpecialPurposeRealm("__management", typeof(PermissionChange), typeof(PermissionOffer), typeof(PermissionOfferResponse)));
        }

        internal static User Create(IntPtr userPtr)
        {
            var userHandle = new SyncUserHandle(userPtr);
            return new User(userHandle);
        }

        internal Uri GetUriForRealm(string path)
        {
            var uriBuilder = new UriBuilder(ServerUri)
            {
                Path = path
            };

            uriBuilder.Scheme = uriBuilder.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "realms" : "realm";

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Logs out the user from the Realm Object Server. Once the Object Server has confirmed the logout the user credentials will be deleted from this device.
        /// </summary>
        [Obsolete("Use LogOutAsync instead")]
        public void LogOut()
        {
            LogOutAsync();
        }

        /// <summary>
        /// Logs out the user from the Realm Object Server. Once the Object Server has confirmed the logout the user credentials will be deleted from this device.
        /// </summary>
        /// <returns>An awaitable Task, that, upon completion indicates that the user has been logged out both locally and on the server.</returns>
        public async Task LogOutAsync()
        {
            var uri = ServerUri;
            var refreshToken = RefreshToken;
            Handle.LogOut();

            try
            {
                await AuthenticationHelper.LogOutAsync(uri, refreshToken);
            }
            catch (Exception ex)
            {
                ErrorMessages.OutputError($"An error has occurred while logging the user out: {ex.Message}. The user is still logged out locally, but their refresh token may not have been revoked yet.");
            }
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="newPassword">The user's new password.</param>
        /// <remarks>
        /// Changing a user's password using an authentication server that doesn't
        /// use HTTPS is a major security flaw, and should only be done while testing.
        /// </remarks>
        /// <returns>An awaitable task that, when successful, indicates that the password has changed.</returns>
        public Task ChangePasswordAsync(string newPassword)
        {
            Argument.Ensure<InvalidOperationException>(State == UserState.Active, "Password may be changed only by active users.");
            Argument.NotNullOrEmpty(newPassword, nameof(newPassword));

            return AuthenticationHelper.ChangePasswordAsync(this, newPassword);
        }

        /// <summary>
        /// Changes another user's password.
        /// </summary>
        /// <param name="userId">The <see cref="Identity"/> of the user we want to change the password for.</param>
        /// <param name="newPassword">The user's new password.</param>
        /// <remarks>
        /// This user needs admin privilege in order to change someone else's password.
        /// <br/>
        /// Changing a user's password using an authentication server that doesn't
        /// use HTTPS is a major security flaw, and should only be done while testing.
        /// </remarks>
        /// <returns>An awaitable task that, when successful, indicates that the password has changed.</returns>
        public Task ChangePasswordAsync(string userId, string newPassword)
        {
            Argument.Ensure<InvalidOperationException>(State == UserState.Active, "Password may be changed only by active users.");
            Argument.Ensure<InvalidOperationException>(IsAdmin, "Other users' passwords may be changed only by admin users.");
            Argument.NotNullOrEmpty(userId, nameof(userId));
            Argument.NotNullOrEmpty(newPassword, nameof(newPassword));

            return AuthenticationHelper.ChangePasswordAsync(this, newPassword, userId);
        }

        /// <summary>
        /// Looks up user's information by provider id. This is useful when you know the id of a user in a provider's system,
        /// e.g. on Facebook and want to find the associated Realm user's Id.
        /// </summary>
        /// <param name="provider">The provider that the user has signed up with.</param>
        /// <param name="providerUserIdentity">The id of the user in the provider's system.</param>
        /// <remarks>
        /// This user needs admin privilege in order to look up other users by provider id.
        /// <br/>
        /// The exact names of built-in providers can be found in <see cref="Credentials.Provider"/>.
        /// </remarks>
        /// <returns>
        /// A <see cref="UserInfo"/>, containing information about the User's Identity in Realm's authentication system,
        /// or <c>null</c> if a user has not been found.
        /// </returns>
        public Task<UserInfo> RetrieveInfoForUserAsync(string provider, string providerUserIdentity)
        {
            Argument.Ensure<InvalidOperationException>(State == UserState.Active, "Users may be looked up only by active users.");
            Argument.Ensure<InvalidOperationException>(IsAdmin, "Users may be looked up only by admin users.");
            Argument.NotNullOrEmpty(provider, nameof(provider));
            Argument.NotNullOrEmpty(providerUserIdentity, nameof(providerUserIdentity));

            return AuthenticationHelper.RetrieveInfoForUserAsync(this, provider, providerUserIdentity);
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

        #region Permissions

        private readonly Lazy<Realm> _permissionRealm;
        private readonly Lazy<Realm> _managementRealm;

        internal Realm PermissionRealm => _permissionRealm.Value;

        internal Realm ManagementRealm => _managementRealm.Value;

        /// <summary>
        /// Asynchronously retrieve all permissions associated with the user calling this method.
        /// </summary>
        /// <returns>
        /// A queryable collection of <see cref="Permission"/> objects that provide detailed information
        /// regarding the granted access.
        /// </returns>
        /// <param name="recipient">The optional recipient of the permission.</param>
        /// <param name="millisecondTimeout">
        /// The timeout in milliseconds for downloading server changes. If the download times out, no error will be thrown
        /// and instead the latest local state will be returned. If set to 0, the latest state will be returned immediately.
        /// </param>
        /// <remarks>
        /// The collection is a live query, similar to what you would get by calling <see cref="Realm.All"/>, so the same
        /// features and limitations apply - you can query and subscribe for notifications, but you cannot pass it between
        /// threads.
        /// </remarks>
        public async Task<IQueryable<Permission>> GetGrantedPermissionsAsync(Recipient recipient = Recipient.Any, int millisecondTimeout = 2000)
        {
            // TODO: this should eventually use GetInstanceAsync

            if (millisecondTimeout > 0)
            {
                var session = PermissionRealm.GetSession();
                try
                {
                    await session.WaitForDownloadAsync().Timeout(millisecondTimeout);
                }
                catch (TimeoutException)
                {
                }
                finally
                {
                    session.CloseHandle();
                }
            }

            var result = PermissionRealm.All<Permission>()
                                        .Where(p => !p.Path.EndsWith("/__permission", StringComparison.OrdinalIgnoreCase) &&
                                                    !p.Path.EndsWith("/__management", StringComparison.OrdinalIgnoreCase));

            var id = Identity;
            switch (recipient)
            {
                case Recipient.CurrentUser:
                    result = result.Where(p => p.UserId == id);
                    break;

                case Recipient.OtherUser:
                    result = result.Where(p => p.UserId != id);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Changes the permissions of a Realm.
        /// </summary>
        /// <returns>
        /// An awaitable task, that, upon completion, indicates that the permissions have been successfully applied by the server.
        /// </returns>
        /// <param name="condition">A <see cref="PermissionCondition"/> that will be used to match existing users against.</param>
        /// <param name="realmUrl">The Realm URL whose permissions settings should be changed. Use <c>*</c> to change the permissions of all Realms managed by this <see cref="User"/>.</param>
        /// <param name="accessLevel">
        /// The access level to grant matching users. Note that the access level setting is absolute, i.e. it may revoke permissions for users that
        /// previously had a higher access level. To revoke all permissions, use <see cref="AccessLevel.None" />
        /// </param>
        public Task ApplyPermissionsAsync(PermissionCondition condition, string realmUrl, AccessLevel accessLevel)
        {
            if (string.IsNullOrEmpty(realmUrl))
            {
                throw new ArgumentNullException(nameof(realmUrl));
            }

            var mayRead = accessLevel >= AccessLevel.Read;
            var mayWrite = accessLevel >= AccessLevel.Write;
            var mayManage = accessLevel >= AccessLevel.Admin;

            PermissionChange change = null;

            if (condition is UserIdCondition userIdCondition)
            {
                change = new PermissionChange(userIdCondition.UserId, realmUrl, mayRead, mayWrite, mayManage);
            }
            else if (condition is KeyValueCondition keyValueCondition)
            {
                change = new PermissionChange(keyValueCondition.Key, keyValueCondition.Value, realmUrl, mayRead, mayWrite, mayManage);
            }
            else
            {
                throw new NotSupportedException("Invalid PermissionCondition.");
            }

            return WriteToManagementRealmAsync(change);
        }

        /// <summary>
        /// Generates a token that can be used for sharing a Realm.
        /// </summary>
        /// <returns>
        /// A token that can be shared with another user, e.g. via email or message and then consumed by
        /// <see cref="AcceptPermissionOfferAsync"/> to obtain permissions to a Realm.</returns>
        /// <param name="realmUrl">The Realm URL whose permissions settings should be changed. Use <c>*</c> to change the permissions of all Realms managed by this <see cref="User"/>.</param>
        /// <param name="accessLevel">
        /// The access level to grant matching users. Note that the access level setting is absolute, i.e. it may revoke permissions for users that
        /// previously had a higher access level. To revoke all permissions, use <see cref="AccessLevel.None" />
        /// </param>
        /// <param name="expiresAt">Optional expiration date of the offer. If set to <c>null</c>, the offer doesn't expire.</param>
        public async Task<string> OfferPermissionsAsync(string realmUrl, AccessLevel accessLevel, DateTimeOffset? expiresAt = null)
        {
            if (string.IsNullOrEmpty(realmUrl))
            {
                throw new ArgumentNullException(nameof(realmUrl));
            }

            if (expiresAt < DateTimeOffset.UtcNow)
            {
                throw new ArgumentException("The expiration date may not be in the past", nameof(expiresAt));
            }

            if (accessLevel == AccessLevel.None)
            {
                throw new ArgumentException("The access level may not be None", nameof(accessLevel));
            }

            var offer = new PermissionOffer(realmUrl,
                                            accessLevel >= AccessLevel.Read,
                                            accessLevel >= AccessLevel.Write,
                                            accessLevel >= AccessLevel.Admin,
                                            expiresAt);

            await WriteToManagementRealmAsync(offer);
            return offer.Token;
        }

        /// <summary>
        /// Consumes a token generated by <see cref="OfferPermissionsAsync"/> to obtain permissions to a shared Realm.
        /// </summary>
        /// <returns>The relative url of the Realm that the token has granted permissions to.</returns>
        /// <param name="offerToken">The token, generated by <see cref="OfferPermissionsAsync"/>.</param>
        public async Task<string> AcceptPermissionOfferAsync(string offerToken)
        {
            if (string.IsNullOrEmpty(offerToken))
            {
                throw new ArgumentNullException(nameof(offerToken));
            }

            var permissionResponse = new PermissionOfferResponse(offerToken);
            await WriteToManagementRealmAsync(permissionResponse);
            return permissionResponse.RealmUrl;
        }

        /// <summary>
        /// Invalidates a permission offer.
        /// </summary>
        /// <remarks>
        /// Invalidating an offer prevents new users from consuming its token. It doesn't revoke any permissions that have
        /// already been granted.
        /// </remarks>
        /// <returns>
        /// An awaitable task, that, upon completion, indicates that the offer has been successfully invalidated by the server.
        /// </returns>
        /// <param name="offer">The offer that should be invalidated.</param>
        public async Task InvalidateOfferAsync(PermissionOffer offer)
        {
            var tcs = new TaskCompletionSource<object>();

            ManagementRealm.Write(() => offer.ExpiresAt = DateTimeOffset.UtcNow);

            var session = ManagementRealm.GetSession();
            try
            {
                await session.WaitForUploadAsync();
            }
            finally
            {
                session.CloseHandle();
            }
        }

        /// <summary>
        /// Gets the permission offers that this user has created by invoking <see cref="OfferPermissionsAsync"/>.
        /// </summary>
        /// <returns>A queryable collection of <see cref="PermissionOffer"/> objects.</returns>
        /// <param name="statuses">Optional statuses to filter by. If empty, will return objects with any status.</param>
        public IQueryable<PermissionOffer> GetPermissionOffers(params ManagementObjectStatus[] statuses)
                    => GetPermissionObjects<PermissionOffer>(statuses);

        /// <summary>
        /// Gets the permission offer responses that this user has created by invoking <see cref="AcceptPermissionOfferAsync"/>.
        /// </summary>
        /// <returns>A queryable collection of <see cref="PermissionOfferResponse"/> objects.</returns>
        /// <param name="statuses">Optional statuses to filter by. If empty, will return objects with any status.</param>
        public IQueryable<PermissionOfferResponse> GetPermissionOfferResponses(params ManagementObjectStatus[] statuses)
                    => GetPermissionObjects<PermissionOfferResponse>(statuses);

        /// <summary>
        /// Gets the permission changes that this user has created by invoking <see cref="ApplyPermissionsAsync"/>.
        /// </summary>
        /// <returns>A queryable collection of <see cref="PermissionChange"/> objects.</returns>
        /// <param name="statuses">Optional statuses to filter by. If empty, will return objects with any status.</param>
        public IQueryable<PermissionChange> GetPermissionChanges(params ManagementObjectStatus[] statuses)
                    => GetPermissionObjects<PermissionChange>(statuses);

        private IQueryable<T> GetPermissionObjects<T>(ManagementObjectStatus[] statuses) where T : RealmObject, IStatusObject
        {
            var result = ManagementRealm.All<T>();
            int? successCode = 0;
            int? notProcessedCode = null;

            if (statuses.Any())
            {
                if (!statuses.Contains(ManagementObjectStatus.Error))
                {
                    result = result.Where(p => p.StatusCode == successCode || p.StatusCode == notProcessedCode);
                }

                if (!statuses.Contains(ManagementObjectStatus.NotProcessed))
                {
                    result = result.Where(p => p.StatusCode != notProcessedCode);
                }

                if (!statuses.Contains(ManagementObjectStatus.Success))
                {
                    result = result.Where(p => p.StatusCode != successCode);
                }
            }

            return result;
        }

        private Realm GetSpecialPurposeRealm(string path, params Type[] objectClasses)
        {
            var configuration = new SyncConfiguration(this, GetUriForRealm($"/~/{path}"))
            {
                ObjectClasses = objectClasses
            };

            return Realm.GetInstance(configuration);
        }

        private Task WriteToManagementRealmAsync<T>(T permissionObject) where T : RealmObject, IPermissionObject
        {
            ManagementRealm.Write(() => ManagementRealm.Add(permissionObject));
            return permissionObject.WaitForProcessingAsync();
        }

        #endregion Permissions
    }
}