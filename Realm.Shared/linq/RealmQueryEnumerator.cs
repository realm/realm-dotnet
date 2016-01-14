/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;

namespace Realms
{
    internal class RealmQueryEnumerator<T> : IEnumerator<T> 
    {
        private long _rowIndex = 0;
        private RealmQueryVisitor _enumerating;
        private Realm _realm;
        private Type _retType = typeof(T);

        internal RealmQueryEnumerator(Realm realm, RealmQueryVisitor qv, Expression expression)
        {
            _realm = realm;
            _enumerating = qv;
            _enumerating.Visit(expression);
        }

        /// <summary>
        /// Return the current related object when iterating a related set.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">When we are not currently pointing at a valid item, either MoveNext has not been called for the first time or have iterated through all the items.</exception>
        public T Current { get; private set; }

        // also needed - https://msdn.microsoft.com/en-us/library/s793z9y2.aspx
        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        /// <summary>
        ///  Move the iterator to the next related object, starting "before" the first object.
        /// </summary>
        /// <returns>True only if can advance.</returns>
        public bool MoveNext()
        {
            var rowHandle = _enumerating.FindNextRowHandle(_rowIndex);
            if (rowHandle.IsInvalid)
            {
                Current = default(T);  // not sure about this
                return false;
            }
            _rowIndex = rowHandle.RowIndex + 1;
            var o = Activator.CreateInstance(_retType);
            ((RealmObject)o)._Manage(_realm, rowHandle);
            Current = (T)o;
            return true;
        }

        /// <summary>
        /// Reset the iter to before the first object, so MoveNext will move to it.
        /// </summary>
        public void Reset()
        {
            _rowIndex = 0;  // by definition BEFORE first item
        }

        /// <summary>
        /// Standard Dispose with no side-effects.
        /// </summary>
        public void Dispose() 
        {
        }
    }
}

