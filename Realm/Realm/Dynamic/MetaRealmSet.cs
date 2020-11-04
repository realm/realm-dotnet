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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Realms.Dynamic
{
    internal class MetaRealmSet : DynamicMetaObject
    {
        internal MetaRealmSet(Expression expression, object value) : base(expression, BindingRestrictions.Empty, value)
        {
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (Value is IRealmCollection<EmbeddedObject>)
            {
                switch (binder.Name)
                {
                    case nameof(ISet<EmbeddedObject>.Add):
                        throw new NotSupportedException("Can't add embedded objects directly. Instead use Realm.DynamicApi.AddEmbeddedObjectToSet.");

                    case nameof(ISet<EmbeddedObject>.UnionWith):
                    case nameof(ISet<EmbeddedObject>.SymmetricExceptWith):
                        throw new NotSupportedException("Set methods that may result in adding embedded objects to a managed collection are not supported.");
                }
            }

            return base.BindInvokeMember(binder, args);
        }
    }
}