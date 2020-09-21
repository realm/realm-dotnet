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
using Realms.Helpers;

namespace Realms.Sync
{
    public class AppConfiguration
    {
        private byte[] _metadataEncryptionKey;

        public string AppId { get; }

        public string BaseFilePath { get; set; }

        public Uri BaseUri { get; set; }

        public string LocalAppName { get; set; }

        public string LocalAppVersion { get; set; }

        public MetadataPersistenceMode? MetadataPersistenceMode { get; set; }

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

        public bool ResetMetadataOnError { get; set; }

        public Action<string, LogLevel> CustomLogger { get; set; }

        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        public TimeSpan? DefaultRequestTimeout { get; set; }

        public AppConfiguration(string appId)
        {
            Argument.NotNullOrEmpty(appId, nameof(appId));

            AppId = appId;
        }
    }
}
