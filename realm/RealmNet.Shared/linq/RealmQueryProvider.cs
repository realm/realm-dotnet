using System;
using System.Linq.Expressions;

namespace RealmNet
{
    public class RealmQueryProvider : QueryProvider
    {
        private Realm _realm;
        private ICoreProvider _coreProvider;

        public RealmQueryProvider(Realm realm, ICoreProvider coreProvider)
        {
            _realm = realm;
            _coreProvider = coreProvider;
        }

        public override object Execute(Expression expression, Type returnType)
        {
            return new RealmQueryVisitor().Process(_realm, _coreProvider, expression, returnType);
        }
    }
}