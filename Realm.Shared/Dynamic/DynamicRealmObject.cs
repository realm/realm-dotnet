using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace Realms.Dynamic
{
    internal class DynamicRealmObject : RealmObject, IDynamicMetaObjectProvider
    {
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new MetaRealmObject(parameter, this);
        }
    }
}

