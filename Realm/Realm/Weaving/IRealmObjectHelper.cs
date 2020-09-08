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

namespace Realms.Weaving
{
    /// <summary>
    /// A helper class for internal use. Helper classes are generated automatically and provide strongly typed class-specific convenience methods.
    /// </summary>
    public interface IRealmObjectHelper
    {
        /// <summary>
        /// Creates an instance of a RealmObjectBase.
        /// </summary>
        /// <returns>The RealmObjectBase.</returns>
        RealmObject CreateInstance();

        /// <summary>
        /// A strongly typed, optimized method to add a RealmObjectBase to the realm.
        /// </summary>
        /// <param name="instance">The RealmObjectBase to add.</param>
        /// <param name="update">If set to <c>true</c>, update the existing value (if any). Otherwise, try to add and throw if an object with the same primary key already exists.</param>
        /// <param name="skipDefaults">
        /// If set to <c>true</c> will not invoke the setters of properties that have default values.
        /// Generally, should be <c>true</c> for newly created objects and <c>false</c> when updating existing ones.
        /// </param>
        void CopyToRealm(RealmObject instance, bool update, bool skipDefaults);

        /// <summary>
        /// Tries the get primary key value from a RealmObjectBase.
        /// </summary>
        /// <returns><c>true</c>, if the class has primary key, <c>false</c> otherwise.</returns>
        /// <param name="instance">The RealmObjectBase instance.</param>
        /// <param name="value">The value of the primary key.</param>
        bool TryGetPrimaryKeyValue(RealmObject instance, out object value);
    }
}