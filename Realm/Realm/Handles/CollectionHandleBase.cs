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
using Realms.Native;

namespace Realms
{
    internal abstract class CollectionHandleBase : NotifiableObjectHandleBase
    {
        protected delegate IntPtr SnapshotDelegate(out NativeException ex);

        public abstract bool IsValid { get; }

        public bool CanSnapshot => SnapshotCore != null;

        protected virtual SnapshotDelegate SnapshotCore => null;

        protected CollectionHandleBase(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public abstract int Count();

        public ResultsHandle Snapshot()
        {
            if (CanSnapshot)
            {
                var ptr = SnapshotCore(out var ex);
                ex.ThrowIfNecessary();
                return new ResultsHandle(Root ?? this, ptr);
            }

            throw new NotSupportedException("Snapshotting this collection is not supported.");
        }

        public abstract ResultsHandle GetFilteredResults(string query);

        public abstract CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle);

        public abstract void Clear();

        protected RealmValue ToRealmValue(PrimitiveValue primitive, RealmObjectBase.Metadata metadata, Realm realm)
        {
            if (primitive.Type != RealmValueType.Object)
            {
                return new RealmValue(primitive);
            }

            var objectHandle = primitive.AsObject(Root);
            if (metadata == null)
            {
                throw new NotImplementedException("Mixed objects are not supported yet.");
            }

            return new RealmValue(realm.MakeObject(metadata, objectHandle));
        }
    }
}