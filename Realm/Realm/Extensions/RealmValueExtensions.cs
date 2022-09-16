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
using Realms.Exceptions;
using Realms.Native;

namespace Realms
{
    internal static class RealmValueExtensions
    {
        private static readonly HashSet<RealmValueType> _numericTypes = new HashSet<RealmValueType>
        {
            RealmValueType.Int,
            RealmValueType.Float,
            RealmValueType.Double,
            RealmValueType.Decimal128
        };

        public static bool IsNumeric(this RealmValueType type) => _numericTypes.Contains(type);

        public static (PrimitiveValue[] Values, RealmValue.HandlesToCleanup?[] Handles) ToPrimitiveValues(this RealmValue[] arguments)
        {
            var primitiveValues = new PrimitiveValue[arguments.Length];
            var handles = new RealmValue.HandlesToCleanup?[arguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                if (argument.Type == RealmValueType.Object && !argument.AsIRealmObject().IsManaged)
                {
                    throw new RealmException("Can't use unmanaged object as argument of Filter");
                }

                (primitiveValues[i], handles[i]) = argument.ToNative();
            }

            return (primitiveValues, handles);
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
