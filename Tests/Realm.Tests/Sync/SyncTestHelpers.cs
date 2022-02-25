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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    public static class AppConfigType
    {
        public const string Default = "integration-tests";
        public const string IntPartitionKey = "int-partition-key";
        public const string ObjectIdPartitionKey = "objectid-partition-key";
        public const string UUIDPartitionKey = "uuid-part-key";
        public const string FlexibleSync = "flexible-sync";
    }

    public static partial class SyncTestHelpers
    {
        public const string DefaultPassword = "123456";
        private const string DummyAppId = "myapp-123";

        private static readonly IDictionary<string, string> _appIds = new Dictionary<string, string>
        {
            [AppConfigType.Default] = DummyAppId,
        };

        private static Uri _baseUri;

        static SyncTestHelpers()
        {
#if !UNITY
            try
            {
                _baseUri = new Uri(System.Configuration.ConfigurationManager.AppSettings["BaasUrl"]);
            }
            catch
            {
            }
#endif
        }

        public static AppConfiguration GetAppConfig(string type = AppConfigType.Default) => new AppConfiguration(_appIds[type])
        {
            BaseUri = _baseUri ?? new Uri("http://localhost:12345"),
            MetadataPersistenceMode = MetadataPersistenceMode.NotEncrypted,
        };

        public static void RunBaasTestAsync(Func<Task> testFunc, int timeout = 30000, bool ensureNoSessionErrors = false)
        {
            if (_baseUri == null)
            {
                Assert.Ignore("MongoDB Realm is not setup.");
            }

            AsyncContext.Run(async () =>
            {
                await CreateBaasAppsAsync();
            });

            if (ensureNoSessionErrors)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var tcs = new TaskCompletionSource<object>();
                Session.Error += HandleSessionError;
                try
                {
                    TestHelpers.RunAsyncTest(testFunc, timeout, tcs.Task);
                }
                finally
                {
                    Session.Error -= HandleSessionError;
                }
#pragma warning restore CS0618 // Type or member is obsolete

                void HandleSessionError(object _, ErrorEventArgs errorArgs)
                {
                    tcs.TrySetException(errorArgs.Exception);
                }
            }
            else
            {
                TestHelpers.RunAsyncTest(testFunc, timeout);
            }
        }

        public static string GetVerifiedUsername() => $"realm_tests_do_autoverify-{Guid.NewGuid()}";

        public static async Task<string[]> ExtractBaasSettingsAsync(string[] args)
        {
            var result = new List<string>();

            string baasCluster = null;
            string baasApiKey = null;
            string baasPrivateApiKey = null;
            string groupId = null;
            string baseUrl = null;

            for (var i = 0; i < args.Length; i++)
            {
                if (!ExtractArg(i, "baasurl", ref baseUrl) &&
                    !ExtractArg(i, "baascluster", ref baasCluster) &&
                    !ExtractArg(i, "baasapikey", ref baasApiKey) &&
                    !ExtractArg(i, "baasprivateapikey", ref baasPrivateApiKey) &&
                    !ExtractArg(i, "baasprojectid", ref groupId))
                {
                    result.Add(args[i]);
                }
            }

            if (baseUrl != null)
            {
                _baseUri = new Uri(baseUrl);
            }

            await CreateBaasAppsAsync(baasCluster, baasApiKey, baasPrivateApiKey, groupId);

            return result.ToArray();

            bool ExtractArg(int index, string name, ref string value)
            {
                var arg = args[index];
                if (arg.StartsWith($"--{name}="))
                {
                    value = arg.Replace($"--{name}=", string.Empty);
                    return true;
                }

                return false;
            }
        }

        public static string[] ExtractBaasSettings(string[] args)
        {
            return AsyncContext.Run(async () =>
            {
                return await ExtractBaasSettingsAsync(args);
            });
        }

        private static async Task CreateBaasAppsAsync(string cluster = null, string apiKey = null, string privateApiKey = null, string groupId = null)
        {
            if (_appIds[AppConfigType.Default] != DummyAppId || _baseUri == null)
            {
                return;
            }

#if !UNITY
            try
            {
                cluster ??= System.Configuration.ConfigurationManager.AppSettings["Cluster"];
                apiKey ??= System.Configuration.ConfigurationManager.AppSettings["ApiKey"];
                privateApiKey ??= System.Configuration.ConfigurationManager.AppSettings["PrivateApiKey"];
                groupId ??= System.Configuration.ConfigurationManager.AppSettings["GroupId"];
            }
            catch
            {
            }
#endif

            BaasClient client;
            if (cluster != null)
            {
                client = await BaasClient.Atlas(_baseUri, cluster, apiKey, privateApiKey, groupId);
            }
            else
            {
                client = await BaasClient.Docker(_baseUri);
            }

            using (client)
            {
                var apps = await client.GetApps();

                TestHelpers.Output.WriteLine($"Found {apps.Length} apps.");

                if (apps.Any())
                {
                    foreach (var app in apps)
                    {
                        _appIds[app.Name] = app.ClientAppId;
                    }
                }
                else
                {
                    var defaultApp = await client.CreateApp(AppConfigType.Default, "string", setupCollections: true);

                    var authFuncId = await client.CreateFunction(defaultApp, "authFunc", @"exports = (loginPayload) => {
                      return loginPayload[""realmCustomAuthFuncUserId""];
                    };");

                    await client.CreateFunction(defaultApp, "documentFunc", @"exports = function(first, second){
                      return {
                        intValue: first.intValue + second.intValue,
                        floatValue: first.floatValue + second.floatValue,
                        stringValue: first.stringValue + second.stringValue,
                        objectId: first.objectId,
                        date: second.date,
                        child: {
                          intValue: first.child.intValue + second.child.intValue
                        },
                        arr: [ first.arr[0], second.arr[0] ]
                      }
                    };");

                    await client.CreateFunction(defaultApp, "mirror", @"exports = function(arg){
                      return arg;
                    };");

                    await client.CreateFunction(defaultApp, "sumFunc", @"exports = function(...args) {
                      return args.reduce((a,b) => a + b, 0);
                    };");

                    await client.EnableProvider(defaultApp, "api-key");
                    _appIds[AppConfigType.Default] = defaultApp.ClientAppId;

                    await client.EnableProvider(defaultApp, "custom-function", new
                    {
                        authFunctionName = "authFunc",
                        authFunctionId = authFuncId
                    });

                    await client.EnableProvider(defaultApp, "custom-token", new
                    {
                        audience = "my-audience",
                        signingAlgorithm = "RS256",
                        useJWKURI = false,
                        signingKey = "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAntjcGTEsm1r7UqEYgovi\nUX3SV6+26ExRHFOGfUVXG+nUejq5Px/vYHl5f0w+MBZ5Pz8IlyTuPod2zm8iyR/I\npqreOjpNH+RdmMQuJohNdzXPUHCHkcZWIU84cpI2ap+/W/0GubHxg6ItHllsDun/\n9Tgc47sJGRLwGrH7JAE/IsUDLdA+ayl18IBE5aq4SqdXbqLQw6wi+xVj4PF+ITpp\n3ZHg3lJUN2QIe2ewdUuesGDkxTM7d4rAO9MuiVQozdViNeW7kYH8JG+WyXRrZX0v\niseQHyOLiAhJrsyk4J/MN0rtm2rzHYFDFaHsQPIkv7n8G7hySJbQfZpPG2JsMQ2L\nywIDAQAB\n-----END PUBLIC KEY-----",
                    }, new[]
                    {
                        new BaasClient.AuthMetadataField("userId", "externalUserId", true),
                        new BaasClient.AuthMetadataField("name.first", "first_name"),
                        new BaasClient.AuthMetadataField("name.last", "last_name"),
                        new BaasClient.AuthMetadataField("jobTitle", "title"),
                        new BaasClient.AuthMetadataField("email", "email"),
                        new BaasClient.AuthMetadataField("pictureUrl", "picture_url"),
                        new BaasClient.AuthMetadataField("gender", "gender"),
                        new BaasClient.AuthMetadataField("birthday", "birthday"),
                        new BaasClient.AuthMetadataField("minAge", "min_age"),
                        new BaasClient.AuthMetadataField("maxAge", "max_age"),
                    });

                    await client.CreateService(defaultApp, "gcm", "gcm", new
                    {
                        senderId = "gcm",
                        apiKey = "gcm",
                    });

                    var intApp = await client.CreateApp(AppConfigType.IntPartitionKey, "long");
                    _appIds[AppConfigType.IntPartitionKey] = intApp.ClientAppId;

                    var uuidApp = await client.CreateApp(AppConfigType.UUIDPartitionKey, "uuid");
                    _appIds[AppConfigType.UUIDPartitionKey] = uuidApp.ClientAppId;

                    var objectIdApp = await client.CreateApp(AppConfigType.ObjectIdPartitionKey, "objectId");
                    _appIds[AppConfigType.ObjectIdPartitionKey] = objectIdApp.ClientAppId;

                    var flexibleSyncApp = await client.CreateFlxApp(AppConfigType.FlexibleSync);
                    _appIds[AppConfigType.FlexibleSync] = flexibleSyncApp.ClientAppId;
                }
            }
        }
    }
}
