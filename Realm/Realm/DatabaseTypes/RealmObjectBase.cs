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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Realms.DataBinding;
using Realms.Extensions;
using Realms.Helpers;
using Realms.Native;
using Realms.Schema;
using Realms.Weaving;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    public abstract class RealmObjectBase
        : IRealmObject,
          INotifyPropertyChanged,
          IReflectableType
    {
        private Realm _realm;  //TODO These 3 fields need to be removed later (here to continue supporting the metadataRealmObject)

        private ObjectHandle _objectHandle;

        private Metadata _metadata;

        private IRealmAccessor _accessor;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event PropertyChangedEventHandler _propertyChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (IsManaged && _propertyChanged == null)
                {
                    SubscribeForNotifications();
                }

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                if (IsManaged &&
                    _propertyChanged == null)
                {
                    UnsubscribeFromNotifications();
                }
            }
        }

        //TODO This is not an autoimplemented property because otherwise the Mongodb.Bson Json serializer serializes it. Need to investigate
        [IgnoreDataMember, XmlIgnore]
        public IRealmAccessor Accessor => _accessor;

        /// <summary>
        /// Gets a value indicating whether the object has been associated with a Realm, either at creation or via
        /// <see cref="Realm.Add{T}(T, bool)"/>.
        /// </summary>
        /// <value><c>true</c> if object belongs to a Realm; <c>false</c> if standalone.</value>
        [IgnoreDataMember]
        public bool IsManaged => Accessor.IsManaged;

        /// <summary>
        /// Gets an object encompassing the dynamic API for this RealmObjectBase instance.
        /// </summary>
        /// <value>A <see cref="Dynamic"/> instance that wraps this RealmObject.</value>
        [IgnoreDataMember]
        public Dynamic DynamicApi => Accessor.DynamicApi;

        /// <summary>
        /// Gets a value indicating whether this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        /// <value><c>true</c> if managed and part of the Realm or unmanaged; <c>false</c> if managed but deleted.</value>
        [IgnoreDataMember]
        public bool IsValid => Accessor.IsValid;

        /// <summary>
        /// Gets a value indicating whether this object is frozen. Frozen objects are immutable
        /// and will not update when writes are made to the Realm. Unlike live objects, frozen
        /// objects can be used across threads.
        /// </summary>
        /// <value><c>true</c> if the object is frozen and immutable; <c>false</c> otherwise.</value>
        /// <seealso cref="FrozenObjectsExtensions.Freeze{T}(T)"/>
        [IgnoreDataMember]
        public bool IsFrozen => Accessor.IsFrozen;

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this object belongs to, or <c>null</c> if it is unmanaged.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this object belongs to.</value>
        [IgnoreDataMember]
        public Realm Realm => Accessor.Realm;

        /// <summary>
        /// Gets the <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        /// <value>A collection of properties describing the underlying schema of this object.</value>
        [IgnoreDataMember, XmlIgnore] // XmlIgnore seems to be needed here as IgnoreDataMember is not sufficient for XmlSerializer.
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        /// <summary>
        /// Gets the number of objects referring to this one via either a to-one or to-many relationship.
        /// </summary>
        /// <remarks>
        /// This property is not observable so the <see cref="PropertyChanged"/> event will not fire when its value changes.
        /// </remarks>
        /// <value>The number of objects referring to this one.</value>
        [IgnoreDataMember]
        public int BacklinksCount => Accessor.BacklinksCount;

        internal RealmObjectBase FreezeImpl() => Accessor.FreezeImpl();

        internal RealmObjectBase()
        {
            _accessor = new UnmanagedAccessor();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RealmObjectBase"/> class.
        /// </summary>
        ~RealmObjectBase()
        {
            //TODO Probably we can dispose the Accessor here
            UnsubscribeFromNotifications();
        }

        internal void SetOwner(Realm realm, ObjectHandle objectHandle, Metadata metadata)
        {
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = metadata;  //TODO needs to be removed later
            _accessor = new ManagedAccessor(realm, objectHandle, metadata, RaisePropertyChanged, this is EmbeddedObject, this);

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }
        }

#pragma warning disable SA1600 // Elements should be documented

        protected RealmValue GetValue(string propertyName)
        {
            return Accessor.GetValue(propertyName);
        }

        protected void SetValue(string propertyName, RealmValue val)
        {
            Accessor.SetValue(propertyName, val);
        }

        protected void SetValueUnique(string propertyName, RealmValue val)
        {
            Accessor.SetValueUnique(propertyName, val);
        }

        protected internal IList<T> GetListValue<T>(string propertyName)
        {
            return Accessor.GetListValue<T>(propertyName);
        }

        protected internal ISet<T> GetSetValue<T>(string propertyName)
        {
            return Accessor.GetSetValue<T>(propertyName);
        }

        protected internal IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return Accessor.GetDictionaryValue<TValue>(propertyName);
        }

        protected IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : RealmObjectBase
        {
            return Accessor.GetBacklinks<T>(propertyName);
        }

        internal RealmResults<T> GetBacklinksForHandle<T>(string propertyName, ResultsHandle resultsHandle)
            where T : RealmObjectBase
        {
            return (Accessor as ManagedAccessor).GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }

#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// Returns all the objects that link to this object in the specified relationship.
        /// </summary>
        /// <param name="objectType">The type of the object that is on the other end of the relationship.</param>
        /// <param name="property">The property that is on the other end of the relationship.</param>
        /// <returns>A queryable collection containing all objects of <c>objectType</c> that link to the current object via <c>property</c>.</returns>
        [Obsolete("Use realmObject.DynamicApi.GetBacklinksFromType() instead.")]
        public IQueryable<dynamic> GetBacklinks(string objectType, string property) => DynamicApi.GetBacklinksFromType(objectType, property);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            // If parameter is null, return false.
            if (obj is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Accessor.ObjectEquals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // _hashCode is only set for managed objects - for unmanaged ones, we
            // fall back to the default behavior.
            return Accessor.HashCode?.Value ?? base.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Accessor.GetStringDescription(GetType().Name);
        }

        /// <summary>
        /// Allows you to raise the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed. If not specified, we'll use the caller name.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Called when a property has changed on this class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <remarks>
        /// For this method to be called, you need to have first subscribed to <see cref="PropertyChanged"/>.
        /// This can be used to react to changes to the current object, e.g. raising <see cref="PropertyChanged"/> for computed properties.
        /// </remarks>
        /// <example>
        /// <code>
        /// class MyClass : RealmObject
        /// {
        ///     public int StatusCodeRaw { get; set; }
        ///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
        ///     protected override void OnPropertyChanged(string propertyName)
        ///     {
        ///         if (propertyName == nameof(StatusCodeRaw))
        ///         {
        ///             RaisePropertyChanged(nameof(StatusCode));
        ///         }
        ///     }
        /// }
        /// </code>
        /// Here, we have a computed property that depends on a persisted one. In order to notify any <see cref="PropertyChanged"/>
        /// subscribers that <c>StatusCode</c> has changed, we override <see cref="OnPropertyChanged"/> and
        /// raise <see cref="PropertyChanged"/> manually by calling <see cref="RaisePropertyChanged"/>.
        /// </example>
        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        /// <summary>
        /// Called when the object has been managed by a Realm.
        /// </summary>
        /// <remarks>
        /// This method will be called either when a managed object is materialized or when an unmanaged object has been
        /// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
        /// it is not yet clear whether the object is managed or not.
        /// </remarks>
        protected internal virtual void OnManaged()
        {
        }

        private void SubscribeForNotifications()
        {
            Accessor.SubscribeForNotifications();
        }

        private void UnsubscribeFromNotifications()
        {
            Accessor.UnsubscribeFromNotifications();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo()
        {
            return TypeInfoHelper.GetInfo(this);
        }

        //TODO Should probably move it out of this class, to its own file and rename to RealmObjectMetadata
        internal class Metadata
        {
            internal readonly TableKey TableKey;

            internal readonly IRealmObjectHelper Helper;

            internal readonly IReadOnlyDictionary<string, IntPtr> PropertyIndices;

            internal readonly ObjectSchema Schema;

            public Metadata(TableKey tableKey, IRealmObjectHelper helper, IDictionary<string, IntPtr> propertyIndices, ObjectSchema schema)
            {
                TableKey = tableKey;
                Helper = helper;
                PropertyIndices = new ReadOnlyDictionary<string, IntPtr>(propertyIndices);
                Schema = schema;
            }
        }

        //TODO Should probably move out of this class, to its own file
        /// <summary>
        /// A class that exposes a set of API to access the data in a managed RealmObject dynamically.
        /// </summary>
        public struct Dynamic
        {
            private readonly IRealmObject _realmObject;

            internal Dynamic(IRealmObject ro)
            {
                _realmObject = ro;
            }

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
            /// object, casting to <see cref="RealmObjectBase"/> is always valid.
            /// </remarks>
            public T Get<T>(string propertyName)
            {
                var property = GetProperty(propertyName);

                if (property.Type.IsComputed())
                {
                    throw new NotSupportedException(
                        $"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (backlinks collection) and can't be accessed using {nameof(Dynamic)}.{nameof(Get)}. Use {nameof(GetBacklinks)} instead.");
                }

                if (property.Type.IsCollection(out var collectionType))
                {
                    var collectionMethodName = collectionType switch
                    {
                        PropertyType.Array => "GetList",
                        PropertyType.Set => "GetSet",
                        PropertyType.Dictionary => "GetDictionary",
                        _ => throw new NotSupportedException($"Invalid collection type received: {collectionType}")
                    };

                    throw new NotSupportedException(
                        $"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} and can't be accessed using {nameof(Dynamic)}.{nameof(Get)}. Use {collectionMethodName} instead.");
                }

                return _realmObject.Accessor.GetValue(propertyName).As<T>();
            }

            /// <summary>
            /// Sets the value of the property at <paramref name="propertyName"/> to
            /// <paramref name="value"/>.
            /// </summary>
            /// <param name="propertyName">The name of the property to set.</param>
            /// <param name="value">The new value of the property.</param>
            public void Set(string propertyName, RealmValue value)
            {
                var property = GetProperty(propertyName);

                if (property.Type.IsComputed())
                {
                    throw new NotSupportedException(
                        $"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (backlinks collection) and can't be set directly");
                }

                if (property.Type.IsCollection(out _))
                {
                    throw new NotSupportedException(
                        $"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (collection) and can't be set directly.");
                }

                if (!property.Type.IsNullable() && value.Type == RealmValueType.Null)
                {
                    throw new ArgumentException($"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} which is not nullable, but the supplied value is <null>.");
                }

                if (!property.Type.IsRealmValue() && value.Type != RealmValueType.Null && property.Type.ToRealmValueType() != value.Type)
                {
                    throw new ArgumentException($"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} but the supplied value is {value.AsAny().GetType().Name} ({value}).");
                }

                if (property.IsPrimaryKey)
                {
                    _realmObject.Accessor.SetValueUnique(propertyName, value);
                }
                else
                {
                    _realmObject.Accessor.SetValue(propertyName, value);
                }
            }

            /// <summary>
            /// Gets the value of a backlink property. This property must have been declared
            /// explicitly and annotated with <see cref="BacklinkAttribute"/>.
            /// </summary>
            /// <param name="propertyName">The name of the backlink property.</param>
            /// <returns>
            /// A queryable collection containing all objects pointing to this one via the
            /// property specified in <see cref="BacklinkAttribute.Property"/>.
            /// </returns>
            public IQueryable<RealmObjectBase> GetBacklinks(string propertyName)
            {
                var property = GetProperty(propertyName, PropertyTypeEx.IsComputed);

                var resultsHandle = _realmObject.GetObjectHandle().GetBacklinks(propertyName, _realmObject.GetObjectMetadata());

                var relatedMeta = _realmObject.Realm.Metadata[property.ObjectType];
                if (relatedMeta.Schema.IsEmbedded)
                {
                    return new RealmResults<EmbeddedObject>(_realmObject.Realm, resultsHandle, relatedMeta);
                }

                return new RealmResults<RealmObject>(_realmObject.Realm, resultsHandle, relatedMeta);
            }

            /// <summary>
            /// Gets a collection of all the objects that link to this object in the specified relationship.
            /// </summary>
            /// <param name="fromObjectType">The type of the object that is on the other end of the relationship.</param>
            /// <param name="fromPropertyName">The property that is on the other end of the relationship.</param>
            /// <returns>
            /// A queryable collection containing all objects of <paramref name="fromObjectType"/> that link
            /// to the current object via <paramref name="fromPropertyName"/>.
            /// </returns>
            public IQueryable<RealmObjectBase> GetBacklinksFromType(string fromObjectType, string fromPropertyName)
            {
                Argument.Ensure(_realmObject.Realm.Metadata.TryGetValue(fromObjectType, out var relatedMeta), $"Could not find schema for type {fromObjectType}", nameof(fromObjectType));

                var resultsHandle = _realmObject.GetObjectHandle().GetBacklinksForType(relatedMeta.TableKey, fromPropertyName, relatedMeta);
                if (relatedMeta.Schema.IsEmbedded)
                {
                    return new RealmResults<EmbeddedObject>(_realmObject.Realm, resultsHandle, relatedMeta);
                }

                return new RealmResults<RealmObject>(_realmObject.Realm, resultsHandle, relatedMeta);
            }

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
            /// contains objects, casting to <see cref="RealmObjectBase"/> is always valid.
            /// </remarks>
            public IList<T> GetList<T>(string propertyName)
            {
                var property = GetProperty(propertyName, PropertyTypeEx.IsList);

                var result = _realmObject.GetObjectHandle().GetList<T>(_realmObject.Realm, propertyName, _realmObject.GetObjectMetadata(), property.ObjectType);
                result.IsDynamic = true;
                return result;
            }

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
            /// contains objects, casting to <see cref="RealmObjectBase"/> is always valid.
            /// </remarks>
            public ISet<T> GetSet<T>(string propertyName)
            {
                var property = GetProperty(propertyName, PropertyTypeEx.IsSet);

                var result = _realmObject.GetObjectHandle().GetSet<T>(_realmObject.Realm, propertyName, _realmObject.GetObjectMetadata(), property.ObjectType);
                result.IsDynamic = true;
                return result;
            }

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
            /// contains objects, casting to <see cref="RealmObjectBase"/> is always valid.
            /// </remarks>
            public IDictionary<string, T> GetDictionary<T>(string propertyName)
            {
                var property = GetProperty(propertyName, PropertyTypeEx.IsDictionary);

                var result = _realmObject.GetObjectHandle().GetDictionary<T>(_realmObject.Realm, propertyName, _realmObject.GetObjectMetadata(), property.ObjectType);
                result.IsDynamic = true;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Property GetProperty(string propertyName)
            {
                if (!_realmObject.ObjectSchema.TryFindProperty(propertyName, out var property))
                {
                    throw new MissingMemberException($"Property {propertyName} does not exist on RealmObject of type {_realmObject.ObjectSchema.Name}", propertyName);
                }

                return property;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Property GetProperty(string propertyName, Func<PropertyType, bool> typeCheck, [CallerMemberName] string methodName = null)
            {
                Argument.NotNull(propertyName, nameof(propertyName));

                if (!_realmObject.ObjectSchema.TryFindProperty(propertyName, out var property))
                {
                    throw new MissingMemberException($"Property {propertyName} does not exist on RealmObject of type {_realmObject.ObjectSchema.Name}", propertyName);
                }

                if (!typeCheck(property.Type))
                {
                    throw new ArgumentException($"{_realmObject.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} which can't be accessed using {methodName}.");
                }

                return property;
            }
        }
    }
}
