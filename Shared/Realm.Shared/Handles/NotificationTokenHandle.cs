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
    // A NotificationToken in object-store references a Collection object.
    // We need to mirror this same relationship here.
    internal class NotificationTokenHandle : RealmHandle
    {
        private CollectionHandleBase _collectionHandle;

        internal NotificationTokenHandle(CollectionHandleBase root) : base(root.Root ?? root)
        {
            // We save this because RealmHandle doesn't support a parent chain like
            // NotificationToken -> List -> Realm
            _collectionHandle = root;
        }

        protected override void Unbind()
        {
            var managedCollectionHandle = _collectionHandle.DestroyNotificationToken(handle);
            GCHandle.FromIntPtr(managedCollectionHandle).Free();
        }
    }
}