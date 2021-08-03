﻿////////////////////////////////////////////////////////////////////////////
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Realms.Sync;

namespace Realms.Logging
{
    /// <summary>
    /// A logger that logs messages originating from Realm. The default logger can be replaced by setting <see cref="Default"/>.
    /// </summary>
    /// <remarks>
    /// A few default implementations are provided by <see cref="Console"/>, <see cref="Null"/>, and <see cref="Function(Action{string})"/>, but you
    /// can implement your own.
    /// </remarks>
    public abstract class Logger
    {
        private readonly Lazy<GCHandle> _gcHandle;

        private static Logger _defaultLogger;

        /// <summary>
        /// Gets a <see cref="ConsoleLogger"/> that outputs messages to the default console. For most project types, that will be
        /// using <see cref="Console.WriteLine()"/> but certain platforms may use different implementations.
        /// </summary>
        /// <value>A <see cref="Logger"/> instance that outputs to the platform's console.</value>
        public static Logger Console { get; internal set; } = new ConsoleLogger();

        /// <summary>
        /// Gets a <see cref="FileLogger"/> that saves the log messages to a file.
        /// </summary>
        /// <param name="filePath">Path of the file to save messages to. The file is created if it does not already exists.</param>
        /// <param name="encoding">Character encoding to use. Defaults to <see cref="System.Text.Encoding.UTF8"/> if not specified.</param>
        /// <remarks>
        /// Please note that this logger is not optimized for performance, and could lead to overall sync performance slowdown with more verbose log levels.
        /// </remarks>
        /// <returns>
        /// A <see cref="Logger"/> instance that will save log messages to a file.
        /// </returns>
        public static Logger File(string filePath, Encoding encoding = null) => new FileLogger(filePath, encoding);

        /// <summary>
        /// Gets a <see cref="NullLogger"/> that ignores all messages.
        /// </summary>
        /// <value>A <see cref="Logger"/> that doesn't output any messages.</value>
        public static Logger Null { get; } = new NullLogger();

        /// <summary>
        /// Gets a <see cref="FunctionLogger"/> that proxies Log calls to the supplied function.
        /// </summary>
        /// <param name="logFunction">Function to proxy log calls to.</param>
        /// <returns>
        /// A <see cref="Logger"/> instance that will invoke <paramref name="logFunction"/> for each message.
        /// </returns>
        public static Logger Function(Action<LogLevel, string> logFunction) => new FunctionLogger(logFunction);

        /// <summary>
        /// Gets a <see cref="FunctionLogger"/> that proxies Log calls to the supplied function. The message will
        /// already be formatted with the default message formatting that includes a timestamp.
        /// </summary>
        /// <param name="logFunction">Function to proxy log calls to.</param>
        /// <returns>
        /// A <see cref="Logger"/> instance that will invoke <paramref name="logFunction"/> for each message.
        /// </returns>
        public static Logger Function(Action<string> logFunction) => new FunctionLogger((level, message) => logFunction(FormatLog(level, message)));

        /// <summary>
        /// Gets or sets the verbosity of log messages.
        /// </summary>
        /// <remarks>
        /// This replaces the deprecated <see cref="AppConfiguration.LogLevel"/>.
        /// </remarks>
        /// <value>The log level for Realm-originating messages.</value>
        public static LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets a custom <see cref="Logger"/> implementation that will be used by
        /// Realm whenever information must be logged.
        /// </summary>
        /// <remarks>
        /// This is the logger that will be used to log diagnostic messages from Sync. It
        /// replaces the deprecated <see cref="AppConfiguration.CustomLogger"/>.
        /// </remarks>
        /// <value>The logger to be used for Realm-originating messages.</value>
        public static Logger Default
        {
            get => _defaultLogger ?? Console;
            set => _defaultLogger = value;
        }

        internal GCHandle GCHandle => _gcHandle.Value;

        // This is only needed for backward compatibility - the App logger sets its own level separately
        // Once that is removed, we should use Logger.LogLevel across the board.
        [Obsolete("Remove when we remove the AppConfiguration.CustomLogger")]
        internal LogLevel? _logLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        protected Logger()
        {
            _gcHandle = new Lazy<GCHandle>(() => GCHandle.Alloc(this));
        }

        internal static void LogDefault(LogLevel level, string message) => Default?.Log(level, message);

        /// <summary>
        /// Log a message at the supplied level.
        /// </summary>
        /// <param name="level">The criticality level for the message.</param>
        /// <param name="message">The message to log.</param>
        public void Log(LogLevel level, string message)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (level < (_logLevel ?? LogLevel))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                return;
            }

            try
            {
                LogImpl(level, message);
            }
            catch (Exception ex)
            {
                Console.Log(LogLevel.Error, $"An exception occurred while trying to log the message: '{message}' at level: {level}. Error: {ex}");
            }
        }

        /// <summary>
        /// The internal implementation being called from <see cref="Log"/>.
        /// </summary>
        /// <param name="level">The criticality level for the message.</param>
        /// <param name="message">The message to log.</param>
        protected abstract void LogImpl(LogLevel level, string message);

        internal static string FormatLog(LogLevel level, string message) => $"{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss.fff} {level}: {message}";

        private class ConsoleLogger : Logger
        {
            protected override void LogImpl(LogLevel level, string message)
            {
                System.Console.WriteLine(FormatLog(level, message));
            }
        }

        private class FileLogger : Logger
        {
            private readonly object locker = new object();
            private readonly string _filePath;
            private readonly Encoding _encoding;

            public FileLogger(string filePath, Encoding encoding = null)
            {
                _filePath = filePath;
                _encoding = encoding ?? Encoding.UTF8;
            }

            protected override void LogImpl(LogLevel level, string message)
            {
                lock (locker)
                {
                    System.IO.File.AppendAllText(_filePath, FormatLog(level, message) + Environment.NewLine, _encoding);
                }
            }
        }

        private class FunctionLogger : Logger
        {
            private readonly Action<LogLevel, string> _logFunction;

            public FunctionLogger(Action<LogLevel, string> logFunction)
            {
                _logFunction = logFunction;
            }

            protected override void LogImpl(LogLevel level, string message) => _logFunction(level, message);
        }

        private class NullLogger : Logger
        {
            protected override void LogImpl(LogLevel level, string message)
            {
            }
        }

        internal class InMemoryLogger : Logger
        {
            private readonly StringBuilder _builder = new StringBuilder();

            protected override void LogImpl(LogLevel level, string message)
            {
                lock (_builder)
                {
                    _builder.AppendLine(FormatLog(level, message));
                }
            }

            public string GetLog() => _builder.ToString();

            public void Clear() => _builder.Clear();
        }
    }
}
