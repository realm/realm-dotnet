﻿////////////////////////////////////////////////////////////////////////////
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
    [TestFixture, Preserve(AllMembers = true)]
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

        private void RunTestInAllModes(Action<Realm> test)
        {
            foreach (var isDynamic in new[] { true, false })
            {
                var config = new RealmConfiguration(Guid.NewGuid().ToString())
                {
                    ObjectClasses = new[] { typeof(DynamicTask), typeof(DynamicSubTask), typeof(CompletionReport), typeof(DynamicSubSubTask) },
                    IsDynamic = isDynamic
                };

                using var realm = GetRealm(config);
                test(realm);
            }
        }

        [Test]
        public void QueryAll_WhenNoObjects_ReturnsNothing()
        {
            RunTestInAllModes(realm =>
            {
                var tasks = (IQueryable<RealmObject>)realm.DynamicApi.All(nameof(DynamicTask));
                Assert.That(tasks.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void QueryAll_ByEmbedded_Fails()
        {
            RunTestInAllModes(realm =>
            {
                var ex = Assert.Throws<ArgumentException>(() => realm.DynamicApi.All(nameof(DynamicSubTask)));
                Assert.That(ex.Message, Does.Contain("The class DynamicSubTask represents an embedded object and thus cannot be queried directly."));
            });
        }

        [Test]
        public void CreateObject_WhenEmbedded_CanAssignToParent()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();
                var now = DateTimeOffset.UtcNow;
                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    parent.DynamicApi.Set(nameof(DynamicTask.Summary), "do great stuff!");

                    var report = (EmbeddedObject)(object)realm.DynamicApi.CreateEmbeddedObjectForProperty(parent, nameof(DynamicTask.CompletionReport));
                    report.DynamicApi.Set(nameof(CompletionReport.CompletionDate), now);
                    parent.DynamicApi.Get<EmbeddedObject>(nameof(DynamicTask.CompletionReport)).DynamicApi.Set(nameof(CompletionReport.Remarks), "success!");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);

                Assert.That(addedParent, Is.Not.Null);
                Assert.That(addedParent.DynamicApi.Get<string>(nameof(DynamicTask.Summary)), Is.EqualTo("do great stuff!"));

                var addedReport = addedParent.DynamicApi.Get<EmbeddedObject>(nameof(DynamicTask.CompletionReport));
                Assert.That(addedReport, Is.Not.Null);
                Assert.That(addedReport.DynamicApi.Get<DateTimeOffset>(nameof(CompletionReport.CompletionDate)), Is.EqualTo(now));
                Assert.That(addedReport.DynamicApi.Get<string>(nameof(CompletionReport.Remarks)), Is.EqualTo("success!"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();
                var dynamicNow = DateTimeOffset.UtcNow;
                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);
                    parent.Summary = "do great stuff!";

                    var report = realm.DynamicApi.CreateEmbeddedObjectForProperty(parent, nameof(DynamicTask.CompletionReport));
                    report.CompletionDate = dynamicNow;
                    parent.CompletionReport.Remarks = "success!";
                });

                dynamic addedDynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);

                Assert.That(addedDynamicParent, Is.Not.Null);
                Assert.That(addedDynamicParent.Summary, Is.EqualTo("do great stuff!"));
                Assert.That(addedDynamicParent.CompletionReport, Is.Not.Null);
                Assert.That(addedDynamicParent.CompletionReport.CompletionDate, Is.EqualTo(dynamicNow));
                Assert.That(addedDynamicParent.CompletionReport.Remarks, Is.EqualTo("success!"));
            });
        }

        [Test]
        public void ListAdd_WhenEmbedded_Throws()
        {
            RunTestInAllModes(realm =>
            {
                var ex = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var subTasks = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                        subTasks.Add(new DynamicEmbeddedObject());
                    });
                });

                Assert.That(ex.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.AddEmbeddedObjectToList)}"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicEx = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        parent.SubTasks.Add(new DynamicEmbeddedObject());
                    });
                });

                Assert.That(dynamicEx.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.AddEmbeddedObjectToList)}"));
            });
        }

        [Test]
        public void DictionaryAdd_WhenEmbedded_Throws()
        {
            RunTestInAllModes(realm =>
            {
                var ex = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());

                        var subTasksDictionary = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));

                        subTasksDictionary.Add("foo", new DynamicSubTask());
                    });
                });

                Assert.That(ex.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.AddEmbeddedObjectToDictionary)}"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicEx = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        parent.SubTasksDictionary.Add("foo", new DynamicSubTask());
                    });
                });

                Assert.That(dynamicEx.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.AddEmbeddedObjectToDictionary)}"));
            });
        }

        [Test]
        public void ListInsert_WhenEmbedded_Throws()
        {
            RunTestInAllModes(realm =>
            {
                var ex = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var subTasks = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                        subTasks.Insert(0, new DynamicEmbeddedObject());
                    });
                });

                Assert.That(ex.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.InsertEmbeddedObjectInList)}"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicEx = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        parent.SubTasks.Insert(0, new DynamicEmbeddedObject());
                    });
                });

                Assert.That(dynamicEx.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.InsertEmbeddedObjectInList)}"));
            });
        }

        [Test]
        public void ListSet_WhenEmbedded_Throws()
        {
            RunTestInAllModes(realm =>
            {
                var ex = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var subTasks = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                        subTasks[0] = new DynamicEmbeddedObject();
                    });
                });

                Assert.That(ex.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.SetEmbeddedObjectInList)}"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicEx = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        parent.SubTasks[0] = new DynamicEmbeddedObject();
                    });
                });

                Assert.That(dynamicEx.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.SetEmbeddedObjectInList)}"));
            });
        }

        [Test]
        public void DictionarySet_WhenEmbedded_Throws()
        {
            RunTestInAllModes(realm =>
            {
                var ex = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var subTasksDictionary = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                        subTasksDictionary["foo"] = new DynamicSubTask();
                    });
                });

                Assert.That(ex.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.SetEmbeddedObjectInDictionary)}"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicEx = Assert.Throws<NotSupportedException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        parent.SubTasksDictionary["foo"] = new DynamicSubTask();
                    });
                });

                Assert.That(dynamicEx.Message, Does.Contain($"{nameof(realm.DynamicApi)}.{nameof(realm.DynamicApi.SetEmbeddedObjectInDictionary)}"));
            });
        }

        [Test]
        public void RealmAddEmbeddedObjectToList()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    var subTasks = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                    var subTask = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subTasks);
                    subTask.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is subtask level 1");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var addedSubTasks = addedParent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                Assert.That(addedSubTasks, Is.Not.Null);
                Assert.That(addedSubTasks.Count, Is.EqualTo(1));
                Assert.That(addedSubTasks[0].DynamicApi.Get<string>(nameof(DynamicTask.Summary)), Is.EqualTo("This is subtask level 1"));

                realm.Write(() =>
                {
                    var secondSubTask = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(addedSubTasks);
                    secondSubTask.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is a second subtask level 1");

                    var subSubTasks = addedSubTasks[0].DynamicApi.GetList<EmbeddedObject>(nameof(DynamicSubTask.SubSubTasks));
                    var secondLevelSubTask = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subSubTasks);
                    secondLevelSubTask.DynamicApi.Set(nameof(DynamicSubSubTask.Summary), "This is subtask level 2");
                });

                addedSubTasks = addedParent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                Assert.That(addedSubTasks.Count, Is.EqualTo(2));
                Assert.That(addedSubTasks[1].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("This is a second subtask level 1"));
                Assert.That(addedSubTasks[1].DynamicApi.GetList<EmbeddedObject>(nameof(DynamicSubTask.SubSubTasks)).Count, Is.EqualTo(0));

                var addedSubSubTasks = addedSubTasks[0].DynamicApi.GetList<EmbeddedObject>(nameof(DynamicSubTask.SubSubTasks));
                Assert.That(addedSubSubTasks.Count, Is.EqualTo(1));
                Assert.That(addedSubSubTasks[0].DynamicApi.Get<string>(nameof(DynamicSubSubTask.Summary)), Is.EqualTo("This is subtask level 2"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);

                    dynamic subTask = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                    subTask.Summary = "This is subtask level 1";
                });

                dynamic dynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicParent.SubTasks, Is.Not.Null);
                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(1));
                Assert.That(dynamicParent.SubTasks[0].Summary, Is.EqualTo("This is subtask level 1"));

                realm.Write(() =>
                {
                    dynamic secondSubTask = realm.DynamicApi.AddEmbeddedObjectToList(dynamicParent.SubTasks);
                    secondSubTask.Summary = "This is a second subtask level 1";

                    dynamic secondLevelSubTask = realm.DynamicApi.AddEmbeddedObjectToList(dynamicParent.SubTasks[0].SubSubTasks);
                    secondLevelSubTask.Summary = "This is subtask level 2";
                });

                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(2));
                Assert.That(dynamicParent.SubTasks[1].Summary, Is.EqualTo("This is a second subtask level 1"));
                Assert.That(dynamicParent.SubTasks[1].SubSubTasks.Count, Is.EqualTo(0));

                Assert.That(dynamicParent.SubTasks[0].SubSubTasks.Count, Is.EqualTo(1));
                Assert.That(dynamicParent.SubTasks[0].SubSubTasks[0].Summary, Is.EqualTo("This is subtask level 2"));
            });
        }

        [Test]
        public void RealmAddEmbeddedObjectToDictionary()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                    var dict = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                    var subTask = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "foo");
                    subTask.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is subtask level 1");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var addedDict = addedParent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                Assert.That(addedDict, Is.Not.Null);
                Assert.That(addedDict.Count, Is.EqualTo(1));
                Assert.That(addedDict["foo"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("This is subtask level 1"));

                realm.Write(() =>
                {
                    var secondSubTask = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(addedDict, "bar");
                    secondSubTask.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is a second subtask level 1");
                });

                Assert.That(addedDict.Count, Is.EqualTo(2));
                Assert.That(addedDict["bar"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("This is a second subtask level 1"));

                if (TestHelpers.IsUnity || TestHelpers.IsAOTTarget)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);

                    dynamic subTask = realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "foo");
                    subTask.Summary = "This is subtask level 1";
                });

                dynamic dynamicAddedParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicAddedParent.SubTasksDictionary, Is.Not.Null);
                Assert.That(dynamicAddedParent.SubTasksDictionary.Count, Is.EqualTo(1));
                Assert.That(dynamicAddedParent.SubTasksDictionary["foo"].Summary, Is.EqualTo("This is subtask level 1"));

                realm.Write(() =>
                {
                    dynamic secondSubTask = realm.DynamicApi.AddEmbeddedObjectToDictionary(dynamicAddedParent.SubTasksDictionary, "bar");
                    secondSubTask.Summary = "This is a second subtask level 1";
                });

                Assert.That(dynamicAddedParent.SubTasksDictionary.Count, Is.EqualTo(2));
                Assert.That(dynamicAddedParent.SubTasksDictionary["bar"].Summary, Is.EqualTo("This is a second subtask level 1"));
            });
        }

        [Test]
        public void RealmInsertEmbeddedObjectInList()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                    var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                    var subTask = (EmbeddedObject)(object)realm.DynamicApi.InsertEmbeddedObjectInList(list, 0);
                    subTask.DynamicApi.Set(nameof(DynamicSubTask.Summary), "first");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var subTasks = addedParent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                Assert.That(subTasks, Is.Not.Null);
                Assert.That(subTasks.Count, Is.EqualTo(1));
                Assert.That(subTasks[0].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("first"));

                realm.Write(() =>
                {
                    var insertAt0 = (EmbeddedObject)(object)realm.DynamicApi.InsertEmbeddedObjectInList(subTasks, 0);
                    insertAt0.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is now at 0");

                    subTasks[1].DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is now at 1");

                    var insertAt2 = (EmbeddedObject)(object)realm.DynamicApi.InsertEmbeddedObjectInList(subTasks, 2);
                    insertAt2.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is now at 2");
                });

                Assert.That(subTasks.Count, Is.EqualTo(3));
                Assert.That(subTasks[0].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("This is now at 0"));
                Assert.That(subTasks[1].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("This is now at 1"));
                Assert.That(subTasks[2].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("This is now at 2"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);

                    dynamic subTask = realm.DynamicApi.InsertEmbeddedObjectInList(parent.SubTasks, 0);
                    subTask.Summary = "first";
                });

                dynamic dynamicAddedParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicAddedParent.SubTasks, Is.Not.Null);
                Assert.That(dynamicAddedParent.SubTasks.Count, Is.EqualTo(1));
                Assert.That(dynamicAddedParent.SubTasks[0].Summary, Is.EqualTo("first"));

                realm.Write(() =>
                {
                    dynamic insertAt0 = realm.DynamicApi.InsertEmbeddedObjectInList(dynamicAddedParent.SubTasks, 0);
                    insertAt0.Summary = "This is now at 0";

                    dynamicAddedParent.SubTasks[1].Summary = "This is now at 1";

                    dynamic insertAt2 = realm.DynamicApi.InsertEmbeddedObjectInList(dynamicAddedParent.SubTasks, 2);
                    insertAt2.Summary = "This is now at 2";
                });

                Assert.That(dynamicAddedParent.SubTasks.Count, Is.EqualTo(3));
                Assert.That(dynamicAddedParent.SubTasks[0].Summary, Is.EqualTo("This is now at 0"));
                Assert.That(dynamicAddedParent.SubTasks[1].Summary, Is.EqualTo("This is now at 1"));
                Assert.That(dynamicAddedParent.SubTasks[2].Summary, Is.EqualTo("This is now at 2"));
            });
        }

        [Test]
        public void RealmSetEmbeddedObjectInList()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                    var subTasks = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                    var task0 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subTasks);
                    var task1 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subTasks);
                    var task2 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subTasks);

                    task0.DynamicApi.Set(nameof(DynamicSubTask.Summary), "initial at 0");
                    task1.DynamicApi.Set(nameof(DynamicSubTask.Summary), "initial at 1");
                    task2.DynamicApi.Set(nameof(DynamicSubTask.Summary), "initial at 2");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var addedSubTasks = addedParent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                Assert.That(addedSubTasks.Count, Is.EqualTo(3));
                Assert.That(addedSubTasks[0].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at 0"));
                Assert.That(addedSubTasks[1].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at 1"));
                Assert.That(addedSubTasks[2].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at 2"));

                realm.Write(() =>
                {
                    var newAt1 = (EmbeddedObject)(object)realm.DynamicApi.SetEmbeddedObjectInList(addedSubTasks, 1);
                    newAt1.DynamicApi.Set(nameof(DynamicSubTask.Summary), "new at 1");
                });

                Assert.That(addedSubTasks.Count, Is.EqualTo(3));
                Assert.That(addedSubTasks[0].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at 0"));
                Assert.That(addedSubTasks[1].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("new at 1"));
                Assert.That(addedSubTasks[2].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at 2"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);

                    dynamic task0 = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                    dynamic task1 = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                    dynamic task2 = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);

                    task0.Summary = "initial at 0";
                    task1.Summary = "initial at 1";
                    task2.Summary = "initial at 2";
                });

                dynamic dynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(3));
                Assert.That(dynamicParent.SubTasks[0].Summary, Is.EqualTo("initial at 0"));
                Assert.That(dynamicParent.SubTasks[1].Summary, Is.EqualTo("initial at 1"));
                Assert.That(dynamicParent.SubTasks[2].Summary, Is.EqualTo("initial at 2"));

                realm.Write(() =>
                {
                    dynamic newAt1 = realm.DynamicApi.SetEmbeddedObjectInList(dynamicParent.SubTasks, 1);
                    newAt1.Summary = "new at 1";
                });

                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(3));
                Assert.That(dynamicParent.SubTasks[0].Summary, Is.EqualTo("initial at 0"));
                Assert.That(dynamicParent.SubTasks[1].Summary, Is.EqualTo("new at 1"));
                Assert.That(dynamicParent.SubTasks[2].Summary, Is.EqualTo("initial at 2"));
            });
        }

        [Test]
        public void RealmSetEmbeddedObjectInDictionary()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);

                    var dict = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                    var task0 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "a");
                    var task1 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "b");
                    var task2 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "c");

                    task0.DynamicApi.Set(nameof(DynamicSubTask.Summary), "initial at a");
                    task1.DynamicApi.Set(nameof(DynamicSubTask.Summary), "initial at b");
                    task2.DynamicApi.Set(nameof(DynamicSubTask.Summary), "initial at c");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var subTasks = addedParent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                Assert.That(subTasks.Count, Is.EqualTo(3));

                Assert.That(subTasks["a"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at a"));
                Assert.That(subTasks["b"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at b"));
                Assert.That(subTasks["c"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at c"));

                realm.Write(() =>
                {
                    var newAt1 = (EmbeddedObject)(object)realm.DynamicApi.SetEmbeddedObjectInDictionary(subTasks, "b");
                    newAt1.DynamicApi.Set(nameof(DynamicSubTask.Summary), "new at b");
                });

                Assert.That(subTasks.Count, Is.EqualTo(3));
                Assert.That(subTasks["a"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at a"));
                Assert.That(subTasks["b"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("new at b"));
                Assert.That(subTasks["c"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("initial at c"));

                if (TestHelpers.IsUnity || TestHelpers.IsAOTTarget)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);

                    dynamic task0 = realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "a");
                    dynamic task1 = realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "b");
                    dynamic task2 = realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "c");

                    task0.Summary = "initial at a";
                    task1.Summary = "initial at b";
                    task2.Summary = "initial at c";
                });

                dynamic dynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicParent.SubTasksDictionary.Count, Is.EqualTo(3));

                Assert.That(dynamicParent.SubTasksDictionary["a"].Summary, Is.EqualTo("initial at a"));
                Assert.That(dynamicParent.SubTasksDictionary["b"].Summary, Is.EqualTo("initial at b"));
                Assert.That(dynamicParent.SubTasksDictionary["c"].Summary, Is.EqualTo("initial at c"));

                realm.Write(() =>
                {
                    dynamic newAt1 = realm.DynamicApi.SetEmbeddedObjectInDictionary(dynamicParent.SubTasksDictionary, "b");
                    newAt1.Summary = "new at b";
                });

                Assert.That(dynamicParent.SubTasksDictionary.Count, Is.EqualTo(3));
                Assert.That(dynamicParent.SubTasksDictionary["a"].Summary, Is.EqualTo("initial at a"));
                Assert.That(dynamicParent.SubTasksDictionary["b"].Summary, Is.EqualTo("new at b"));
                Assert.That(dynamicParent.SubTasksDictionary["c"].Summary, Is.EqualTo("initial at c"));
            });
        }

        [Test]
        public void RealmSetEmbeddedObjectInList_WhenOutOfBounds_Throws()
        {
            RunTestInAllModes(realm =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                        realm.DynamicApi.SetEmbeddedObjectInList(list, 0);
                    });
                });

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                        realm.DynamicApi.SetEmbeddedObjectInList(list, -1);
                    });
                });

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        realm.DynamicApi.SetEmbeddedObjectInList(parent.SubTasks, 0);
                    });
                });

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        realm.DynamicApi.SetEmbeddedObjectInList(parent.SubTasks, -1);
                    });
                });
            });
        }

        [Test]
        public void RealmAddEmbeddedObjectInDictionary_WhenDuplicateKeys_Throws()
        {
            RunTestInAllModes(realm =>
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var dict = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                        realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "foo");
                        realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "foo");
                    });
                }, $"An item with the key 'foo' has already been added.");

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "foo");
                        realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "foo");
                    });
                }, $"An item with the key 'foo' has already been added.");
            });
        }

        [Test]
        public void RealmInsertEmbeddedObjectInList_WhenOutOfBounds_Throws()
        {
            RunTestInAllModes(realm =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                        // Insert at list.Count works like Add
                        realm.DynamicApi.InsertEmbeddedObjectInList(list, 1);
                    });
                });

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                        realm.DynamicApi.SetEmbeddedObjectInList(list, -1);
                    });
                });

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());

                        // Insert at list.Count works like Add
                        realm.DynamicApi.InsertEmbeddedObjectInList(parent.SubTasks, 1);
                    });
                });

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    realm.Write(() =>
                    {
                        dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), Guid.NewGuid().ToString());
                        realm.DynamicApi.SetEmbeddedObjectInList(parent.SubTasks, -1);
                    });
                });
            });
        }

        [Test]
        public void List_RemoveAt()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));

                    var first = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(list);
                    var second = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(list);

                    first.DynamicApi.Set(nameof(DynamicSubTask.Summary), "first");
                    second.DynamicApi.Set(nameof(DynamicSubTask.Summary), "second");
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var list = addedParent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                Assert.That(list.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    list.RemoveAt(0);
                });

                Assert.That(list.Count, Is.EqualTo(1));
                Assert.That(list[0].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("second"));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);
                    dynamic first = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                    dynamic second = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);

                    first.Summary = "first";
                    second.Summary = "second";
                });

                dynamic dynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    dynamicParent.SubTasks.RemoveAt(0);
                });

                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(1));
                Assert.That(dynamicParent.SubTasks[0].Summary, Is.EqualTo("second"));
            });
        }

        [Test]
        public void List_Remove()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                var second = realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    var list = parent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                    var first = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(list);
                    var second = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(list);

                    first.DynamicApi.Set(nameof(DynamicSubTask.Summary), "first");
                    second.DynamicApi.Set(nameof(DynamicSubTask.Summary), "second");

                    return second;
                });

                var addedParent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var subTasks = addedParent.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                Assert.That(subTasks.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    subTasks.Remove(second);
                });

                Assert.That(subTasks.Count, Is.EqualTo(1));
                Assert.That(subTasks[0].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("first"));

                realm.Write(() =>
                {
                    subTasks.Remove(subTasks[0]);
                });

                Assert.That(subTasks.Count, Is.EqualTo(0));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                dynamic dynamicSecond = realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);
                    dynamic first = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);
                    dynamic second = realm.DynamicApi.AddEmbeddedObjectToList(parent.SubTasks);

                    first.Summary = "first";
                    second.Summary = "second";

                    return second;
                });

                dynamic dynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    dynamicParent.SubTasks.Remove(dynamicSecond);
                });

                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(1));
                Assert.That(dynamicParent.SubTasks[0].Summary, Is.EqualTo("first"));

                realm.Write(() =>
                {
                    dynamicParent.SubTasks.Remove(dynamicParent.SubTasks[0]);
                });

                Assert.That(dynamicParent.SubTasks.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void Dictionary_Remove()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();

                var second = realm.Write(() =>
                {
                    var parent = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    var dict = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                    var first = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "first");
                    var second = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToDictionary(dict, "second");

                    first.DynamicApi.Set(nameof(DynamicSubTask.Summary), "first");
                    second.DynamicApi.Set(nameof(DynamicSubTask.Summary), "second");

                    return second;
                });

                var parent = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var dict = parent.DynamicApi.GetDictionary<EmbeddedObject>(nameof(DynamicTask.SubTasksDictionary));
                Assert.That(dict.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    dict.Remove("second");
                });

                Assert.That(dict.Count, Is.EqualTo(1));
                Assert.That(dict["first"].DynamicApi.Get<string>(nameof(DynamicSubTask.Summary)), Is.EqualTo("first"));

                realm.Write(() =>
                {
                    dict.Remove("first");
                });

                Assert.That(dict.Count, Is.EqualTo(0));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();

                dynamic dynamicSecond = realm.Write(() =>
                {
                    dynamic parent = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);
                    dynamic first = realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "first");
                    dynamic second = realm.DynamicApi.AddEmbeddedObjectToDictionary(parent.SubTasksDictionary, "second");

                    first.Summary = "first";
                    second.Summary = "second";

                    return second;
                });

                dynamic dynamicParent = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                Assert.That(dynamicParent.SubTasksDictionary.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    dynamicParent.SubTasksDictionary.Remove("second");
                });

                Assert.That(dynamicParent.SubTasksDictionary.Count, Is.EqualTo(1));
                Assert.That(dynamicParent.SubTasksDictionary["first"].Summary, Is.EqualTo("first"));

                realm.Write(() =>
                {
                    dynamicParent.SubTasksDictionary.Remove("first");
                });

                Assert.That(dynamicParent.SubTasksDictionary.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void Embedded_Backlinks()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();
                realm.Write(() =>
                {
                    var task = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    task.DynamicApi.Set(nameof(DynamicTask.Summary), "This is the task");

                    var subTasks = task.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks));
                    var subTask = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subTasks);
                    subTask.DynamicApi.Set(nameof(DynamicSubTask.Summary), "This is level 1 subtask");

                    var subSubTasks = task.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubSubTasks));
                    var subSubTask1 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subSubTasks);

                    var subTaskSubSubTasks = subTask.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicSubTask.SubSubTasks));
                    var subSubtask2 = (EmbeddedObject)(object)realm.DynamicApi.AddEmbeddedObjectToList(subTaskSubSubTasks);
                });

                var addedTask = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var addedSubTask = addedTask.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubTasks))[0];
                var addedSubSubTask1 = addedTask.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicTask.SubSubTasks))[0];
                var addedSubSubTask2 = addedSubTask.DynamicApi.GetList<EmbeddedObject>(nameof(DynamicSubTask.SubSubTasks))[0];

                var parentTask1 = addedSubSubTask1.DynamicApi.GetBacklinks(nameof(DynamicSubSubTask.ParentTask));
                var parentSubTask1 = addedSubSubTask1.DynamicApi.GetBacklinks(nameof(DynamicSubSubTask.ParentSubTask));
                Assert.That(parentTask1.Count(), Is.EqualTo(1));
                Assert.That(parentSubTask1.Count(), Is.EqualTo(0));
                Assert.That(parentTask1.Single(), Is.EqualTo(addedTask));

                var parentTask2 = addedSubSubTask2.DynamicApi.GetBacklinks(nameof(DynamicSubSubTask.ParentTask));
                var parentSubTask2 = addedSubSubTask2.DynamicApi.GetBacklinks(nameof(DynamicSubSubTask.ParentSubTask));
                Assert.That(parentTask2.Count(), Is.EqualTo(0));
                Assert.That(parentSubTask2.Count(), Is.EqualTo(1));
                Assert.That(parentSubTask2.Single(), Is.EqualTo(addedSubTask));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();
                realm.Write(() =>
                {
                    dynamic task = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);
                    task.Summary = "This is the task";

                    dynamic subTask = realm.DynamicApi.AddEmbeddedObjectToList(task.SubTasks);
                    subTask.Summary = "This is level 1 subtask";

                    dynamic subSubTask1 = realm.DynamicApi.AddEmbeddedObjectToList(task.SubSubTasks);
                    dynamic subSubtask2 = realm.DynamicApi.AddEmbeddedObjectToList(subTask.SubSubTasks);
                });

                dynamic dynamicTask = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                dynamic dynamicSubTask = dynamicTask.SubTasks[0];
                dynamic dynamicSubSubTask1 = dynamicTask.SubSubTasks[0];
                dynamic dynamicSubSubTask2 = dynamicSubTask.SubSubTasks[0];

                Assert.That(dynamicSubSubTask1.ParentTask.Count, Is.EqualTo(1));
                Assert.That(dynamicSubSubTask1.ParentSubTask.Count, Is.EqualTo(0));
                Assert.That(((IQueryable<RealmObject>)dynamicSubSubTask1.ParentTask).Single(), Is.EqualTo(dynamicTask));

                Assert.That(dynamicSubSubTask2.ParentTask.Count, Is.EqualTo(0));
                Assert.That(dynamicSubSubTask2.ParentSubTask.Count, Is.EqualTo(1));
                Assert.That(((IQueryable<EmbeddedObject>)dynamicSubSubTask2.ParentSubTask).Single(), Is.EqualTo(dynamicSubTask));
            });
        }

        [Test]
        public void Embedded_DynamicBacklinks()
        {
            RunTestInAllModes(realm =>
            {
                var id = Guid.NewGuid().ToString();
                realm.Write(() =>
                {
                    var task = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicTask), id);
                    task.DynamicApi.Set(nameof(DynamicTask.Summary), "This is the task");

                    var report = (EmbeddedObject)(object)realm.DynamicApi.CreateEmbeddedObjectForProperty(task, nameof(DynamicTask.CompletionReport));
                    report.DynamicApi.Set(nameof(CompletionReport.Remarks), "ok");
                });

                var addedTask = (RealmObject)(object)realm.DynamicApi.Find(nameof(DynamicTask), id);
                var addedReport = addedTask.DynamicApi.Get<EmbeddedObject>(nameof(DynamicTask.CompletionReport));

                var reportParents = addedReport.DynamicApi.GetBacklinksFromType(nameof(DynamicTask), nameof(CompletionReport));
                Assert.That(reportParents.Count(), Is.EqualTo(1));
                Assert.That(reportParents.Single(), Is.EqualTo(addedTask));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicId = Guid.NewGuid().ToString();
                realm.Write(() =>
                {
                    dynamic task = realm.DynamicApi.CreateObject(nameof(DynamicTask), dynamicId);
                    task.Summary = "This is the task";

                    dynamic report = realm.DynamicApi.CreateEmbeddedObjectForProperty(task, nameof(DynamicTask.CompletionReport));
                    report.Remarks = "ok";
                });

                dynamic dynamicTask = realm.DynamicApi.Find(nameof(DynamicTask), dynamicId);
                dynamic dynamicReport = dynamicTask.CompletionReport;

                IQueryable<dynamic> dynamicParents = dynamicReport.GetBacklinks(nameof(DynamicTask), nameof(CompletionReport));
                Assert.That(dynamicParents.Count(), Is.EqualTo(1));
                Assert.That(dynamicParents.Single(), Is.EqualTo(dynamicTask));
            });
        }
    }
}
