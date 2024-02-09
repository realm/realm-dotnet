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
        /// </summary>
        ReportIndefinitely,

        /// <summary>
        /// The callback will be active until the current transferable bytes are transferred. When invoked, it will always report the
        /// progress estimate as a percentage related to the initial transferable bytes. When those bytes are transfered, the callback will
        /// be unregistered.
        /// </summary>
        ForCurrentlyOutstandingWork
    }
}
