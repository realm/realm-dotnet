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

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extensions methods over the <see cref="User"/> class that expose functionality for managing synchronized Realm permissions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UserPermissionsExtensions
    {
        internal static async Task WaitForProcessingAsync<T>(this T permissionObject) where T : RealmObject, IPermissionObject
        {
            // Retain the object, otherwise it gets GC'd and the handler is never invoked
            var handle = GCHandle.Alloc(permissionObject);
            try
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
                                tcs.TrySetException(new PermissionException(permissionObject.ErrorCode.Value, permissionObject.StatusMessage));
                                break;
                        }
                    }
                });

                permissionObject.PropertyChanged += handler;

                await tcs.Task;
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
