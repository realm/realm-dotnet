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
        private readonly LogLevel _originalLogLevel = RealmLogger.GetLogLevel(LogCategory.Realm);
        private RealmLogger _originalLogger = null!;

        private class UserDefinedLogger : RealmLogger
        {
            private readonly Action<LogLevel, LogCategory, string> _logFunction;

            public UserDefinedLogger(Action<LogLevel, LogCategory, string> logFunction)
            {
                _logFunction = logFunction;
            }

            protected override void LogImpl(LogLevel level, LogCategory category, string message) => _logFunction(level, category, message);
        }

        [Obsolete("Using obsolete logger.")]
        private class ObsoleteUserDefinedLogger : Logger
        {
            private readonly Action<LogLevel, string> _logFunction;

            public ObsoleteUserDefinedLogger(Action<LogLevel, string> logFunction)
            {
                _logFunction = logFunction;
            }

            protected override void LogImpl(LogLevel level, string message) => _logFunction(level, message);
        }

        [SetUp]
        public void Setup()
        {
            _originalLogger = RealmLogger.Default;
        }

        [TearDown]
        public void TearDown()
        {
            RealmLogger.Default = _originalLogger;
            RealmLogger.SetLogLevel(_originalLogLevel, _originalLogCategory);
        }

        private void AssertLogMessageContains(string actual, LogLevel level, LogCategory category, string message)
        {
            Assert.That(actual, Does.Contain(level.ToString()));
            Assert.That(actual, Does.Contain(category.Name));
            Assert.That(actual, Does.Contain(message));
            Assert.That(actual, Does.Contain(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")));
        }

        [Test]
        public void Logger_CanSetDefaultLoggerToBuiltInLogger()
        {
            var messages = new List<string>();
            RealmLogger.Default = RealmLogger.Function(message => messages.Add(message));

            RealmLogger.LogDefault(LogLevel.Warn, LogCategory.Realm.SDK, "This is very dangerous!");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "This is very dangerous!");
        }

        [Test]
        public void Logger_CanSetDefaultLoggerToUserDefinedLogger()
        {
            var messages = new List<string>();
            RealmLogger.Default = new UserDefinedLogger((level, category, message) => messages.Add(RealmLogger.FormatLog(level, category, message)));

            RealmLogger.LogDefault(LogLevel.Warn, LogCategory.Realm.SDK, "A log message");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "A log message");
        }

        [Test]
        [Obsolete("Using obsolete logger class.")]
        public void ObsoleteLogger_CanSetDefaultLoggerToUserDefinedLogger()
        {
            var messages = new List<string>();
            Logger.Default = new ObsoleteUserDefinedLogger((level, message) => messages.Add(Logger.FormatLog(level, LogCategory.Realm.SDK, message)));

            Logger.LogDefault(LogLevel.Warn, "A log message");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "A log message");
        }

        [Test]
        public void Logger_SkipsDebugMessagesByDefault()
        {
            var messages = new List<string>();
            RealmLogger.Default = RealmLogger.Function(message => messages.Add(message));

            RealmLogger.LogDefault(LogLevel.Debug, "This is a debug message!");

            Assert.That(messages.Count, Is.EqualTo(0));
        }

        [Test]
        public void Logger_SetsLogLevelAtGivenCategory()
        {
            var categories = LogCategory.NameToCategory.Values;
            foreach (var category in categories)
            {
                RealmLogger.SetLogLevel(LogLevel.All, category);
                Assert.That(RealmLogger.GetLogLevel(category), Is.EqualTo(LogLevel.All));
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
                Assert.That(RealmLogger.GetLogLevel(category), Is.Not.EqualTo(LogLevel.Error));
            }

            RealmLogger.SetLogLevel(LogLevel.Error, LogCategory.Realm.Storage);
            foreach (var category in storageCategories)
            {
                Assert.That(RealmLogger.GetLogLevel(category), Is.EqualTo(LogLevel.Error));
            }
        }

        [Test]
        [Obsolete("Using LogLevel set accessor.")]
        public void Logger_WhenUsingLogLevelSetter_OverwritesCategory()
        {
            var category = LogCategory.Realm.Storage;
            RealmLogger.SetLogLevel(LogLevel.Error, category);
            Assert.That(RealmLogger.GetLogLevel(category), Is.EqualTo(LogLevel.Error));

            RealmLogger.LogLevel = LogLevel.All;
            Assert.That(RealmLogger.GetLogLevel(category), Is.EqualTo(LogLevel.All));
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
                RealmLogger.Default = RealmLogger.Function(message => messages.Add(message));
                RealmLogger.SetLogLevel(level, category);

                RealmLogger.LogDefault(level - 1, category, "This is at level - 1");
                RealmLogger.LogDefault(level, category, "This is at the same level");
                RealmLogger.LogDefault(level + 1, category, "This is at level + 1");

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
                RealmLogger.Default = RealmLogger.Function((message) => messages.Add(message));

                RealmLogger.LogDefault(LogLevel.Warn, category, "A log message");

                Assert.That(messages.Count, Is.EqualTo(1));
                AssertLogMessageContains(messages[0], LogLevel.Warn, category, "A log message");
            }
        }

        [Test]
        public void Logger_LogsSdkCategoryByDefault()
        {
            var messages = new List<string>();
            RealmLogger.Default = RealmLogger.Function((message) => messages.Add(message));

            RealmLogger.LogDefault(LogLevel.Warn, "A log message");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "A log message");
        }

        [Test]
        public void Logger_CallsCustomFunction()
        {
            var messages = new List<string>();
            RealmLogger.Default = RealmLogger.Function((level, category, message) => messages.Add(RealmLogger.FormatLog(level, category, message)));

            RealmLogger.LogDefault(LogLevel.Warn, LogCategory.Realm.SDK, "A log message");

            Assert.That(messages.Count, Is.EqualTo(1));
            AssertLogMessageContains(messages[0], LogLevel.Warn, LogCategory.Realm.SDK, "A log message");
        }

        [Test]
        [Obsolete("Using function not accepting category.")]
        public void Logger_CallsObsoleteCustomFunction()
        {
            var messages = new List<string>();
            RealmLogger.Default = RealmLogger.Function((level, message) => messages.Add(RealmLogger.FormatLog(level, LogCategory.Realm.SDK, message)));

            RealmLogger.LogDefault(LogLevel.Warn, "A log message");

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

            RealmLogger.SetLogLevel(LogLevel.All);
            RealmLogger.Default = RealmLogger.File(tempFilePath);

            var warnMessage = "This is very dangerous!";
            var debugMessage = "This is a debug message!";
            var errorMessage = "This is an error!";
            var timeString = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

            RealmLogger.LogDefault(LogLevel.Warn, warnMessage);
            RealmLogger.LogDefault(LogLevel.Debug, debugMessage);
            RealmLogger.LogDefault(LogLevel.Error, errorMessage);

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
