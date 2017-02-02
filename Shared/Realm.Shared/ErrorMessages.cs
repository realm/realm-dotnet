﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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

namespace Realms
{
    internal static class ErrorMessages
    {
        public const string RealmNotifyErrorNoSubscribers =
            "A realm-level exception has occurred. To handle and react to those, subscribe to the Realm.Error event.";

        public static readonly string RealmWriteAsyncThreadChange =
            "You are using Realm.WriteAsync on a thread without SynchronizationContext (likely a background thread)." +
            Environment.NewLine +
            "This will work as expected, however, upon returning from the await, the original Realm will be on a " +
            "different thread, making it unusable. We recommend that you use the synchronous version - Realm.Write - instead.";

        public static void OutputError(string error)
        {
            Console.Error.WriteLine(error);
        }
    }
}