using System;
using System.Linq.Expressions;

namespace RealmNet
{
    public class RealmQueryProvider : QueryProvider
    {
        private Realm _realm;

        public RealmQueryProvider(Realm realm)
        {
            _realm = realm;
        }

        public override object Execute(Expression expression, Type returnType)
        {
            return new RealmQueryVisitor().Process(_realm, expression, returnType);
        }
    }
}