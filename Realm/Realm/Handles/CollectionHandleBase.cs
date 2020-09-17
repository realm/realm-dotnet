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
using Realms.Schema;

namespace Realms
{
    internal abstract class CollectionHandleBase : NotifiableObjectHandleBase
    {
        public abstract bool IsValid { get; }

        protected CollectionHandleBase(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public bool TryGetObjectAtIndex(int index, out ObjectHandle objectHandle)
        {
            var result = GetObjectAtIndexCore((IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(Root, result);
            return true;
        }

        protected abstract IntPtr GetObjectAtIndexCore(IntPtr index, out NativeException nativeException);

        public PrimitiveValue GetPrimitiveAtIndex(int index, PropertyType type)
        {
            var result = new PrimitiveValue
            {
                Type = type
            };

            GetPrimitiveAtIndexCore((IntPtr)index, ref result, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        protected abstract void GetPrimitiveAtIndexCore(IntPtr index, ref PrimitiveValue result, out NativeException nativeException);

        public abstract string GetStringAtIndex(int index);

        public abstract byte[] GetByteArrayAtIndex(int index);

        public abstract int Count();

        public abstract ResultsHandle Snapshot();

        public abstract ResultsHandle GetFilteredResults(string query);
    }
}