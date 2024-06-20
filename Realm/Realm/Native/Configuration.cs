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
using System.Runtime.InteropServices;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Configuration
    {
        internal StringValue path;

        internal StringValue fallbackPipePath;

        public Schema schema;

        internal ulong schema_version;

        internal ulong max_number_of_active_versions;

        internal IntPtr managed_config;

        internal MarshaledVector<byte> encryption_key;

        internal NativeBool read_only;

        internal NativeBool in_memory;

        internal NativeBool delete_if_migration_needed;

        internal NativeBool enable_cache;

        internal NativeBool use_legacy_guid_representation;

        internal NativeBool invoke_should_compact_callback;

        internal NativeBool invoke_initial_data_callback;

        internal NativeBool invoke_migration_callback;

        internal NativeBool automatically_migrate_embedded;

        internal NativeBool flexible_schema;
    }
}
