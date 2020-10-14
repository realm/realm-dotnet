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
    internal static class NotificationsHelper
    {
        /// <summary>
        /// INotifiable represents a reactive object (e.g. RealmObjectBase/Collection).
        /// </summary>
        internal interface INotifiable
        {
            /// <summary>
            /// Method called when there are changes to report for that object.
            /// </summary>
            /// <param name="changes">The changes that occurred.</param>
            /// <param name="exception">An exception if one occurred.</param>
            void NotifyCallbacks(NotifiableObjectHandleBase.CollectionChangeSet? changes, NativeException? exception);
        }

        internal static readonly NotifiableObjectHandleBase.NotificationCallbackDelegate NotificationCallback = NotificationCallbackImpl;

        [MonoPInvokeCallback(typeof(NotifiableObjectHandleBase.NotificationCallbackDelegate))]
        private static void NotificationCallbackImpl(IntPtr managedHandle, IntPtr changes, IntPtr exception)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is INotifiable notifiable)
            {
                notifiable.NotifyCallbacks(new PtrTo<NotifiableObjectHandleBase.CollectionChangeSet>(changes).Value, new PtrTo<NativeException>(exception).Value);
            }
        }
    }
}