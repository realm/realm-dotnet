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

namespace Realms
{
    /// <summary>
    /// A <see cref="ChangeSet" /> describes the changes inside a <see cref="IRealmCollection{T}" /> since the last time the notification callback was invoked.
    /// </summary>
    public class ChangeSet
    {
        /// <summary>
        /// Gets the indices in the new version of the <see cref="IRealmCollection{T}" /> which were newly inserted.
        /// </summary>
        /// <value>An array, containing the indices of the inserted objects.</value>
        public int[] InsertedIndices { get; }

        /// <summary>
        /// Gets the indices in the *old* version of the <see cref="IRealmCollection{T}"/> which were modified.
        /// This means that either the property of an object at that index was modified or the property of
        /// of an object it's related to has changed.
        /// </summary>
        /// <value>An array, containing the indices of the modified objects.</value>
        public int[] ModifiedIndices { get; }

        /// <summary>
        /// Gets the indices in the *new* version of the <see cref="IRealmCollection{T}"/> which were modified.
        /// Conceptually, it contains the same entries as <see cref="ModifiedIndices"/> but after the insertions
        /// and deletions have been accounted for.
        /// </summary>
        /// <value>An array, containing the indices of the modified objects.</value>
        public int[] NewModifiedIndices { get; }

        /// <summary>
        /// Gets the indices of objects in the previous version of the <see cref="IRealmCollection{T}"/> which have been removed from this one.
        /// </summary>
        /// <value>An array, containing the indices of the deleted objects.</value>
        public int[] DeletedIndices { get; }

        /// <summary>
        /// Gets the rows in the collection which moved.
        /// </summary>
        /// <remarks>
        /// Every <see cref="Move.From"/> index will be present in <see cref="DeletedIndices"/> and every <see cref="Move.To"/>
        /// index will be present in <see cref="InsertedIndices"/>.
        /// </remarks>
        /// <value>An array of <see cref="Move"/> structs, indicating the source and the destination index of the moved row.</value>
        public Move[] Moves { get; }

        internal ChangeSet(int[] insertedIndices, int[] modifiedIndices, int[] newModifiedIndices, int[] deletedIndices, Move[] moves)
        {
            InsertedIndices = insertedIndices;
            ModifiedIndices = modifiedIndices;
            NewModifiedIndices = newModifiedIndices;
            DeletedIndices = deletedIndices;
            Moves = moves;
        }

        /// <summary>
        /// A <see cref="Move" /> contains information about objects that moved within the same <see cref="IRealmCollection{T}"/>.
        /// </summary>
        public struct Move
        {
            /// <summary>
            /// Gets the index in the old version of the <see cref="IRealmCollection{T}" /> from which the object has moved.
            /// </summary>
            /// <value>The source index of the object.</value>
            public int From { get; }

            /// <summary>
            /// Gets the index in the new version of the <see cref="IRealmCollection{T}" /> to which the object has moved.
            /// </summary>
            /// <value>The destination index of the object.</value>
            public int To { get; }

            internal Move(int from, int to)
            {
                From = from;
                To = to;
            }
        }
    }
}
