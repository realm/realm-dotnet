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
using System.Runtime.InteropServices;
using Realms.Exceptions;
using Realms.Schema;

namespace Realms
{
    internal class ManagedAccessor : IRealmAccessor, IThreadConfined
    {
        private bool _isEmbedded;  //TODO Eventually remove

        private Lazy<int> _hashCode;

        private Realm _realm;

        private ObjectHandle _objectHandle;

        private RealmObjectBase.Metadata _metadata;

        private NotificationTokenHandle _notificationToken;

        internal ObjectHandle ObjectHandle => _objectHandle;

        internal RealmObjectBase.Metadata ObjectMetadata => _metadata;

        public bool IsManaged => true;

        public bool IsValid => _objectHandle?.IsValid != false;

        public bool IsFrozen => _realm?.IsFrozen == true;

        public Realm Realm => _realm;

        public ObjectSchema ObjectSchema => _metadata?.Schema;

        public Lazy<int> HashCode => _hashCode;

        public int BacklinksCount => _objectHandle?.GetBacklinkCount() ?? 0;

        public IThreadConfinedHandle Handle => _objectHandle;

        public RealmObjectBase.Metadata Metadata => _metadata;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event PropertyChangedEventHandler _propertyChanged;

        public ManagedAccessor(Realm realm, ObjectHandle objectHandle, RealmObjectBase.Metadata metadata, bool isEmbedded)
        {
            _isEmbedded = isEmbedded;
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = metadata;
            _hashCode = new Lazy<int>(() => _objectHandle.GetObjHash());
        }

        public RealmObjectBase FreezeImpl()
        {
            var frozenRealm = Realm.Freeze();
            var frozenHandle = ObjectHandle.Freeze(frozenRealm.SharedRealmHandle);
            return frozenRealm.MakeObject(ObjectMetadata, frozenHandle);
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
            where T : RealmObjectBase
        {
            var resultsHandle = _objectHandle.GetBacklinks(propertyName, _metadata);
            return GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }

        internal RealmResults<T> GetBacklinksForHandle<T>(string propertyName, ResultsHandle resultsHandle)
            where T : RealmObjectBase
        {
            _metadata.Schema.TryFindProperty(propertyName, out var property);
            var relatedMeta = _realm.Metadata[property.ObjectType];

            return new RealmResults<T>(_realm, resultsHandle, relatedMeta);
        }

        public void SubscribeForNotifications()
        {
            Debug.Assert(_notificationToken == null, "_notificationToken must be null before subscribing.");

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

        public string GetStringDescription()
        {
            var typeString = GetType().Name;  //TODO THIS IS WRONG...

            if (!IsManaged)  //TODO This can be removed
            {
                return $"{typeString} (unmanaged)";
            }

            if (!IsValid)
            {
                return $"{typeString} (removed)";
            }

            //if (!_isEmbedded && ObjectMetadata.Helper.TryGetPrimaryKeyValue(ro, out var pkValue))  //TODO We need to remove the helper from here
            //{
            //    var pkProperty = ObjectMetadata.Schema.PrimaryKeyProperty;
            //    return $"{typeString} ({pkProperty.Value.Name} = {pkValue})";
            //}

            return typeString;
        }
    }
}
