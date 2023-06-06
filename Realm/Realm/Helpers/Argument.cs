////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;
using Realms.Logging;

namespace Realms.Helpers
{
    internal static class Argument
    {
        private const string OpenIssueText = "Please create a new issue at http://github.com/realm/realm-dotnet/issues/new.";

        public static void NotNullOrEmpty(string value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{paramName} must not be empty", paramName);
            }
        }

        public static void EnsureRange(double value, double min, double max, string paramName)
        {
            if (value < min || value > max)
            {
                throw new ArgumentException($"{paramName} must be a value in the range of [{min}, {max}]", paramName);
            }
        }

        public static void Ensure<T>([DoesNotReturnIf(false)] bool condition, string message)
            where T : Exception
        {
            if (!condition)
            {
                throw (T)Activator.CreateInstance(typeof(T), message)!;
            }
        }

        public static T EnsureType<T>(object obj, string message, string paramName)
        {
            if (obj is not T tObj)
            {
                throw new ArgumentException(message, paramName);
            }

            return tObj;
        }

        public static void Ensure([DoesNotReturnIf(false)] bool condition, string message, string paramName)
        {
            if (!condition)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        public static void NotNull(object? value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        [return: NotNullIfNotNull("value")]
        public static T ValidateNotNull<T>(T value, string paramName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        public static void AssertDebug(string message)
        {
            Logger.LogDefault(LogLevel.Error, $"{message} {OpenIssueText}");

#if DEBUG
            throw new Exception(message);
#endif
        }
    }
}
