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
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realms.Logging
{
    /// <summary>
    /// A logger that logs messages originating from Realm. The default logger can be replaced by setting <see cref="Default"/>.
    /// <br/>
    /// A few built-in implementations are provided by <see cref="Console"/>, <see cref="Null"/>, and <see cref="Function(Action{string})"/>,
    /// but you can implement your own.
    /// </summary>
    public abstract class RealmLogger
    {
        private readonly Lazy<GCHandle> _gcHandle;

        private static readonly LogCategory _defaultLogCategory = LogCategory.Realm;
        private static RealmLogger? _defaultLogger;

        /// <summary>
        /// Gets a <see cref="ConsoleLogger"/> that outputs messages to the default console. For most project types, that will be
        /// using <see cref="Console.WriteLine()"/> but certain platforms may use different implementations.
        /// </summary>
        /// <value>A <see cref="RealmLogger"/> instance that outputs to the platform's console.</value>
        public static RealmLogger Console { get; internal set; } = new ConsoleLogger();

        /// <summary>
        /// Gets a <see cref="FileLogger"/> that saves the log messages to a file.
        /// </summary>
        /// <param name="filePath">Path of the file to save messages to. The file is created if it does not already exists.</param>
        /// <param name="encoding">Character encoding to use. Defaults to <see cref="System.Text.Encoding.UTF8"/> if not specified.</param>
        /// <remarks>
        /// Please note that this logger is not optimized for performance, and could lead to overall sync performance slowdown with more verbose log levels.
        /// </remarks>
        /// <returns>
        /// A <see cref="RealmLogger"/> instance that will save log messages to a file.
        /// </returns>
        public static RealmLogger File(string filePath, Encoding? encoding = null) => new FileLogger(filePath, encoding);

        /// <summary>
        /// Gets a <see cref="NullLogger"/> that ignores all messages.
        /// </summary>
        /// <value>A <see cref="RealmLogger"/> that doesn't output any messages.</value>
        public static RealmLogger Null { get; } = new NullLogger();

        /// <summary>
        /// Gets a <see cref="FunctionLogger"/> that proxies Log calls to the supplied function. The message will
        /// already be formatted with the default message formatting that includes a timestamp.
        /// </summary>
        /// <param name="logFunction">Function to proxy log calls to.</param>
        /// <returns>
        /// A <see cref="RealmLogger"/> instance that will invoke <paramref name="logFunction"/> for each message.
        /// </returns>
        public static RealmLogger Function(Action<string> logFunction) => new FunctionLogger((level, category, message) => logFunction(FormatLog(level, category, message)));

        /// <summary>
        /// Gets a <see cref="FunctionLogger"/> that proxies Log calls to the supplied function.
        /// </summary>
        /// <param name="logFunction">Function to proxy log calls to.</param>
        /// <returns>
        /// A <see cref="RealmLogger"/> instance that will invoke <paramref name="logFunction"/> for each message.
        /// </returns>
        public static RealmLogger Function(Action<LogLevel, LogCategory, string> logFunction) => new FunctionLogger(logFunction);

        /// <summary>
        /// Gets the verbosity of log messages for the given category.
        /// </summary>
        /// <param name="category">The category to get the level for. Defaults to <see cref="LogCategory.Realm"/> if not specified.</param>
        /// <returns>
        /// The log level used for the given category.
        /// </returns>
        public static LogLevel GetLogLevel(LogCategory? category = null)
        {
            category ??= _defaultLogCategory;
            return SharedRealmHandle.GetLogLevel(category);
        }

        /// <summary>
        /// Sets the verbosity of log messages for the given category.
        /// </summary>
        /// <param name="level">The log level to use for messages.</param>
        /// <param name="category">The category to set the level for. Defaults to <see cref="LogCategory.Realm"/> if not specified.</param>
        public static void SetLogLevel(LogLevel level, LogCategory? category = null)
        {
            category ??= _defaultLogCategory;
            SharedRealmHandle.SetLogLevel(level, category);
        }

        /// <summary>
        /// Gets or sets a custom <see cref="RealmLogger"/> implementation that will be used by
        /// Realm whenever information must be logged.
        /// </summary>
        /// <value>The logger to be used for Realm-originating messages.</value>
        public static RealmLogger Default
        {
            get => _defaultLogger ?? Console;
            set => _defaultLogger = value;
        }

        internal GCHandle GCHandle => _gcHandle.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmLogger"/> class.
        /// </summary>
        protected RealmLogger()
        {
            _gcHandle = new Lazy<GCHandle>(() => GCHandle.Alloc(this));
        }

        /// <summary>
        /// Log a message at the supplied level and default category <see cref="LogCategory.RealmLogCategory.SDK"/>.
        /// </summary>
        /// <param name="level">The criticality level for the message.</param>
        /// <param name="message">The message to log.</param>
        public void Log(LogLevel level, string message)
        {
            Log(level, LogCategory.Realm.SDK, message);
        }

        /// <summary>
        /// Log a message at the supplied level and category.
        /// </summary>
        /// <param name="level">The criticality level for the message.</param>
        /// <param name="category">The category for the message.</param>
        /// <param name="message">The message to log.</param>
        public void Log(LogLevel level, LogCategory category, string message)
        {
            if (level < GetLogLevel(category))
            {
                return;
            }

            LogAnyLevel(level, category, message);
        }

        /// <summary>
        /// Log a message without calling into Core to check the current level. Logs from
        /// Core should always call this API as they already check the level prior to notifying.
        /// </summary>
        internal void LogAnyLevel(LogLevel level, LogCategory category, string message)
        {
            try
            {
                LogImpl(level, category, message);
            }
            catch (Exception ex)
            {
                Console.Log(LogLevel.Error, $"An exception occurred while trying to log the message: '{message}' at level: '{level}' in category: '{category}'. Error: {ex}");
            }
        }

        /// <summary>
        /// The internal implementation being called from <see cref="Log(Realms.Logging.LogLevel, LogCategory, string)"/>.
        /// </summary>
        /// <param name="level">The criticality level for the message.</param>
        /// <param name="category">The category for the message.</param>
        /// <param name="message">The message to log.</param>
        protected abstract void LogImpl(LogLevel level, LogCategory category, string message);

        internal static string FormatLog(LogLevel level, LogCategory category, string message) => $"{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss.fff} {category} {level}: {message}";

        private class ConsoleLogger : RealmLogger
        {
            protected override void LogImpl(LogLevel level, LogCategory category, string message)
            {
                System.Console.WriteLine(FormatLog(level, category, message));
            }
        }

        private class FileLogger(string filePath, Encoding? encoding = null)
            : RealmLogger
        {
            private readonly object _locker = new();
            private readonly Encoding _encoding = encoding ?? Encoding.UTF8;

            protected override void LogImpl(LogLevel level, LogCategory category, string message)
            {
                lock (_locker)
                {
                    System.IO.File.AppendAllText(filePath, FormatLog(level, category, message) + Environment.NewLine, _encoding);
                }
            }
        }

        private class FunctionLogger(Action<LogLevel, LogCategory, string> logFunction)
            : RealmLogger
        {
            protected override void LogImpl(LogLevel level, LogCategory category, string message) => logFunction(level, category, message);
        }

        private class NullLogger : RealmLogger
        {
            protected override void LogImpl(LogLevel level, LogCategory category, string message)
            {
            }
        }

        internal class InMemoryLogger : RealmLogger
        {
            private readonly StringBuilder _builder = new();

            protected override void LogImpl(LogLevel level, LogCategory category, string message)
            {
                lock (_builder)
                {
                    _builder.AppendLine(FormatLog(level, category, message));
                }
            }

            public string GetLog()
            {
                lock (_builder)
                {
                    return _builder.ToString();
                }
            }

            public void Clear()
            {
                lock (_builder)
                {
                    _builder.Clear();
                }
            }
        }

        internal class AsyncFileLogger : RealmLogger, IDisposable
        {
            private readonly ConcurrentQueue<string> _queue = new();
            private readonly string _filePath;
            private readonly Encoding _encoding;
            private readonly AutoResetEvent _hasNewItems = new(false);
            private readonly AutoResetEvent _flush = new(false);
            private readonly Task _runner;
            private volatile bool _isFlushing;

            public AsyncFileLogger(string filePath, Encoding? encoding = null)
            {
                _filePath = filePath;
                _encoding = encoding ?? Encoding.UTF8;
                _runner = Task.Run(Run);
            }

            public void Dispose()
            {
                _isFlushing = true;
                _flush.Set();
                _runner.Wait();

                _hasNewItems.Dispose();
                _flush.Dispose();
            }

            protected override void LogImpl(LogLevel level, LogCategory category, string message)
            {
                if (!_isFlushing)
                {
                    _queue.Enqueue(FormatLog(level, category, message));
                    _hasNewItems.Set();
                }
            }

            private void Run()
            {
                var sb = new StringBuilder();
                while (true)
                {
                    sb.Clear();
                    WaitHandle.WaitAny(new[] { _hasNewItems, _flush });
                    while (_queue.TryDequeue(out var item))
                    {
                        sb.AppendLine(item);
                    }

                    System.IO.File.AppendAllText(_filePath, sb.ToString(), _encoding);

                    if (_isFlushing)
                    {
                        return;
                    }
                }
            }
        }
    }
}
