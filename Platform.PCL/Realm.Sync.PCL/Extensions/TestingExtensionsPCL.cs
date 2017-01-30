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

using Realms.Sync.Exceptions;

namespace Realms.Sync.Testing
{
    /// <summary>
    /// A set of extension methods to be used in unit-testing scenarios. Should not be used in production.
    /// </summary>
    public static class TestingExtensions
    {
        /// <summary>
        /// Simulates a session error.
        /// </summary>
        /// <param name="session">The session where the simulated error will occur.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="isFatal">If set to <c>true</c> the error will be marked as fatal.</param>
        /// <remarks>
        /// Use this method to test your error handling code without connecting to a Realm Object Server.
        /// Some error codes, such as <see cref="ErrorCode.OtherSessionError"/> will be ignored and will not be reported
        /// to <see cref="Session.Error"/> subscribers.
        /// </remarks>
        public static void SimulateError(this Session session, ErrorCode errorCode, string message, bool isFatal = false)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Simulates a progress update.
        /// </summary>
        /// <param name="session">Session which will report progress.</param>
        /// <param name="downloadedBytes">Downloaded bytes.</param>
        /// <param name="downloadableBytes">Downloadable bytes.</param>
        /// <param name="uploadedBytes">Uploaded bytes.</param>
        /// <param name="uploadableBytes">Uploadable bytes.</param>
        /// <remarks>
        /// Use this method to test your progress handling code without connecting to a Realm Object Server.
        /// Some throttling may occur at a native level, so it is recommended to use <c>Task.Delay()</c> between invocations.
        /// </remarks>
        public static void SimulateProgress(this Session session,
                                            ulong downloadedBytes, ulong downloadableBytes,
                                            ulong uploadedBytes, ulong uploadableBytes)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}