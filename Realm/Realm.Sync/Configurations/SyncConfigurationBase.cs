////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.Linq;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfigurationBase"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using the
    /// Realm Object Server.
    /// </summary>
    /// <seealso cref="User.LoginAsync"/>
    /// <seealso cref="Credentials"/>
    /// <seealso cref="FullSyncConfiguration"/>
    /// <seealso cref="QueryBasedSyncConfiguration"/>
    public abstract class SyncConfigurationBase : RealmConfigurationBase
    {
        internal abstract bool IsFullSync { get; }

        /// <summary>
        /// Gets the <see cref="Uri"/> used to create this <see cref="SyncConfigurationBase"/>.
        /// </summary>
        /// <value>The <see cref="Uri"/> where the Realm Object Server is hosted.</value>
        public Uri ServerUri { get; }

        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="SyncConfigurationBase"/>.
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

        internal SyncConfigurationBase(Uri serverUri, User user = null, string optionalPath = null)
        {
            Argument.Ensure(user != null || User.AllLoggedIn.Length == 1,
                "The user must be explicitly specified when the number of logged-in users is not 1.",
                nameof(user));

            User = user ?? User.Current;
            if (!serverUri.IsAbsoluteUri)
            {
                ServerUri = User.GetUriForRealm(serverUri);
            }
            else
            {
                Argument.Ensure(serverUri.Scheme.StartsWith("realm"), "Unexpected protocol for server url. Expected realm:// or realms://.", nameof(serverUri));
                ServerUri = serverUri;
            }

            DatabasePath = GetPathToRealm(optionalPath ?? SharedRealmHandleExtensions.GetRealmPath(User, ServerUri));
        }

        /// <summary>
        /// Gets or sets a value indicating how detailed the sync client's logs will be.
        /// </summary>
        public static LogLevel LogLevel
        {
            get
            {
                return SharedRealmHandleExtensions.GetLogLevel();
            }
            set
            {
                SharedRealmHandleExtensions.SetLogLevel(value);
            }
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var configuration = new Realms.Native.Configuration
            {
                Path = DatabasePath,
                schema_version = SchemaVersion,
                enable_cache = EnableCache
            };

            var srHandle = SharedRealmHandleExtensions.OpenWithSync(configuration, ToNative(), schema, EncryptionKey);
            if (IsDynamic && !schema.Any())
            {
                srHandle.GetSchema(nativeSchema => schema = RealmSchema.CreateFromObjectStoreSchema(nativeSchema));
            }

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

        internal Native.SyncConfiguration ToNative()
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
                client_validate_ssl = EnableSSLValidation,
                TrustedCAPath = TrustedCAPath,
                is_partial = !IsFullSync,
                PartialSyncIdentifier = null
            };
        }
    }
}