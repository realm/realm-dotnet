////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Collections;
using System.Collections.Generic;

namespace Realms.Schema
{
    /// <summary>
    /// Describes the complete set of classes which may be stored in a Realm, either from assembly declarations or,
    /// dynamically, by evaluating a Realm from disk.
    /// </summary>
    /// <remarks>
    /// By default this will be all the <see cref="RealmObject"/>s in all your assemblies unless you restrict with 
    /// <see cref="RealmConfigurationBase.ObjectClasses"/>. Just because a given class <em>may</em> be stored in a 
    /// Realm doesn't imply much overhead. There will be a small amount of metadata but objects only start to
    /// take up space once written.
    /// </remarks>
    public class RealmSchema : IReadOnlyCollection<ObjectSchema>
    {
        /// <summary>
        /// Gets the number of known classes in the schema.
        /// </summary>
        /// <value>The number of known classes.</value>
        public int Count { get; }

        private RealmSchema()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Finds the definition of a class in this schema.
        /// </summary>
        /// <param name="name">A valid class name which may be in this schema.</param>
        /// <exception cref="ArgumentException">Thrown if a name is not supplied.</exception>
        /// <returns>An <see cref="ObjectSchema"/> or <c>null</c> to indicate not found.</returns>
        public ObjectSchema Find(string name)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <inheritdoc/>
        public IEnumerator<ObjectSchema> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}