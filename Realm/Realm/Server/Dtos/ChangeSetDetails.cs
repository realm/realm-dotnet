////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using Realms.Native;
using Realms.Server.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Realms.Server
{
    internal class ChangeSetDetails : IChangeSetDetails
    {
        public IReadOnlyList<dynamic> Insertions { get; }

        public IReadOnlyList<IModificationDetails> Modifications { get; }

        public IReadOnlyList<dynamic> Deletions { get; }

        public ChangeSetDetails(Realm previous, Realm current, string className, IEnumerable<ObjectKey> insertions, IEnumerable<NativeModificationDetails> modifications, IEnumerable<ObjectKey> deletions)
        {
            if (previous == null || !previous.Metadata.TryGetValue(className, out var previousMetadata))
            {
                if (deletions.Any() || modifications.Any())
                {
                    throw new NotSupportedException($"Failed to find metadata for object of type {className} in the previous Realm.");
                }

                previousMetadata = null;
            }

            var currentMetadata = current.Metadata[className];

            Insertions = insertions.Select(objKey => current.MakeObject(currentMetadata, objKey)).ToArray();
            Deletions = deletions.Select(objKey => previous.MakeObject(previousMetadata, objKey)).ToArray();
            Modifications = modifications.Select(m => new ModificationDetails(() => previous.MakeObject(previousMetadata, m.obj),
                                                                              () => current.MakeObject(currentMetadata, m.obj),
                                                                              () => new HashSet<string>(m.changed_columns.AsEnumerable().Select(currentMetadata.Table.GetColumnName))))
                                         .ToArray();
        }
    }
}
