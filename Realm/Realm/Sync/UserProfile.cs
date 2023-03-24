////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

namespace Realms.Sync
{
    /// <summary>
    /// A class containing profile information about <see cref="User"/>.
    /// </summary>
    public class UserProfile
    {
        private readonly User _user;

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>A string representing the user's name or <c>null</c> if not available.</value>
        public string? Name => _user.Handle.GetProfileData(UserProfileField.Name);

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        /// <value>A string representing the user's email or <c>null</c> if not available.</value>
        public string? Email => _user.Handle.GetProfileData(UserProfileField.Email);

        /// <summary>
        /// Gets the url for the user's profile picture.
        /// </summary>
        /// <value>A string representing the user's profile picture url or <c>null</c> if not available.</value>
        public Uri? PictureUrl
        {
            get
            {
                var url = _user.Handle.GetProfileData(UserProfileField.PictureUrl);
                return url != null ? new Uri(url) : null;
            }
        }

        /// <summary>
        /// Gets the first name of the user.
        /// </summary>
        /// <value>A string representing the user's first name or <c>null</c> if not available.</value>
        public string? FirstName => _user.Handle.GetProfileData(UserProfileField.FirstName);

        /// <summary>
        /// Gets the last name of the user.
        /// </summary>
        /// <value>A string representing the user's last name or <c>null</c> if not available.</value>
        public string? LastName => _user.Handle.GetProfileData(UserProfileField.LastName);

        /// <summary>
        /// Gets the gender of the user.
        /// </summary>
        /// <value>A string representing the user's gender or <c>null</c> if not available.</value>
        public string? Gender => _user.Handle.GetProfileData(UserProfileField.Gender);

        /// <summary>
        /// Gets the birthday of the user.
        /// </summary>
        /// <value>A string representing the user's birthday or <c>null</c> if not available.</value>
        public string? Birthday => _user.Handle.GetProfileData(UserProfileField.Birthday);

        /// <summary>
        /// Gets the minimum age of the user.
        /// </summary>
        /// <value>A string representing the user's minimum age or <c>null</c> if not available.</value>
        public string? MinAge => _user.Handle.GetProfileData(UserProfileField.MinAge);

        /// <summary>
        /// Gets the maximum age of the user.
        /// </summary>
        /// <value>A string representing the user's maximum age or <c>null</c> if not available.</value>
        public string? MaxAge => _user.Handle.GetProfileData(UserProfileField.MaxAge);

        internal UserProfile(User user)
        {
            _user = user;
        }
    }
}
