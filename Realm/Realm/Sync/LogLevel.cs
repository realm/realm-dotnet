////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
    /// Specifies the criticality level above which messages will be logged
    /// by the default sync client logger.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Log everything. This will seriously harm the performance of the
        /// sync client and should never be used in production scenarios.
        /// </summary>
        All,

        /// <summary>
        /// A version of 'debug' that allows for very high volume output.
        /// This may seriously affect the performance of the sync client.
        /// </summary>
        Trace,

        /// <summary>
        /// Reveal information that can aid debugging, no longer paying
        /// attention to efficiency.
        /// </summary>
        Debug,

        /// <summary>
        /// Same as 'Info', but prioritize completeness over minimalism.
        /// </summary>
        Detail,

        /// <summary>
        /// Log operational sync client messages, but in a minimalistic fashion to
        /// avoid general overhead from logging and to keep volume down.
        /// </summary>
        Info,

        /// <summary>
        /// Log errors and warnings.
        /// </summary>
        Warn,

        /// <summary>
        /// Log errors only.
        /// </summary>
        Error,

        /// <summary>
        /// Log only fatal errors.
        /// </summary>
        Fatal,

        /// <summary>
        /// Log nothing.
        /// </summary>
        Off
    }
}
