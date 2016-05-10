using System;
using System.IO;
using System.Linq;
using Couchbase.Lite;
using System.Collections.Generic;

namespace Benchmarkr.Couchbase
{
    public class Benchmark : BenchmarkBase
    {
        public override string Name
        {
            get
            {
                return "Couchbase";
            }
        }

        private Database db;
        public override IDisposable OpenDB()
        {
            var manager = new Manager(new DirectoryInfo(this.Path), ManagerOptions.Default);
            manager.StorageType = "ForestDB";
            return this.db = manager.GetDatabase("cbdb");
        }

        public override void DeleteDB()
        {
            try
            {
                Directory.Delete(this.Path, true);
            }
            catch (Exception)
            {
                
            }
        }

        public override void RunInTransaction(Action action)
        {
            this.db.RunInTransaction(() =>
                {
                    try
                    {
                        action();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
        }

        public override void InsertObject(uint index)
        {
            this.db.CreateDocument()
                .PutProperties(new Dictionary<string, object>
            {
                ["name"] = BenchmarkBase.NameValue(index),
                ["age"] = BenchmarkBase.AgeValue(index),
                ["is_hired"] = BenchmarkBase.IsHiredValue(index)
            });
        }

        public override int Count(EmployeeQuery query)
        {
            using (var enumerator = this.ConvertQuery(query).Run())
            {
                return enumerator.Count;
            }
        }

        public override long Enumerate(EmployeeQuery query)
        {
            using (var enumerator = this.ConvertQuery(query).Run())
            {
                long ages = 0;
                foreach (var row in enumerator)
                {
                    ages += row.Document.GetProperty<long>("age");
                }
                return ages;
            }
        }

        private Query ConvertQuery(EmployeeQuery query)
        {
            var q = this.db.CreateAllDocumentsQuery();
            q.PostFilter = row =>
            {
                    var name = row.Document.GetProperty<string>("name");
                    var age = row.Document.GetProperty<long>("age");
                    var isHired = row.Document.GetProperty<bool>("is_hired");

                    return name.Contains(query.Name) && age >= query.MinAge && age <= query.MaxAge && isHired == query.IsHired;
            };

            return q;
        }
    }
}

