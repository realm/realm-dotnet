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
    /// An object that contains information about the change that occurred on a single
    /// object.
    /// </summary>
    public interface IModificationDetails
    {
        /// <summary>
        /// Gets the object as it was before the change.
        /// </summary>
        /// <value>A <see cref="RealmObject"/> instance.</value>
        dynamic PreviousObject { get; }

        /// <summary>
        /// Gets the object as it is after the change.
        /// </summary>
        /// <value>A <see cref="RealmObject"/> instance.</value>
        dynamic CurrentObject { get; }

        /// <summary>
        /// Gets the names of the properties that were changed.
        /// </summary>
        ISet<string> ChangedProperties { get; }
    }
}
