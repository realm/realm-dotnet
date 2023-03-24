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

using System.Runtime.InteropServices;
using MongoDB.Bson;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct UserApiKey
    {
        internal static readonly int Size = Marshal.SizeOf<UserApiKey>();

        private PrimitiveValue id;

        private PrimitiveValue key;

        private PrimitiveValue name;

        [MarshalAs(UnmanagedType.U1)]
        public bool disabled;

        public ObjectId Id => ObjectId.Parse(id.AsString());

        public string? Key => key.Type == RealmValueType.Null ? null : key.AsString();

        public string Name => name.AsString();
    }
}
