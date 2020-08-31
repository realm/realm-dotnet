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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The User instance will own its handle.")]
        public static User Current
        {
            get
            {
                SharedRealmHandleExtensions.DoInitialMetadataConfiguration();

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
                SharedRealmHandleExtensions.DoInitialMetadataConfiguration();

                return SyncUserHandle.GetAllLoggedInUsers()
                                     .Select(handle => new User(handle))
                                     .ToArray();
            }
        }

        /// <summary>
        /// Logs the user in to the Realm Object Server.
        /// </summary>
        /// <param name="credentials">The credentials to use for authentication.</param>
        /// <param name="serverUri">The URI of the server that the user is authenticated against.</param>
        /// <returns>An awaitable Task, that, upon completion, contains the logged in user.</returns>
        public static async Task<User> LoginAsync(Credentials credentials, Uri serverUri)
        {
            Argument.NotNull(credentials, nameof(credentials));
            Argument.NotNull(serverUri, nameof(serverUri));
            Argument.Ensure(serverUri.Scheme.StartsWith("http"), "Unexpected protocol for login url. Expected http:// or https://.", nameof(serverUri));

            SharedRealmHandleExtensions.DoInitialMetadataConfiguration();

            if (credentials.IdentityProvider == Credentials.Provider.AdminToken)
            {
                return new User(SyncUserHandle.GetAdminTokenUser(serverUri.AbsoluteUri, credentials.Token));
            }

            if (credentials.IdentityProvider == Credentials.Provider.CustomRefreshToken)
            {
                var userId = (string)credentials.UserInfo[Credentials.Keys.Identity];
                var isAdmin = (bool)credentials.UserInfo[Credentials.Keys.IsAdmin];
                return new User(SyncUserHandle.GetSyncUser(userId, serverUri.AbsoluteUri, credentials.Token, isAdmin));
            }

            var result = await AuthenticationHelper.LoginAsync(credentials, serverUri);
            var handle = SyncUserHandle.GetSyncUser(result.UserId, serverUri.AbsoluteUri, result.RefreshToken, result.IsAdmin);
            return new User(handle);
        }

        /// <summary>
        /// Gets a logged in user with a specified identity.
        /// </summary>
        /// <returns>A user instance if a logged in user with that id exists, <c>null</c> otherwise.</returns>
        /// <param name="identity">The identity of the user.</param>
        /// <param name="serverUri">The URI of the server that the user is authenticated against.</param>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The User instance will own its handle.")]
        public static User GetLoggedInUser(string identity, Uri serverUri)
        {
            Argument.NotNull(identity, nameof(identity));
            Argument.NotNull(serverUri, nameof(serverUri));

            SharedRealmHandleExtensions.DoInitialMetadataConfiguration();

            if (SyncUserHandle.TryGetLoggedInUser(identity, serverUri.AbsoluteUri, out var userHandle))
            {
                return new User(userHandle);
            }

            return null;
        }

        #endregion static

        /// <summary>
        /// Gets or sets this user's refresh token. This is the user's credential for accessing the Realm Object Server and should be treated as sensitive data.
        /// Setting the refresh token is only supported for users authenticated with <see cref="Credentials.CustomRefreshToken"/>.
        /// </summary>
        /// <value>A unique string that can be used for refreshing the user's credentials.</value>
        public string RefreshToken
        {
            get => Handle.GetRefreshToken();
            set
            {
                Argument.NotNull(value, nameof(value));
                Handle.SetRefreshToken(value);
            }
        }

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
        }

        internal static User Create(IntPtr userPtr)
        {
            var userHandle = new SyncUserHandle(userPtr);
            return new User(userHandle);
        }

        internal Uri GetUriForRealm(string path)
        {
            if (!path.StartsWith("/"))
            {
                path = $"/{path}";
            }

            return GetUriForRealm(new Uri(path, UriKind.Relative));
        }

        internal Uri GetUriForRealm(Uri uri)
        {
            Argument.Ensure(!uri.IsAbsoluteUri, "The passed Uri must be relative", nameof(uri));
            var uriBuilder = new UriBuilder(new Uri(ServerUri, uri));
            uriBuilder.Scheme = uriBuilder.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "realms" : "realm";
            return uriBuilder.Uri;
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

        /// <summary>
        /// Request a password reset email to be sent to a user's email. This method requires internet connection
        /// and will not throw an exception, even if the email doesn't belong to a Realm Object Server user.
        /// </summary>
        /// <remarks>
        /// This can only be used for users who authenticated with <see cref="Credentials.UsernamePassword"/>
        /// and passed a valid email address as a username.
        /// </remarks>
        /// <param name="serverUri">The URI of the server that the user is authenticated against.</param>
        /// <param name="email">The email that corresponds to the user's username.</param>
        /// <returns>An awaitable task that, upon completion, indicates that the request has been sent.</returns>
        public static Task RequestPasswordResetAsync(Uri serverUri, string email)
        {
            Argument.NotNull(serverUri, nameof(serverUri));
            Argument.NotNullOrEmpty(email, nameof(email));

            return AuthenticationHelper.UpdateAccountAsync(serverUri, "reset_password", email);
        }

        /// <summary>
        /// Complete the password reset flow by using the reset token sent to the user's email as a one-time
        /// authorization token to change the password.
        /// </summary>
        /// <remarks>
        /// By default, the link that will be sent to the user's email will redirect to a webpage where
        /// they can enter their new password. If you wish to provide a native UX, you may wish to modify
        /// the url to use deep linking to open the app, extract the token, and navigate to a view that
        /// allows them to change their password within the app.
        /// </remarks>
        /// <param name="serverUri">The URI of the server that the user is authenticated against.</param>
        /// <param name="token">The token that was sent to the user's email address.</param>
        /// <param name="newPassword">The user's new password.</param>
        /// <returns>An awaitable task that, when successful, indicates that the password has changed.</returns>
        public static Task CompletePasswordResetAsync(Uri serverUri, string token, string newPassword)
        {
            Argument.NotNull(serverUri, nameof(serverUri));
            Argument.NotNullOrEmpty(token, nameof(token));
            Argument.NotNullOrEmpty(newPassword, nameof(newPassword));

            var data = new Dictionary<string, string>
            {
                ["token"] = token,
                ["new_password"] = newPassword
            };
            return AuthenticationHelper.UpdateAccountAsync(serverUri, "complete_reset", data: data);
        }

        /// <summary>
        /// Request an email confirmation email to be sent to a user's email. This method requires internet connection
        /// and will not throw an exception, even if the email doesn't belong to a Realm Object Server user.
        /// </summary>
        /// <param name="serverUri">The URI of the server that the user is authenticated against.</param>
        /// <param name="email">The email that corresponds to the user's username.</param>
        /// <returns>An awaitable task that, upon completion, indicates that the request has been sent.</returns>
        public static Task RequestEmailConfirmationAsync(Uri serverUri, string email)
        {
            Argument.NotNull(serverUri, nameof(serverUri));
            Argument.NotNullOrEmpty(email, nameof(email));

            return AuthenticationHelper.UpdateAccountAsync(serverUri, "request_email_confirmation", email);
        }

        /// <summary>
        /// Complete the password reset flow by using the confirmation token sent to the user's email as a one-time
        /// authorization token to confirm their email.
        /// </summary>
        /// <remarks>
        /// By default, the link that will be sent to the user's email will redirect to a webpage where
        /// they'll see a generic "Thank you for confirming" text. If you wish to provide a native UX, you
        /// may wish to modify the url to use deep linking to open the app, extract the token, and inform them
        /// that their email has been confirmed.
        /// </remarks>
        /// <param name="serverUri">The URI of the server that the user is authenticated against.</param>
        /// <param name="token">The token that was sent to the user's email address.</param>
        /// <returns>An awaitable task that, when successful, indicates that the email has been confirmed.</returns>
        public static Task ConfirmEmailAsync(Uri serverUri, string token)
        {
            Argument.NotNull(serverUri, nameof(serverUri));
            Argument.NotNullOrEmpty(token, nameof(token));

            var data = new Dictionary<string, string>
            {
                ["token"] = token
            };

            return AuthenticationHelper.UpdateAccountAsync(serverUri, "confirm_email", data: data);
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

        /// <summary>
        /// Asynchronously retrieve all permissions associated with the user calling this method.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="PathPermission"/> objects that provide detailed information
        /// regarding the granted access.
        /// </returns>
        /// <param name="recipient">The optional recipient of the permission.</param>
        public async Task<IEnumerable<PathPermission>> GetGrantedPermissionsAsync(Recipient recipient = Recipient.Any)
        {
            var result = await MakePermissionRequestAsync(HttpMethod.Get, $"permissions?recipient={recipient}");
            return result["permissions"].ToObject<IEnumerable<PathPermission>>();
        }

        /// <summary>
        /// Changes the permissions of a Realm.
        /// </summary>
        /// <returns>
        /// An awaitable task, that, upon completion, indicates that the permissions have been successfully applied by the server.
        /// </returns>
        /// <param name="condition">A <see cref="PermissionCondition"/> that will be used to match existing users against.</param>
        /// <param name="realmPath">The Realm path whose permissions settings should be changed. Use <c>*</c> to change the permissions of all Realms managed by this <see cref="User"/>.</param>
        /// <param name="accessLevel">
        /// The access level to grant matching users. Note that the access level setting is absolute, i.e. it may revoke permissions for users that
        /// previously had a higher access level. To revoke all permissions, use <see cref="AccessLevel.None" />.
        /// </param>
        public async Task ApplyPermissionsAsync(PermissionCondition condition, string realmPath, AccessLevel accessLevel)
        {
            Argument.NotNull(condition, nameof(condition));

            if (string.IsNullOrEmpty(realmPath))
            {
                throw new ArgumentNullException(nameof(realmPath));
            }

            var payload = new Dictionary<string, object>
            {
                ["condition"] = condition.ToJsonObject(),
                ["realmPath"] = realmPath,
                ["accessLevel"] = accessLevel.ToString().ToLower()
            };
            await MakePermissionRequestAsync(HttpMethod.Post, "permissions/apply", payload);
        }

        /// <summary>
        /// Generates a token that can be used for sharing a Realm.
        /// </summary>
        /// <returns>
        /// A token that can be shared with another user, e.g. via email or message and then consumed by
        /// <see cref="AcceptPermissionOfferAsync"/> to obtain permissions to a Realm.</returns>
        /// <param name="realmPath">The Realm URL whose permissions settings should be changed. Use <c>*</c> to change the permissions of all Realms managed by this <see cref="User"/>.</param>
        /// <param name="accessLevel">
        /// The access level to grant matching users. Note that the access level setting is absolute, i.e. it may revoke permissions for users that
        /// previously had a higher access level. To revoke all permissions, use <see cref="AccessLevel.None" />.
        /// </param>
        /// <param name="expiresAt">Optional expiration date of the offer. If set to <c>null</c>, the offer doesn't expire.</param>
        public async Task<string> OfferPermissionsAsync(string realmPath, AccessLevel accessLevel, DateTimeOffset? expiresAt = null)
        {
            if (string.IsNullOrEmpty(realmPath))
            {
                throw new ArgumentNullException(nameof(realmPath));
            }

            if (expiresAt < DateTimeOffset.UtcNow)
            {
                throw new ArgumentException("The expiration date may not be in the past", nameof(expiresAt));
            }

            if (accessLevel == AccessLevel.None)
            {
                throw new ArgumentException("The access level may not be None", nameof(accessLevel));
            }

            var payload = new Dictionary<string, object>
            {
                ["expiresAt"] = expiresAt?.ToString("O"),
                ["realmPath"] = realmPath,
                ["accessLevel"] = accessLevel.ToString().ToLower()
            };

            var result = await MakePermissionRequestAsync(HttpMethod.Post, "permissions/offers", payload);
            return result.ToObject<PermissionOffer>().Token;
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

            var result = await MakePermissionRequestAsync(HttpMethod.Post, $"permissions/offers/{offerToken}/accept");
            return result["path"].Value<string>();
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
        [Obsolete("Use InvalidateOfferAsync(string) by passing the offer.Token instead.")]
        public Task InvalidateOfferAsync(PermissionOffer offer)
        {
            Argument.NotNull(offer, nameof(offer));

            return InvalidateOfferAsync(offer.Token);
        }

        /// <summary>
        /// Invalidates a permission offer by its token.
        /// </summary>
        /// <remarks>
        /// Invalidating an offer prevents new users from consuming its token. It doesn't revoke any permissions that have
        /// already been granted.
        /// </remarks>
        /// <returns>
        /// An awaitable task, that, upon completion, indicates that the offer has been successfully invalidated by the server.
        /// </returns>
        /// <param name="offerToken">The token of the offer that should be invalidated.</param>
        public Task InvalidateOfferAsync(string offerToken) => MakePermissionRequestAsync(HttpMethod.Delete, $"permissions/offers/{offerToken}");

        /// <summary>
        /// Asynchronously retrieve the permission offers that this user has created by invoking <see cref="OfferPermissionsAsync"/>.
        /// </summary>
        /// <returns>A collection of <see cref="PermissionOffer"/> objects.</returns>
        public async Task<IEnumerable<PermissionOffer>> GetPermissionOffersAsync()
        {
            var result = await MakePermissionRequestAsync(HttpMethod.Get, $"permissions/offers");
            return result["offers"].ToObject<IEnumerable<PermissionOffer>>();
        }

        private Task<JObject> MakePermissionRequestAsync(HttpMethod method, string relativeUri, IDictionary<string, object> body = null)
            => AuthenticationHelper.MakeAuthRequestAsync(method, new Uri(ServerUri, relativeUri), body, RefreshToken);

        #endregion Permissions
    }
}