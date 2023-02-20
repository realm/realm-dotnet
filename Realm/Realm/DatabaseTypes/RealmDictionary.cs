////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Realms.Dynamic;
using Realms.Helpers;

namespace Realms
{
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "IList conformance is needed for UWP databinding. IList<T> is not necessary.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmDictionary<TValue>
        : RealmCollectionBase<KeyValuePair<string, TValue>>,
          IDictionary<string, TValue>,
          IDynamicMetaObjectProvider,
          IRealmCollectionBase<DictionaryHandle>,
          INotifiable<DictionaryHandle.DictionaryChangeSet>
    {
        private readonly List<DictionaryNotificationCallbackDelegate<TValue>> _keyCallbacks = new();
        private readonly DictionaryHandle _dictionaryHandle;

        private bool _deliveredInitialKeyNotification;
        private NotificationTokenHandle? _keyNotificationToken;

        public TValue this[string key]
        {
            get
            {
                if (TryGetValue(key, out var result))
                {
                    return result;
                }

                throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
            }

            set
            {
                ValidateKey(key);
                var realmValue = Operator.Convert<TValue, RealmValue>(value);

                if (_isEmbedded && realmValue.Type != RealmValueType.Null)
                {
                    if (IsDynamic)
                    {
                        throw new NotSupportedException("Can't set embedded objects directly. Instead use Realm.DynamicApi.SetEmbeddedObjectInDictionary.");
                    }

                    Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _dictionaryHandle.SetEmbedded(key));
                    return;
                }

                AddToRealmIfNecessary(realmValue);
                _dictionaryHandle.Set(key, realmValue);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                var resultsHandle = _dictionaryHandle.GetKeys();
                return new RealmResults<string>(Realm, resultsHandle, metadata: null);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var resultsHandle = _dictionaryHandle.GetValues();
                return new RealmResults<TValue>(Realm, resultsHandle, Metadata);
            }
        }

        internal RealmResults<TValue> ToResults()
        {
            return (RealmResults<TValue>)Values;
        }

        // Get filtered results from dictionary's values
        internal RealmResults<TValue> GetFilteredValueResults(string query, RealmValue[] arguments)
        {
            var resultsHandle = Handle.Value.GetFilteredResults(query, arguments);
            return new RealmResults<TValue>(Realm, resultsHandle, Metadata);
        }

        DictionaryHandle IRealmCollectionBase<DictionaryHandle>.NativeHandle => _dictionaryHandle;

        internal RealmDictionary(Realm realm, DictionaryHandle adoptedDictionary, Metadata metadata)
            : base(realm, metadata)
        {
            _dictionaryHandle = adoptedDictionary;
        }

        public void Add(string key, TValue value)
        {
            ValidateKey(key);
            var realmValue = Operator.Convert<TValue, RealmValue>(value);

            if (_isEmbedded && realmValue.Type != RealmValueType.Null)
            {
                if (IsDynamic)
                {
                    throw new NotSupportedException("Can't add embedded objects directly. Instead use Realm.DynamicApi.AddEmbeddedObjectToDictionary.");
                }

                Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _dictionaryHandle.AddEmbedded(key));
                return;
            }

            AddToRealmIfNecessary(realmValue);
            _dictionaryHandle.Add(key, realmValue);
        }

        public void Add(KeyValuePair<string, TValue> item) => Add(item.Key, item.Value);

        public bool ContainsKey(string key) => key != null && _dictionaryHandle.ContainsKey(key);

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmDictionary(expression, this);

        public override int IndexOf(KeyValuePair<string, TValue> value) => throw new NotSupportedException();

        public bool Remove(string key) => key != null && _dictionaryHandle.Remove(key);

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            if (item.Key == null)
            {
                return false;
            }

            var realmValue = Operator.Convert<TValue, RealmValue>(item.Value);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsIRealmObject().IsManaged)
            {
                return false;
            }

            return _dictionaryHandle.Remove(item.Key, realmValue);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            if (key != null && _dictionaryHandle.TryGet(key, Realm, out var realmValue))
            {
                value = realmValue.As<TValue>();
                return true;
            }

            value = default;
            return false;
        }

        public IDisposable SubscribeForKeyNotifications(DictionaryNotificationCallbackDelegate<TValue> callback)
        {
            Argument.NotNull(callback, nameof(callback));

            if (_keyCallbacks.Count == 0)
            {
                SubscribeForNotifications();
            }
            else if (_deliveredInitialKeyNotification)
            {
                callback(this, null);
            }

            _keyCallbacks.Add(callback);

            return NotificationToken.Create(callback, UnsubscribeFromNotifications);
        }

        private void SubscribeForNotifications()
        {
            Debug.Assert(_keyNotificationToken == null, "_keyNotificationToken must be null before subscribing.");

            Realm.ExecuteOutsideTransaction(() =>
            {
                var managedResultsHandle = GCHandle.Alloc(this, GCHandleType.Weak);
                _keyNotificationToken = _dictionaryHandle.AddKeyNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle));
            });
        }

        private void UnsubscribeFromNotifications(DictionaryNotificationCallbackDelegate<TValue> callback)
        {
            if (_keyCallbacks.Remove(callback) &&
                _keyCallbacks.Count == 0)
            {
                UnsubscribeFromNotifications();
            }
        }

        private void UnsubscribeFromNotifications()
        {
            _keyNotificationToken?.Dispose();
            _keyNotificationToken = null;
            _deliveredInitialKeyNotification = false;
        }

        private static string ValidateKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Value cannot be null.");
            }

            if (key.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"A persisted dictionary cannot have a key that starts with '$': {key}");
            }

            if (key.Contains('.'))
            {
                throw new NotSupportedException($"A persisted dictionary cannot have a key that contains '.': {key}");
            }

            return key;
        }

        internal override RealmCollectionBase<KeyValuePair<string, TValue>> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmDictionary<TValue>(realm, (DictionaryHandle)handle, Metadata);

        internal override CollectionHandleBase GetOrCreateHandle() => _dictionaryHandle;

        protected override KeyValuePair<string, TValue> GetValueAtIndex(int index) => _dictionaryHandle.GetValueAtIndex<TValue>(index, Realm);

        void INotifiable<DictionaryHandle.DictionaryChangeSet>.NotifyCallbacks(DictionaryHandle.DictionaryChangeSet? changes, bool shallow)
        {
            Debug.Assert(!shallow, "Shallow should always be false here as we don't expose a way to configure it.");

            DictionaryChangeSet? changeset = null;
            if (changes != null)
            {
                var actualChanges = changes.Value;
                changeset = new DictionaryChangeSet(
                    deletedKeys: actualChanges.Deletions.AsEnumerable().Select(v => v.AsString()).ToArray(),
                    insertedKeys: actualChanges.Insertions.AsEnumerable().Select(v => v.AsString()).ToArray(),
                    modifiedKeys: actualChanges.Modifications.AsEnumerable().Select(v => v.AsString()).ToArray());
            }
            else
            {
                _deliveredInitialKeyNotification = true;
            }

            foreach (var callback in _keyCallbacks.ToArray())
            {
                callback(this, changeset);
            }
        }
    }
}
