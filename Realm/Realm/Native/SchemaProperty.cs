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

using System.Runtime.InteropServices;
using Realms.Schema;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaProperty
    {
        internal static readonly int Size = Marshal.SizeOf<SchemaProperty>();

        [MarshalAs(UnmanagedType.LPStr)]
        internal string name;

        [MarshalAs(UnmanagedType.U2)]
        internal PropertyType type;

        [MarshalAs(UnmanagedType.LPStr)]
        internal string? object_type;

        [MarshalAs(UnmanagedType.LPStr)]
        internal string? link_origin_property_name;

        [MarshalAs(UnmanagedType.U1)]
        internal bool is_primary;

        internal IndexType index;
    }
}
