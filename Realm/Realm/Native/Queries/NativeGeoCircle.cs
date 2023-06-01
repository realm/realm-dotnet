﻿////////////////////////////////////////////////////////////////////////////
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

using System.Runtime.InteropServices;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NativeGeoCircle
    {
        private readonly NativeGeoPoint _center;
        private readonly double _radius_in_radians;

        public NativeGeoCircle(NativeGeoPoint center, double radius_in_radians)
        {
            _center = center;
            _radius_in_radians = radius_in_radians;
        }

        public override string ToString() => $"Circle {{center: {_center}, radius: {_radius_in_radians}}}";
    }
}
