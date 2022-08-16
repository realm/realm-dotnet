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
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Exceptions;
using Realms.Schema;

namespace Realms
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class ManagedAccessor
        : IRealmAccessor, IThreadConfined, INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>
    {
        private Lazy<int> _hashCode;

        private NotificationTokenHandle _notificationToken;

        private Action<string> _onNotifyPropertyChanged;

        internal ObjectHandle ObjectHandle { get; private set; }

        public bool IsManaged => true;

        public bool IsValid => ObjectHandle?.IsValid != false;

        public bool IsFrozen => Realm?.IsFrozen == true;

        public Realm Realm { get; private set; }

        public ObjectSchema ObjectSchema => Metadata?.Schema;

        public int BacklinksCount => ObjectHandle?.GetBacklinkCount() ?? 0;

        IThreadConfinedHandle IThreadConfined.Handle => ObjectHandle;

        internal Metadata Metadata { get; private set; }

        public RealmObjectBase.Dynamic DynamicApi => new(this);

        Metadata IMetadataObject.Metadata => Metadata;

        internal void Initialize(Realm realm,
            ObjectHandle objectHandle,
            Metadata metadata)
        {
            Realm = realm;
            ObjectHandle = objectHandle;
            Metadata = metadata;
            _hashCode = new Lazy<int>(() => ObjectHandle.GetObjHash());
        }

        public RealmValue GetValue(string propertyName)
        {
            return ObjectHandle.GetValue(propertyName, Metadata, Realm);
        }

        public void SetValue(string propertyName, RealmValue val)
        {
            ObjectHandle.SetValue(propertyName, Metadata, val, Realm);
        }

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

        public IList<T> GetListValue<T>(string propertyName)
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            return ObjectHandle.GetList<T>(Realm, propertyName, Metadata, property.ObjectType);
        }

        public ISet<T> GetSetValue<T>(string propertyName)
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            return ObjectHandle.GetSet<T>(Realm, propertyName, Metadata, property.ObjectType);
        }

        public IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            Metadata.Schema.TryFindProperty(propertyName, out var property);
            return ObjectHandle.GetDictionary<TValue>(Realm, propertyName, Metadata, property.ObjectType);
        }

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
            var relatedMeta = Realm.Metadata[property.ObjectType];

            return new RealmResults<T>(Realm, resultsHandle, relatedMeta);
        }

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

        public void UnsubscribeFromNotifications()
        {
            _notificationToken?.Dispose();
            _notificationToken = null;
        }

        /// <inheritdoc/>
        void INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>.NotifyCallbacks(NotifiableObjectHandleBase.CollectionChangeSet? changes, NativeException? exception)
        {
            var managedException = exception?.Convert();

            if (managedException != null)
            {
                Realm.NotifyError(managedException);
            }
            else if (changes.HasValue)
            {
                foreach (int propertyIndex in changes.Value.Properties.AsEnumerable())
                {
                    // Due to a yet another Mono compiler bug, using LINQ fails here :/
                    var i = 0;
                    foreach (var property in ObjectSchema)
                    {
                        // Backlinks should be ignored. See Realm.CreateRealmObjectMetadata
                        if (property.Type.IsComputed())
                        {
                            continue;
                        }

                        if (i == propertyIndex)
                        {
                            RaisePropertyChanged(property.PropertyInfo?.Name ?? property.Name);
                            break;
                        }

                        ++i;
                    }
                }

                if (changes.Value.Deletions.AsEnumerable().Any())
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

        private void RaisePropertyChanged(string propertyName = null)
        {
            _onNotifyPropertyChanged(propertyName);
        }

        public IQueryable<dynamic> GetBacklinks(string objectType, string property) => DynamicApi.GetBacklinksFromType(objectType, property);

        public override int GetHashCode()
        {
            return _hashCode.Value;
        }

        public override string ToString()
        {
            var typeName = Metadata.Schema.Type.Name;

            if (!IsValid)
            {
                return $"{typeName} (removed)";
            }

            if (ObjectSchema.PrimaryKeyProperty is Property pkProperty)
            {
                var pkName = pkProperty.Name;
                var pkValue = GetValue(pkName);
                return $"{typeName} ({pkName} = {pkValue})";
            }

            return typeName;
        }

        public override bool Equals(object obj)
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

    internal class GenericManagedAccessor : ManagedAccessor
    {
    }
}
