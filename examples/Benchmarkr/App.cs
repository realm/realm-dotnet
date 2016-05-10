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
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Xamarin.Forms;

namespace Benchmarkr
{
    public class App : Application
    {
        private Label theLabel;
        private Button startBench;

        public App()
        {
            theLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = "Benchmark"
            };
            startBench = new Button { Text = "Start", HorizontalOptions=LayoutOptions.Center };
            startBench.Clicked += OnStartClicked;

            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        theLabel, 
                        startBench
                    }
                }
            };
        }

        //protected override async void OnStart()
        protected async void OnStartClicked(object sender, EventArgs e)
        {
            theLabel.Text = "Running...";
            startBench.IsEnabled = false;

            var benchmarks = new BenchmarkBase[] { new Realm.Benchmark(), new SQLite.Benchmark(), new Couchbase.Benchmark() };

            theLabel.Text = "APIs: ";
            foreach (var benchmark in benchmarks)
            {
                benchmark.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{benchmark.Name}.db");
                benchmark.DeleteDB();
                theLabel.Text += $"{benchmark.Name} ";
            }

            var tests = new Dictionary<string, Action<BenchmarkBase>>
            {
                ["Insert Single Transactions"] = Insert_Single_Transactions,
//                ["Insert Multiple Transactions"] = benchmark => {
//                    using (benchmark.OpenDB())
//                    {
//                        for (uint i = 0; i < 1500; i++)
//                        {
//                            benchmark.RunInTransaction(() => benchmark.InsertObject(i));
//                        }
//                    }
//                },
                ["Query Count"] = Query_Count,
                ["Query Enumerate"] = Query_Enumerate
            };

            foreach (var test in tests)
            {
                System.GC.Collect(0, GCCollectionMode.Forced, true);
                theLabel.Text += $"\n{test.Key}: ";
                foreach (var benchmark in benchmarks)
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(0.1));
                    using (benchmark.OpenDB())
                    {
                        var averageDuration = Enumerable.Range(0, 3).Select(_ => benchmark.PerformTest(test.Value)).Min(t => t.TotalMilliseconds);
                        theLabel.Text += $"{(long)averageDuration} ";
                    }
                }
            }

            startBench.IsEnabled = true;
        }

        private static void Insert_Single_Transactions(BenchmarkBase benchmark)
        {
            benchmark.RunInTransaction(() => 
            {
                for (uint i = 0; i < 150000; i++)
                {
                    benchmark.InsertObject(i);
                }
            });
        }

        private static readonly EmployeeQuery Query = new EmployeeQuery
        {
            Name = "0",
            MinAge = 20,
            MaxAge = 50,
            IsHired = true
        };

        private static void Query_Count(BenchmarkBase benchmark)
        {
            Console.WriteLine($"Counting employees: {benchmark.Count(Query)}");
        }

        private static void Query_Enumerate(BenchmarkBase benchmark)
        {
            Console.WriteLine($"Total ages: {benchmark.Enumerate(Query)}");
        }
    }
}

