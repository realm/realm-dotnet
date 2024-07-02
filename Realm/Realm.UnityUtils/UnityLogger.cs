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

using Realms.Logging;

namespace UnityUtils
{
    public class UnityLogger : Logger
    {
        protected override void LogImpl(LogLevel level, string message, LogCategory? category = null)
        {
            var toLog = FormatLog(level, message, category!);
            switch (level)
            {
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(toLog);
                    break;
                case LogLevel.Error:
                case LogLevel.Warn:
                    UnityEngine.Debug.LogWarning(toLog);
                    break;
                default:
                    UnityEngine.Debug.Log(toLog);
                    break;
            }
        }
    }
}
