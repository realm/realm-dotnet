////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using Realms.Helpers;
using Realms.Sync;

using NativeSyncConfiguration = Realms.Sync.Native.SyncConfiguration;

namespace Realms.Server
{
    /// <summary>
    /// A Notifier configuration specifying various settings that affect the Notifier's behavior.
    /// </summary>
    public class NotifierConfiguration
    {
        /// <summary>
        /// Gets the <see cref="Sync.User"/> used to create this <see cref="NotifierConfiguration"/>.
        /// </summary>
        /// <value>The <see cref="Sync.User"/> whose <see cref="Realm"/>s will be synced.</value>
        public User User { get; }

        /// <summary>
        /// Gets or sets a collection of <see cref="INotificationHandler"/>s that will be invoked when
        /// a change occurs in a Realm file.
        /// </summary>
        /// <value>The <see cref="IList{INotificationHandler}"/> that will handle Realm changes.</value>
        /// <remarks>
        /// The members of the collection will be called sequentially in the order that they appear.
        /// </remarks>
        public IList<INotificationHandler> Handlers { get; set; } = new List<INotificationHandler>();

        /// <summary>
        /// Gets or sets the directory which the <see cref="INotifier"/> will use to store the Realms it observes.
        /// </summary>
        /// <value>A folder on the filesystem, that your application has permissions to write to.</value>
        public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

        private byte[] _encryptionKey;

        /// <summary>
        /// Gets or sets the key, used to encrypt the Realms at rest that the <see cref="INotifier"/> observes.
        /// Once set, must be specified each time a notifier is started in the same working directory.
        /// </summary>
        /// <value>Full 64byte (512bit) key for AES-256 encryption.</value>
        public byte[] EncryptionKey
        {
            get
            {
                return _encryptionKey;
            }
            set
            {
                if (value != null && value.Length != 64)
                {
                    throw new FormatException("EncryptionKey must be 64 bytes");
                }

                _encryptionKey = value;
            }
        }

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
        /// Initializes a new instance of the <see cref="NotifierConfiguration"/> class.
        /// </summary>
        /// <param name="user">A valid <see cref="Sync.User"/> that has administrative access.</param>
        public NotifierConfiguration(User user)
        {
            Argument.NotNull(user, nameof(user));
            Argument.Ensure(user.IsAdmin, "User must be an administrator", nameof(user));

            User = user;
        }

        internal NativeSyncConfiguration ToNative()
        {
            if (!string.IsNullOrEmpty(TrustedCAPath) &&
                !File.Exists(TrustedCAPath))
            {
                throw new FileNotFoundException($"{nameof(TrustedCAPath)} has been specified, but the file was not found.", TrustedCAPath);
            }

            return new NativeSyncConfiguration
            {
                SyncUserHandle = User.Handle,
                Url = User.GetUriForRealm(string.Empty).ToString().TrimEnd('/'),
                client_validate_ssl = EnableSSLValidation,
                TrustedCAPath = TrustedCAPath,
                PartialSyncIdentifier = WorkingDirectory // we hijack this field of the struct
            };
        }
    }
}