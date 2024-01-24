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
using System.Collections;
using System.Collections.Generic;
using Realms.Native;

namespace Realms
{
    internal abstract class CollectionHandleBase : NotifiableObjectHandleBase
    {
        public abstract bool IsValid { get; }

        public virtual bool CanSnapshot => false;

        protected CollectionHandleBase(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public abstract int Count();

        public ResultsHandle Snapshot()
        {
            EnsureIsOpen();

            var ptr = SnapshotCore(out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(Root!, ptr);
        }

        public ResultsHandle GetFilteredResults(string query, QueryArgument[] arguments)
        {
            EnsureIsOpen();

            var (nativeArgs, handles) = arguments.ToNativeValues();
            var ptr = GetFilteredResultsCore(query, nativeArgs, out var ex);
            handles.Dispose();

            ex.ThrowIfNecessary();
            return new ResultsHandle(Root!, ptr);
        }

        public abstract CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle);

        public abstract void Clear();

        protected abstract IntPtr GetFilteredResultsCore(string query, NativeQueryArgument[] arguments, out NativeException ex);

        protected virtual IntPtr SnapshotCore(out NativeException ex) => throw new NotSupportedException("Snapshotting this collection is not supported.");

        public abstract NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, bool shallow);

        public virtual NotificationTokenHandle AddNotificationCallbackKeypaths(IntPtr managedObjectHandle, PrimitiveKeyPathsCollection arguments)
        {
            //TODO This should be just abstract, so we need to implement it in all handles
            return AddNotificationCallback(managedObjectHandle, true);
        }
    }
}
