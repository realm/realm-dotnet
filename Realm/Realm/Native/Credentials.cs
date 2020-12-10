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
using static Realms.Sync.Credentials;

namespace Realms.Sync.Native
{
    internal struct Credentials
    {
        internal AuthProvider provider;

        [MarshalAs(UnmanagedType.LPWStr)]
        private string token;
        private IntPtr token_len;

        internal string Token
        {
            set
            {
                token = value;
                token_len = (IntPtr)(value?.Length ?? 0);
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string additional_info;
        private IntPtr additional_info_len;

        internal string AdditionalInfo
        {
            set
            {
                additional_info = value;
                additional_info_len = (IntPtr)(value?.Length ?? 0);
            }
        }
    }
}
