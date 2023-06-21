////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

using System.ComponentModel;
using Realms.Schema;
using Realms.Weaving;

namespace Realms.Dynamic
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Ignored]
    public abstract class DynamicRealmObjectBase : IRealmObjectBase
    {
        private IRealmAccessor _accessor = null!;

        public bool IsManaged => true;

        public bool IsValid => Accessor.IsValid;

        public bool IsFrozen => Accessor.IsFrozen;

        public Realm? Realm => Accessor.Realm;

        public ObjectSchema? ObjectSchema => Accessor.ObjectSchema;

        public DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        public int BacklinksCount => Accessor.BacklinksCount;

        public IRealmAccessor Accessor => _accessor;

        public void SetManagedAccessor(IRealmAccessor accessor, IRealmObjectHelper? helper = null, bool update = false, bool skipDefaults = false)
        {
            _accessor = accessor;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is InvalidObject)
            {
                return !IsValid;
            }

            if (obj is not IRealmObjectBase iro)
            {
                return false;
            }

            return _accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => _accessor.GetHashCode();

        public override string? ToString() => _accessor.ToString();
    }
}
