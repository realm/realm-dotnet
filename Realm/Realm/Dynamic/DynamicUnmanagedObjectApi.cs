////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Realms.Dynamic
{
    public class DynamicUnmanagedObjectApi : DynamicObjectApi
    {
        private readonly UnmanagedAccessor _unmanagedAccessor;

        public DynamicUnmanagedObjectApi(UnmanagedAccessor unmanagedAccessor)
        {
            _unmanagedAccessor = unmanagedAccessor;
        }

        /// <inheritdoc/>
        public override RealmValue Get(string propertyName)
        {
            return _unmanagedAccessor.GetValue(propertyName);
        }

        /// <inheritdoc/>
        public override T Get<T>(string propertyName)
        {
            return _unmanagedAccessor.GetValue(propertyName).As<T>();
        }

        /// <inheritdoc/>
        public override bool TryGet(string propertyName, out RealmValue propertyValue)
        {
            return _unmanagedAccessor.TryGet(propertyName, out propertyValue);
        }

        /// <inheritdoc/>
        public override bool TryGet<T>(string propertyName, out T? propertyValue) where T : default
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IList<T> GetList<T>(string propertyName)
        {
            return _unmanagedAccessor.GetListValue<T>(propertyName);
        }

        /// <inheritdoc/>
        public override IDictionary<string, T> GetDictionary<T>(string propertyName)
        {
            return _unmanagedAccessor.GetDictionaryValue<T>(propertyName);
        }

        /// <inheritdoc/>
        public override ISet<T> GetSet<T>(string propertyName)
        {
            return _unmanagedAccessor.GetSetValue<T>(propertyName);
        }

        /// <inheritdoc/>
        public override void Set(string propertyName, RealmValue value)
        {
            _unmanagedAccessor.SetValue(propertyName, value);
        }

        /// <inheritdoc/>
        public override bool Unset(string propertyName)
        {
            return _unmanagedAccessor.Unset(propertyName);
        }

        /// <inheritdoc/>
        public override IQueryable<IRealmObjectBase> GetBacklinks(string propertyName) =>
           throw new NotSupportedException("Using the GetBacklinks is only possible for managed (persisted) objects.");

        /// <inheritdoc/>
        public override IQueryable<IRealmObjectBase> GetBacklinksFromType(string fromObjectType, string fromPropertyName) =>
            throw new NotSupportedException("Using the GetBacklinks is only possible for managed (persisted) objects.");
    }
}
