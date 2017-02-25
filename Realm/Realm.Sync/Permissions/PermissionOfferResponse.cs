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
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// Objects of this class are used to accept a <see cref="PermissionOffer"/> using a provided <see cref="Token"/>.
    /// </summary>
    /// <remarks>
    /// Create an instance of <see cref="PermissionOfferResponse"/> using the provided <see cref="PermissionOffer.Token"/>
    /// and add it to the <see cref="User"/>'s ManagementRealm.
    /// After that, you should subscribe to <see cref="RealmObject.PropertyChanged"/> to be notified when the server
    /// processes the response.
    /// Once the request has been processed, the <see cref="Status"/>, <see cref="StatusMessage"/>, and
    /// <see cref="ErrorCode"/> will be updated accordingly.
    /// If the request has been processed successfully, the <see cref="RealmUrl"/> will be populated and you can use it
    /// to create a new <see cref="SyncConfiguration"/>.
    /// If the request has failed, the <see cref="StatusMessage"/> will be updated with relevant information about the
    /// failure and <see cref="ErrorCode"/> will be set to a non-null value.
    /// </remarks>
    [Explicit]
    public class PermissionOfferResponse : RealmObject, IPermissionObject
    {
        /// <inheritdoc />
        [PrimaryKey, Required]
        [MapTo("id")]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        [MapTo("createdAt")]
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <inheritdoc />
        [MapTo("updatedAt")]
        public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

        [MapTo("statusCode")]
        private int? StatusCode { get; set; }

        /// <inheritdoc />
        [MapTo("statusMessage")]
        public string StatusMessage { get; private set; }

        /// <inheritdoc />
        public ManagementObjectStatus Status
        {
            get
            {
                switch (StatusCode)
                {
                    case null:
                        return ManagementObjectStatus.NotProcessed;
                    case 0:
                        return ManagementObjectStatus.Success;
                    default:
                        return ManagementObjectStatus.Error;
                }
            }
        }

        /// <inheritdoc />
        public ErrorCode? ErrorCode => ErrorCodeHelper.GetErrorCode(StatusCode);

        /// <summary>
        /// Gets the token that was provided by the offering user.
        /// </summary>
        [Required]
        [MapTo("token")]
        public string Token { get; private set; }

        /// <summary>
        /// Gets the url of the Realm that the token has granted permissions to.
        /// </summary>
        /// <remarks>
        /// Filled by the server after the <see cref="PermissionOfferResponse"/> was processed.
        /// </remarks>
        [MapTo("realmUrl")]
        public string RealmUrl { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionOfferResponse"/> class.
        /// </summary>
        /// <param name="token">The token that was provided by the offering user.</param>
        public PermissionOfferResponse(string token)
        {
            Token = token;
        }

        private PermissionOfferResponse()
        {
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(StatusCode))
            {
                RaisePropertyChanged(nameof(Status));
            }
        }
    }
}