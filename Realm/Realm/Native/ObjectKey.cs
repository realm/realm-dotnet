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

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjectKey : IEquatable<ObjectKey>
    {
        private Int64 value;

        public bool Equals(ObjectKey other) => value.Equals(other.value);

        public override bool Equals(object obj)
        {
            switch(obj)
            {
                case ObjectKey other:
                    return value.Equals(other.value);
                default:
                    return false;
            }
        }

        public override int GetHashCode() => value.GetHashCode();

        public static bool operator ==(ObjectKey left, ObjectKey right) => left.value == right.value;

        public static bool operator !=(ObjectKey left, ObjectKey right) => left.value != right.value;
    }
}
