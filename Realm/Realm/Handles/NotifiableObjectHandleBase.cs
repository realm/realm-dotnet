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
using Realms.Native;

namespace Realms
{
    internal abstract class NotifiableObjectHandleBase : RealmHandle, IThreadConfinedHandle
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct CollectionChangeSet
        {
            public MarshaledVector<nint> Deletions;
            public MarshaledVector<nint> Insertions;
            public MarshaledVector<nint> Modifications;
            public MarshaledVector<nint> Modifications_New;

            [StructLayout(LayoutKind.Sequential)]
            public struct Move
            {
                public nint From;
                public nint To;
            }

            public MarshaledVector<Move> Moves;

            public NativeBool Cleared;

            public MarshaledVector<int> Properties;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void NotificationCallback(IntPtr managedHandle, CollectionChangeSet* changes, bool shallow);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void NotificationCallbackKeypath(IntPtr managedHandle, CollectionChangeSet* changes, IntPtr callback);

        protected NotifiableObjectHandleBase(SharedRealmHandle? root, IntPtr handle) : base(root, handle)
        {
        }

        public abstract ThreadSafeReferenceHandle GetThreadSafeReference();

        [MonoPInvokeCallback(typeof(NotificationCallback))]
        public static unsafe void NotifyObjectChanged(IntPtr managedHandle, CollectionChangeSet* changes, bool shallow)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is INotifiable<CollectionChangeSet> notifiable)
            {
                notifiable.NotifyCallbacks(changes == null ? null : *changes, shallow);
            }
        }

        [MonoPInvokeCallback(typeof(NotificationCallbackKeypath))]
        public static unsafe void NotifyObjectChangedKeypath(IntPtr managedHandle, CollectionChangeSet* changes, IntPtr callback)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is INotifiable<CollectionChangeSet> notifiable)
            {
                notifiable.NotifyCallbacksKeypath(changes == null ? null : *changes, callback);
            }
        }
    }
}
