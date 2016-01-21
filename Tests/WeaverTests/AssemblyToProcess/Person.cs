/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using Realms;
using System;
using System.Collections.Generic;

namespace AssemblyToProcess
{
    public class PhoneNumber : RealmObject
    {
        public string Kind { get; set; }
        public string Number { get; set; }
    }

    public class Person : RealmObject
    {
        // Automatically implemented (overridden) properties
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public float Score { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTimeOffset Birthday { get; set; }

        public int Age { get; set; }

        public bool IsInteresting { get; set; }

        // Ignored property
        [Ignored]
        public bool IsOnline { get; set; }

        // Composite property
        public string FullName
        {
            get { return FirstName + " " + LastName; }

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
            get { return Email_; } 
            set { 
                if (!value.Contains("@")) throw new Exception("Invalid email address"); 
                Email_ = value; 
            }
        }

        // Manually implemented property
        public string Address
        {
            get { return GetStringValue("Address"); }
            set { SetStringValue("Address", value); }
        }

        // One-to-one relationship
        public PhoneNumber PrimaryNumber { get; set; }

        // One-to-many relationship
        //public RealmList<PhoneNumber> PhoneNumbers { get; set; }

        public Person()
        {
        }
    }

}
