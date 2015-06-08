using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realm
{
    public class RealmList<T> : IQueryable where T : RealmObject
    {
        public Type ElementType
        {
            get { throw new NotImplementedException(); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryProvider Provider
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
