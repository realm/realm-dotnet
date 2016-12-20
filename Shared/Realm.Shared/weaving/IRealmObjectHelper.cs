﻿////////////////////////////////////////////////////////////////////////////
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
        /// Creates an instance of a RealmObject.
        /// </summary>
        /// <returns>The RealmObject.</returns>
        RealmObject CreateInstance();

        /// <summary>
        /// A strongly typed, optimized method to add a RealmObject to the realm.
        /// </summary>
        /// <param name="instance">The RealmObject to add.</param>
        /// <param name="update">If set to <c>true</c>, update the existing value (if any). Otherwise, try to add and throw if an object with the same primary key already exists.</param>
        /// <param name="setPrimaryKey">If set to <c>true</c> will set the primary key of the object (if any).</param>
        void CopyToRealm(RealmObject instance, bool update, bool setPrimaryKey);

        /// <summary>
        /// Tries the get primary key value from a RealmObject.
        /// </summary>
        /// <returns><c>true</c>, if the class has primary key, <c>false</c> otherwise.</returns>
        /// <param name="instance">The RealmObject instance.</param>
        /// <param name="value">The value of the primary key.</param>
        bool TryGetPrimaryKeyValue(RealmObject instance, out object value);
    }
}