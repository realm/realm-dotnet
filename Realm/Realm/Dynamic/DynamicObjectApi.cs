////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Realms.Helpers;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// A class that exposes a set of API to access the data in a managed RealmObject dynamically.
    /// </summary>
    /// <see cref="RealmObjectBase.DynamicApi"/>
    public abstract class DynamicObjectApi
    {
        //TODO Add docs
        public abstract RealmValue Get(string propertyName);

        /// <summary>
        /// Gets the value of the property <paramref name="propertyName"/> and casts it to
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting to <see cref="RealmValue"/> is always valid. When the property is of type
        /// object, casting to <see cref="IRealmObjectBase"/> is always valid.
        /// </remarks>
        public abstract T Get<T>(string propertyName);

        //TODO Add docs
        public abstract bool TryGet(string propertyName, out RealmValue propertyValue);

        //TODO Add docs
        public abstract bool TryGet<T>(string propertyName, out T? propertyValue);

        /// <summary>
        /// Sets the value of the property at <paramref name="propertyName"/> to
        /// <paramref name="value"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The new value of the property.</param>
        public abstract void Set(string propertyName, RealmValue value);

        //TODO Add docs
        public abstract bool Unset(string propertyName);

        /// <summary>
        /// Gets the value of a backlink property. This property must have been declared
        /// explicitly and annotated with <see cref="BacklinkAttribute"/>.
        /// </summary>
        /// <param name="propertyName">The name of the backlink property.</param>
        /// <returns>
        /// A queryable collection containing all objects pointing to this one via the
        /// property specified in <see cref="BacklinkAttribute.Property"/>.
        /// </returns>
        public abstract IQueryable<IRealmObjectBase> GetBacklinks(string propertyName);

        /// <summary>
        /// Gets a collection of all the objects that link to this object in the specified relationship.
        /// </summary>
        /// <param name="fromObjectType">The type of the object that is on the other end of the relationship.</param>
        /// <param name="fromPropertyName">The property that is on the other end of the relationship.</param>
        /// <returns>
        /// A queryable collection containing all objects of <paramref name="fromObjectType"/> that link
        /// to the current object via <paramref name="fromPropertyName"/>.
        /// </returns>
        public abstract IQueryable<IRealmObjectBase> GetBacklinksFromType(string fromObjectType, string fromPropertyName);

        /// <summary>
        /// Gets a <see cref="IList{T}"/> property.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="propertyName">The name of the list property.</param>
        /// <returns>The value of the list property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting the elements to <see cref="RealmValue"/> is always valid. When the collection
        /// contains objects, casting to <see cref="IRealmObjectBase"/> is always valid.
        /// </remarks>
        public abstract IList<T> GetList<T>(string propertyName);

        /// <summary>
        /// Gets a <see cref="ISet{T}"/> property.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the Set.</typeparam>
        /// <param name="propertyName">The name of the Set property.</param>
        /// <returns>The value of the Set property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting the elements to <see cref="RealmValue"/> is always valid. When the collection
        /// contains objects, casting to <see cref="IRealmObjectBase"/> is always valid.
        /// </remarks>
        public abstract ISet<T> GetSet<T>(string propertyName);

        /// <summary>
        /// Gets a <see cref="IDictionary{TKey, TValue}"/> property.
        /// </summary>
        /// <typeparam name="T">The type of the values in the dictionary.</typeparam>
        /// <param name="propertyName">The name of the dictionary property.</param>
        /// <returns>The value of the dictionary property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting the values to <see cref="RealmValue"/> is always valid. When the collection
        /// contains objects, casting to <see cref="IRealmObjectBase"/> is always valid.
        /// </remarks>
        public abstract IDictionary<string, T> GetDictionary<T>(string propertyName);
    }
}
