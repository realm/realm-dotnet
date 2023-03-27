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
    public abstract class Logger
    {
        private readonly Lazy<GCHandle> _gcHandle;

        private static Logger? _defaultLogger;
        private static LogLevel logLevel = LogLevel.Info;

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
        public static Logger File(string filePath, Encoding? encoding = null) => new FileLogger(filePath, encoding);

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
        /// <value>The log level for Realm-originating messages.</value>
        public static LogLevel LogLevel
        {
            get => logLevel;
            set
            {
                logLevel = value;
                SharedRealmHandle.SetLogLevel(value);
            }
        }

        /// <summary>
        /// Gets or sets a custom <see cref="Logger"/> implementation that will be used by
        /// Realm whenever information must be logged.
        /// </summary>
        /// <value>The logger to be used for Realm-originating messages.</value>
        public static Logger Default
        {
            get => _defaultLogger ?? Console;
            set => _defaultLogger = value;
        }

        internal GCHandle GCHandle => _gcHandle.Value;

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
            if (level < LogLevel)
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
            private readonly object locker = new();
            private readonly string _filePath;
            private readonly Encoding _encoding;

            public FileLogger(string filePath, Encoding? encoding = null)
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
            private readonly StringBuilder _builder = new();

            protected override void LogImpl(LogLevel level, string message)
            {
                lock (_builder)
                    {
                        _builder.AppendLine(FormatLog(level, message));
                    }
                }

            public string GetLog()
            {
                lock (_builder)
                {
                    return _builder.ToString();
                }
            }

            public void Clear() => _builder.Clear();
        }

        internal class AsyncFileLogger : Logger, IDisposable
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

            protected override void LogImpl(LogLevel level, string message)
            {
                if (!_isFlushing)
                {
                    _queue.Enqueue(FormatLog(level, message));
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
