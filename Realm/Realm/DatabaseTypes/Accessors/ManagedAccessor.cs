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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Realms.DataBinding;
using Realms.Exceptions;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Represents the base class for an accessor to be used with a managed object.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class ManagedAccessor
        : IRealmAccessor, IThreadConfined, INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>
    {
        private readonly Lazy<int> _hashCode;

        private NotificationTokenHandle? _notificationToken;

        private Action<string>? _onNotifyPropertyChanged;

        internal ObjectHandle ObjectHandle { get; private set; }

        /// <inheritdoc/>
        public Realm Realm { get; private set; }

        internal Metadata Metadata { get; private set; }

        /// <inheritdoc cref="IRealmAccessor.IsManaged" />
        public bool IsManaged => true;

        /// <inheritdoc cref="IRealmAccessor.IsValid" />
        public bool IsValid => ObjectHandle?.IsValid != false;

        /// <inheritdoc/>
        public bool IsFrozen => Realm.IsFrozen;

        /// <inheritdoc/>
        public ObjectSchema ObjectSchema => Metadata.Schema;

        /// <inheritdoc/>
        public int BacklinksCount => ObjectHandle?.GetBacklinkCount() ?? 0;

        /// <inheritdoc/>
        IThreadConfinedHandle IThreadConfined.Handle => ObjectHandle;

        /// <inheritdoc/>
        public DynamicObjectApi DynamicApi => new(this);

        /// <inheritdoc/>
        Metadata IMetadataObject.Metadata => Metadata;

#pragma warning disable CS8618 // These fields are set by Initialize which is called immediately after creating the instance
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAccessor"/> class.
        /// </summary>
        protected ManagedAccessor()
#pragma warning restore CS8618
        {
            _hashCode = new(() => ObjectHandle!.GetObjHash());
        }

        [MemberNotNull(nameof(Realm), nameof(ObjectHandle), nameof(Metadata))]
        internal void Initialize(Realm realm,
            ObjectHandle objectHandle,
            Metadata metadata)
        {
            Realm = realm;
            ObjectHandle = objectHandle;
            Metadata = metadata;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ManagedAccessor"/> class.
        /// </summary>
        ~ManagedAccessor()
        {
            UnsubscribeFromNotifications();
        }

        /// <inheritdoc/>
        public RealmValue GetValue(string propertyName)
        {
            return ObjectHandle.GetValue(propertyName, Metadata, Realm);
        }

        /// <inheritdoc/>
        public void SetValue(string propertyName, RealmValue val)
        {
            ObjectHandle.SetValue(propertyName, Metadata, val, Realm);
        }

        /// <inheritdoc/>
        public void SetValueUnique(string propertyName, RealmValue val)
        {
            if (Realm.IsInMigration)
            {
                ObjectHandle.SetValue(propertyName, Metadata, val, Realm);
            }
            else
            {
                ObjectHandle.SetValueUnique(propertyName, Metadata, val);
            }
        }

        /// <inheritdoc/>
        public IList<T> GetListValue<T>(string propertyName)
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            return ObjectHandle.GetList<T>(Realm, propertyName, Metadata, property.ObjectType);
        }

        /// <inheritdoc/>
        public ISet<T> GetSetValue<T>(string propertyName)
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            return ObjectHandle.GetSet<T>(Realm, propertyName, Metadata, property.ObjectType);
        }

        /// <inheritdoc/>
        public IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            return ObjectHandle.GetDictionary<TValue>(Realm, propertyName, Metadata, property.ObjectType);
        }

        /// <inheritdoc/>
        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase
        {
            var resultsHandle = ObjectHandle.GetBacklinks(propertyName, Metadata);
            return GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }

        internal RealmResults<T> GetBacklinksForHandle<T>(string propertyName, ResultsHandle resultsHandle)
            where T : IRealmObjectBase
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            var relatedMeta = Realm.Metadata[property.ObjectType!];

            return new RealmResults<T>(Realm, resultsHandle, relatedMeta);
        }

        /// <inheritdoc/>
        public IRealmObjectBase GetParent()
        {
            if (Metadata.Schema.BaseType != ObjectSchema.ObjectType.EmbeddedObject)
            {
                throw new InvalidOperationException("It is not possible to access a parent of an object that is not embedded.");
            }

            var parentHandle = ObjectHandle.GetParent(out var tableKey);
            var parentMetadata = Realm.Metadata[tableKey];

            return Realm.MakeObject(parentMetadata, parentHandle);
        }

        /// <inheritdoc/>
        public void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate)
        {
            Debug.Assert(_notificationToken == null, "_notificationToken must be null before subscribing.");

            _onNotifyPropertyChanged = notifyPropertyChangedDelegate;

            if (IsFrozen)
            {
                throw new RealmFrozenException("It is not possible to add a change listener to a frozen RealmObjectBase since it never changes.");
            }

            Realm.ExecuteOutsideTransaction(() =>
            {
                if (ObjectHandle.IsValid)
                {
                    var managedObjectHandle = GCHandle.Alloc(this, GCHandleType.Weak);
                    _notificationToken = ObjectHandle.AddNotificationCallback(GCHandle.ToIntPtr(managedObjectHandle));
                }
            });
        }

        /// <inheritdoc/>
        public void UnsubscribeFromNotifications()
        {
            _notificationToken?.Dispose();
            _notificationToken = null;
        }

        /// <inheritdoc/>
        void INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>.NotifyCallbacks(NotifiableObjectHandleBase.CollectionChangeSet? changes,
            KeyPathsCollectionType type, Delegate? callback)
        {
            Debug.Assert(callback == null, "Object notifications don't support keypaths, so callback should always be null");
            if (changes.HasValue)
            {
                foreach (var propertyIndex in changes.Value.Properties)
                {
                    // Due to a yet another Mono compiler bug, using LINQ fails here :/
                    var i = 0;
                    foreach (var property in ObjectSchema)
                    {
                        //// Backlinks should be ignored. See Realm.CreateRealmObjectMetadata
                        //if (property.Type.IsComputed())
                        //{
                        //    continue;
                        //}

                        if (i == propertyIndex)
                        {
                            RaisePropertyChanged(property.ManagedName);
                            break;
                        }

                        ++i;
                    }
                }

                if (changes.Value.Deletions.Count > 0)
                {
                    RaisePropertyChanged(nameof(IsValid));

                    if (!IsValid)
                    {
                        // We can proactively unsubscribe because the object has been deleted
                        UnsubscribeFromNotifications();
                    }
                }
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            _onNotifyPropertyChanged?.Invoke(propertyName);
        }

        /// <inheritdoc/>
        public TypeInfo GetTypeInfo(IRealmObjectBase obj)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            return TypeInfoHelper.GetInfo(obj);
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _hashCode.Value;
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            var typeName = Metadata.Schema.Type!.Name;

            if (!IsValid)
            {
                return $"{typeName} (removed)";
            }

            if (ObjectSchema.PrimaryKeyProperty is { } pkProperty)
            {
                var pkName = pkProperty.Name;
                var pkValue = GetValue(pkName);
                return $"{typeName} ({pkName} = {pkValue})";
            }

            return typeName;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not ManagedAccessor ma)
            {
                return false;
            }

            if (ObjectSchema.Name != ma.ObjectSchema.Name)
            {
                return false;
            }

            return ObjectHandle.ObjEquals(ma.ObjectHandle);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Better code organisation")]
    internal class GenericManagedAccessor : ManagedAccessor
    {
    }
}
