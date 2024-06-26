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

namespace Realms.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to open a file whose <see cref="Realms.Schema.RealmSchema"/> differs from your current class declarations.
    /// </summary>
    /// <seealso href="https://www.mongodb.com/docs/realm/sdk/dotnet/model-data/change-an-object-model/#migrate-a-schema/">Read more about Migrations.</seealso>
    public class RealmMigrationNeededException : RealmFileAccessErrorException
    {
        internal RealmMigrationNeededException(string message) : base(message)
        {
            HelpLink = "https://www.mongodb.com/docs/realm/sdk/dotnet/model-data/change-an-object-model/#migrate-a-schema/";
        }
    }
}
