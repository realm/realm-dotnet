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
using System.ComponentModel;
using System.Threading.Tasks;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extensions methods over the <see cref="User"/> class that expose functionality for managing synchronized Realm permissions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UserPermissionsExtensions
    {
        /// <summary>
        /// Returns an instance of the Management Realm owned by the user.
        /// </summary>
        /// <remarks>
        /// This Realm can be used to control access and permissions for Realms owned by the user. This includes
        /// giving other users access to Realms.
        /// </remarks>
        /// <seealso href="https://realm.io/docs/realm-object-server/#modifying-permissions">How to control permissions</seealso>
        /// <param name="user">The user whose Management Realm to get.</param>
        /// <returns>A Realm that can be used to control access and permissions for Realms owned by the user.</returns>
        [Obsolete]
        public static Realm GetManagementRealm(this User user)
        {
            return user.ManagementRealm;
        }

        /// <summary>
        /// Returns an instance of the Permission Realm owned by the user.
        /// </summary>
        /// <remarks>
        /// This Realm can be used to review access permissions for Realms managed by the user
        /// and to Realms which the user was granted access to by other users.
        /// </remarks>
        /// <param name="user">The user whose Permission Realm to get.</param>
        /// <returns>A Realm that can be used to inspect access to other Realms.</returns>
        [Obsolete]
        public static Realm GetPermissionRealm(this User user)
        {
            return user.PermissionRealm;
        }

        public static Task WaitForProcessing<T>(this T permissionObject) where T : RealmObject, IPermissionObject
        {
            var tcs = new TaskCompletionSource<object>();
            PropertyChangedEventHandler handler = null;
            handler = new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == nameof(PermissionChange.Status) &&
                    permissionObject.Status != ManagementObjectStatus.NotProcessed)
                {
                    permissionObject.PropertyChanged -= handler;
                    switch (permissionObject.Status)
                    {
                        case ManagementObjectStatus.Success:
                            tcs.TrySetResult(permissionObject);
                            break;
                        case ManagementObjectStatus.Error:
                            tcs.TrySetException(new Exception(permissionObject.StatusMessage));
                            break;
                    }
                }
            });

            permissionObject.PropertyChanged += handler;

            return tcs.Task;
        }
    }
}
