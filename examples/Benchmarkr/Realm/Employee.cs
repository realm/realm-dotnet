using System;
using Realms;

namespace Benchmarkr.Realm
{
    public class Employee : RealmObject, IEmployee
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public bool IsHired { get; set; }      
    }
}

