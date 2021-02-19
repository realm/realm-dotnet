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

namespace Realms.Exceptions
{
    /// <summary>
    /// Exception thrown when the current schema doesn't respect Realm's set of rule. The details in the message helps to understand what the broken rule is.
    /// More general info about Realm's schema can be found <see href="https://docs.mongodb.com/realm/dotnet/realms/#std-label-dotnet-realm-schema">here</see>.
    /// </summary>
    public class RealmSchemaValidationException : RealmException
    {
        internal RealmSchemaValidationException(string detailMessage) : base(detailMessage)
        {
        }
    }
}