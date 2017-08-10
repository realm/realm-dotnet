////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
    /// An exception thrown when attempting to use a feature that is not available at your edition level.
    /// If you're using a paid edition of the Realm Mobile Platform, make sure you call
    /// <see cref="Realm.SetAccessToken"/> before any calls to <see cref="Realm.GetInstance(RealmConfigurationBase)"/>.
    /// </summary>
    /// <seealso href="https://realm.io/docs/realm-object-server/pe-ee/#enabling-professional-and-enterprise-apis">
    /// See more details on Enabling Professional and Enterprise APIs in the documentation.
    /// </seealso>
    public class RealmFeatureUnavailableException : RealmException
    {
        internal RealmFeatureUnavailableException(string message) : base(message)
        {
        }
    }
}