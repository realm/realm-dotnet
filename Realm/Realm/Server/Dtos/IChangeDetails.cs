////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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

using System.Collections.Generic;

namespace Realms.Server
{
    /// <summary>
    /// An object containing information about the change that occurred to a Realm.
    /// </summary>
    public interface IChangeDetails
    {
        /// <summary>
        /// Gets a value representing the relative path of the Realm.
        /// </summary>
        /// <value>A relative path in the form of <c>/some-user-id/myrealm</c>.</value>
        string RealmPath { get; }

        /// <summary>
        /// Gets an instance of the Realm just before the change occurred. It can be used to obtain the deleted
        /// items or to compare the properties of the changed items. This instance is readonly and may be null if
        /// the Realm was just created.
        /// </summary>
        /// <value>A <see cref="Realm"/> instance.</value>
        Realm PreviousRealm { get; }

        /// <summary>
        /// Gets an instance of the Realm just after the change has occurred. This instance is readonly. If you wish
        /// to write some data in response to the change, you use <see cref="GetRealmForWriting"/>.
        /// </summary>
        /// <value>A <see cref="Realm"/> instance.</value>
        Realm CurrentRealm { get; }

        /// <summary>
        /// Gets a collection of detailed change information. The keys of the dictionary contain the names of the objects
        /// that have been modified, while the values contain <see cref="IChangeSetDetails"/> instances describing the
        /// indexes of the changed objects.
        /// </summary>
        /// <value>A <see cref="IReadOnlyDictionary{TKey, TValue}"/> of object name-change details pair.</value>
        IReadOnlyDictionary<string, IChangeSetDetails> Changes { get; }

        /// <summary>
        /// Gets an instance of the Realm that can be used for writing new information or updating existing
        /// objects. Because changes may have occurred in the background, this Realm may contain slightly newer
        /// data than <see cref="CurrentRealm"/>.
        /// </summary>
        /// <remarks>
        /// Writing to this Realm will cause changes to be propagated to all synchronized clients, including the
        /// <see cref="INotifier"/>. A change notification will then be sent to handlers so care must be taken to
        /// avoid creating an endless loop.
        /// </remarks>
        /// <returns>A writeable <see cref="Realm"/> instance.</returns>
        Realm GetRealmForWriting();
    }
}
