////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture(DynamicTestObjectType.RealmObject)]
    [TestFixture(DynamicTestObjectType.DynamicRealmObject)]
    [Preserve(AllMembers = true)]
    public class DynamicEmbeddedTests : RealmInstanceTest
    {
        private class DynamicTask : RealmObject
        {
            [PrimaryKey]
            public string Id { get; set; }

            public string Summary { get; set; }

            public CompletionReport CompletionReport { get; set; }

            public IList<DynamicSubTask> SubTasks { get; }
        }

        private class DynamicSubTask : EmbeddedObject
        {
            public string Summary { get; set; }

            public IList<DynamicSubTask> SubTasks { get; }

            public CompletionReport CompletionReport { get; set; }

            // Singular because we only expect 1
            [Backlink(nameof(DynamicTask.SubTasks))]
            public IQueryable<DynamicTask> ParentTask { get; }

            // Singular because we only expect 1
            [Backlink(nameof(SubTasks))]
            public IQueryable<DynamicSubTask> ParentSubTask { get; }
        }

        private class CompletionReport : EmbeddedObject
        {
            public DateTimeOffset CompletionDate { get; set; }

            public string Remarks { get; set; }
        }

        private readonly DynamicTestObjectType _mode;

        public DynamicEmbeddedTests(DynamicTestObjectType mode)
        {
            _mode = mode;
        }

        protected override RealmConfiguration CreateConfiguration(string path)
        {
            return new RealmConfiguration(path)
            {
                ObjectClasses = new[] { typeof(DynamicTask), typeof(DynamicSubTask), typeof(CompletionReport) },
                IsDynamic = _mode == DynamicTestObjectType.DynamicRealmObject
            };
        }

        [Test]
        public void QueryAll_WhenNoObjects_ReturnsNothing()
        {
            var tasks = _realm.DynamicApi.All(nameof(DynamicTask));
            Assert.That(tasks.Count(), Is.EqualTo(0));
        }

        [Test]
        public void QueryAll_ByEmbedded_Fails()
        {
            var ex = Assert.Throws<ArgumentException>(() => _realm.DynamicApi.All(nameof(DynamicSubTask)));
            Assert.That(ex.Message, Does.Contain("The class DynamicSubTask represents an embedded object and thus cannot be queried directly."));
        }

        [Test]
        public void CreateObject_WhenEmbedded_CanAssignToParent()
        {
            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                parent.Summary = "do great stuff!";

                var report = _realm.DynamicApi.CreateEmbeddedObjectForProperty(parent, nameof(DynamicTask.CompletionReport));
                report.CompletionDate = DateTimeOffset.UtcNow;
                parent.CompletionReport.Remarks = "success!";
            });
        }
    }
}
