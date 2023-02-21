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
using System.Runtime.InteropServices;
using Realms.Logging;

namespace Realms.Sync.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AppConfiguration
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string app_id;
        private IntPtr app_id_len;

        internal string AppId
        {
            set
            {
                app_id = value;
                app_id_len = (IntPtr)value.Length;
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string base_file_path;
        private IntPtr base_file_path_len;

        internal string BaseFilePath
        {
            set
            {
                base_file_path = value;
                base_file_path_len = (IntPtr)value.Length;
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string? base_url;
        private IntPtr base_url_len;

        internal string? BaseUrl
        {
            set
            {
                base_url = value;
                base_url_len = value.IntPtrLength();
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string? local_app_name;
        private IntPtr local_app_name_len;

        internal string? LocalAppName
        {
            set
            {
                local_app_name = value;
                local_app_name_len = value.IntPtrLength();
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string? local_app_version;
        private IntPtr local_app_version_len;

        internal string? LocalAppVersion
        {
            set
            {
                local_app_version = value;
                local_app_version_len = value.IntPtrLength();
            }
        }

        internal UInt64 default_request_timeout_ms;

        private MetadataPersistenceMode metadata_persistence;

        [MarshalAs(UnmanagedType.U1)]
        private bool metadata_persistence_has_value;

        internal MetadataPersistenceMode? MetadataPersistence
        {
            set
            {
                metadata_persistence = value.HasValue ? value.Value : default;
                metadata_persistence_has_value = value.HasValue;
            }
        }

        internal LogLevel log_level;

        internal IntPtr managed_logger;

        internal IntPtr managed_http_client;

        internal UInt64 sync_connect_timeout_ms;

        internal UInt64 sync_connection_linger_time_ms;

        internal UInt64 sync_ping_keep_alive_period_ms;

        internal UInt64 sync_pong_keep_alive_timeout_ms;

        internal UInt64 sync_fast_reconnect_limit;
    }
}
