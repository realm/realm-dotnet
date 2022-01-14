////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Realms.Tests.Sync
{
    public class BaasClient : IDisposable
    {
        private const string ConfirmFuncSource =
            @"exports = ({ token, tokenId, username }) => {
                  // process the confirm token, tokenId and username
                  if (username.includes(""realm_tests_do_autoverify"")) {
                    return { status: 'success' }
                  }
                  // do not confirm the user
                  return { status: 'fail' };
                };";

        private const string ResetFuncSource =
            @"exports = ({ token, tokenId, username, password }) => {
                  // process the reset token, tokenId, username and password
                  if (password.includes(""realm_tests_do_reset"")) {
                    return { status: 'success' };
                  }
                  // will not reset the password
                  return { status: 'fail' };
                };";

        private readonly HttpClient _client = new HttpClient();

        private readonly string _clusterName;

        private string _groupId;

        private string _appSuffix => $"-{_clusterName}";

        private BaasClient(Uri baseUri, string clusterName = null)
        {
            _client.BaseAddress = new Uri(baseUri, "api/admin/v3.0/");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            _clusterName = clusterName;
        }

        public static async Task<BaasClient> Docker(Uri baseUri)
        {
            var result = new BaasClient(baseUri);

            await result.Authenticate("local-userpass", new
            {
                username = "unique_user@domain.com",
                password = "password"
            });

            var groupDoc = await result.GetAsync<BsonDocument>("auth/profile");
            result._groupId = groupDoc["roles"].AsBsonArray[0].AsBsonDocument["group_id"].AsString;

            return result;
        }

        public static async Task<BaasClient> Atlas(Uri baseUri, string clusterName, string apiKey, string privateApiKey, string groupId)
        {
            var result = new BaasClient(baseUri, clusterName);
            await result.Authenticate("mongodb-cloud", new
            {
                username = apiKey,
                apiKey = privateApiKey
            });

            result._groupId = groupId;

            return result;
        }

        private async Task Authenticate(string provider, object credentials)
        {
            var authDoc = await PostAsync<BsonDocument>($"auth/providers/{provider}/login", credentials);

            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {authDoc["access_token"].AsString}");
        }

        public async Task<BaasApp> CreateApp(string name, string partitionKeyType, bool setupCollections = false)
        {
            TestHelpers.Output.WriteLine($"Creating PBS app {name}...");

            var (app, mongoServiceId) = await CreateAppCore(name, new BsonDocument
            {
                {
                    "sync", new BsonDocument
                    {
                        { "state", "enabled" },
                        { "database_name", $"{partitionKeyType}_partition_key_data" },
                        {
                            "partition", new BsonDocument
                            {
                                { "key", "realm_id" },
                                { "type", partitionKeyType },
                                {
                                    "permissions", new BsonDocument
                                    {
                                        { "read", true },
                                        { "write", true },
                                    }
                                }
                            }
                        }
                    }
                }
            });

            await PutAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/sync/config", new
            {
                development_mode_enabled = true,
            });

            if (setupCollections)
            {
                await CreateSchema(app, mongoServiceId, Schemas.Sales(partitionKeyType));
                await CreateSchema(app, mongoServiceId, Schemas.Users(partitionKeyType));
                await CreateSchema(app, mongoServiceId, Schemas.Foos(partitionKeyType));

                await PatchAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/custom_user_data", new
                {
                    mongo_service_id = mongoServiceId,
                    enabled = true,
                    database_name = "my-db",
                    collection_name = "users",
                    user_id_field = "user_id"
                });
            }

            return app;
        }

        public async Task<BaasApp> CreateFlxApp(string name)
        {
            TestHelpers.Output.WriteLine($"Creating FLX app {name}...");

            var (app, _) = await CreateAppCore(name, new BsonDocument
            {
                {
                    "flexible_sync", new BsonDocument
                    {
                        { "state", "enabled" },
                        { "database_name", "flexible_sync_data" },
                        { "queryable_fields_names", new BsonArray { nameof(SyncAllTypesObject.Int64Property), nameof(SyncAllTypesObject.GuidProperty), nameof(SyncAllTypesObject.DoubleProperty), nameof(IntPropertyObject.Int) } },
                        {
                            "permissions", new BsonDocument
                            {
                                { "rules", new BsonDocument() },
                                {
                                    "defaultRoles", new BsonArray
                                    {
                                        new BsonDocument
                                        {
                                            { "name", "all" },
                                            { "applyWhen", new BsonDocument() },
                                            { "read", true },
                                            { "write", true },
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return app;
        }

        private async Task<(BaasApp App, string MongoServiceId)> CreateAppCore(string name, BsonDocument mongoConfig)
        {
            var doc = await PostAsync<BsonDocument>($"groups/{_groupId}/apps", new { name = $"{name}{_appSuffix}" });
            var appId = doc["_id"].AsString;
            var clientAppId = doc["client_app_id"].AsString;
            var app = new BaasApp(appId, clientAppId, name);

            var confirmFuncId = await CreateFunction(app, "confirmFunc", ConfirmFuncSource);
            var resetFuncId = await CreateFunction(app, "resetFunc", ResetFuncSource);

            await EnableProvider(app, "anon-user");
            await EnableProvider(app, "local-userpass", new
            {
                autoConfirm = false,
                confirmEmailSubject = string.Empty,
                confirmationFunctionName = "confirmFunc",
                confirmationFunctionId = confirmFuncId,
                emailConfirmationUrl = "http://localhost/confirmEmail",
                resetFunctionName = "resetFunc",
                resetFunctionId = resetFuncId,
                resetPasswordSubject = string.Empty,
                resetPasswordUrl = "http://localhost/resetPassword",
                runConfirmationFunction = true,
                runResetFunction = true,
            });

            var serviceName = "mongodb";
            if (_clusterName == null)
            {
                mongoConfig["uri"] = "mongodb://localhost:26000";
            }
            else
            {
                mongoConfig["clusterName"] = _clusterName;
                serviceName = "mongodb-atlas";
            }

            var mongoServiceId = await CreateService(app, "BackingDB", serviceName, mongoConfig);

            await PutAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/sync/config", new
            {
                development_mode_enabled = true,
            });

            return (app, mongoServiceId);
        }

        public async Task EnableProvider(BaasApp app, string type, object config = null, AuthMetadataField[] metadataFields = null)
        {
            TestHelpers.Output.WriteLine($"Enabling provider {type} for {app.Name}...");

            var url = $"groups/{_groupId}/apps/{app}/auth_providers";

            // Api key is slightly special, thus this annoying custom handling.
            if (type == "api-key")
            {
                var providers = await GetAsync<BsonArray>(url);
                var apiKeyProviderId = providers.Select(p => p.AsBsonDocument)
                    .Single(p => p["type"] == "api-key")["_id"].AsString;

                await PutAsync<BsonDocument>($"{url}/{apiKeyProviderId}/enable", new { });
            }
            else
            {
                await PostAsync<BsonDocument>(url, new
                {
                    name = type,
                    type = type,
                    disabled = false,
                    config = config,
                    metadata_fields = metadataFields,
                });
            }
        }

        public async Task<string> CreateFunction(BaasApp app, string name, string source)
        {
            TestHelpers.Output.WriteLine($"Creating function {name} for {app.Name}...");

            var response = await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/functions", new
            {
                name = name,
                can_evaluate = new { },
                @private = false,
                source = source
            });

            return response["_id"].AsString;
        }

        public async Task<BaasApp[]> GetApps()
        {
            var response = await GetAsync<BsonArray>($"groups/{_groupId}/apps");
            return response
                .Select(x => x.AsBsonDocument)
                .Where(doc => doc["name"].AsString.EndsWith(_appSuffix))
                .Select(doc =>
                {
                    var name = doc["name"].AsString;

                    if (!name.EndsWith(_appSuffix))
                    {
                        return null;
                    }

                    var appName = name.Substring(0, name.Length - _appSuffix.Length);
                    return new BaasApp(doc["_id"].AsString, doc["client_app_id"].AsString, appName);
                })
                .Where(a => a != null)
                .ToArray();
        }

        public async Task<string> CreateService(BaasApp app, string name, string type, BsonDocument config)
        {
            TestHelpers.Output.WriteLine($"Creating service {name} for {app.Name}...");

            var response = await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services", new BsonDocument
            {
                { "name", name },
                { "type", type },
                { "config", config },
            });

            return response["_id"].AsString;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private static HttpContent GetJsonContent(object obj)
        {
            var json = obj is BsonDocument doc ? doc.ToJson() : obj.ToJson();
            var content = new StringContent(json);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }

        private async Task<string> CreateSchema(BaasApp app, string mongoServiceId, object schema)
        {
            TestHelpers.Output.WriteLine($"Creating schema for {app.Name}...");

            var response = await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services/{mongoServiceId}/rules", schema);

            return response["_id"].AsString;
        }

        private Task<T> PostAsync<T>(string relativePath, object obj) => SendAsync<T>(HttpMethod.Post, relativePath, obj);

        private Task<T> GetAsync<T>(string relativePath) => SendAsync<T>(HttpMethod.Get, relativePath);

        private Task<T> PutAsync<T>(string relativePath, object obj) => SendAsync<T>(HttpMethod.Put, relativePath, obj);

        private Task<T> PatchAsync<T>(string relativePath, object obj) => SendAsync<T>(new HttpMethod("PATCH"), relativePath, obj);

        private async Task<T> SendAsync<T>(HttpMethod method, string relativePath, object payload = null)
        {
            using var message = new HttpRequestMessage(method, new Uri(relativePath, UriKind.Relative));
            if (payload != null)
            {
                message.Content = GetJsonContent(payload);
            }

            var response = await _client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"An error occurred while executing {method} {relativePath}: {content}");
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(json))
            {
                return BsonSerializer.Deserialize<T>(json);
            }

            return default;
        }

        public class BaasApp
        {
            public string AppId { get; }

            public string ClientAppId { get; }

            public string Name { get; }

            public BaasApp(string appId, string clientAppId, string name)
            {
                AppId = appId;
                ClientAppId = clientAppId;
                Name = name;
            }

            public override string ToString() => AppId;
        }

        public class AuthMetadataField
        {
            [BsonElement("name")]
            public string Name { get; }

            [BsonElement("field_name")]
            public string FieldName { get; }

            [BsonElement("required")]
            public bool Required { get; }

            public AuthMetadataField(string name, string fieldName, bool required = false)
            {
                Name = name;
                FieldName = fieldName;
                Required = required;
            }
        }

        private static class Schemas
        {
            private static object _defaultRoles => new
            {
                name = "default",
                apply_when = new { },
                insert = true,
                delete = true,
                search = true,
                additional_fields = new { }
            };

            public static object Sales(string partitionKeyType) => new
            {
                collection = "sales",
                database = "my-db",
                roles = new[] { _defaultRoles },
                schema = new
                {
                    title = "Sale",
                    bsonType = "object",
                    properties = new
                    {
                        _id = new { bsonType = "int" },
                        date = new { bsonType = "date" },
                        item = new { bsonType = "string" },
                        price = new { bsonType = "decimal" },
                        quantity = new { bsonType = "decimal" },
                        realm_id = new { bsonType = partitionKeyType }
                    },
                    required = new[] { "_id" },
                }
            };

            public static object Users(string partitionKeyType) => new
            {
                collection = "users",
                database = "my-db",
                roles = new[] { _defaultRoles },
                schema = new
                {
                    title = "User",
                    bsonType = "object",
                    properties = new
                    {
                        _id = new { bsonType = "objectId" },
                        realm_id = new { bsonType = partitionKeyType },
                        user_id = new { bsonType = "string" }
                    },
                    required = new[] { "_id" },
                }
            };

            public static object Foos(string partitionKeyType) => new
            {
                collection = "foos",
                database = "my-db",
                roles = new[] { _defaultRoles },
                schema = new
                {
                    title = "Foo",
                    bsonType = "object",
                    properties = new
                    {
                        _id = new { bsonType = "objectId" },
                        realm_id = new { bsonType = partitionKeyType },
                        longValue = new { bsonType = "long" },
                        stringValue = new { bsonType = "string" },
                    },
                    required = new[] { "_id" },
                }
            };
        }
    }
}
