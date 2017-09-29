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
using System.IO;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using the
    /// Realm Object Server. A valid <see cref="User"/> is required to create a <see cref="SyncConfiguration"/>.
    /// </summary>
    /// <seealso cref="User.LoginAsync"/>
    /// <seealso cref="Credentials"/>
    public class SyncConfiguration : RealmConfigurationBase
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> used to create this <see cref="SyncConfiguration"/>.
        /// </summary>
        /// <value>The <see cref="Uri"/> where the Realm Object Server is hosted.</value>
        public Uri ServerUri { get; }

        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="SyncConfiguration"/>.
        /// </summary>
        /// <value>The <see cref="User"/> whose <see cref="Realm"/>s will be synced.</value>
        public User User { get; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL certificate validation is enabled for the connection associated
        /// with this configuration value.
        /// </summary>
        /// <value><c>true</c> if SSL validation is enabled; otherwise, <c>false</c>. Default value is <c>true</c>.</value>
        public bool EnableSSLValidation { get; set; } = true;

        /// <summary>
        /// Gets or sets the path to the trusted root certificate(s) authority (CA) in PEM format, that should
        /// be used to validate the TLS connections to the Realm Object Server.
        /// </summary>
        /// <value>The path to the certificate.</value>
        /// <remarks>
        /// The file will be copied at runtime into the internal storage.
        /// <br/>
        /// It is recommended to include only the root CA you trust, and not the entire list of root CA as this file
        /// will be loaded at runtime. It is your responsibility to download and verify the correct PEM for the root CA
        /// you trust.
        /// <br/>
        /// This property is ignored on Apple platforms - you should use the KeyChain API to install your certificate
        /// instead.
        /// </remarks>
        /// <seealso href="https://www.openssl.org/docs/man1.0.2/ssl/SSL_CTX_load_verify_locations.html">
        /// OpenSSL documentation for SSL_CTX_load_verify_locations.
        /// </seealso>
        /// <seealso href="https://ccadb-public.secure.force.com/mozilla/IncludedCACertificateReport">
        /// Mozilla Included CA Certificate List
        /// </seealso>
        public string TrustedCAPath { get; set; }

        /// <summary>
        /// Gets or sets IsPartial. TODO
        /// </summary>
        public bool IsPartial { get; set; }

        /// <summary>
        /// Gets or sets the PartialSyncIdentifier. TODO
        /// </summary>
        public string PartialSyncIdentifier { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">A valid <see cref="User"/>.</param>
        /// <param name="serverUri">
        /// A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public SyncConfiguration(User user, Uri serverUri, string optionalPath = null)
            : base(optionalPath ?? SharedRealmHandleExtensions.GetRealmPath(user, serverUri))
        {
            Argument.NotNull(user, nameof(user));
            Argument.NotNull(serverUri, nameof(serverUri));
            Argument.Ensure(serverUri.Scheme.StartsWith("realm"), "Unexpected protocol for server url. Expected realm:// or realms://.", nameof(serverUri));

            User = user ?? throw new ArgumentNullException(nameof(user));
            ServerUri = serverUri ?? throw new ArgumentNullException(nameof(serverUri));
        }

        /// <summary>
        /// Sets the feature token, associated with your edition. You only need to call it if you're using a professional
        /// or higher edition and only on platforms where features are disabled for lower editions.
        /// </summary>
        /// <param name="token">The feature token provided to you by the Realm team.</param>
        /// <seealso href="https://realm.io/docs/realm-object-server/pe-ee/#enabling-professional-and-enterprise-apis">
        /// See more details on Enabling Professional and Enterprise APIs in the documentation.
        /// </seealso>
        public static void SetFeatureToken(string token)
        {
            Argument.NotNullOrEmpty(token, nameof(token));

            SharedRealmHandleExtensions.SetFeatureToken(token);
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var configuration = new Realms.Native.Configuration
            {
                Path = DatabasePath,
                schema_version = SchemaVersion
            };

            var srHandle = SharedRealmHandleExtensions.OpenWithSync(configuration, ToNative(), schema, EncryptionKey);
            return new Realm(srHandle, this, schema);
        }

        internal override async Task<Realm> CreateRealmAsync(RealmSchema schema)
        {
            var session = new Session(SharedRealmHandleExtensions.GetSession(DatabasePath, ToNative(), EncryptionKey));
            try
            {
                await session.WaitForDownloadAsync();
            }
            finally
            {
                session.CloseHandle();
            }

            return CreateRealm(schema);
        }

        private Native.SyncConfiguration ToNative()
        {
            if (!string.IsNullOrEmpty(TrustedCAPath) &&
                !File.Exists(TrustedCAPath))
            {
                throw new FileNotFoundException($"{nameof(TrustedCAPath)} has been specified, but the file was not found.", TrustedCAPath);
            }

            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                Url = ServerUri.ToString(),
                EnableSSLValidation = EnableSSLValidation,
                TrustedCAPath = TrustedCAPath,
                IsPartial = IsPartial,
                PartialSyncIdentifier = PartialSyncIdentifier
            };
        }
    }
}