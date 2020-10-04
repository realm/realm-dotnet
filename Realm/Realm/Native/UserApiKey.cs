﻿////////////////////////////////////////////////////////////////////////////
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
using System.Text;
using MongoDB.Bson;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct UserApiKey
    {
        internal static readonly int Size = Marshal.SizeOf<UserApiKey>();

        private PrimitiveValue id;

        private byte* key_buf;
        private IntPtr key_len;

        private byte* name_buf;
        private IntPtr name_len;

        [MarshalAs(UnmanagedType.U1)]
        public bool disabled;

        public ObjectId Id => id.ToObjectId();

        public string Key => key_buf == null ? null : Encoding.UTF8.GetString(key_buf, (int)key_len);

        public string Name => name_buf == null ? null : Encoding.UTF8.GetString(name_buf, (int)name_len);
    }
}
