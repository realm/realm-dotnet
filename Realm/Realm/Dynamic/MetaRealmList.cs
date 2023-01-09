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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Realms.Dynamic
{
    internal class MetaRealmList : DynamicMetaObject
    {
        internal MetaRealmList(Expression expression, object value) : base(expression, BindingRestrictions.Empty, value)
        {
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            var limitedSelf = Expression;
            if (limitedSelf.Type != LimitType)
            {
                limitedSelf = Expression.Convert(limitedSelf, LimitType);
            }

            var indexer = LimitType.GetProperty("Item")?.GetGetMethod() ?? throw new NotSupportedException("Couldn't find indexer for list");
            Expression expression = Expression.Call(limitedSelf, indexer, indexes.Select(i => i.Expression));
            if (binder.ReturnType != expression.Type)
            {
                expression = Expression.Convert(expression, binder.ReturnType);
            }

            return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (Value is IRealmCollection<IEmbeddedObject>)
            {
                throw new NotSupportedException("Can't set embedded objects directly. Instead use Realm.DynamicApi.SetEmbeddedObjectInList.");
            }

            return base.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (Value is IRealmCollection<IEmbeddedObject>)
            {
                switch (binder.Name)
                {
                    case "Add":
                        throw new NotSupportedException("Can't add embedded objects directly. Instead use Realm.DynamicApi.AddEmbeddedObjectToList.");
                    case "Insert":
                        throw new NotSupportedException("Can't insert embedded objects directly. Instead use Realm.DynamicApi.InsertEmbeddedObjectInList.");
                }
            }

            return base.BindInvokeMember(binder, args);
        }
    }
}
