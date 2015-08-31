using RealmNet;
using System;

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

        // Ignored property
        [Ignore]
        public bool IsOnline { get; set; }

        // Composite property
        [Ignore]
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
        [Ignore]
        public string Email 
        { 
            get { return Email_; } 
            set { 
                if (!value.Contains("@")) throw new Exception("Invalid email address"); 
                Email_ = value; 
            }
        }

        // Manually implemented property
        [Ignore]
        public string Address
        {
            get { return GetValue<string>("Address"); }
            set { SetValue("Address", value); }
        }

        // One-to-one relationship
        //public PhoneNumber PrimaryNumber { get; set; }

        // One-to-many relationship
        //public RealmList<PhoneNumber> PhoneNumbers { get; set; }

        public Person()
        {
            FirstName = "Jesper";
        }
    }

}
