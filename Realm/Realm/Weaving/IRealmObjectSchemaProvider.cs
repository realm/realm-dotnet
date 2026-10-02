////////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 Realm Inc.
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

using Realms.Schema;

namespace Realms.Weaving
{
    /// <summary>
    /// Implemented by generated helper classes to supply the object schema without reflection.
    /// </summary>
    public interface IRealmObjectSchemaProvider
    {
        /// <summary>
        /// Gets the schema of the class the helper was generated for.
        /// </summary>
        /// <value>The generated <see cref="Schema.ObjectSchema"/>.</value>
        ObjectSchema ObjectSchema { get; }
    }
}
