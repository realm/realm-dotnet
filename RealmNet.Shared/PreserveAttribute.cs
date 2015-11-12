/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
namespace RealmNet
{
    // Tell the linker to preserve a class or method even if it looks like it's not invoked.
    // Since it matches by name, it works to just declare it here.
    [System.AttributeUsage(System.AttributeTargets.All)]
    public class PreserveAttribute : System.Attribute
    {
        public PreserveAttribute (string reason = null) {}
        public bool Conditional { get; set; }
    }
}

