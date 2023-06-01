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
    [StructLayout(LayoutKind.Explicit)]
    internal struct NativeQueryArgument
    {
        [FieldOffset(0)]
        private PrimitiveValue _primitive;

        [FieldOffset(0)]
        private NativeGeoBox _box;

        [FieldOffset(0)]
        private NativeGeoPolygon _polygon;

        [FieldOffset(0)]
        private NativeGeoCircle _circle;

        [FieldOffset(32)]
        [MarshalAs(UnmanagedType.U1)]
        public QueryArgumentType _type;

        public static NativeQueryArgument Primitive(PrimitiveValue primitive) => new()
        {
            _primitive = primitive,
            _type = QueryArgumentType.Primitive
        };

        public static NativeQueryArgument GeoBox(NativeGeoBox box) => new()
        {
            _box = box,
            _type = QueryArgumentType.Box
        };

        public static NativeQueryArgument GeoPolygon(NativeGeoPolygon polygon) => new()
        {
            _polygon = polygon,
            _type = QueryArgumentType.Polygon
        };

        public static NativeQueryArgument GeoCircle(NativeGeoCircle circle) => new()
        {
            _circle = circle,
            _type = QueryArgumentType.Circle
        };

        public override string ToString()
        {
            return $"[QueryArgument] " + _type switch
            {
                QueryArgumentType.Primitive => $"primitive {_primitive.Type}",
                QueryArgumentType.Box => _box.ToString(),
                QueryArgumentType.Polygon => _polygon.ToString(),
                QueryArgumentType.Circle => _circle.ToString(),
                _ => "Unknown",
            };
        }
    }

    internal enum QueryArgumentType : byte
    {
        Primitive,
        Box,
        Polygon,
        Circle,
    }
}
