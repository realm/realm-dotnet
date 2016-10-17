////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Realms.Native
{
    internal class SortDescriptorBuilder
    {
        public class Clause
        {
            // This list contains the column indices necessary to traverse
            // the links. The last index in the list must be to a non-link column.
            // I.e. if sort is being applied to Dog.Owner.Name, there will
            // be two indices, the first pointing to Dog.Owner and the second
            // to Person.Name (given Owner is an instance of Person).
            public List<IntPtr> PropertyIndexChain;
            public bool Ascending;

            [StructLayout(LayoutKind.Sequential)]
            public struct Marshalable
            {
                public IntPtr Offset;
                public IntPtr Count;

                [MarshalAs(UnmanagedType.I1)]
                public bool Ascending;
            }
        }

        private readonly List<Clause> _clauses = new List<Clause>();
        private readonly RealmObject.Metadata _metadata;

        public TableHandle TableHandle => _metadata.Table;

        public SortDescriptorBuilder(RealmObject.Metadata metadata)
        {
            _metadata = metadata;
        }

        // This method adds a direct sort clause, i.e. a clause that doesn't involve
        // any links.
        public void AddClause(string columnName, bool @ascending)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            _clauses.Add(new Clause
            {
                PropertyIndexChain = new List<IntPtr> { propertyIndex }, 
                Ascending = @ascending 
            });
        }

        /// <summary>
        /// Create a flattened array of all the clauses by concatenating the lists.
        /// </summary>
        /// <returns>
        /// A tuple with two elements. Item1 is the concatenated list of indices. Item2 is the list of clauses,
        /// indicating the offset and count, as well as the ascending flag for each clause.
        /// </returns>
        public Tuple<IntPtr[], Clause.Marshalable[]> Flatten()
        {
            var propertyIndexFlattener = new List<IntPtr>();
            var sortClauses = new List<Clause.Marshalable>();
            foreach (var clause in _clauses)
            {
                sortClauses.Add(new Clause.Marshalable
                {
                    Offset = (IntPtr)propertyIndexFlattener.Count,
                    Count = (IntPtr)clause.PropertyIndexChain.Count,
                    Ascending = clause.Ascending
                });

                propertyIndexFlattener.AddRange(clause.PropertyIndexChain);
            }

            return Tuple.Create(propertyIndexFlattener.ToArray(), sortClauses.ToArray());
        }
    }
}