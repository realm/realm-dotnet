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
using Realms.Logging;

namespace Realms.Sync
{
    /// <summary>
    /// A class exposing configuration options for a <see cref="App"/>.
    /// </summary>
    /// <seealso cref="App.Create(AppConfiguration)"/>.
    public class AppConfiguration
    {
        private byte[] _metadataEncryptionKey;

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
        public string BaseFilePath { get; set; }

        /// <summary>
        /// Gets or sets the base url for this Realm application.
        /// </summary>
        /// <remarks>
        /// This only needs to be set if for some reason your application isn't hosted on realm.mongodb.com. This can be the case if you're
        /// testing locally or are using a preproduction environment.
        /// </remarks>
        /// <value>The app's base url.</value>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the local app's name.
        /// </summary>
        /// <remarks>
        /// The local app name is typically used to differentiate between client applications that use the same
        /// Atlas App Services app. These can be the same conceptual app developed for different platforms, or
        /// significantly different client side applications that operate on the same data - e.g. an event managing
        /// service that has different clients apps for organizers and attendees.
        /// </remarks>
        /// <value>The friendly name identifying the current client application.</value>
        /// <seealso cref="LocalAppVersion"/>
        public string LocalAppName { get; set; }

        /// <summary>
        /// Gets or sets the local app's version.
        /// </summary>
        /// <remarks>
        /// The local app version is typically used to differentiate between versions of the same client application.
        /// </remarks>
        /// <value>The client application's version.</value>
        /// <seealso cref="LocalAppName"/>
        public string LocalAppVersion { get; set; }

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
        public byte[] MetadataEncryptionKey
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
        /// Gets or sets a custom log function that will be invoked for each log message emitted by sync.
        /// </summary>
        /// <remarks>
        /// The first argument of the action is the log message itself, while the second one is the <see cref="LogLevel"/>
        /// at which the log message was emitted.
        /// </remarks>
        /// <value>The custom logger.</value>
        /// <seealso cref="Logger.Default"/>
        [Obsolete("Configure the global Realms.Logging.Logger.Default instead")]
        public Action<string, LogLevel> CustomLogger { get; set; }

        /// <summary>
        /// Gets or sets the log level for sync operations.
        /// </summary>
        /// <value>The sync log level.</value>
        /// <seealso cref="Logger.LogLevel"/>
        [Obsolete("Configure the global Realms.Logging.Logger.LogLevel instead")]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets the default request timeout for HTTP requests to MongoDB Atlas.
        /// </summary>
        /// <value>The default HTTP request timeout.</value>
        public TimeSpan? DefaultRequestTimeout { get; set; }

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
        public HttpMessageHandler HttpClientHandler { get; set; }

        /// <summary>
        /// Gets or sets the options for the assorted types of connection timeouts for sync connections
        /// opened for this app.
        /// </summary>
        /// <value>The sync timeout options applied to synchronized Realms.</value>
        public SyncTimeoutOptions SyncTimeoutOptions { get; set; } = new();

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
