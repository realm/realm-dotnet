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
        private string url;
        private IntPtr url_len;

        internal string Url
        {
            set
            {
                url = value;
                url_len = (IntPtr)value.Length;
            }
        }
    }
}
