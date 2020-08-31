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
using Realms.Native;

namespace Realms.Server.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeChangeDetails
    {
        public MarshaledString path;

        public MarshaledString path_on_disk;

        public IntPtr previous_realm;

        public IntPtr current_realm;

        public MarshaledVector<NativeChangeSet> change_sets;

        public string Path => path.ToString();

        public string PathOnDisk => path_on_disk.ToString();
    }
}
