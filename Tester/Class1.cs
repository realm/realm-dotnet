using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tester
{
    public class Realm
    {
        public T CreateObject<T>()
        {
            return default(T);
        }
    }

    public interface IPerson
    {
        string Name { get; set; }
    }

    public class Test
    {
        public void SimpleTest()
        {
            Console.WriteLine("Hello world");

            var realm = new Realm();
            var person = realm.CreateObject<IPerson>();

            person.Name = "John";

        }
    }
}
