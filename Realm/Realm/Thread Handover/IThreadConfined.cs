////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms
{
    /// <summary>
    /// An interface representing a thread confined object.
    /// </summary>
    internal interface IThreadConfined
    {
        /// <summary>
        /// Gets a value indicating whether the object is managed.
        /// </summary>
        bool IsManaged { get; }

        /// <summary>
        /// Gets a value indicating whether the object is still valid (i.e. its Realm isn't closed and the object isn't deleted).
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value representing the object's metadata.
        /// </summary>
        RealmObjectBase.Metadata Metadata { get; }

        /// <summary>
        /// Gets a value representing the native handle for that object.
        /// </summary>
        IThreadConfinedHandle Handle { get; }
    }
}
