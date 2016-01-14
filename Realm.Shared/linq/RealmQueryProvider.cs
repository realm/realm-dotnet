/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Linq.Expressions;

namespace Realms
{
    internal class RealmQueryProvider : QueryProvider
    {
        internal Realm _realm;

        internal RealmQueryProvider(Realm realm)
        {
            _realm = realm;
        }

        internal RealmQueryVisitor MakeVisitor()
        {
            return new RealmQueryVisitor(_realm);
        }

        public override object Execute(Expression expression, Type returnType)
        {
            return null; // new RealmQueryVisitor().Process(_realm, expression, returnType);
        }
    }
}