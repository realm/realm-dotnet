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

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception thrown when a transport error occurs during authentication. 
    /// </summary>
    public class HttpException : Exception
    {
        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> of the response.
        /// </summary>
        /// <value>A well known <see cref="HttpStatusCode"/>.</value>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the Reason-Phrase of the HTTP response.
        /// </summary>
        /// <value>The Reason-Phrase of the HTTP response.</value>
        public string ReasonPhrase { get; }

        /// <summary>
        /// Gets the body of the HTTP response.
        /// </summary>
        /// <value>The body of the HTTP response.</value>
        public string Payload { get; }

        internal HttpException(HttpStatusCode code, string reasonPhrase, string payload) :
            this(code, reasonPhrase, payload, "An HTTP exception has occurred.")
        {
        }

        internal HttpException(HttpStatusCode statusCode, string reasonPhrase, string payload, string message) : base(message)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Payload = payload;
        }
    }
}
