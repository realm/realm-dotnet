////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An error that indicates the user does not have permission to perform an operation
    /// upon a synced Realm. For example, a user may receive this error if they attempt to
    /// open a Realm they do not have at least read access to, or write to a Realm they only
    /// have read access to.
    /// <para />
    /// This error may also occur if a user incorrectly opens a Realm they have read-only
    /// permissions to without using the <see cref="Realm.GetInstanceAsync"/> API.
    /// A Realm that suffers a permission denied error is, by default, flagged so that its
    /// local copy will be deleted the next time the application starts.
    /// <para />
    /// The <see cref="PermissionDeniedException"/> exposes a method that
    /// can be called with a single argument: <c>true</c> to immediately delete the Realm file,
    /// or <c>false</c> to not delete the file at all (either now or upon restart). This method
    /// should only be called with <c>true</c> if and when your app disposes of every
    /// instance of the offending Realm on all threads.
    /// </summary>
    public class PermissionDeniedException : SessionException
    {
        private PermissionDeniedException()
            : base(null, ErrorCode.PermissionDenied)
        {
        }

        /// <summary>
        /// A method that can be called to manually initiate or cancel the Realm file deletion process. If the method
        /// isn't called at all, the Realm file will be deleted the next time your application is launched and the
        /// sync subsystem is initialized. Can only be called once.
        /// </summary>
        /// <param name="deleteRealm">Controls whether to initiate deletion immediately or cancel it altogether.</param>
        /// <returns><c>true</c> if actions were run successfully, <c>false</c> otherwise.</returns>
        public bool DeleteRealmUserInfo(bool deleteRealm)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }
    }
}
