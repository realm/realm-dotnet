////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
    internal struct TableKey : IEquatable<TableKey>
    {
        private UInt32 value;

        public bool Equals(TableKey other) => value.Equals(other.value);

        public override bool Equals(object obj) => obj is TableKey other && Equals(other);

        public override int GetHashCode() => value.GetHashCode();

        public static bool operator ==(TableKey left, TableKey right) => left.value == right.value;

        public static bool operator !=(TableKey left, TableKey right) => left.value != right.value;
    }
}
