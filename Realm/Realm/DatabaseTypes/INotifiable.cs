////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// INotifiable represents a reactive object (e.g. RealmObjectBase/Collection).
    /// </summary>
    /// <typeparam name="TChangeset">The type of the changeset.</typeparam>
    internal interface INotifiable<TChangeset>
        where TChangeset : struct
    {
        /// <summary>
        /// Method called when there are changes to report for that object.
        /// </summary>
        /// <param name="changes">The changes that occurred.</param>
        /// <param name="keypathsIdentifier">Identifier for the collection of keypaths used for the subscription.</param>
        void NotifyCallbacks(TChangeset? changes, KeyPathIdentifier keypathsIdentifier);
    }

    internal class NotificationToken<TCallback> : IDisposable
    {
        private TCallback? _callback;
        private Action<TCallback>? _unsubscribe;

        internal NotificationToken(TCallback callback, Action<TCallback> unsubscribe)
        {
            Argument.NotNull(callback, nameof(callback));
            Argument.NotNull(unsubscribe, nameof(unsubscribe));

            _callback = callback;
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (_callback == null || _unsubscribe == null)
            {
                // Double dispose - ignore
                return;
            }

            _unsubscribe(_callback);
            _callback = default;
            _unsubscribe = null;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is just a helper for the generic token")]
    internal static class NotificationToken
    {
        public static NotificationToken<T> Create<T>(T callback, Action<T> unsubscribe) => new(callback, unsubscribe);
    }
}
