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

namespace Realms.Sync
{
    /// <summary>
    /// The desired behavior of a progress notification subscription.
    /// </summary>
    public enum ProgressMode
    {
        /// <summary>
        /// The callback will be called forever, or until it is unregistered by disposing the subscription token.
        /// Notifications will always report the latest number of transferred bytes, and the most up-to-date number of 
        /// total transferable bytes.
        /// </summary>
        ReportIndefinitely,

        /// <summary>
        /// The callback will, upon registration, store the total number of bytes to be transferred. When invoked, it will 
        /// always report the most up-to-date number of transferable bytes out of that original number of transferable bytes.
        /// When the number of transferred bytes reaches or exceeds the number of transferable bytes, the callback will 
        /// be unregistered.
        /// </summary>
        ForCurrentlyOutstandingWork
    }
}
