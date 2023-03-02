////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Runtime.InteropServices;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SyncError
    {
        public ErrorCode error_code;

        public StringValue message;

        public StringValue log_url;

        [MarshalAs(UnmanagedType.U1)]
        public bool is_client_reset;

        public MarshaledVector<StringStringPair> user_info_pairs;

        public MarshaledVector<CompensatingWriteInfo> compensating_writes;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CompensatingWriteInfo
    {
        public StringValue reason;
        public StringValue object_name;
        public PrimitiveValue primary_key;
    }
}
