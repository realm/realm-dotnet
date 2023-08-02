////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace Realms.Sync;

/// <summary>
/// An enum controlling when <see cref="CollectionExtensions.SubscribeAsync{T}">query.SubscribeAsync</see> will
/// wait for synchronization before returning.
/// </summary>
/// <remarks>
/// When the [Subscription] is created for the first time, data needs to be downloaded from the
/// server before it becomes available, so depending on whether you run the query against the local
/// database before or after this has happened, you query results might not look correct.
/// <br/>
/// This enum thus defines the behaviour of when the query is run, so it possible to make the
/// appropriate tradeoff between correctness and availability.
/// </remarks>
public enum WaitForSyncMode
{
    /// <summary>
    /// This mode will wait for the server data the first time a subscription is created before
    /// returning the local query. Later calls to <see cref="CollectionExtensions.SubscribeAsync{T}">query.SubscribeAsync</see>
    /// will detect that the subscription already exist and return immediately.
    /// <br/>
    /// This is the default mode.
    /// </summary>
    FirstTime,

    /// <summary>
    /// With this mode enabled, Realm will always download the latest server state before returning from
    /// <see cref="CollectionExtensions.SubscribeAsync{T}">query.SubscribeAsync</see>. This means that your
    /// query result is always seeing the latest data, but it also requires the app to be online.
    /// </summary>
    /// <remarks>
    /// When using this mode, it is strongly advised to supply a <see cref="CancellationToken"/> to
    /// <see cref="CollectionExtensions.SubscribeAsync{T}">query.SubscribeAsync</see> to make sure your
    /// app doesn't wait forever.
    /// </remarks>
    Always,

    /// <summary>
    /// With this mode enabled, Realm will always return as soon as the the subscription is created
    /// while any server data is being downloaded in the background. This update is not atomic, which
    /// means that if you subscribe to notifications using
    /// <see cref="CollectionExtensions.SubscribeForNotifications{T}(IQueryable{T}, NotificationCallbackDelegate{T})"/>
    /// or <see cref="INotifyCollectionChanged.CollectionChanged"/>
    /// you might see multiple events being fired as the server sends objects matching the subscription.
    /// </summary>
    Never
}
