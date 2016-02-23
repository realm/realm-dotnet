﻿/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;

namespace Realms
{
    internal class RealmResultsEnumerator<T> : IEnumerator<T> 
    {
        private long _rowIndex = 0;
        private RealmResultsVisitor _enumerating;
        private Realm _realm;
        private Type _retType = typeof(T);

        internal RealmResultsEnumerator(Realm realm, RealmResultsVisitor qv, Expression expression)
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
            var nextObj = _enumerating.FindNextObject(ref _rowIndex);
            Current = (T)((object)nextObj);
            return nextObj != null;
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

