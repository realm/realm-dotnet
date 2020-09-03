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
using System.Diagnostics.CodeAnalysis;
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

            // V10TODO: do the native login
            return new User(null);
        }

        #endregion static

        /// <summary>
        /// Gets this user's refresh token. This is the user's credential for accessing the Realm Object Server and should be treated as sensitive data.
        /// </summary>
        /// <value>A unique string that can be used for refreshing the user's credentials.</value>
        public string RefreshToken
        {
            get => Handle.GetRefreshToken();
        }

        /// <summary>
        /// Gets the identity of this user on the Realm Object Server. The identity is a guaranteed to be unique among all users on the Realm Object Server.
        /// </summary>
        /// <value>A string that uniquely identifies that user in Realm Object Server.</value>
        public string Identity => Handle.GetIdentity();

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
        /// <returns>An awaitable Task, that, upon completion indicates that the user has been logged out both locally and on the server.</returns>
        public Task LogOutAsync()
        {
            Handle.LogOut();

            // V10TODO: native logout must be async
            return Task.CompletedTask;
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
            return Identity.Equals(other?.Identity);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }
    }
}