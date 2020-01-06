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

namespace Realms.Server
{
    /// <summary>
    /// An object that contains information about the change that occurred on a single
    /// object.
    /// </summary>
    public interface IModificationDetails
    {
        /// <summary>
        /// Gets the index of the modified object in the collection before the change. If the
        /// object has been inserted, it will return -1.
        /// </summary>
        /// <value>An integer index.</value>
        int PreviousIndex { get; }

        /// <summary>
        /// Gets the index of the modified object in the collection after the change. If the
        /// object has been deleted, it will return -1.
        /// </summary>
        /// <value>An integer index.</value>
        int CurrentIndex { get; }

        /// <summary>
        /// Gets the object as it was before the change. If the object has been inserted, it will
        /// return <c>null</c>.
        /// </summary>
        /// <value>A <see cref="RealmObject"/> instance.</value>
        dynamic PreviousObject { get; }

        /// <summary>
        /// Gets the object as it is after the change. If the object has been deleted, it will
        /// return <c>null</c>.
        /// </summary>
        /// <value>A <see cref="RealmObject"/> instance.</value>
        dynamic CurrentObject { get; }
    }
}
