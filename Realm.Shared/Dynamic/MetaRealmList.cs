using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Realms.Dynamic
{
    internal class MetaRealmList : DynamicMetaObject
    {
        internal MetaRealmList(Expression expression, IRealmList value) : base(expression, BindingRestrictions.Empty, value)
        {
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            var limitedSelf = Expression;
            if (limitedSelf.Type != LimitType)
            {
                limitedSelf = Expression.Convert(limitedSelf, LimitType);
            }

            var indexer = LimitType.GetProperty("Item");
            Expression expression = Expression.Call(limitedSelf, indexer.GetGetMethod(), indexes.Select(i => i.Expression));
            if (binder.ReturnType != expression.Type)
            {
                expression = Expression.Convert(expression, binder.ReturnType);
            }

            return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }
    }
}

