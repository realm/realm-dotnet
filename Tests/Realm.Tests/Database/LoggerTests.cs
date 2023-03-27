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
        private Logger _originalLogger = null!;
        private LogLevel _originalLogLevel;

        [SetUp]
        public void Setup()
        {
            _originalLogger = Logger.Default;
            _originalLogLevel = Logger.LogLevel;
        }

        [TearDown]
        public void TearDown()
        {
            Logger.Default = _originalLogger;
            Logger.LogLevel = _originalLogLevel;
        }

        [Test]
        public void Logger_CanSetDefaultLogger()
        {
            var messages = new List<string>();
            Logger.Default = Logger.Function(message => messages.Add(message));

            Logger.LogDefault(LogLevel.Warn, "This is very dangerous!");

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0], Does.Contain(LogLevel.Warn.ToString()));
            Assert.That(messages[0], Does.Contain(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")));
            Assert.That(messages[0], Does.Contain("This is very dangerous!"));
        }

        [Test]
        public void Logger_SkipsDebugMessagesByDefault()
        {
            var messages = new List<string>();
            Logger.Default = Logger.Function(message => messages.Add(message));

            Logger.LogDefault(LogLevel.Debug, "This is a debug message!");

            Assert.That(messages.Count, Is.EqualTo(0));
        }

        [TestCase(LogLevel.Error)]
        [TestCase(LogLevel.Info)]
        [TestCase(LogLevel.Debug)]
        public void Logger_WhenLevelIsSet_LogsOnlyExpectedLevels(LogLevel level)
        {
            var messages = new List<string>();
            Logger.Default = Logger.Function(message => messages.Add(message));
            Logger.LogLevel = level;

            Logger.LogDefault(level - 1, "This is at level - 1");
            Logger.LogDefault(level, "This is at the same level");
            Logger.LogDefault(level + 1, "This is at level + 1");

            Assert.That(messages.Count, Is.EqualTo(2));

            Assert.That(messages[0], Does.Contain(level.ToString()));
            Assert.That(messages[0], Does.Contain("This is at the same level"));

            Assert.That(messages[1], Does.Contain((level + 1).ToString()));
            Assert.That(messages[1], Does.Contain("This is at level + 1"));
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
