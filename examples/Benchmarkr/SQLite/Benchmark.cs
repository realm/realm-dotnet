////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

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

