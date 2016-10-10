using Realms;

namespace AssemblyToProcess
{
    // Static properties should be ignored by the weaver and not trigger any errors or warnings.
    // See #588
    public class Dog : RealmObject
    {
        public string Name { get; set; }

        private static Dog myDog;

        public static Dog MyOnlyDog
        {
            get
            {
                return myDog;

                // if (myDog != null)
                // {
                //     return myDog;
                // }
                // var dogs = Realm.GetInstance().All<Dog>();
                // if (dogs.Count() == 0)
                // {
                //     Realm.GetInstance().Write(() =>
                //     {
                //         var dog = Realm.GetInstance().CreateObject<Dog>();
                //         dog.Name = "my precious";
                //         myDog = dog;
                //     });
                // }
                // else
                // {
                //     myDog = dogs.First();
                // }
                // return myDog;
            }
        }
    }
}
