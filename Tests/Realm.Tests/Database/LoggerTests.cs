////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.IO;
using NUnit.Framework;
using Realms.Logging;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class LoggerTests
    {
        private readonly LogCategory _originalLogCategory = LogCategory.Realm;
        private readonly LogLevel _originalLogLevel = Logger.GetLogLevel(LogCategory.Realm);
        private Logger _originalLogger = null!;

        [SetUp]
        public void Setup()
        {
            _originalLogger = Logger.Default;
        }

        [TearDown]
        public void TearDown()
        {
            Logger.Default = _originalLogger;
            Logger.SetLogLevel(_originalLogLevel, _originalLogCategory);
        }

        private void AssertLogMessageContains(string actual, LogLevel level, LogCategory category, string message)
        {
            Assert.That(actual, Does.Contain(level.ToString()));
            Assert.That(actual, Does.Contain(category.Name));
            Assert.That(actual, Does.Contain(message));
            Assert.That(actual, Does.Contain(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")));
        }

        [Test]
        public void Logger_CanSetDefaultLogger()
        {
            var messages = new List<string>();
            Logger.Default = Logger.Function(message => messages.Add(message));

            Logger.LogDefault(LogLevel.Warn, LogCategory.Realm.SDK, "This is very dangerous!");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "This is very dangerous!");
        }

        [Test]
        public void Logger_SkipsDebugMessagesByDefault()
        {
            var messages = new List<string>();
            Logger.Default = Logger.Function(message => messages.Add(message));

            Logger.LogDefault(LogLevel.Debug, "This is a debug message!");

            Assert.That(messages.Count, Is.EqualTo(0));
        }

        [Test]
        public void Logger_SetsLogLevelAtGivenCategory()
        {
            var categories = LogCategory.NameToCategory.Values;
            foreach (var category in categories)
            {
                Logger.SetLogLevel(LogLevel.All, category);
                Assert.That(Logger.GetLogLevel(category), Is.EqualTo(LogLevel.All));
            }
        }

        [Test]
        public void Logger_SetsLogLevelAtSubcategories()
        {
            var storageCategories = new[]
            {
                LogCategory.Realm.Storage.Transaction,
                LogCategory.Realm.Storage.Query,
                LogCategory.Realm.Storage.Object,
                LogCategory.Realm.Storage.Notification
            };
            foreach (var category in storageCategories)
            {
                Assert.That(Logger.GetLogLevel(category), Is.Not.EqualTo(LogLevel.Error));
            }

            Logger.SetLogLevel(LogLevel.Error, LogCategory.Realm.Storage);
            foreach (var category in storageCategories)
            {
                Assert.That(Logger.GetLogLevel(category), Is.EqualTo(LogLevel.Error));
            }
        }

        [Test]
        public void Logger_WhenUsingLogLevelSetter_OverwritesCategory()
        {
            var category = LogCategory.Realm.Storage;
            Logger.SetLogLevel(LogLevel.Error, category);
            Assert.That(Logger.GetLogLevel(category), Is.EqualTo(LogLevel.Error));

            Logger.LogLevel = LogLevel.All;
            Assert.That(Logger.GetLogLevel(category), Is.EqualTo(LogLevel.All));
        }

        [TestCase(LogLevel.Error)]
        [TestCase(LogLevel.Info)]
        [TestCase(LogLevel.Debug)]
        public void Logger_WhenLevelIsSet_LogsOnlyExpectedLevels(LogLevel level)
        {
            var categories = LogCategory.NameToCategory.Values;
            foreach (var category in categories)
            {
                var messages = new List<string>();
                Logger.Default = Logger.Function(message => messages.Add(message));
                Logger.SetLogLevel(level, category);

                Logger.LogDefault(level - 1, category, "This is at level - 1");
                Logger.LogDefault(level, category, "This is at the same level");
                Logger.LogDefault(level + 1, category, "This is at level + 1");

                Assert.That(messages.Count, Is.EqualTo(2));
                AssertLogMessageContains(messages[0], level, category, "This is at the same level");
                AssertLogMessageContains(messages[1], level + 1, category, "This is at level + 1");
            }
        }

        [Test]
        public void Logger_LogsAtGivenCategory()
        {
            var categories = LogCategory.NameToCategory.Values;
            foreach (var category in categories)
            {
                var messages = new List<string>();
                Logger.Default = Logger.Function((message) => messages.Add(message));

                Logger.LogDefault(LogLevel.Warn, category, "A log message");

                Assert.That(messages.Count, Is.EqualTo(1));
                AssertLogMessageContains(messages[0], LogLevel.Warn, category, "A log message");
            }
        }

        [Test]
        public void Logger_CallsCustomFunction()
        {
            var messages = new List<string>();
            Logger.Default = Logger.Function((level, category, message) => messages.Add(Logger.FormatLog(level, category, message)));

            Logger.LogDefault(LogLevel.Warn, LogCategory.Realm.SDK, "A log message");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "A log message");
        }

        [Test]
        public void Logger_MatchesCoreCategoryNames()
        {
            var coreCategoryNames = SharedRealmHandle.GetLogCategoryNames();
            var sdkCategoriesMap = LogCategory.NameToCategory;

            Assert.That(sdkCategoriesMap.Count, Is.EqualTo(coreCategoryNames.Length));
            foreach (var name in coreCategoryNames)
            {
                Assert.That(sdkCategoriesMap.TryGetValue(name!, out var category), Is.True);
                Assert.That(category!.Name, Is.EqualTo(name));
                Assert.That(LogCategory.FromName(name!), Is.SameAs(category));
            }
        }

        [Test]
        public void Logger_WhenNonExistentCategoryName_FromNameThrows()
        {
            var nonExistentNames = new[] { "realm", "Realm.app", string.Empty };
            foreach (var name in nonExistentNames)
            {
                Assert.That(() => LogCategory.FromName(name), Throws.TypeOf<ArgumentException>().And.Message.Contains($"Unexpected category name: '{name}'"));
            }
        }

        [Test]
        public void FileLogger()
        {
            var tempFilePath = Path.GetTempFileName();

            Logger.LogLevel = LogLevel.All;
            Logger.Default = Logger.File(tempFilePath);

            var warnMessage = "This is very dangerous!";
            var debugMessage = "This is a debug message!";
            var errorMessage = "This is an error!";
            var timeString = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

            Logger.LogDefault(LogLevel.Warn, warnMessage);
            Logger.LogDefault(LogLevel.Debug, debugMessage);
            Logger.LogDefault(LogLevel.Error, errorMessage);

            var loggedStrings = File.ReadAllLines(tempFilePath);

            Assert.That(loggedStrings.Length, Is.EqualTo(3));

            Assert.That(loggedStrings[0], Does.Contain(LogLevel.Warn.ToString()));
            Assert.That(loggedStrings[0], Does.Contain(timeString));
            Assert.That(loggedStrings[0], Does.Contain(warnMessage));

            Assert.That(loggedStrings[1], Does.Contain(LogLevel.Debug.ToString()));
            Assert.That(loggedStrings[1], Does.Contain(timeString));
            Assert.That(loggedStrings[1], Does.Contain(debugMessage));

            Assert.That(loggedStrings[2], Does.Contain(LogLevel.Error.ToString()));
            Assert.That(loggedStrings[2], Does.Contain(timeString));
            Assert.That(loggedStrings[2], Does.Contain(errorMessage));
        }
    }
}
