/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 
 
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    /// <summary>
    /// Iterable collection of one kind of RealmObject resulting from Realm.All or from a LINQ query expression.
    /// </summary>
    /// <typeparam name="T">Type of the RealmObject which is being returned.</typeparam>
    public class RealmResults<T> : IQueryable<T>
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => null;

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmResults to be used in a <c>foreach</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }


        /// <summary>
        /// Count all objects if created by <see cref="Realm.All"/> of the parameterised type, faster than a search.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>.
        /// </remarks>
        public int Count()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }    
    }
}