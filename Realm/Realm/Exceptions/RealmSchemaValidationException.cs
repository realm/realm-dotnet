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
    /// Exception thrown when the schema specified in your C# models doesn't pass validation for the type of Realm you're trying to open.
    /// The message contains information about the validation errors and how to correct them.
    /// </summary>
    /// <seealso href="https://www.mongodb.com/docs/realm/sdk/dotnet/model-data/define-object-model/">
    /// General information about Realm's schema.
    /// </seealso>
    public class RealmSchemaValidationException : RealmException
    {
        internal RealmSchemaValidationException(string detailMessage) : base(detailMessage)
        {
        }
    }
}
