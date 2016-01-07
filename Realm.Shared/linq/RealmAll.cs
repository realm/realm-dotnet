/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Realms
{
    public class RealmAll<T> : RealmQuery<T>,  IQueryable<T>
    {
        Realm _realm;
        internal RealmAll(Realm realm) : base(null, null)
        {
            // lazily init this._provider and this.Expression only if needed
            _realm = realm;
        }


        /// <summary>
        /// Count all objects of the parameterised type, faster than a search.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>.
        /// </remarks>
        public int Count()
        {
            // use the type captured at build based on generic T
            var tableHandle = _realm._tableHandles[ElementType];
            return (int) NativeTable.count_all(tableHandle);
        }

        // do what our parent does in ctor but we avoided in case not needed
        void CompleteLazyInit()
        {
            _provider = new RealmQueryProvider(_realm);
            this.Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmQuery to be used in a <c>foreach</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public new IEnumerator<T> GetEnumerator()
        {
            if (_provider == null)
                CompleteLazyInit();
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_provider == null)
                CompleteLazyInit();
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }

    }
}

