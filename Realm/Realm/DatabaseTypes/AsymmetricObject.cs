////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
    /// Base for any object that can be persisted in a <see cref="Realm"/> but that cannot retrieved, hence cannot modified.
    /// </summary>
    /// <remarks>
    /// The benefit of using <see cref="AsymmetricObject"/> is that the performance of each sync operation is much higher.
    /// The drawback is that an <see cref="AsymmetricObject"/> is synced unidirectionally, so it cannot be queried.
    /// You should use this base when you have a write-heavy use case.
    /// If, instead you want to persist an object that you can also query against, use <see cref="RealmObject"/> instead.
    /// RealmObjects and EmbeddedObjects can't link (or backlink) to AsymmetricObjects. AsymmetricObjects can only link to EmbeddedObjects.
    /// </remarks>
    /// <seealso href="https://www.mongodb.com/docs/realm/sdk/dotnet/data-types/asymmetric-objects/"/>
    public class AsymmetricObject : RealmObjectBase, IAsymmetricObject
    {
    }
}
