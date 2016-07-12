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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Realms
{
    public static class RealmResultsCollectionChanged
    {
        /// <summary>
        /// Wraps a <see cref="RealmResults{T}" /> in an implementation of <see cref="INotifyCollectionChanged" /> so that it may be used in MVVM databinding.
        /// </summary>
        /// <param name="results">The <see cref="RealmResults{T}"/ > to observe for changes.</param>
        /// <param name="errorCallback">An error callback that will be invoked if the observing thread raises an error.</param>
        /// <returns>An <see cref="ObservableCollection{T}" />-like object useful for MVVM databinding.</returns>
        /// <seealso cref="RealmResults{T}.SubscribeForNotifications(RealmResults{T}.NotificationCallback)"/>
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback) where T : RealmObject
        {
            return ToNotifyCollectionChanged(results, errorCallback, coalesceMultipleChangesIntoReset: false);
        }

        /// <summary>
        /// Wraps a <see cref="RealmResults{T}" /> in an implementation of <see cref="INotifyCollectionChanged" /> so that it may be used in MVVM databinding.
        /// </summary>
        /// <param name="results">The <see cref="RealmResults{T}"/ > to observe for changes.</param>
        /// <param name="errorCallback">An error callback that will be invoked if the observing thread raises an error.</param>
        /// <param name="coalesceMultipleChangesIntoReset">
        /// When a lot of items have been added or removed at once it is more efficient to raise <see cref="INotifyCollectionChanged.CollectionChanged" /> once
        /// with <see cref="NotifyCollectionChangedAction.Reset" /> instead of multiple times for every single change. Pass <c>true</c> to opt-in to this behavior.
        /// </param>
        /// <returns>An <see cref="ObservableCollection{T}" />-like object useful for MVVM databinding.</returns>
        /// <seealso cref="RealmResults{T}.SubscribeForNotifications(RealmResults{T}.NotificationCallback)"/>
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback, bool coalesceMultipleChangesIntoReset) where T : RealmObject
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            if (!(results is RealmResults<T>))
            {
                throw new ArgumentException($"{nameof(results)} must be an instance of RealmResults<{typeof(T).Name}>", nameof(results));
            }

            if (errorCallback == null)
            {
                throw new ArgumentNullException(nameof(errorCallback));
            }

            return new ReadOnlyObservableCollection<T>(new Adapter<T>((RealmResults<T>)results, errorCallback, coalesceMultipleChangesIntoReset));
        }

        sealed class Adapter<T> : ObservableCollection<T> where T : RealmObject
        {
            readonly RealmResults<T> _results;
            readonly IDisposable _token;
            readonly Action<Exception> _errorCallback;
            readonly bool _coalesceMultipleChangesIntoReset;

            private bool _suspendNotifications;

            internal Adapter(RealmResults<T> results, Action<Exception> errorCallback, bool coalesceMultipleChangesIntoReset) : base(results)
            {
                _results = results;
                _errorCallback = errorCallback;
                _coalesceMultipleChangesIntoReset = coalesceMultipleChangesIntoReset;

                _token = results.SubscribeForNotifications(OnChange);
                Debug.Assert(_token != null);
            }

            ~Adapter()
            {
                _token.Dispose();
            }

            void OnChange(RealmResults<T> sender, RealmResults<T>.ChangeSet change, Exception error)
            {
                if (error != null)
                {
                    _errorCallback(error);
                }
                else if (change != null)
                {
                    _suspendNotifications = _coalesceMultipleChangesIntoReset && change.InsertedIndices.Length + change.DeletedIndices.Length > 1;

                    foreach (var removed in change.DeletedIndices.Reverse())
                    {
                        // the row has been deleted, we need to recreate the adapter's contents
                        if (!this[removed].RowHandle.IsAttached)
                        {
                            Recreate();
                            return;
                        }

                        RemoveAt(removed);
                    }

                    foreach (var added in change.InsertedIndices)
                    {
                        InsertItem(added, _results[added]);
                    }

                    if (_suspendNotifications)
                    {
                        RaiseReset();
                        _suspendNotifications = false;
                    }
                }
            }

            void Recreate()
            {
                _suspendNotifications = true;
                this.Clear();
                foreach (var item in _results)
                {
                    this.Add(item);
                }
                _suspendNotifications = false;
                RaiseReset();
            }

            void RaiseReset()
            {
                base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                base.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (!_suspendNotifications)
                {
                    base.OnCollectionChanged(e);
                }
            }

            protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (!_suspendNotifications)
                {
                    base.OnPropertyChanged(e);
                }
            }
        }
    }
}
