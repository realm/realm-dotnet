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

namespace Realms
{
    /// <summary>
    /// Exception thrown when a file exists but doesn't appear to be a Realm database, may indicate corruption.
    /// </summary>
    public class RealmInvalidDatabaseException : RealmFileAccessErrorException
    {
        internal RealmInvalidDatabaseException(string message) : base(message)
        {
        }
    }
}