////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.Threading;
using System.Threading.Tasks;

namespace Realms.Helpers
{
    internal static class AsyncHelper
    {
        private const string MissingContextErrorMessage = "Asynchronous operations require a synchronization context. " +
            "Either invoke this method on the main thread or install a synchronization context on your background thread, " +
            "for example, by using a 3rd party package, such as Nito.AsyncEx.Context.";

        public static void EnsureValidContext()
        {
            if (!TryGetValidContext(out _))
            {
                throw new NotSupportedException(MissingContextErrorMessage)
                {
                    HelpLink = "https://www.mongodb.com/docs/atlas/device-sdks/sdk/dotnet/crud/threading/"
                };
            }
        }

        public static bool TryGetValidContext([MaybeNullWhen(false)] out SynchronizationContext synchronizationContext)
        {
            synchronizationContext = SynchronizationContext.Current;
            return synchronizationContext != null;
        }

        public static bool TryGetScheduler([MaybeNullWhen(false)] out TaskScheduler scheduler)
        {
            if (TryGetValidContext(out _))
            {
                scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                return true;
            }

            scheduler = null;
            return false;
        }
    }
}
