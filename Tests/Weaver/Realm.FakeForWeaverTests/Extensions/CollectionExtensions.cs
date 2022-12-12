////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

using System.Linq;
using System;
using System.Collections.Generic;

namespace Realms
{
    public static class CollectionExtensions
    {
        public static IDisposable SubscribeForNotifications<T>(this IQueryable<T> results, NotificationCallbackDelegate<T> callback)
            where T : IRealmObjectBase => default;

        public static IDisposable SubscribeForNotifications<T>(this ISet<T> set, NotificationCallbackDelegate<T> callback) => default;

        public static IDisposable SubscribeForNotifications<T>(this IList<T> list, NotificationCallbackDelegate<T> callback) => default;

        public static IDisposable SubscribeForNotifications<T>(this IDictionary<string, T> dictionary, NotificationCallbackDelegate<KeyValuePair<string, T>> callback) => default;
    }
}
