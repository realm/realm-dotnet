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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Exceptions;
using Realms.Schema;

namespace Realms
{
    internal class ManagedAccessor
        : IRealmAccessor, IThreadConfined, INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>
    {
        private Lazy<int> _hashCode;

        private Realm _realm;

        private ObjectHandle _objectHandle;

        private Metadata _metadata;

        private NotificationTokenHandle _notificationToken;

        private Action<string> _onNotifyPropertyChanged;

        public ObjectHandle ObjectHandle => _objectHandle;

        public bool IsManaged => true;

        public bool IsValid => _objectHandle?.IsValid != false;

        public bool IsFrozen => _realm?.IsFrozen == true;

        public Realm Realm => _realm;

        public ObjectSchema ObjectSchema => _metadata?.Schema;

        public int BacklinksCount => _objectHandle?.GetBacklinkCount() ?? 0;

        public IThreadConfinedHandle Handle => _objectHandle;

        public Metadata Metadata => _metadata;

        public RealmObjectBase.Dynamic DynamicApi => new(this);

        internal ManagedAccessor(Realm realm,
            ObjectHandle objectHandle,
            Metadata metadata)
        {
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = metadata;
            _hashCode = new Lazy<int>(() => _objectHandle.GetObjHash());
        }

        ~ManagedAccessor()
        {
            UnsubscribeFromNotifications();
        }

        public RealmValue GetValue(string propertyName)
        {
            return _objectHandle.GetValue(propertyName, _metadata, _realm);
        }

        public void SetValue(string propertyName, RealmValue val)
        {
            _objectHandle.SetValue(propertyName, _metadata, val, _realm);
        }

        public void SetValueUnique(string propertyName, RealmValue val)
        {
            if (_realm.IsInMigration)
            {
                _objectHandle.SetValue(propertyName, _metadata, val, _realm);
            }
            else
            {
                _objectHandle.SetValueUnique(propertyName, _metadata, val);
            }
        }

        public IList<T> GetListValue<T>(string propertyName)
        {
            _metadata.Schema.TryFindProperty(propertyName, out var property);
            return _objectHandle.GetList<T>(_realm, propertyName, _metadata, property.ObjectType);
        }

        public ISet<T> GetSetValue<T>(string propertyName)
        {
            _metadata.Schema.TryFindProperty(propertyName, out var property);
            return _objectHandle.GetSet<T>(_realm, propertyName, _metadata, property.ObjectType);
        }

        public IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            _metadata.Schema.TryFindProperty(propertyName, out var property);
            return _objectHandle.GetDictionary<TValue>(_realm, propertyName, _metadata, property.ObjectType);
        }

        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase
        {
            var resultsHandle = _objectHandle.GetBacklinks(propertyName, _metadata);
            return GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }

        internal RealmResults<T> GetBacklinksForHandle<T>(string propertyName, ResultsHandle resultsHandle)
            where T : IRealmObjectBase
        {
            _metadata.Schema.TryFindProperty(propertyName, out var property);
            var relatedMeta = _realm.Metadata[property.ObjectType];

            return new RealmResults<T>(_realm, resultsHandle, relatedMeta);
        }

        public IRealmObjectBase GetParent()
        {
            if (_metadata.Schema.BaseType != ObjectSchema.ObjectType.EmbeddedObject)
            {
                throw new InvalidOperationException("It is not possible to access a parent of an object that is not embedded.");
            }

            var parentHandle = _objectHandle.GetParent(out var tableKey);
            var parentMetadata = _realm.Metadata[tableKey];

            return _realm.MakeObject(parentMetadata, parentHandle);
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
        void INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>.NotifyCallbacks(NotifiableObjectHandleBase.CollectionChangeSet? changes)
        {
            if (changes.HasValue)
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
            var typeName = _metadata.Schema.Type.Name;

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
}
