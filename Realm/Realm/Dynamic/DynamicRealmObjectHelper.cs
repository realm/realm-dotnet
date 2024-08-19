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
using Realms.Helpers;
using Realms.Schema;
using Realms.Weaving;

namespace Realms.Dynamic
{
    internal class DynamicRealmObjectHelper : IRealmObjectHelper
    {
        private static readonly DynamicRealmObjectHelper _embeddedInstance = new(ObjectSchema.ObjectType.EmbeddedObject);
        private static readonly DynamicRealmObjectHelper _objectInstance = new(ObjectSchema.ObjectType.RealmObject);

        private readonly ObjectSchema.ObjectType _schemaType;

        internal static DynamicRealmObjectHelper Instance(ObjectSchema schema) =>
            schema.BaseType switch
            {
                ObjectSchema.ObjectType.RealmObject => _objectInstance,
                ObjectSchema.ObjectType.EmbeddedObject => _embeddedInstance,
                _ => throw new NotSupportedException($"{schema.BaseType} type not supported, yet."),
            };

        private DynamicRealmObjectHelper(ObjectSchema.ObjectType realmSchemaType)
        {
            _schemaType = realmSchemaType;
        }

        public void CopyToRealm(IRealmObjectBase instance, bool update, bool setPrimaryKey)
        {
            throw new NotSupportedException("DynamicRealmObjectHelper cannot exist in unmanaged state, so CopyToRealm should not be called ever.");
        }

        public IRealmObjectBase CreateInstance() =>
            _schemaType switch
            {
                ObjectSchema.ObjectType.RealmObject => new DynamicRealmObject(),
                ObjectSchema.ObjectType.EmbeddedObject => new DynamicEmbeddedObject(),
                _ => throw new NotSupportedException($"{_schemaType} type not supported, yet."),
            };

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out RealmValue value)
        {
            Argument.Ensure(instance.IsManaged, "DynamicRealmObjectHelper should only operate on managed instances", nameof(instance));

            if (!instance.ObjectSchema.PrimaryKeyProperty.HasValue)
            {
                value = RealmValue.Null;
                return false;
            }

            value = instance.Accessor.GetValue(instance.ObjectSchema.PrimaryKeyProperty.Value.Name);
            return true;
        }

        public ManagedAccessor? CreateAccessor() => null;
    }
}
