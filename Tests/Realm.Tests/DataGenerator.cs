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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Tests
{
    internal static class DataGenerator
    {
        private static readonly Random _random = new();

        private static readonly Dictionary<Type, Array> _data = new()
        {
            [typeof(bool)] = new[] { true, false },
            [typeof(char)] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray(),
        };

        private static readonly Dictionary<Type, Func<object>> _generators = new()
        {
            [typeof(short)] = () => (short)_random.Next(short.MinValue, short.MaxValue),
            [typeof(byte)] = () => (byte)_random.Next(byte.MinValue, byte.MaxValue),
            [typeof(int)] = () => _random.Next(int.MinValue, int.MaxValue),
            [typeof(long)] = () => (long)_random.Next(),
            [typeof(float)] = () => Convert.ToSingle(GenerateDouble(float.MaxValue, float.MinValue)),
            [typeof(double)] = () => GenerateDouble(int.MinValue, int.MaxValue),
            [typeof(decimal)] = () => Convert.ToDecimal(GenerateDouble((double)decimal.MaxValue, (double)decimal.MinValue)),
            [typeof(Decimal128)] = () => new Decimal128(GenerateDouble((double)decimal.MaxValue, (double)decimal.MinValue)),
            [typeof(ObjectId)] = () => ObjectId.GenerateNewId(),
            [typeof(string)] = () => Guid.NewGuid().ToString(),
            [typeof(byte[])] = () => TestHelpers.GetBytes(10),
            [typeof(DateTimeOffset)] = () => new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(GenerateInt(-365 * 1000, 365 * 1000)),
            [typeof(Guid)] = () => Guid.NewGuid(),
            [typeof(RealmValue)] = () => GenerateRealmValue(),
        };

        public static void FillCollection<T>(ICollection<T> collection, int elementCount)
        {
            for (var i = 0; i < elementCount; i++)
            {
                collection.Add(GenerateRandom<T>());
            }
        }

        public static void FillCollection(object collection, int elementCount)
        {
            foreach (var iface in collection.GetType().GetInterfaces())
            {
                if (iface.IsClosedGeneric(typeof(ICollection<>), out var innerTypes))
                {
                    var mi = typeof(DataGenerator).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == nameof(FillCollection) && m.IsGenericMethod);
                    mi.MakeGenericMethod(innerTypes[0]).Invoke(null, new[] { collection, elementCount });

                    return;
                }
            }

            throw new NotSupportedException($"Can't populate {collection.GetType()} because it's not ICollection<>");
        }

        public static T GenerateRandom<T>() => (T)GenerateRandom(typeof(T));

        public static object GenerateRandom(Type type)
        {
            if (type.IsClosedGeneric(typeof(Nullable<>), out var args))
            {
                type = args.Single();
            }

            if (type.IsClosedGeneric(typeof(KeyValuePair<,>), out var kvpArgs))
            {
                Argument.Ensure(kvpArgs[0] == typeof(string), "Only KeyValuePair<string, T> is supported", nameof(type));
                var value = GenerateRandom(kvpArgs[1]);
                return Activator.CreateInstance(type, Guid.NewGuid().ToString(), value)!;
            }

            if (_data.TryGetValue(type, out var items))
            {
                return PickRandomElement(items);
            }

            if (_generators.TryGetValue(type, out var generator))
            {
                return generator();
            }

            throw new NotSupportedException($"Can't generate a random value for {type}.");
        }

        private static object PickRandomElement(Array items)
        {
            var index = _random.Next(0, items.Length);
            return items.GetValue(index)!;
        }

        private static T PickRandomElement<T>(T[] items) => (T)PickRandomElement((Array)items);

        private static int GenerateInt(int minValue, int maxValue) => _random.Next(minValue, maxValue);

        private static double GenerateDouble(double minValue, double maxValue) => (_random.NextDouble() * (maxValue - minValue)) + minValue;

        private static RealmValueType[] _realmValueTypes = Enum.GetValues(typeof(RealmValueType)).Cast<RealmValueType>().ToArray();

        private static RealmValue GenerateRealmValue()
        {
            var type = PickRandomElement(_realmValueTypes);
            return type switch
            {
                RealmValueType.Int => GenerateRandom<int>(),
                RealmValueType.Bool => GenerateRandom<bool>(),
                RealmValueType.String => GenerateRandom<string>(),
                RealmValueType.Data => GenerateRandom<byte[]>(),
                RealmValueType.Date => GenerateRandom<DateTimeOffset>(),
                RealmValueType.Float => GenerateRandom<float>(),
                RealmValueType.Double => GenerateRandom<double>(),
                RealmValueType.Decimal128 => GenerateRandom<Decimal128>(),
                RealmValueType.ObjectId => GenerateRandom<ObjectId>(),
                RealmValueType.Guid => GenerateRandom<Guid>(),
                _ => RealmValue.Null,
            };
        }
    }
}
