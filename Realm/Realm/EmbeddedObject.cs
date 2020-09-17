////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

namespace Realms
{
    /// <summary>
    /// Base for any embedded object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    [Serializable]
    public class EmbeddedObject : RealmObjectBase
    {
        /// <summary>
        /// Gets the parent of this <see cref="EmbeddedObject"/>. It can be either another
        /// <see cref="EmbeddedObject"/> or a standalone <see cref="RealmObject"/>.
        /// </summary>
        public RealmObjectBase Parent { get; }
    }
}
