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
using System.Net;
using Realms.Sync.Native;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception thrown from operations interacting with a Atlas App Services app.
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code returned by the remote operation.
        /// </summary>
        /// <value>The HTTP status code of the operation that failed or <c>null</c> if the error was not an http one.</value>
        public HttpStatusCode? StatusCode { get; }

        internal AppException(AppError appError)
            : this($"{appError.ErrorCategory}: {appError.Message}", appError.LogsLink, appError.http_status_code)
        {
        }

        internal AppException(string message, string? helpLink, int httpStatusCode)
            : base(message)
        {
            HelpLink = helpLink;
            if (httpStatusCode != 0)
            {
                StatusCode = (HttpStatusCode)httpStatusCode;
            }
        }
    }
}
