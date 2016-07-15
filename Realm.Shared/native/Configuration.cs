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

        [MarshalAs(UnmanagedType.I1)]
        internal bool read_only;
        [MarshalAs(UnmanagedType.I1)]
        internal bool in_memory;

        [MarshalAs(UnmanagedType.LPArray)]
        internal byte[] encryption_key;

        private IntPtr schema;
        internal SchemaHandle Schema
        {
            set { schema = value.DangerousGetHandle(); }
        }

        internal UInt64 schema_version;
    }
}

