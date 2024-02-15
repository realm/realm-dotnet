// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2023 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using MongoDB.Bson;

#if !NETCOREAPP2_1_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Baas
{
    public static class Utils
    {
        public static HttpContent GetJsonContent(object obj)
        {
            string jsonContent;

            if (obj is Array arr)
            {
                var bsonArray = new BsonArray();
                foreach (var elem in arr)
                {
                    bsonArray.Add(elem.ToBsonDocument());
                }

                jsonContent = bsonArray.ToJson();
            }
            else
            {
                jsonContent = obj is BsonDocument doc ? doc.ToJson() : obj.ToJson();
            }

            var content = new StringContent(jsonContent);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }

        public static (Dictionary<string, string> Extracted, string[] RemainingArgs) ExtractArguments(string[] args, params string[] toExtract)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var extracted = new Dictionary<string, string>();
            var remainingArgs = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                if (!toExtract.Any(name => ExtractArg(i, name)))
                {
                    remainingArgs.Add(args[i]);
                }
            }

            return (extracted, remainingArgs.ToArray());

            bool ExtractArg(int index, string name)
            {
                var arg = args[index];
                if (arg.StartsWith($"--{name}="))
                {
                    extracted[name] = arg.Replace($"--{name}=", string.Empty);
                    return true;
                }

                return false;
            }
        }

#if !NETCOREAPP2_1_OR_GREATER

        [return: NotNullIfNotNull("defaultValue")]
        public static T? GetValueOrDefault<T>(this IDictionary<string, T> dictionary, string key, T? defaultValue = default)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
#endif
    }
}
