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

using System.Runtime.InteropServices;
using Realms.Native;
using ManagedSubscription = Realms.Sync.Subscription;

namespace Realms.Sync.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Subscription
    {
        private PrimitiveValue id;

        private PrimitiveValue name;

        private PrimitiveValue object_type;

        private PrimitiveValue query;

        private PrimitiveValue created_at;

        private PrimitiveValue updated_at;

        [MarshalAs(UnmanagedType.I1)]
        private bool has_value;

        public ManagedSubscription? ManagedSubscription
        {
            get
            {
                if (!has_value)
                {
                    return null;
                }

                return new(id: id.AsObjectId(),
                    name: name.Type == RealmValueType.Null ? null : name.AsString(),
                    objectType: object_type.AsString(),
                    query: query.AsString(),
                    createdAt: created_at.AsDate(),
                    updatedAt: updated_at.AsDate());
            }
        }
    }
}
