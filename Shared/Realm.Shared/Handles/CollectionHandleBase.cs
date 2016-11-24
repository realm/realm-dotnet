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
using System.Runtime.InteropServices;

namespace Realms
{
    internal abstract class CollectionHandleBase : RealmHandle
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct CollectionChangeSet
        {
            public MarshaledVector<IntPtr> Deletions;
            public MarshaledVector<IntPtr> Insertions;
            public MarshaledVector<IntPtr> Modifications;

            [StructLayout(LayoutKind.Sequential)]
            public struct Move
            {
                public IntPtr From;
                public IntPtr To;
            }

            public MarshaledVector<Move> Moves;
        }

        internal delegate void NotificationCallbackDelegate(IntPtr managedCollectionHandle, IntPtr collectionChanges, IntPtr notficiationException);

        public abstract IntPtr AddNotificationCallback(IntPtr managedCollectionHandle, NotificationCallbackDelegate callback);

        public abstract IntPtr DestroyNotificationToken(IntPtr token);

        public abstract IntPtr GetObjectAtIndex(long index);
    }
}