using System;
using System.Linq;
using Realms;

using RealmType = Realms.Realm;

namespace Benchmarkr.Realm
{
    public class Benchmark : BenchmarkBase
    {
        public override string Name
        {
            get
            {
                return "Realm";
            }
        }

        private RealmType realm;
        public override IDisposable OpenDB()
        {
            return this.realm = RealmType.GetInstance(RealmConfiguration.DefaultConfiguration.ConfigWithPath(this.Path));
        }

        public override void DeleteDB()
        {
            RealmType.DeleteRealm(RealmConfiguration.DefaultConfiguration.ConfigWithPath(this.Path));
        }

        public override void RunInTransaction(Action action)
        {
            using (var transaction = this.realm.BeginWrite())
            {
                try
                {
                    action();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        public override void InsertObject(uint index)
        {
            var employee = this.realm.CreateObject<Employee>();
            employee.Name = BenchmarkBase.NameValue(index);
            employee.Age = Benchmark.AgeValue(index);
            employee.IsHired = Benchmark.IsHiredValue(index);
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
                var name = employee.Name;
                var isHired = employee.IsHired;
                ages += employee.Age;
            }
            return ages;
        }

        private IQueryable<Employee> ConvertQuery(EmployeeQuery query)
        {
            var name = query.Name;
            var minAge = query.MinAge;
            var maxAge = query.MaxAge;
            var isHired = query.IsHired;

            return from employee in this.realm.All<Employee>()
                   where employee.Name.Contains("0") // String methods in Realm queries still support only literals
                       && employee.Age >= minAge
                       && employee.Age <= maxAge
                       && employee.IsHired == isHired
                   select employee;
        }
    }
}

