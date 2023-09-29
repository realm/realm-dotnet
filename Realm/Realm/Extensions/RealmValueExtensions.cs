////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

using System.Collections.Generic;
using Realms.Native;

namespace Realms
{
    internal static class RealmValueExtensions
    {
        private static readonly HashSet<RealmValueType> _numericTypes = new()
        {
            RealmValueType.Int,
            RealmValueType.Float,
            RealmValueType.Double,
            RealmValueType.Decimal128
        };

        private static readonly HashSet<RealmValueType> _collectionTypes = new()
        {
            RealmValueType.List,
            RealmValueType.Set,
            RealmValueType.Dictionary,
        };

        public static bool IsNumeric(this RealmValueType type) => _numericTypes.Contains(type);

        public static bool IsCollection(this RealmValueType type) => _collectionTypes.Contains(type);

        public static (NativeQueryArgument[] Values, RealmValue.HandlesToCleanup?[] Handles) ToNativeValues(this QueryArgument[] arguments)
        {
            var nativeArgs = new NativeQueryArgument[arguments.Length];
            var handles = new RealmValue.HandlesToCleanup?[arguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                (nativeArgs[i], handles[i]) = arguments[i].ToNative();
            }

            return (nativeArgs, handles);
        }

        public static void Dispose(this RealmValue.HandlesToCleanup?[] handles)
        {
            foreach (var argHandles in handles)
            {
                argHandles?.Dispose();
            }
        }
    }
}
