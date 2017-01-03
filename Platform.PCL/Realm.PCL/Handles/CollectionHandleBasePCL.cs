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

namespace Realms
{
    internal abstract class CollectionHandleBase : RealmHandle
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct CollectionChangeSet
        {
        }

        internal delegate void NotificationCallbackDelegate(IntPtr managedCollectionHandle, IntPtr collectionChanges, IntPtr notficiationException);

        protected CollectionHandleBase(RealmHandle root) : base(root)
        {
        }

        public abstract IntPtr AddNotificationCallback(IntPtr managedCollectionHandle, NotificationCallbackDelegate callback);

        public abstract IntPtr GetObjectAtIndex(int index);

        public abstract int Count();

        public IntPtr DestroyNotificationToken(IntPtr token)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }
    }
}