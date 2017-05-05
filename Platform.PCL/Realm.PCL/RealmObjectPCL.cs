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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    public class RealmObject : INotifyPropertyChanged, ISchemaSource
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            remove
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object has been associated with a Realm, either at creation or via 
        /// <see cref="Realm.Add"/>.
        /// </summary>
        /// <value><c>true</c> if object belongs to a Realm; <c>false</c> if standalone.</value>
        public bool IsManaged
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        /// <value><c>true</c> if managed and part of the Realm or unmanaged; <c>false</c> if managed but deleted.</value>
        public bool IsValid
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this object belongs to, or <c>null</c> if it is unmanaged.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this object belongs to.</value>
        public Realm Realm
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        /// <value>A collection of properties describing the underlying schema of this object.</value>
        public ObjectSchema ObjectSchema
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        #region Getters

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected string GetStringValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected char GetCharValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return ' ';
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected char? GetNullableCharValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected byte GetByteValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected byte? GetNullableByteValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected short GetInt16Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected short? GetNullableInt16Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected int GetInt32Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected int? GetNullableInt32Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected long GetInt64Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected long? GetNullableInt64Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected float GetSingleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0.0f;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected float? GetNullableSingleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected double GetDoubleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0.0;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected double? GetNullableDoubleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected bool GetBooleanValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected bool? GetNullableBooleanValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(DateTimeOffset);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected IList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected byte[] GetByteArrayValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected IQueryable<T> GetBacklinks<T>(string propertyName) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
        #endregion

        #region Setters

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetStringValue(string propertyName, string value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetStringValueUnique(string propertyName, string value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetCharValue(string propertyName, char value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetCharValueUnique(string propertyName, char value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableCharValue(string propertyName, char? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetByteValue(string propertyName, byte value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableCharValueUnique(string propertyName, char? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetByteValueUnique(string propertyName, byte value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableByteValueUnique(string propertyName, byte? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt16Value(string propertyName, short value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt16ValueUnique(string propertyName, short? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt32Value(string propertyName, int value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt32ValueUnique(string propertyName, int? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt64Value(string propertyName, long value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableInt64ValueUnique(string propertyName, long? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetSingleValue(string propertyName, float value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetDoubleValue(string propertyName, double value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetBooleanValue(string propertyName, bool value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
        protected void SetByteArrayValue(string propertyName, byte[] value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        /// <summary>
        /// Allows you to raise the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed. If not specified, we'll use the caller name.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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

        /// <summary>
        /// Called when the object has been managed by a Realm.
        /// </summary>
        /// <remarks>
        /// This method will be called either when a managed object is materialized or when an unmanaged object has been
        /// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
        /// it is not yet clear whether the object is managed or not.
        /// </remarks>
        protected virtual void OnManaged()
        {
        }
    }
}
