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
using Realms.Weaving;

namespace Realms.Dynamic
{
    internal class DynamicRealmObjectHelper : IRealmObjectHelper
    {
        private static readonly DynamicRealmObjectHelper _embeddedInstance = new DynamicRealmObjectHelper(embedded: true);
        private static readonly DynamicRealmObjectHelper _objectInstance = new DynamicRealmObjectHelper(embedded: false);

        private readonly bool _embedded;

        internal static DynamicRealmObjectHelper Instance(bool embedded) => embedded ? _embeddedInstance : _objectInstance;

        private DynamicRealmObjectHelper(bool embedded)
        {
            _embedded = embedded;
        }

        public void CopyToRealm(RealmObjectBase instance, bool update, bool setPrimaryKey)
        {
            throw new NotSupportedException("DynamicRealmObjectHelper cannot exist in unmanaged state, so CopyToRealm should not be called ever.");
        }

        public RealmObjectBase CreateInstance()
        {
            if (_embedded)
            {
                return new DynamicEmbeddedObject();
            }

            return new DynamicRealmObject();
        }

        public bool TryGetPrimaryKeyValue(RealmObject instance, out object value)
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