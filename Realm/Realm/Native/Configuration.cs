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
        public unsafe struct SchemaArray
        {
            private readonly SchemaObject* first;
            private readonly nint count;

            public SchemaArray(in MarshaledVector<SchemaObject> vec)
            {
                first = vec.Pointer;
                count = vec.Count;
            }

            public static implicit operator SchemaArray(in MarshaledVector<SchemaObject> vec) => new(vec);

            public static implicit operator MarshaledVector<SchemaObject>(in SchemaArray arr) => new(arr.first, arr.count);
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string path;
        private IntPtr path_len;

        internal string Path
        {
            set
            {
                path = value;
                path_len = (IntPtr)value.Length;
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string? fallback_path;
        private IntPtr fallback_path_len;

        internal string? FallbackPipePath
        {
            set
            {
                fallback_path = value;
                fallback_path_len = value.IntPtrLength();
            }
        }

        [MarshalAs(UnmanagedType.U1)]
        internal bool read_only;
        [MarshalAs(UnmanagedType.U1)]
        internal bool in_memory;

        [MarshalAs(UnmanagedType.U1)]
        internal bool delete_if_migration_needed;

        public SchemaArray schema;

        internal ulong schema_version;

        [MarshalAs(UnmanagedType.U1)]
        internal bool enable_cache;

        internal ulong max_number_of_active_versions;

        [MarshalAs(UnmanagedType.U1)]
        internal bool use_legacy_guid_representation;

        internal IntPtr managed_config;

        [MarshalAs(UnmanagedType.U1)]
        internal bool invoke_should_compact_callback;

        [MarshalAs(UnmanagedType.U1)]
        internal bool invoke_initial_data_callback;

        [MarshalAs(UnmanagedType.U1)]
        internal bool invoke_migration_callback;

        [MarshalAs(UnmanagedType.U1)]
        internal bool automatically_migrate_embedded;
    }
}
