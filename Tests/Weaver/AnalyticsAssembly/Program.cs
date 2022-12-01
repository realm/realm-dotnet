////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Threading.Tasks;
using Realms.Sync;

public class Program
{
    public static void Main(string[] args)
    {
    }

    public static void CreateApp()
    {
#if CREATE_APP_STRING
        _ = App.Create("abc");
#endif
    }

    public static async Task LoginAnonymous()
    {
#if LOGIN_ANONYMOUS
        App app = null;
        _ = await app.LogInAsync(Credentials.Anonymous());
#endif
    }

    [Obsolete]
    public static void CreateLegacySyncConfig()
    {
#if CREATE_LEGACY_SYNC_CONFIG
        _ = new SyncConfiguration(null, null);
#endif
    }
}
