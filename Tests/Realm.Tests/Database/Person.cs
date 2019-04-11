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
using System.Collections.Generic;

namespace Realms.Tests.Database
{
    public class Person : RealmObject
    {
        // Automatically implemented (overridden) properties
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public float Score { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public long Salary { get; set; }

        public bool? IsAmbivalent { get; set; }

        public DateTimeOffset Birthday { get; set; }

        public byte[] PublicCertificateBytes { get; set; }

        public string OptionalAddress { get; set; }

        // Property that's not persisted in Realm
        [Ignored]
        public bool IsOnline { get; set; }

        // Composite property
        [Ignored]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }

            set
            {
                var parts = value.Split(' ');
                FirstName = parts[0];
                LastName = parts[parts.Length - 1];
            }
        }

        // Re-mapped property
        [MapTo("Email")]
        private string Email_ { get; set; }

        // Wrapped version of previous property
        [Ignored]
        public string Email
        {
            get
            {
                return Email_;
            }

            set
            {
                if (!value.Contains("@"))
                {
                    throw new Exception("Invalid email address");
                }

                Email_ = value;
            }
        }

        public bool IsInteresting { get; set; }

        private string _nickname;

        public string Nickname
        {
            get
            {
                return _nickname;
            }

            set
            {
                _nickname = value;
            }
        }

        public IList<Person> Friends { get; }
    }
}
