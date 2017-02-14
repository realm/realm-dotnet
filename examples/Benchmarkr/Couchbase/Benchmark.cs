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

        static Benchmark()
        {
            global::Couchbase.Lite.Storage.ForestDB.Plugin.Register();
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

