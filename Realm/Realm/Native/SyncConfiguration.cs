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

namespace Realms.Sync.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SyncConfiguration
    {
        private IntPtr sync_user_ptr;

        internal SyncUserHandle SyncUserHandle
        {
            set
            {
                sync_user_ptr = value.DangerousGetHandle();
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string partition;

        private IntPtr partition_len;

        internal string Partition
        {
            set
            {
                partition = value;
                partition_len = (IntPtr)value.Length;
            }
        }

        internal SessionStopPolicy session_stop_policy;

        internal SchemaMode schema_mode;

        [MarshalAs(UnmanagedType.I1)]
        internal bool is_flexible_sync;

        internal ClientResyncMode client_resync_mode;

        [MarshalAs(UnmanagedType.I1)]
        internal bool cancel_waits_on_nonfatal_error;
    }
}
