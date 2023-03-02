////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception class that indicates that one more object changes have been reverted
    /// by the server.
    /// </summary>
    /// <remarks>
    /// The two typical cases in which the server will revert a client write are:
    /// 1. The client created an object that doesn't match any <see cref="Realm.Subscriptions"/>.
    /// 2. The client created/updated an object it didn't have permissions to.
    /// </remarks>
    public class CompensatingWriteException : SessionException
    {
        /// <summary>
        /// Gets a list of the compensating writes performed by the server.
        /// </summary>
        /// <value>The compensating writes performed by the server.</value>
        public IEnumerable<CompensatingWriteInfo> CompensatingWrites { get; }

        internal CompensatingWriteException(string message, IEnumerable<CompensatingWriteInfo> compensatingWrites)
            : base(message, ErrorCode.CompensatingWrite)
        {
            CompensatingWrites = compensatingWrites;
        }
    }

    /// <summary>
    /// A class containing the details for a compensating write performed by the server.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is closely related to CompensatingWriteException")]
    public class CompensatingWriteInfo
    {
        /// <summary>
        /// Gets the type of the object which was affected by the compensating write.
        /// </summary>
        /// <value>The object type.</value>
        public string ObjectType { get; }

        /// <summary>
        /// Gets the reason for the server to perform a compensating write.
        /// </summary>
        /// <value>The compensating write reason.</value>
        public string Reason { get; }

        /// <summary>
        /// Gets the primary key of the object which was affected by the compensating write.
        /// </summary>
        /// <value>The object primary key.</value>
        public RealmValue PrimaryKey { get; }

        internal CompensatingWriteInfo(string objectName, string reason, RealmValue primaryKey)
        {
            ObjectType = objectName;
            Reason = reason;
            PrimaryKey = primaryKey;
        }
    }
}
