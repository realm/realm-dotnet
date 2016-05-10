using System;
using System.Linq;
using SQLite;
using Realms;

namespace Benchmarkr.SQLite
{
    public class Benchmark : BenchmarkBase
    {
        public override string Name
        {
            get
            {
                return "SQLite";
            }
        }

        private SQLiteConnection connection;
        public override IDisposable OpenDB()
        {
            this.connection = new SQLiteConnection(this.Path);
            this.connection.CreateTable<Employee>();
            return this.connection;
        }

        public override void DeleteDB()
        {
            System.IO.File.Delete(this.Path);
        }

        public override void RunInTransaction(Action action)
        {
            this.connection.BeginTransaction();
            try
            {
                action();
                this.connection.Commit();
            }
            catch
            {
                this.connection.Rollback();
            }
        }

        public override void InsertObject(uint index)
        {
            var employee = new Employee
            { 
                Name = BenchmarkBase.NameValue(index),
                Age = BenchmarkBase.AgeValue(index),
                IsHired = BenchmarkBase.IsHiredValue(index)
            };
            this.connection.Insert(employee);
        }

        public override int Count(EmployeeQuery query)
        {
            return this.ConvertQuery(query).Count();
        }

        public override long Enumerate(EmployeeQuery query)
        {
            long ages = 0;
            foreach (var employee in this.ConvertQuery(query))
            {
                ages += employee.Age;
            }
            return ages;
        }

        private TableQuery<Employee> ConvertQuery(EmployeeQuery query)
        {
            return from employee in this.connection.Table<Employee>()
                   where employee.Name.Contains(query.Name)
                       && employee.Age >= query.MinAge
                       && employee.Age <= query.MaxAge
                       && employee.IsHired == query.IsHired
                   select employee;
        }
    }
}

