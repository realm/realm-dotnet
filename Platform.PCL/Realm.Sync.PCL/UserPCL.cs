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
using System.Threading.Tasks;
using Realms.Exceptions;

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
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Logs the user in to the Realm Object Server.
        /// </summary>
        /// <param name="credentials">The credentials to use for authentication.</param>
        /// <param name="serverUrl">The URI of the server that the user is authenticated against.</param>
        /// <returns>An awaitable Task, that, upon completion, contains the logged in user.</returns>
        public static Task<User> LoginAsync(Credentials credentials, Uri serverUrl)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        /// <summary>
        /// Gets this user's refresh token. This is the user's credential for accessing the Realm Object Server and
        /// should be treated as sensitive data.
        /// </summary>
        /// <value>A unique string that can be used for refreshing the user's credentials.</value>
        public string RefreshToken
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the identity of this user on the Realm Object Server. The identity is a guaranteed to be unique
        /// among all users on the Realm Object Server.
        /// </summary>
        /// <value>A string that uniquely identifies that user in Realm Object Server.</value>
        public string Identity
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the server <see cref="Uri"/> that was used for authentication.
        /// </summary>
        /// <value>The <see cref="Uri"/> used to connect to the authentication service.</value>
        public Uri ServerUri
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the current state of the user.
        /// </summary>
        /// <value>A value indicating whether the user is active, logged out, or an error has occurred.</value>
        public UserState State
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return default(UserState);
            }
        }

        /// <summary>
        /// Logs out the user from the Realm Object Server. Once the Object Server has confirmed the logout the user credentials will be deleted from this device.
        /// </summary>
        public void LogOut()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <inheritdoc />
        public bool Equals(User other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }
    }
}