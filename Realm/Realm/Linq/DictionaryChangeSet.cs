////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

namespace Realms
{
    /// <summary>
    /// A <see cref="DictionaryChangeSet" /> describes the changes inside a <see cref="IDictionary{String,TValue}" /> since the last time the notification callback was invoked.
    /// </summary>
    public class DictionaryChangeSet
    {
        /// <summary>
        /// Gets the keys in the <see cref="IDictionary{String, TValue}"/> which were deleted.
        /// </summary>
        /// <value>An array, containing the keys of the removed values.</value>
        public string[] DeletedKeys { get; }

        /// <summary>
        /// Gets the keys in the <see cref="IDictionary{String, TValue}"/> which were newly inserted.
        /// </summary>
        /// <value>An array, containing the keys of the inserted values.</value>
        public string[] InsertedKeys { get; }

        /// <summary>
        /// Gets the keys in the <see cref="IDictionary{String, TValue}"/> whose values were modified.
        /// </summary>
        /// <value>An array, containing the keys of the modified values.</value>
        public string[] ModifiedKeys { get; }

        internal DictionaryChangeSet(string[] deletedKeys, string[] insertedKeys, string[] modifiedKeys)
        {
            DeletedKeys = deletedKeys;
            InsertedKeys = insertedKeys;
            ModifiedKeys = modifiedKeys;
        }
    }
}
