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
using Realms.Schema;
using Realms.Weaving;

namespace Realms.Dynamic
{
    internal class DynamicRealmObjectHelper : IRealmObjectHelper
    {
        private static readonly DynamicRealmObjectHelper _embeddedInstance = new DynamicRealmObjectHelper(ObjectSchema.ObjectSchemaType.Embedded);
        private static readonly DynamicRealmObjectHelper _objectInstance = new DynamicRealmObjectHelper(ObjectSchema.ObjectSchemaType.TopLevel);
        private static readonly DynamicRealmObjectHelper _asymmetricInstance = new DynamicRealmObjectHelper(ObjectSchema.ObjectSchemaType.TopLevelAsymmetric);

        private readonly ObjectSchema.ObjectSchemaType _schemaType;

        internal static DynamicRealmObjectHelper Instance(ObjectSchema schema) =>
            schema.RealmSchemaType switch
            {
                ObjectSchema.ObjectSchemaType.TopLevel => _objectInstance,
                ObjectSchema.ObjectSchemaType.Embedded => _embeddedInstance,
                ObjectSchema.ObjectSchemaType.TopLevelAsymmetric => _asymmetricInstance,
                _ => throw new NotSupportedException($"{schema.RealmSchemaType} type not supported, yet."),
            };

        private DynamicRealmObjectHelper(ObjectSchema.ObjectSchemaType realmSchemaType)
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
                ObjectSchema.ObjectSchemaType.TopLevel => new DynamicRealmObject(),
                ObjectSchema.ObjectSchemaType.Embedded => new DynamicEmbeddedObject(),
                ObjectSchema.ObjectSchemaType.TopLevelAsymmetric => new DynamicAsymmetricObject(),
                _ => throw new NotSupportedException($"{_schemaType} type not supported, yet."),
            };

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            if (!instance.ObjectSchema.PrimaryKeyProperty.HasValue)
            {
                value = null;
                return false;
            }

            value = instance.ObjectSchema.PrimaryKeyProperty.Value.PropertyInfo.GetValue(instance);
            return true;
        }
    }
}
