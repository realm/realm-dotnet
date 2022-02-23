﻿////////////////////////////////////////////////////////////////////////////
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
using Realms.Native;

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
        public delegate void NotificationCallback(IntPtr managedHandle, IntPtr changes, IntPtr notificationException);

        protected NotifiableObjectHandleBase(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public abstract NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle);

        public abstract ThreadSafeReferenceHandle GetThreadSafeReference();

        [MonoPInvokeCallback(typeof(NotificationCallback))]
        public static void NotifyObjectChanged(IntPtr managedHandle, IntPtr changes, IntPtr exception)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is INotifiable<CollectionChangeSet> notifiable)
            {
                notifiable.NotifyCallbacks(new PtrTo<CollectionChangeSet>(changes).Value, new PtrTo<NativeException>(exception).Value);
            }
        }
    }
}
