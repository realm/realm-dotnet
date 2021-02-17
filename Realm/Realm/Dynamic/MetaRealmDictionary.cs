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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Realms.Dynamic
{
    internal class MetaRealmDictionary : DynamicMetaObject
    {
        private bool _isEmbedded;

        internal MetaRealmDictionary(Expression expression, object value) : base(expression, BindingRestrictions.Empty, value)
        {
            _isEmbedded = value.GetType().IsClosedGeneric(typeof(RealmDictionary<>), out var valueType) && valueType.Single().IsEmbeddedObject();
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (_isEmbedded)
            {
                throw new NotSupportedException("Can't set embedded objects directly. Instead use Realm.DynamicApi.SetEmbeddedObjectInDictionary.");
            }

            return base.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (_isEmbedded)
            {
                switch (binder.Name)
                {
                    case nameof(RealmDictionary<EmbeddedObject>.Add):
                        throw new NotSupportedException("Can't add embedded objects directly. Instead use Realm.DynamicApi.AddEmbeddedObjectToDictionary.");
                }
            }

            return base.BindInvokeMember(binder, args);
        }
    }
}