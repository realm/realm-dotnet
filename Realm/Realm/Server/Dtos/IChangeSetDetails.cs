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
    /// An object containing information about the insertions, deletions, and modifications
    /// performed on a single collection of a certain object type.
    /// </summary>
    public interface IChangeSetDetails
    {
        /// <summary>
        /// Gets a collection of objects that have been inserted to the collection.
        /// </summary>
        /// <value>An array of <see cref="RealmObject"/>s.</value>
        IReadOnlyList<dynamic> Insertions { get; }

        /// <summary>
        /// Gets a collection of <see cref="IModificationDetails"/> instances, describing
        /// the objects that have been modified in the collection.
        /// </summary>
        /// <value>An array of modifications.</value>
        IReadOnlyList<IModificationDetails> Modifications { get; }

        /// <summary>
        /// Gets a collection of the objects that have been deleted from the collection,
        /// at the version just prior to their deletion.
        /// </summary>
        /// <value>An array of <see cref="RealmObject"/>s.</value>
        IReadOnlyList<dynamic> Deletions { get; }
    }
}
