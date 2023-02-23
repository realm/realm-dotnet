////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Represents an accessor that encapsulates the methods and properties necessary for interfacing with the associated Realm object.
    /// </summary>
    public interface IRealmAccessor
    {
        /// <summary>
        /// Gets a value indicating whether the object has been associated with a Realm, either at creation or via
        /// <see cref="Realm.Add{T}(T, bool)">Realm.Add</see>.
        /// </summary>
        /// <value><c>true</c> if object belongs to a Realm; <c>false</c> if standalone.</value>
        bool IsManaged { get; }

        /// <summary>
        /// Gets a value indicating whether this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        /// <value><c>true</c> if managed and part of the Realm or unmanaged; <c>false</c> if managed but deleted.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether this object is frozen. Frozen objects are immutable
        /// and will not update when writes are made to the Realm. Unlike live objects, frozen
        /// objects can be used across threads.
        /// </summary>
        /// <value><c>true</c> if the object is frozen and immutable; <c>false</c> otherwise.</value>
        /// <seealso cref="FrozenObjectsExtensions.Freeze{T}(T)"/>
        bool IsFrozen { get; }

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this object belongs to, or <c>null</c> if it is unmanaged.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this object belongs to.</value>
        Realm? Realm { get; }

        /// <summary>
        /// Gets the <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        /// <value>A collection of properties describing the underlying schema of this object.</value>
        /// <remarks>
        /// This will always be available for models that use the Realm source generator tool (i.e. inheriting from <see cref="IRealmObject"/>,
        /// <see cref="IEmbeddedObject"/>, or <see cref="IAsymmetricObject"/>). It will be <c>null</c> for unmanaged objects if the models have
        /// been processed by the Fody weaver (i.e. inheriting from <see cref="RealmObject"/>, <see cref="EmbeddedObject"/>, or
        /// <see cref="AsymmetricObject"/>).
        /// </remarks>
        ObjectSchema? ObjectSchema { get; }

        /// <summary>
        /// Gets the number of objects referring to this one via either a to-one or to-many relationship.
        /// </summary>
        /// <remarks>
        /// This property is not observable so the <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> event will not fire when its value changes.
        /// </remarks>
        /// <value>The number of objects referring to this one.</value>
        int BacklinksCount { get; }

        /// <summary>
        /// Gets an object encompassing the dynamic API for this RealmObjectBase instance.
        /// </summary>
        /// <value>A <see cref="DynamicObjectApi"/> instance that wraps this RealmObject.</value>
        DynamicObjectApi DynamicApi { get; }

        /// <summary>
        /// Gets the value of a property of the object.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <remarks>This method cannot be used with collection properties. Please use one of the collection-specific methods for that.</remarks>
        /// <returns>The value of the property.</returns>
        RealmValue GetValue(string propertyName);

        /// <summary>
        /// Set the value of a property of the object.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="val">The value to set.</param>
        /// <remarks>This method cannot be used with collection properties.</remarks>
        void SetValue(string propertyName, RealmValue val);

        /// <summary>
        /// Set the value of the primary key of the object.
        /// </summary>
        /// <param name="propertyName">The name of the primary key property.</param>
        /// <param name="val">The value to set.</param>
        void SetValueUnique(string propertyName, RealmValue val);

        /// <summary>
        /// Gets the <see cref="IList{T}"/> property of the object.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the <see cref="IList{T}"/> property.</returns>
        IList<T> GetListValue<T>(string propertyName);

        /// <summary>
        /// Gets the <see cref="ISet{T}"/> property of the object.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the <see cref="ISet{T}"/> property.</returns>
        ISet<T> GetSetValue<T>(string propertyName);

        /// <summary>
        /// Gets the <see cref="IDictionary{String, TValue}"/> property of the object.
        /// </summary>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the <see cref="IDictionary{String, TValue}"/> property.</returns>
        IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName);

        /// <summary>
        /// Gets the value of a backlink property. This property must have been declared
        /// explicitly and annotated with <see cref="BacklinkAttribute"/>.
        /// </summary>
        /// <param name="propertyName">The name of the backlink property.</param>
        /// <typeparam name="T">The type of the object that is on the other end of the relationship.</typeparam>
        /// <returns>
        /// A queryable collection containing all objects pointing to this one via the
        /// property specified in <see cref="BacklinkAttribute.Property"/>.
        /// </returns>
        IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase;

        /// <summary>
        /// Gets the parent of the <see cref="IEmbeddedObject">embedded object</see>. It can be either another
        /// <see cref="IEmbeddedObject">embedded object</see>, a standalone <see cref="IRealmObject">realm object</see>,
        /// or an <see cref="IAsymmetricObject">asymmetric object</see>.
        /// </summary>
        /// <returns>The parent of the embedded object.</returns>
        IRealmObjectBase? GetParent();

        /// <summary>
        /// A method called internally to subscribe to the notifications for the associated object.
        /// </summary>
        /// <param name="notifyPropertyChangedDelegate">The delegate invoked when a notification is raised.</param>
        void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate);

        /// <summary>
        /// A method called internally to unsubscribe to the notifications for the associated object.
        /// </summary>
        void UnsubscribeFromNotifications();

        /// <summary>
        /// Gets the <see cref="TypeInfo"/> of the input object.
        /// </summary>
        /// <param name="obj">The object to derive the <see cref="TypeInfo"/> from.</param>
        /// <returns>
        /// The <see cref="TypeInfo"/> of the input object.
        /// </returns>
        TypeInfo GetTypeInfo(IRealmObjectBase obj);
    }
}
