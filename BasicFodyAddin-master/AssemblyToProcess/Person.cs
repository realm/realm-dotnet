using System;

namespace AssemblyToProcess
{
    public class Person : Realm.RealmObject
    {
        // Automatically implemented (overridden) properties
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Ignored property
        [Realm.Ignore]
        public bool IsOnline { get; set; }

        // Composite property
        [Realm.Ignore] // TODO: Make unnecessary!
        public string FullName      // Implicit Realm.Ignore because it's not an auto-propery
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
        [Realm.MapTo("Email")]
        private string Email_ { get; set; }
        
        // Exposed version of previous property
        [Realm.Ignore]
        public string Email 
        { 
            get { return Email_; } 
            set { if (!value.Contains("@")) throw new Exception(); Email_ = value; }
        }

        // Manually implemented property
        [Realm.Ignore] // TODO: Make unnecessary!
        public string Address
        {
            get { return GetValue<string>("Address"); }
            set { SetValue("Address", value); }
        }
    }
}
