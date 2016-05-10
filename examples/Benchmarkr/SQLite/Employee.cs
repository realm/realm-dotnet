using System;
using SQLite;

namespace Benchmarkr.SQLite
{
    public class Employee : IEmployee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public bool IsHired { get; set; }  
    }
}

