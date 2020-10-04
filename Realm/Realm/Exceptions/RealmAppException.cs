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

using Realms.Exceptions;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception being thrown when performing app-related operations.
    /// </summary>
    public class RealmAppException : RealmException
    {
        /// <summary>
        /// Gets the category (type) of this exception.
        /// </summary>
        /// <value>The exception category - for example Client Error, Http Error, etc.</value>
        public string Category { get; }

        internal RealmAppException(RealmExceptionCodes code, string detailMessage) : base(detailMessage)
        {
            Category = GetCategory(code);
        }

        private static string GetCategory(RealmExceptionCodes code)
        {
            return code switch
            {
                RealmExceptionCodes.AppClientError => "Client Error",
                RealmExceptionCodes.AppCustomError => "Custom Error",
                RealmExceptionCodes.AppHttpError => "Http Error",
                RealmExceptionCodes.AppJsonError => "Json Error",
                RealmExceptionCodes.AppServiceError => "Service Error",
                _ => "Unknown"
            };
        }
    }
}
