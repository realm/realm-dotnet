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
using Realms.Exceptions.Sync;
using Realms.Helpers;
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
        /// Use this method to test your error handling code without connecting to Atlas Device Sync.
        /// Some error codes, such as <see cref="ErrorCode.OtherSessionError"/> will be ignored and will not be reported
        /// to <see cref="Session.Error"/> subscribers.
        /// </remarks>
        public static void SimulateError(this Session session, ErrorCode errorCode, string message, bool isFatal = false)
        {
            Argument.NotNull(session, nameof(session));
            Argument.NotNull(message, nameof(message));

            session.ReportErrorForTesting((int)errorCode, SessionErrorCategory.SessionError, message, isFatal, ServerRequestsAction.ApplicationBug);
        }

        /// <summary>
        /// Simulates a client reset.
        /// </summary>
        /// <param name="session">The session where the simulated client reset will occur.</param>
        /// <param name="message">Error message.</param>
        /// <remarks>
        /// You still need to set up a Realm integration testing environment, namely a testing app to connect to.
        /// </remarks>
        [Obsolete("This method will not work with the automatic client reset handlers.")]
        public static void SimulateClientReset(this Session session, string message)
        {
            Argument.NotNull(session, nameof(session));
            Argument.NotNull(message, nameof(message));

            session.ReportErrorForTesting((int)ErrorCode.DivergingHistories, SessionErrorCategory.SessionError, message, false, ServerRequestsAction.ClientReset);
        }

        /// <summary>
        /// Simulates an error occurring during automatic handling of client reset.
        /// </summary>
        /// <param name="session">The session where the simulated client reset will occur.</param>
        /// <param name="message">Error message.</param>
        [Obsolete("This method will not work with the automatic client reset handlers.")]
        public static void SimulateAutomaticClientResetFailure(this Session session, string message)
        {
            Argument.NotNull(session, nameof(session));
            Argument.NotNull(message, nameof(message));

            session.ReportErrorForTesting((int)ClientError.AutoClientResetFailed, SessionErrorCategory.ClientError, message, false, ServerRequestsAction.NoAction);
        }
    }
}
