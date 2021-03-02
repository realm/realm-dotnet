////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using Realms.Helpers;

namespace Realms.Logging
{
    public class Logger : ILogger
    {
        private readonly Action<LogLevel, string> _logFunction;

        public static ILogger Console { get; } = new ConsoleLogger();

        public static ILogger Null { get; } = new NullLogger();

        public Logger(Action<LogLevel, string> logFunction)
        {
            Argument.NotNull(logFunction, nameof(logFunction));
            _logFunction = logFunction;
        }

        public void Log(LogLevel level, string message)
        {
            try
            {
                _logFunction(level, message);
            }
            catch (Exception ex)
            {
                Console.Log(LogLevel.Error, $"An exception occurred while trying to log the message: '{message}' at level: {level}. Error: {ex}");
            }
        }

        private class ConsoleLogger : Logger
        {
            public ConsoleLogger() : base((level, message) => System.Console.WriteLine($"{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss.fff} {level}: {message}"))
            {
            }
        }

        private class NullLogger : Logger
        {
            public NullLogger() : base((_, __) => { })
            {
            }
        }
    }
}
