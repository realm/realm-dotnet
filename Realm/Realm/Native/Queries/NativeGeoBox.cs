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

using System.Runtime.InteropServices;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NativeGeoBox
    {
        private readonly NativeGeoPoint _bottom_left_corner;
        private readonly NativeGeoPoint _top_right_corner;

        public NativeGeoBox(NativeGeoPoint bottom_left, NativeGeoPoint top_right)
        {
            _bottom_left_corner = bottom_left;
            _top_right_corner = top_right;
        }
    }
}
