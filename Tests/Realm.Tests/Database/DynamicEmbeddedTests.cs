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
using Realms.Dynamic;

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

            public IList<DynamicSubSubTask> SubSubTasks { get; }

            public IDictionary<string, DynamicSubTask> SubTasksDictionary { get; }
        }

        private class DynamicSubTask : EmbeddedObject
        {
            public string Summary { get; set; }

            public CompletionReport CompletionReport { get; set; }

            public IList<DynamicSubSubTask> SubSubTasks { get; }
        }

        private class DynamicSubSubTask : EmbeddedObject
        {
            public string Summary { get; set; }

            // Singular because we only expect 1
            [Backlink(nameof(DynamicSubTask.SubSubTasks))]
            public IQueryable<DynamicSubTask> ParentSubTask { get; }

            [Backlink(nameof(DynamicTask.SubSubTasks))]
            public IQueryable<DynamicTask> ParentTask { get; }
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
                ObjectClasses = new[] { typeof(DynamicTask), typeof(DynamicSubTask), typeof(CompletionReport), typeof(DynamicSubSubTask) },
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
            var id = Guid.NewGuid().ToString();
            var now = DateTimeOffset.UtcNow;
            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                parent.Summary = "do great stuff!";

                var report = _realm.DynamicApi.CreateEmbeddedObjectForProperty(parent, nameof(DynamicTask.CompletionReport));
                report.CompletionDate = now;
                parent.CompletionReport.Remarks = "success!";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);

            Assert.That(addedParent, Is.Not.Null);
            Assert.That(addedParent.Summary, Is.EqualTo("do great stuff!"));
            Assert.That(addedParent.CompletionReport, Is.Not.Null);
            Assert.That(addedParent.CompletionReport.CompletionDate, Is.EqualTo(now));
            Assert.That(addedParent.CompletionReport.Remarks, Is.EqualTo("success!"));
        }

        [Test]
        public void ListAdd_WhenEmbedded_Throws()
        {
            var ex = Assert.Throws<NotSupportedException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    parent.SubTasks.Add(new DynamicEmbeddedObject());
                });
            });

            Assert.That(ex.Message, Does.Contain($"{nameof(_realm.DynamicApi)}.{nameof(_realm.DynamicApi.AddEmbeddedObjectToList)}"));
        }

        [Test]
        public void DictionaryAdd_WhenEmbedded_Throws()
        {
            var ex = Assert.Throws<NotSupportedException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    parent.SubTasksDictionary.Add("foo", new DynamicSubTask());
                });
            });

            Assert.That(ex.Message, Does.Contain($"{nameof(_realm.DynamicApi)}.{nameof(_realm.DynamicApi.AddEmbeddedObjectToDictionary)}"));
        }

        [Test]
        public void ListInsert_WhenEmbedded_Throws()
        {
            var ex = Assert.Throws<NotSupportedException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    parent.SubTasks.Insert(0, new DynamicEmbeddedObject());
                });
            });

            Assert.That(ex.Message, Does.Contain($"{nameof(_realm.DynamicApi)}.{nameof(_realm.DynamicApi.InsertEmbeddedObjectInList)}"));
        }

        [Test]
        public void ListSet_WhenEmbedded_Throws()
        {
            var ex = Assert.Throws<NotSupportedException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    parent.SubTasks[0] = new DynamicEmbeddedObject();
                });
            });

            Assert.That(ex.Message, Does.Contain($"{nameof(_realm.DynamicApi)}.{nameof(_realm.DynamicApi.SetEmbeddedObjectInList)}"));
        }

        [Test]
        public void DictionarySet_WhenEmbedded_Throws()
        {
            var ex = Assert.Throws<NotSupportedException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    parent.SubTasksDictionary["foo"] = new DynamicSubTask();
                });
            });

            Assert.That(ex.Message, Does.Contain($"{nameof(_realm.DynamicApi)}.{nameof(_realm.DynamicApi.SetEmbeddedObjectInDictionary)}"));
        }

        [Test]
        public void RealmAddEmbeddedObjectToList()
        {
            var id = Guid.NewGuid().ToString();

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                var subTask = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                subTask.Summary = "This is subtask level 1";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasks, Is.Not.Null);
            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("This is subtask level 1"));

            _realm.Write(() =>
            {
                var secondSubTask = _realm.DynamicApi.AddEmbeddedObjectToList(addedParent.SubTasks);
                secondSubTask.Summary = "This is a second subtask level 1";

                var secondLevelSubTask = _realm.DynamicApi.AddEmbeddedObjectToList(addedParent.SubTasks[0].SubSubTasks);
                secondLevelSubTask.Summary = "This is subtask level 2";
            });

            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(2));
            Assert.That(addedParent.SubTasks[1].Summary, Is.EqualTo("This is a second subtask level 1"));
            Assert.That(addedParent.SubTasks[1].SubSubTasks.Count, Is.EqualTo(0));

            Assert.That(addedParent.SubTasks[0].SubSubTasks.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasks[0].SubSubTasks[0].Summary, Is.EqualTo("This is subtask level 2"));
        }

        [Test]
        public void RealmAddEmbeddedObjectToDictionary()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            var id = Guid.NewGuid().ToString();

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                var subTask = _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "foo");
                subTask.Summary = "This is subtask level 1";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasksDictionary, Is.Not.Null);
            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasksDictionary["foo"].Summary, Is.EqualTo("This is subtask level 1"));

            _realm.Write(() =>
            {
                var secondSubTask = _realm.DynamicApi.AddEmbeddedObjectToDictionary(addedParent.SubTasksDictionary, "bar");
                secondSubTask.Summary = "This is a second subtask level 1";
            });

            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(2));
            Assert.That(addedParent.SubTasksDictionary["bar"].Summary, Is.EqualTo("This is a second subtask level 1"));
        }

        [Test]
        public void RealmInsertEmbeddedObjectInList()
        {
            var id = Guid.NewGuid().ToString();

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                var subTask = _realm.DynamicApi.InsertEmbeddedObjectInList(parent.SubTasks, 0);
                subTask.Summary = "first";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasks, Is.Not.Null);
            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("first"));

            _realm.Write(() =>
            {
                var insertAt0 = _realm.DynamicApi.InsertEmbeddedObjectInList(addedParent.SubTasks, 0);
                insertAt0.Summary = "This is now at 0";

                addedParent.SubTasks[1].Summary = "This is now at 1";

                var insertAt2 = _realm.DynamicApi.InsertEmbeddedObjectInList(addedParent.SubTasks, 2);
                insertAt2.Summary = "This is now at 2";
            });

            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(3));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("This is now at 0"));
            Assert.That(addedParent.SubTasks[1].Summary, Is.EqualTo("This is now at 1"));
            Assert.That(addedParent.SubTasks[2].Summary, Is.EqualTo("This is now at 2"));
        }

        [Test]
        public void RealmSetEmbeddedObjectInList()
        {
            var id = Guid.NewGuid().ToString();

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                var task0 = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                var task1 = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                var task2 = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);

                task0.Summary = "initial at 0";
                task1.Summary = "initial at 1";
                task2.Summary = "initial at 2";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(3));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("initial at 0"));
            Assert.That(addedParent.SubTasks[1].Summary, Is.EqualTo("initial at 1"));
            Assert.That(addedParent.SubTasks[2].Summary, Is.EqualTo("initial at 2"));

            _realm.Write(() =>
            {
                var newAt1 = _realm.DynamicApi.SetEmbeddedObjectInList(addedParent.SubTasks, 1);
                newAt1.Summary = "new at 1";
            });

            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(3));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("initial at 0"));
            Assert.That(addedParent.SubTasks[1].Summary, Is.EqualTo("new at 1"));
            Assert.That(addedParent.SubTasks[2].Summary, Is.EqualTo("initial at 2"));
        }

        [Test]
        public void RealmSetEmbeddedObjectInDictionary()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            var id = Guid.NewGuid().ToString();

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                var task0 = _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "a");
                var task1 = _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "b");
                var task2 = _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "c");

                task0.Summary = "initial at a";
                task1.Summary = "initial at b";
                task2.Summary = "initial at c";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(3));

            Assert.That(addedParent.SubTasksDictionary["a"].Summary, Is.EqualTo("initial at a"));
            Assert.That(addedParent.SubTasksDictionary["b"].Summary, Is.EqualTo("initial at b"));
            Assert.That(addedParent.SubTasksDictionary["c"].Summary, Is.EqualTo("initial at c"));

            _realm.Write(() =>
            {
                var newAt1 = _realm.DynamicApi.SetEmbeddedObjectInDictionary(addedParent.SubTasksDictionary, "b");
                newAt1.Summary = "new at b";
            });

            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(3));
            Assert.That(addedParent.SubTasksDictionary["a"].Summary, Is.EqualTo("initial at a"));
            Assert.That(addedParent.SubTasksDictionary["b"].Summary, Is.EqualTo("new at b"));
            Assert.That(addedParent.SubTasksDictionary["c"].Summary, Is.EqualTo("initial at c"));
        }

        [Test]
        public void RealmSetEmbeddedObjectInList_WhenOutOfBounds_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    _realm.DynamicApi.SetEmbeddedObjectInList(parent.SubTasks, 0);
                });
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    _realm.DynamicApi.SetEmbeddedObjectInList(parent.SubTasks, -1);
                });
            });
        }

        [Test]
        public void RealmAddEmbeddedObjectInDictionary_WhenDuplicateKeys_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "foo");
                    _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "foo");
                });
            }, $"An item with the key 'foo' has already been added.");
        }

        [Test]
        public void RealmInsertEmbeddedObjectInList_WhenOutOfBounds_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());

                    // Insert at list.Count works like Add
                    _realm.DynamicApi.InsertEmbeddedObjectInList(parent.SubTasks, 1);
                });
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _realm.Write(() =>
                {
                    var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                    _realm.DynamicApi.SetEmbeddedObjectInList(parent.SubTasks, -1);
                });
            });
        }

        [Test]
        public void List_RemoveAt()
        {
            var id = Guid.NewGuid().ToString();

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                var first = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                var second = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);

                first.Summary = "first";
                second.Summary = "second";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                addedParent.SubTasks.RemoveAt(0);
            });

            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("second"));
        }

        [Test]
        public void List_Remove()
        {
            var id = Guid.NewGuid().ToString();

            dynamic second = null;

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                var first = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                second = _realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);

                first.Summary = "first";
                second.Summary = "second";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                addedParent.SubTasks.Remove(second);
            });

            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasks[0].Summary, Is.EqualTo("first"));

            _realm.Write(() =>
            {
                addedParent.SubTasks.Remove(addedParent.SubTasks[0]);
            });

            Assert.That(addedParent.SubTasks.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dictionary_Remove()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            var id = Guid.NewGuid().ToString();

            dynamic second = null;

            _realm.Write(() =>
            {
                var parent = _realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                var first = _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "first");
                second = _realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "second");

                first.Summary = "first";
                second.Summary = "second";
            });

            dynamic addedParent = _realm.DynamicApi.Find(nameof(DynamicTask), id);
            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                addedParent.SubTasksDictionary.Remove("second");
            });

            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(1));
            Assert.That(addedParent.SubTasksDictionary["first"].Summary, Is.EqualTo("first"));

            _realm.Write(() =>
            {
                addedParent.SubTasksDictionary.Remove("first");
            });

            Assert.That(addedParent.SubTasksDictionary.Count, Is.EqualTo(0));
        }

        [Test]
        public void Embedded_Backlinks()
        {
            _realm.Write(() =>
            {
                var task = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                task.Summary = "This is the task";

                var subTask = _realm.DynamicApi.AddEmbeddedObjectToList(task.SubTasks);
                subTask.Summary = "This is level 1 subtask";

                var subSubTask1 = _realm.DynamicApi.AddEmbeddedObjectToList(task.SubSubTasks);
                var subSubtask2 = _realm.DynamicApi.AddEmbeddedObjectToList(subTask.SubSubTasks);
            });

            var addedTask = _realm.DynamicApi.All(nameof(DynamicTask)).Single();
            var addedSubTask = addedTask.SubTasks[0];
            var addedSubSubTask1 = addedTask.SubSubTasks[0];
            var addedSubSubTask2 = addedSubTask.SubSubTasks[0];

            Assert.That(addedSubSubTask1.ParentTask.Count, Is.EqualTo(1));
            Assert.That(addedSubSubTask1.ParentSubTask.Count, Is.EqualTo(0));
            Assert.That(((IQueryable<RealmObject>)addedSubSubTask1.ParentTask).Single(), Is.EqualTo(addedTask));

            Assert.That(addedSubSubTask2.ParentTask.Count, Is.EqualTo(0));
            Assert.That(addedSubSubTask2.ParentSubTask.Count, Is.EqualTo(1));
            Assert.That(((IQueryable<EmbeddedObject>)addedSubSubTask2.ParentSubTask).Single(), Is.EqualTo(addedSubTask));
        }

        [Test]
        public void Embedded_DynamicBacklinks()
        {
            _realm.Write(() =>
            {
                var task = _realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                task.Summary = "This is the task";

                var report = _realm.DynamicApi.CreateEmbeddedObjectForProperty(task, nameof(DynamicTask.CompletionReport));
                report.Remarks = "ok";
            });

            var addedTask = _realm.DynamicApi.All(nameof(DynamicTask)).Single();
            var addedReport = addedTask.CompletionReport;

            var reportParents = addedReport.GetBacklinks(nameof(DynamicTask), nameof(CompletionReport));
            Assert.That(reportParents.Count, Is.EqualTo(1));
            Assert.That(((IQueryable<RealmObject>)reportParents).Single(), Is.EqualTo(addedTask));
        }
    }
}
