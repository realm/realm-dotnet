﻿////////////////////////////////////////////////////////////////////////////
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter")]
    internal struct SchemaProperty
    {
        internal static readonly int Size = Marshal.SizeOf<SchemaProperty>();

        [MarshalAs(UnmanagedType.LPStr)]
        internal string name;

        [MarshalAs(UnmanagedType.U1)]
        internal Realms.Schema.PropertyType type;

        [MarshalAs(UnmanagedType.LPStr)]
        internal string object_type;

        [MarshalAs(UnmanagedType.I1)]
        internal bool is_nullable;

        [MarshalAs(UnmanagedType.I1)]
        internal bool is_primary;

        [MarshalAs(UnmanagedType.I1)]
        internal bool is_indexed;
    }
}