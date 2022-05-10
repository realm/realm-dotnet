﻿////////////////////////////////////////////////////////////////////////////
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
using Realms.Extensions;
using Realms.Schema;

namespace Realms
{
    internal class ManagedAccessor
        : IManagedAccessor
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

        public int? HashCode => _hashCode.Value;

        public int BacklinksCount => _objectHandle?.GetBacklinkCount() ?? 0;

        public IThreadConfinedHandle Handle => _objectHandle;

        public Metadata Metadata => _metadata;

        public RealmObjectBase.Dynamic DynamicApi => new(this);

        private ManagedAccessor(Realm realm,
            ObjectHandle objectHandle,
            Metadata metadata)
        {
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = metadata;
            _hashCode = new Lazy<int>(() => _objectHandle.GetObjHash());
        }

        public static ManagedAccessor Create(Realm realm,
            ObjectHandle objectHandle,
            Metadata metadata)
        {
            return new ManagedAccessor(realm, objectHandle, metadata);
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

        public string GetStringDescription(string typeName)
        {
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

        public bool ObjectEquals(object obj)
        {
            // Special case to cover possible bugs similar to WPF (#1903)
            if (obj is InvalidObject)
            {
                return !IsValid;
            }

            // If run-time types are not exactly the same, return false.
            if (obj is not IRealmObjectBase iro)
            {
                return false;
            }

            // standalone objects cannot participate in the same store check
            if (!iro.Accessor.IsManaged)
            {
                return false;
            }

            if (ObjectSchema.Name != iro.Accessor.ObjectSchema.Name)
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return ObjectHandle.ObjEquals(iro.GetObjectHandle());
        }

        public IQueryable<dynamic> GetBacklinks(string objectType, string property) => DynamicApi.GetBacklinksFromType(objectType, property);
    }
}