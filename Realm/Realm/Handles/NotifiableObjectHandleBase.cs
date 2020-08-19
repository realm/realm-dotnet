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
    internal abstract class NotifiableObjectHandleBase : RealmHandle, IThreadConfinedHandle
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct CollectionChangeSet
        {
            public MarshaledVector<IntPtr> Deletions;
            public MarshaledVector<IntPtr> Insertions;
            public MarshaledVector<IntPtr> Modifications;
            public MarshaledVector<IntPtr> Modifications_New;

            [StructLayout(LayoutKind.Sequential)]
            public struct Move
            {
                public IntPtr From;
                public IntPtr To;
            }

            public MarshaledVector<Move> Moves;
            public MarshaledVector<IntPtr> Properties;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NotificationCallbackDelegate(IntPtr managedHandle, IntPtr changes, IntPtr notificationException);

        protected NotifiableObjectHandleBase(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public abstract NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback);

        public abstract ThreadSafeReferenceHandle GetThreadSafeReference();

        public abstract bool IsFrozen { get; }
    }
}
