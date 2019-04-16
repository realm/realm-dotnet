////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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

namespace Realms.Server.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeChangeSet
    {
        public byte* class_name_buf;
        public IntPtr class_name_len;

        public MarshaledVector<IntPtr> deletions;
        public MarshaledVector<IntPtr> insertions;
        public MarshaledVector<IntPtr> previous_modifications;
        public MarshaledVector<IntPtr> current_modifications;

        public string ClassName => Encoding.UTF8.GetString(class_name_buf, (int)class_name_len);
    }
}
