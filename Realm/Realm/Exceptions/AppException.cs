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
using Realms.Native;

namespace Realms.Exceptions
{
    /// <summary>
    /// An exception thrown from operations interacting with a MongoDB Realm app.
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// Gets the error code, associated with the error.
        /// </summary>
        public int ErrorCode { get; }

        internal AppException(AppError appError)
            : this($"{appError.ErrorCategory}: {appError.Message}", appError.error_code)
        {
        }

        internal AppException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        internal enum AppErrorCodes
        {
            ApiKeyNotFound = 35,
        }
    }
}
