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
    /// Objects of this class are used to offer permissions to owned Realms.
    /// They are created exclusively by the client and are processed by the server
    /// as indicated by the status fields.
    /// </summary>
    /// <remarks>
    /// When offering permissions, you should create the offer and add it to the <see cref="User"/>'s Management Realm.
    /// Then you should subscribe to <see cref="RealmObject.PropertyChanged"/> to be notified when the server has 
    /// processed the request.
    /// Once the request has been processed, the <see cref="Status"/>, <see cref="StatusMessage"/>, and
    /// <see cref="ErrorCode"/> will be updated accordingly.
    /// If the request has been processed successfully, the <see cref="Token"/> will be populated and you can share it
    /// with users you wish to grant permissions to.
    /// If the request has failed, the <see cref="StatusMessage"/> will be updated with relevant information about the
    /// failure and <see cref="ErrorCode"/> will be set to a non-null value.
    /// </remarks>
    [Explicit]
    public class PermissionOffer : RealmObject, IPermissionObject
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

        /// <inheritdoc />
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
        /// Gets the token that can be used to offer the permissions defined in this object to another user.
        /// </summary>
        /// <value>A string, set by the server, that can be used to create a <see cref="PermissionOfferResponse"/>.</value>
        [MapTo("token")]
        [Indexed]
        public string Token { get; private set; }

        /// <summary>
        /// Gets the url of the Realm to offer permissions to.
        /// </summary>
        [MapTo("realmUrl")]
        [Required]
        public string RealmUrl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to read from the Realm.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to read data from the <see cref="Realm"/>.</value>
        [MapTo("mayRead")]
        public bool MayRead { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to write to the Realm.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to write data to the <see cref="Realm"/>.</value>
        [MapTo("mayWrite")]
        public bool MayWrite { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to manage access rights for others.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to offer others access to the <see cref="Realm"/>.</value>
        [MapTo("mayManage")]
        public bool MayManage { get; private set; }

        /// <summary>
        /// Gets or sets the expiration date and time of the offer.
        /// </summary>
        /// <value>If <c>null</c>, the offer will never expire. Otherwise, the offer may not be consumed past the expiration date.</value>
        [MapTo("expiresAt")]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionOffer"/> class.
        /// </summary>
        /// <param name="realmUrl">The Realm URL to offer permissions to.</param>
        /// <param name="mayRead">If set to <c>true</c> grants read access.</param>
        /// <param name="mayWrite">If set to <c>true</c> grants write access.</param>
        /// <param name="mayManage">If set to <c>true</c> grants manage access.</param>
        /// <param name="expiresAt">Optional expiration date of the offer. If set to <c>null</c>, the offer doesn't expire.</param>
        public PermissionOffer(string realmUrl, bool mayRead = true, bool mayWrite = false, bool mayManage = false, DateTimeOffset? expiresAt = null)
        {
            RealmUrl = realmUrl;
            MayRead = mayRead;
            MayWrite = mayWrite;
            MayManage = mayManage;
            ExpiresAt = expiresAt;
        }

        private PermissionOffer()
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
