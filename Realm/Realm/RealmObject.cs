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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Realms.Native;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    [Preserve(AllMembers = true, Conditional = false)]
    public class RealmObject : IReflectableType, INotifyPropertyChanged, ISchemaSource
    {
        #region static

        [NativeCallback(typeof(NativeCommon.NotifyRealmObjectCallback))]
        internal static bool NotifyRealmObjectPropertyChanged(IntPtr realmObjectHandle, IntPtr propertyIndex)
        {
            var gch = GCHandle.FromIntPtr(realmObjectHandle);
            var realmObject = (RealmObject)gch.Target;
            if (realmObject != null)
            {
                var property = realmObject.ObjectSchema.ElementAtOrDefault((int)propertyIndex);
                realmObject.RaisePropertyChanged(property.PropertyInfo?.Name ?? property.Name);
                return true;
            }

            gch.Free();
            return false;
        }

        #endregion

        private Realm _realm;
        private ObjectHandle _objectHandle;
        private Metadata _metadata;
        private GCHandle? _notificationsHandle;

        private event PropertyChangedEventHandler _propertyChanged;

        /// <inheritdoc />
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

        internal ObjectHandle ObjectHandle => _objectHandle;

        internal Metadata ObjectMetadata => _metadata;

        /// <summary>
        /// Gets a value indicating whether the object has been associated with a Realm, either at creation or via 
        /// <see cref="Realm.Add"/>.
        /// </summary>
        /// <value><c>true</c> if object belongs to a Realm; <c>false</c> if standalone.</value>
        public bool IsManaged => _realm != null;

        /// <summary>
        /// Gets a value indicating whether this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        /// <value><c>true</c> if managed and part of the Realm or unmanaged; <c>false</c> if managed but deleted.</value>
        public bool IsValid => _objectHandle?.IsValid != false;

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this object belongs to, or <c>null</c> if it is unmanaged.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this object belongs to.</value>
        public Realm Realm => _realm;

        /// <summary>
        /// Gets the <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        /// <value>A collection of properties describing the underlying schema of this object.</value>
        public ObjectSchema ObjectSchema => _metadata?.Schema;

        internal void _SetOwner(Realm realm, ObjectHandle objectHandle, Metadata metadata)
        {
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = metadata;

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }
        }

        internal class Metadata
        {
            internal TableHandle Table;

            internal Weaving.IRealmObjectHelper Helper;

            internal Dictionary<string, IntPtr> PropertyIndices;

            internal ObjectSchema Schema;
        }

        #region Getters

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected string GetStringValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetString(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected char GetCharValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (char)_objectHandle.GetInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected char? GetNullableCharValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (char?)_objectHandle.GetNullableInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected byte GetByteValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (byte)_objectHandle.GetInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected byte? GetNullableByteValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (byte?)_objectHandle.GetNullableInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected short GetInt16Value(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (short)_objectHandle.GetInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected short? GetNullableInt16Value(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (short?)_objectHandle.GetNullableInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected int GetInt32Value(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (int)_objectHandle.GetInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected int? GetNullableInt32Value(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return (int?)_objectHandle.GetNullableInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected long GetInt64Value(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected long? GetNullableInt64Value(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetNullableInt64(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected float GetSingleValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetSingle(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetNullableSingle(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected double GetDoubleValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetDouble(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected double? GetNullableDoubleValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetNullableDouble(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected bool GetBooleanValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetBoolean(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetNullableBoolean(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetDateTimeOffset(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetNullableDateTimeOffset(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected IList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var relatedMeta = _realm.Metadata[property.ObjectType];

            var listHandle = _objectHandle.TableLinkList(_metadata.PropertyIndices[propertyName]);
            return new RealmList<T>(_realm, listHandle, relatedMeta);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            var linkedObjectPtr = _objectHandle.GetLink(_metadata.PropertyIndices[propertyName]);
            if (linkedObjectPtr == IntPtr.Zero)
            {
                return null;
            }

            Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var objectType = property.ObjectType;
            return (T)_realm.MakeObject(objectType, linkedObjectPtr);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected byte[] GetByteArrayValue(string propertyName)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            return _objectHandle.GetByteArray(_metadata.PropertyIndices[propertyName]);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected IQueryable<T> GetBacklinks<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            var resultsHandle = _objectHandle.GetBacklinks(_metadata.PropertyIndices[propertyName]);
            return GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }

        internal RealmResults<T> GetBacklinksForHandle<T>(string propertyName, ResultsHandle resultsHandle) where T : RealmObject
        {
            Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var relatedMeta = _realm.Metadata[property.ObjectType];

            return new RealmResults<T>(_realm, resultsHandle, relatedMeta);
        }

        #endregion

        #region Setters

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetStringValue(string propertyName, string value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetString(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetStringValueUnique(string propertyName, string value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetStringUnique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetCharValue(string propertyName, char value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetCharValueUnique(string propertyName, char value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableCharValue(string propertyName, char? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableCharValueUnique(string propertyName, char? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetByteValue(string propertyName, byte value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetByteValueUnique(string propertyName, byte value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableByteValueUnique(string propertyName, byte? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt16Value(string propertyName, short value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt16ValueUnique(string propertyName, short? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt32Value(string propertyName, int value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt32ValueUnique(string propertyName, int? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt64Value(string propertyName, long value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt64ValueUnique(string propertyName, long? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableInt64Unique(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetSingleValue(string propertyName, float value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetSingle(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableSingle(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetDoubleValue(string propertyName, double value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetDouble(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableDouble(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetBooleanValue(string propertyName, bool value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetBoolean(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableBoolean(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetDateTimeOffset(_metadata.PropertyIndices[propertyName], value);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetNullableDateTimeOffset(_metadata.PropertyIndices[propertyName], value);
        }

        // Originally a generic fallback, now used only for RealmObject To-One relationship properties
        // most other properties handled with woven type-specific setters above
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            if (value == null)
            {
                _objectHandle.ClearLink(_metadata.PropertyIndices[propertyName]);
            }
            else
            {
                if (!value.IsManaged)
                {
                    _realm.Add(value);
                }

                _objectHandle.SetLink(_metadata.PropertyIndices[propertyName], value.ObjectHandle);
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetByteArrayValue(string propertyName, byte[] value)
        {
            Debug.Assert(IsManaged, "Object is not managed, but managed access was attempted");

            _objectHandle.SetByteArray(_metadata.PropertyIndices[propertyName], value);
        }

        #endregion

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false. 
            if (GetType() != obj.GetType())
            {
                return false;
            }

            // standalone objects cannot participate in the same store check
            if (!IsManaged)
            {
                return false;
            }

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return ObjectHandle.Equals(((RealmObject)obj).ObjectHandle);
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
        /// 
        ///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
        /// 
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

        TypeInfo IReflectableType.GetTypeInfo()
        {
            // TODO
            // return RealmObjectTypeInfo.FromType(GetType());
            return GetType().GetTypeInfo();
        }

        private void SubscribeForNotifications()
        {
            Debug.Assert(!_notificationsHandle.HasValue, "Notification handle must be null before subscribing");

            _notificationsHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            _realm.SharedRealmHandle.AddObservedObject(ObjectHandle, GCHandle.ToIntPtr(_notificationsHandle.Value));
        }

        private void UnsubscribeFromNotifications()
        {
            Debug.Assert(_notificationsHandle.HasValue, "Notification handle must not be null to unsubscribe");

            if (_notificationsHandle.HasValue)
            {
                _realm.SharedRealmHandle.RemoveObservedObject(GCHandle.ToIntPtr(_notificationsHandle.Value));
                _notificationsHandle.Value.Free();
                _notificationsHandle = null;
            }
        }
    }
}