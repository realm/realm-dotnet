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
using System.Reflection;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// Gets or sets a callback that is invoked when download progress is made when using <see cref="Realm.GetInstanceAsync"/>.
        /// This will only be invoked for the initial download of the Realm and will not be invoked as futher download
        /// progress is made during the lifetime of the Realm. It is ignored when using
        /// <see cref="Realm.GetInstance(RealmConfigurationBase)"/>.
        /// </summary>
        public Action<SyncProgress> OnProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how detailed the sync client's logs will be.
        /// </summary>
        public static LogLevel LogLevel
        {
            get => SharedRealmHandleExtensions.GetLogLevel();
            set => SharedRealmHandleExtensions.SetLogLevel(value);
        }

        private static Action<string, LogLevel> _customLogger;

        /// <summary>
        /// Gets or sets a custom log function that will be invoked by Sync instead of writing
        /// to the standard error. This must be set before using any of the sync API.
        /// </summary>
        /// <remarks>
        /// This callback will not be invoked in a thread-safe manner, so it's up to the implementor to ensure
        /// that log messages arriving from multiple threads are processed without garbling the final output.
        /// </remarks>
        /// <value>The custom log function.</value>
        public static Action<string, LogLevel> CustomLogger
        {
            get => _customLogger;
            set
            {
                _customLogger = value;
                SharedRealmHandleExtensions.InstallLogCallback();
            }
        }

        private static string _userAgent;

        /// <summary>
        /// Gets or sets a string identifying this application which is included in the User-Agent
        /// header of sync connections.
        /// </summary>
        /// <remarks>
        /// This property must be set prior to opening a synchronized Realm for the first
        /// time. Any modifications made after opening a Realm will be ignored.
        /// </remarks>
        /// <value>
        /// The custom user agent that will be appended to the one generated by the SDK.
        /// </value>
        public static string UserAgent
        {
            get => _userAgent;
            set
            {
                SharedRealmHandleExtensions.SetUserAgent(value);
                _userAgent = value;
            }
        }

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
        /// Configures various parameters of the sync system, such as the way users are persisted or the base
        /// path relative to which files will be saved.
        /// </summary>
        /// <param name="mode">The user persistence mode.</param>
        /// <param name="encryptionKey">The key to encrypt the persistent user store with.</param>
        /// <param name="resetOnError">If set to <c>true</c> reset the persistent user store on error.</param>
        /// <param name="basePath">The base folder relative to which Realm files will be stored.</param>
        /// <remarks>
        /// Users are persisted in a realm file within the application's sandbox.
        /// <para>
        /// By default <see cref="User"/> objects are persisted and are additionally protected with an encryption key stored
        /// in the iOS Keychain when running on an iOS device (but not on a Simulator).
        /// On Android users are persisted in plaintext, because the AndroidKeyStore API is only supported on API level 18 and up.
        /// You might want to provide your own encryption key on Android or disable persistence for security reasons.
        /// </para>
        /// </remarks>
        public static void Initialize(UserPersistenceMode mode, byte[] encryptionKey = null, bool resetOnError = false, string basePath = null)
        {
            if (mode == UserPersistenceMode.Encrypted && encryptionKey != null && encryptionKey.Length != 64)
            {
                throw new ArgumentException("The encryption key must be 64 bytes long", nameof(encryptionKey));
            }

            SharedRealmHandleExtensions.Configure(mode, encryptionKey, resetOnError, basePath);
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
            var configuration = new Realms.Native.Configuration
            {
                Path = DatabasePath,
                schema_version = SchemaVersion,
                enable_cache = EnableCache
            };

            // Keep that until we open the Realm on the foreground.
            var backgroundHandle = await SharedRealmHandleExtensions.OpenWithSyncAsync(configuration, ToNative(), schema, EncryptionKey);

            var foregroundHandle = SharedRealmHandleExtensions.OpenWithSync(configuration, ToNative(), schema, EncryptionKey);
            backgroundHandle.Close();
            if (IsDynamic && !schema.Any())
            {
                foregroundHandle.GetSchema(nativeSchema => schema = RealmSchema.CreateFromObjectStoreSchema(nativeSchema));
            }

            return new Realm(foregroundHandle, this, schema);
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

        internal static string GetSDKUserAgent()
        {
            var version = typeof(SyncConfigurationBase).GetTypeInfo().Assembly.GetName().Version;
            return $"RealmDotNet/{version} ({RuntimeInformation.FrameworkDescription})";
        }
    }
}