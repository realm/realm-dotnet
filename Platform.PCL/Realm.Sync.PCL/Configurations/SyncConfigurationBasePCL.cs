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

        internal SyncConfigurationBase(Uri serverUri, User user = null, string optionalPath = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Gets or sets a value indicating how detailed the sync client's logs will be.
        /// </summary>
        public static LogLevel LogLevel { get; set; }
    }
}