////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Net.Http;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A class exposing configuration options for a <see cref="App"/>.
    /// </summary>
    public class AppConfiguration
    {
        private string? _baseFilePath;

        private byte[]? _metadataEncryptionKey;

        /// <summary>
        /// Gets the unique app id that identifies the Realm application.
        /// </summary>
        /// <value>The Atlas App Services App's id.</value>
        public string AppId { get; }

        /// <summary>
        /// Gets or sets the root folder relative to which all local data for this application will be stored. This data includes
        /// metadata for users and synchronized Realms.
        /// </summary>
        /// <value>The app's base path.</value>
        public string BaseFilePath
        {
            get => _baseFilePath ?? InteropConfig.GetDefaultStorageFolder("Could not determine a writable folder to store app files (such as metadata and Realm files). When constructing the app, set AppConfiguration.BaseFilePath to an absolute path where the app is allowed to write.");
            set
            {
                Argument.NotNull(value, nameof(value));
                _baseFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the base url for this Realm application.
        /// </summary>
        /// <remarks>
        /// This only needs to be set if for some reason your application isn't hosted on realm.mongodb.com. This can be the case if you're
        /// testing locally or are using a preproduction environment.
        /// </remarks>
        /// <value>The app's base url.</value>
        public Uri BaseUri { get; set; } = new Uri("https://realm.mongodb.com");

        /// <summary>
        /// Gets or sets the local app's name.
        /// </summary>
        /// <value>The friendly name identifying the current client application.</value>
        [Obsolete("This property has no effect and will be removed in a future version.")]
        public string? LocalAppName { get; set; }

        /// <summary>
        /// Gets or sets the local app's version.
        /// </summary>
        /// <value>The client application's version.</value>
        /// <seealso cref="LocalAppName"/>
        [Obsolete("This property has no effect and will be removed in a future version.")]
        public string? LocalAppVersion { get; set; }

        /// <summary>
        /// Gets or sets the persistence mode for user metadata on this device.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="MetadataPersistenceMode.Encrypted"/> for iOS devices and <see cref="MetadataPersistenceMode.NotEncrypted"/>
        /// for all other platforms. On iOS we integrate with the system keychain to generate and store a random encryption key the first time the app
        /// is launched. On other platforms, <see cref="MetadataEncryptionKey"/> needs to be set if <see cref="MetadataPersistenceMode.Encrypted"/> is
        /// specified.
        /// </remarks>
        /// <value>The user metadata persistence mode.</value>
        public MetadataPersistenceMode? MetadataPersistenceMode { get; set; }

        /// <summary>
        /// Gets or sets the encryption key for user metadata on this device.
        /// </summary>
        /// <remarks>
        /// This will not change the encryption key for individual Realms. This should still be set in <see cref="RealmConfigurationBase.EncryptionKey"/>
        /// when opening the <see cref="Realm"/>.
        /// </remarks>
        /// <value>The user metadata encryption key.</value>
        public byte[]? MetadataEncryptionKey
        {
            get => _metadataEncryptionKey;
            set
            {
                if (value != null && value.Length != 64)
                {
                    throw new FormatException("EncryptionKey must be 64 bytes");
                }

                _metadataEncryptionKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the default request timeout for HTTP requests to MongoDB Atlas. Default is 1 minute.
        /// </summary>
        /// <value>The default HTTP request timeout.</value>
        public TimeSpan DefaultRequestTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the <see cref="HttpMessageHandler"/> that will be used
        /// for the http requests to MongoDB Atlas.
        /// </summary>
        /// <value>The http client handler that configures things like certificates and proxy settings.</value>
        /// <remarks>
        /// You can use this to override the default http client handler and configure settings like proxies,
        /// client certificates, and cookies. While these are not required to connect to MongoDB Atlas under
        /// normal circumstances, they can be useful if client devices are behind corporate firewall or use
        /// a more complex networking setup.
        /// </remarks>
        public HttpMessageHandler? HttpClientHandler { get; set; }

        /// <summary>
        /// Gets or sets the options for the assorted types of connection timeouts for sync connections
        /// opened for this app.
        /// </summary>
        /// <value>The sync timeout options applied to synchronized Realms.</value>
        public SyncTimeoutOptions SyncTimeoutOptions { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether to cache app instances created with this configuration.
        /// </summary>
        /// <remarks>
        /// When an app is created using <see cref="App.Create(AppConfiguration)"/>, the default behavior is
        /// to get or add the <see cref="App"/> instance from a cache keyed on the app id. This has certain
        /// performance benefits when calling <see cref="App.Create(AppConfiguration)"/> multiple times.
        /// </remarks>
        /// <value><c>true</c> if the app should be cached; <c>false</c> otherwise. Default value is <c>true</c>.</value>
        public bool UseAppCache { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfiguration"/> class with the specified <paramref name="appId"/>.
        /// </summary>
        /// <param name="appId">The Atlas App Services App id.</param>
        public AppConfiguration(string appId)
        {
            Argument.NotNullOrEmpty(appId, nameof(appId));

            AppId = appId;
        }
    }
}
