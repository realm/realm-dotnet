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

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Native;

namespace Realms
{
    public class GeoPolygon : GeoBase
    {
        public GeoPoint[] Points { get; }

        public GeoPolygon()
        {
            throw new NotImplementedException("this can't be implemented until https://github.com/realm/realm-core/pull/6529 is merged");
        }

        internal unsafe (NativeGeoPolygon NativePolygon, RealmValue.HandlesToCleanup? Handles) ToNative()
        {
            var handle = GCHandle.Alloc(Points.Select(p => p.ToNative()).ToArray(), GCHandleType.Pinned);

            return (new() { Points = (NativeGeoPoint*)handle.AddrOfPinnedObject(), PointsLength = (IntPtr)Points.Length }, new(handle));
        }

        internal (NativeQueryArgument QueryArgument, RealmValue.HandlesToCleanup? Handles) ToNativeQueryArgument()
        {
            var (polygon, handles) = ToNative();
            return (NativeQueryArgument.GeoPolygon(polygon), handles);
        }
    }
}
