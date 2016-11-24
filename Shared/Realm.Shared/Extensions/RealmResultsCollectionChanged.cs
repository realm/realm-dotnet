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
using System.Linq;

namespace Realms
{
    public static class RealmResultsCollectionChanged
    {
        /// <summary>
        /// A convenience method that casts <c>IQueryable{T}</c> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        /// <returns>The a collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IQueryable<T> results) where T : RealmObject
        {
            if (!(results is IRealmCollection<T>))
            {
                throw new ArgumentException($"{nameof(results)} must be an instance of IRealmCollection<{typeof(T).Name}>", nameof(results));
            }

            return (IRealmCollection<T>)results;
        }

        /// <summary>
        /// A convenience method that casts <c>IQueryable{T}</c> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="RealmResults{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public static IDisposable SubscribeForNotifications<T>(this IQueryable<T> results, NotificationCallbackDelegate<T> callback) where T : RealmObject
        {
            if (!(results is IRealmCollection<T>))
            {
                throw new ArgumentException($"{nameof(results)} must be an instance of IRealmCollection<{typeof(T).Name}>", nameof(results));
            }

            return ((IRealmCollection<T>)results).SubscribeForNotifications(callback);
        }

        /// <summary>
        /// Wraps a <see cref="RealmResults{T}" /> in an implementation of <see cref="INotifyCollectionChanged" /> so that it may be used in MVVM databinding.
        /// </summary>
        /// <param name="results">The <see cref="RealmResults{T}" /> to observe for changes.</param>
        /// <param name="errorCallback">An error callback that will be invoked if the observing thread raises an error.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <returns>An <see cref="ObservableCollection{T}" />-like object useful for MVVM databinding.</returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        [Obsolete("Use .ToNotifyCollectionChanged without arguments. For error callback, use Realm.Error.")]
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback) where T : RealmObject
        {
            return ToNotifyCollectionChanged(results, errorCallback, coalesceMultipleChangesIntoReset: false);
        }

        /// <summary>
        /// Wraps a <see cref="RealmResults{T}" /> in an implementation of <see cref="INotifyCollectionChanged" /> so that it may be used in MVVM databinding.
        /// </summary>
        /// <param name="results">The <see cref="RealmResults{T}" /> to observe for changes.</param>
        /// <param name="errorCallback">An error callback that will be invoked if the observing thread raises an error.</param>
        /// <param name="coalesceMultipleChangesIntoReset">
        /// When a lot of items have been added or removed at once it is more efficient to raise <see cref="INotifyCollectionChanged.CollectionChanged" /> once
        /// with <see cref="NotifyCollectionChangedAction.Reset" /> instead of multiple times for every single change. Pass <c>true</c> to opt-in to this behavior.
        /// </param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <returns>An <see cref="ObservableCollection{T}" />-like object useful for MVVM databinding.</returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        [Obsolete("Use .ToNotifyCollectionChanged without arguments. For error callback, use Realm.Error.")]
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

            return (RealmResults<T>)results;
        }
    }
}
