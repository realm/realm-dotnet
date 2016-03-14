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
    /// <summary>
    ///  This is now more of a skinny wrapper on top of the ObjectStore Results class.
    /// </summary>
    internal class RealmResultsEnumerator<T> : IEnumerator<T> 
    {
        private long _index = -1;  // must match Reset(), zero-based with no gaps indexing an ObjectStore Results
        private ResultsHandle _enumeratingResults = null;
        private Realm _realm;
        private Type _retType = typeof(T);


        internal RealmResultsEnumerator(Realm realm, ResultsHandle rh)
        {
            _realm = realm;
            _enumeratingResults = rh;
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
            if (_enumeratingResults == null)
                return false;
            
            ++_index;
            var rowHandle = NativeResults.get_row(_enumeratingResults, (IntPtr)_index);
            object nextObj = null;
            if (!rowHandle.IsInvalid)                 
                nextObj = _realm.MakeObjectForRow(_retType, rowHandle);
            Current = (T)nextObj;
            return nextObj != null;
        }


        /// <summary>
        /// Reset the iter to before the first object, so MoveNext will move to it.
        /// </summary>
        public void Reset()
        {
            _index = -1;  // by definition BEFORE first item
        }

        /// <summary>
        /// Standard Dispose with no side-effects.
        /// </summary>
        public void Dispose() 
        {
        }
    }
}

