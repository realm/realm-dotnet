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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Sync.Testing;

namespace Realms.Tests.Sync
{
    public enum AppConfigType
    {
        Default,
        IntPartitionKey,
        ObjectIdPartitionKey,
        UUIDPartitionKey,
    }

    public static class SyncTestHelpers
    {
        public const string DefaultPassword = "123456";
        private const string DummyAppId = "myapp-123";

        private static readonly IDictionary<AppConfigType, string> _appIds = new Dictionary<AppConfigType, string>
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

        public static AppConfiguration GetAppConfig(AppConfigType type = AppConfigType.Default) => new AppConfiguration(_appIds[type])
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

            ExtractBaasApps();

            if (ensureNoSessionErrors)
            {
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

        public static string[] ExtractBaasSettings(string[] args)
        {
            var result = new List<string>();

            string baasApps = null;

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--baasurl":
                        _baseUri = new Uri(args[++i]);
                        break;
                    case "--baasapps":
                        baasApps = args[++i];
                        break;
                    default:
                        result.Add(args[i]);
                        break;
                }
            }

            ExtractBaasApps(baasApps);

            return result.ToArray();
        }

        private static void ExtractBaasApps(string appsJson = null)
        {
            if (_appIds[AppConfigType.Default] != DummyAppId || _baseUri == null)
            {
                return;
            }

            AsyncContext.Run(async () =>
            {
                (string, string)[] apps;

                appsJson = GetAppsJson(appsJson);

                if (string.IsNullOrEmpty(appsJson))
                {
                    using var client = new BaasClient(_baseUri);

                    apps = await client.GetApps();
                }
                else
                {
                    apps = JsonConvert.DeserializeObject<Dictionary<string, string>>(appsJson)
                                      .Select(kvp => (kvp.Key, kvp.Value))
                                      .ToArray();
                }

                foreach (var (appName, appId) in apps)
                {
                    _appIds[GetConfigTypeForId(appName)] = appId;
                }
            });
        }

        public static Task<T> SimulateSessionErrorAsync<T>(Session session, ErrorCode code, string message, Action<Session> sessionAssertions)
            where T : Exception
        {
            var tcs = new TaskCompletionSource<T>();
            EventHandler<ErrorEventArgs> handler = null;
            handler = new EventHandler<ErrorEventArgs>((sender, e) =>
            {
                try
                {
                    Assert.That(sender, Is.TypeOf<Session>());
                    Assert.That(e.Exception, Is.TypeOf<T>());
                    sessionAssertions((Session)sender);
                    tcs.TrySetResult((T)e.Exception);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }

                Session.Error -= handler;
            });

            Session.Error += handler;

            session.SimulateError(code, message);

            return tcs.Task;
        }

        private static AppConfigType GetConfigTypeForId(string appName)
        {
            if (appName == "integration-tests")
            {
                return AppConfigType.Default;
            }

            if (appName == "int-partition-key")
            {
                return AppConfigType.IntPartitionKey;
            }

            if (appName == "objectid-part-key")
            {
                return AppConfigType.ObjectIdPartitionKey;
            }

            if (appName == "uuid-partition-key")
            {
                return AppConfigType.UUIDPartitionKey;
            }

            return (AppConfigType)(-1);
        }

        private static string GetAppsJson(string cliArgument)
        {
            var json = cliArgument;
            if (string.IsNullOrEmpty(json))
            {
                // Try to get the apps config from an environment variable if possible
                json = Environment.GetEnvironmentVariable("APPS_CONFIG");
            }

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            // Try to decode the argument from base64 as it may be encoded
            try
            {
                var decodedBytes = Convert.FromBase64String(json);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            catch
            {
            }

            // Assume it's a json encoded string and just return it.
            return json;
        }

        private class BaasClient : IDisposable
        {
            private readonly Uri _baseUri;
            private readonly HttpClient _client = new HttpClient();

            public BaasClient(Uri baseUri)
            {
                _baseUri = baseUri;
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            }

            private async Task Authenticate()
            {
                if (_client.DefaultRequestHeaders.Authorization != null)
                {
                    return;
                }

                using var request = GetJsonContent(new
                {
                    username = "unique_user@domain.com",
                    password = "password"
                });

                var response = await _client.PostAsync(GetAdminUri("auth/providers/local-userpass/login"), request);
                var content = await response.Content.ReadAsStringAsync();
                var doc = BsonDocument.Parse(content);
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {doc["access_token"].AsString}");
            }

            private async Task<string> GetGroupId()
            {
                await Authenticate();
                var response = await _client.GetAsync(GetAdminUri("auth/profile"));
                var content = await response.Content.ReadAsStringAsync();
                var doc = BsonDocument.Parse(content);
                return doc["roles"].AsBsonArray[0].AsBsonDocument["group_id"].AsString;
            }

            public async Task<(string AppName, string AppId)[]> GetApps()
            {
                var groupId = await GetGroupId();
                var response = await _client.GetAsync(GetAdminUri($"groups/{groupId}/apps"));
                var content = await response.Content.ReadAsStringAsync();
                return BsonSerializer.Deserialize<BsonArray>(content)
                    .Select(x => x.AsBsonDocument)
                    .Select(doc => (doc["name"].AsString, doc["client_app_id"].AsString))
                    .ToArray();
            }

            public void Dispose()
            {
                _client.Dispose();
            }

            private Uri GetAdminUri(string relativePath)
            {
                var builder = new UriBuilder(_baseUri);
                builder.Path = $"api/admin/v3.0/{relativePath}";
                return builder.Uri;
            }

            private static HttpContent GetJsonContent(object obj)
            {
                var json = obj.ToJson();
                var content = new StringContent(json);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return content;
            }
        }
    }
}
