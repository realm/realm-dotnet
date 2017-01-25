using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace SmokeTestWin32Console
{

    public class Employee : RealmObject
    {
        public string Name { get; set; }

        public IList<Employee> Reports { get; }

        public Employee Boss { get; set; }
    }

    public static class TestRealm
    {
        public static string SmokeTest()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            var realm = Realm.GetInstance();
            realm.Write(() =>
            {
                var theBoss = realm.Add(new Employee
                {
                    Name = "Sally",
                    Reports = {
                            new Employee {Name="Sree" },
                            new Employee {Name="Jake"}
                        }
                });
                // explicitly set relationship because not using backlinks
                foreach (var emp in theBoss.Reports)
                {
                    emp.Boss = theBoss;
                }
            });
            var jakes = from d in realm.All<Employee>() where d.Name == "Jake" select d;
            var jake = realm.All<Employee>().Single(p => p.Name == "Jake");
            return $"There are {realm.All<Employee>().Count()} employees and the boss of Jake is {jake.Boss.Name}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("About to test Realm on desktop:");
            var msg = TestRealm.SmokeTest();
            Console.WriteLine(msg);
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
