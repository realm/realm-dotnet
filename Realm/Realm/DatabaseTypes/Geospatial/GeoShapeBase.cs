////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using System.Linq;

namespace Realms
{
    /// <summary>
    /// A base class for the geometry types supported by Realm. It should not be used directly -
    /// instead you should use one of its inheritors, such as <see cref="GeoBox"/>, <see cref="GeoCircle"/>, or
    /// <see cref="GeoPolygon"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="GeoShapeBase"/> and its inheritors cannot be used as properties in a Realm model. Instead,
    /// they are only used as an argument to <see cref="QueryMethods.GeoWithin"/> or
    /// <see cref="CollectionExtensions.Filter{T}(IQueryable{T}, string, QueryArgument[])"/>.
    /// </remarks>
    /// <seealso cref="QueryMethods.GeoWithin"/>
    public abstract class GeoShapeBase
    {
    }
}
