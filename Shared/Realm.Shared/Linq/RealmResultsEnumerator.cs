////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

namespace Realms
{
    // This is now more of a skinny wrapper on top of the ObjectStore Results class.
    internal class RealmResultsEnumerator<T> : IEnumerator<T>
    {
        private long _index = -1;  // must match Reset(), zero-based with no gaps indexing an ObjectStore Results
        private ResultsHandle _enumeratingResults = null;
        private Realm _realm;
        private readonly Schema.ObjectSchema _schema;

        internal RealmResultsEnumerator(Realm realm, ResultsHandle rh, Schema.ObjectSchema schema)
        {
            _realm = realm;
            _enumeratingResults = rh;
            _schema = schema;
        }

        /// <summary>
        /// Gets the current related object when iterating a related set.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When we are not currently pointing at a valid item, either MoveNext has not been called for the first time or have iterated through all the items.</exception>
        public T Current { get; private set; }

        // also needed - https://msdn.microsoft.com/en-us/library/s793z9y2.aspx
        object IEnumerator.Current => this.Current;

        /// <summary>
        ///  Move the iterator to the next related object, starting "before" the first object.
        /// </summary>
        /// <remarks>
        /// Is a factory for RealmObjects, loading a new one and updating <c>Current</c>. 
        /// </remarks>
        /// <returns>True only if can advance.</returns>
        public bool MoveNext()
        {
            if (_enumeratingResults == null)
            {
                return false;
            }

            ++_index;
            var rowPtr = _enumeratingResults.GetObjectAtIndex(_index);
            if (rowPtr == IntPtr.Zero)
            {
                Current = (T)(object)null;
                return false;
            }

            Current = (T)(object)_realm.MakeObject(_schema.Name, rowPtr);
            return true;
        }

        /// <summary>
        /// Reset the iterator to before the first object, so MoveNext will move to it.
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