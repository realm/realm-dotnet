
namespace AssemblyToProcess
{
    public class Person : Realm.RealmObject
    {
        private string FirstName { get; set; }
        private string LastName { get; set; }

        [Realm.Ignore] // TODO: Make unnecessary!
        public string FullName      // Implicit Realm.Ignore because it's not an auto-propery
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

        [Realm.Ignore]
        public bool IsOnline { get; set; }

        [Realm.Ignore]
        public string Address
        {
            get { return GetValue<string>("Address"); }
            set { SetValue("Address", value); }
        }
    }
}
