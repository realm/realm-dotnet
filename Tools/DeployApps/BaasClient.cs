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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Baas
{
    public static class AppConfigType
    {
        public const string Default = "pbs-str";
        public const string IntPartitionKey = "pbs-int";
        public const string ObjectIdPartitionKey = "pbs-oid";
        public const string UUIDPartitionKey = "pbs-uuid";
        public const string FlexibleSync = "flx";
    }

    public class BaasClient
    {
        public class FunctionReturn
        {
            public int Deleted { get; set; }
        }

        private const string ConfirmFuncSource =
            @"exports = async function ({ token, tokenId, username }) {
                  // process the confirm token, tokenId and username
                  if (username.includes(""realm_tests_do_autoverify"")) {
                    return { status: 'success' };
                  }

                  if (username.includes(""realm_tests_do_not_confirm"")) {
                    const mongodb = context.services.get('BackingDB');
                    let collection = mongodb.db('test_db').collection('not_confirmed');
                    let result = await collection.findOne({'email': username});

                    if(result === null)
                    {
                        let newVal = {
                            'email': username,
                            'token': token,
                            'tokenId': tokenId,
                        }

                        await collection.insertOne(newVal);
                        return { status: 'pending' };
                    }

                    return { status: 'success' };
                  }

                  // fail the user confirmation
                  return { status: 'fail' };
                };";

        private const string ResetFuncSource =
            @"exports = async function ({ token, tokenId, username, password, currentPasswordValid }) {
                  // process the reset token, tokenId, username and password
                  if (password.includes(""realm_tests_do_reset"")) {
                    return { status: 'success' };
                  }

                  if (password.includes(""realm_tests_do_not_reset"")) {
                    const mongodb = context.services.get('BackingDB');
                    let collection = mongodb.db('test_db').collection('not_reset');
                    let result = await collection.findOne({'email': username});

                    if(result === null)
                    {
                        let newVal = {
                            'email': username,
                            'token': token,
                            'tokenId': tokenId,
                        }

                        await collection.insertOne(newVal);
                        return { status: 'pending' };
                    }

                    return { status: 'success' };
                   }

                  // will not reset the password
                  return { status: 'fail' };
                };";

        private const string TriggerClientResetOnSyncServerFuncSource =
            @"exports = async function(userId, appId = '') {
                const mongodb = context.services.get('BackingDB');
                console.log('user.id: ' + context.user.id);
                try {
                  let dbName = '__realm_sync';
                  if (appId !== '')
                  {
                    dbName += `_${appId}`;
                  }
                  const deletionResult = await mongodb.db(dbName).collection('clientfiles').deleteMany({ ownerId: userId });
                  console.log('Deleted documents: ' + deletionResult.deletedCount);

                  return { Deleted: deletionResult.deletedCount };
                } catch(err) {
                  throw 'Deletion failed: ' + err;
                }
            };";

        private const string ConfirmationInfoFuncSource =
            @"exports = async function(username){
              const mongodb = context.services.get('BackingDB');
              let collection = mongodb.db('test_db').collection('not_confirmed');
              return await collection.findOne({'email': username});
            };";

        private const string ResetPasswordInfoFuncSource =
            @"exports = async function(username){
              const mongodb = context.services.get('BackingDB');
              let collection = mongodb.db('test_db').collection('not_reset');
              return await collection.findOne({'email': username});
            };";

        private readonly HttpClient _client = new();

        private readonly string? _clusterName;

        private readonly TextWriter _output;

        private string _groupId = null!;
        private string? _refreshToken;

        private string _shortSuffix
        {
            get
            {
                var completeSuffix = $"{Differentiator}-{_clusterName}";
                if (completeSuffix.Length < 8)
                {
                    return completeSuffix;
                }

                using var sha = SHA256.Create();
                var inputBytes = Encoding.ASCII.GetBytes(completeSuffix);
                var hashBytes = sha.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (var i = 0; i < 4; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString().ToLower();
            }
        }

        private string _appSuffix => $"-{_shortSuffix}";

        public string Differentiator { get; }

        static BaasClient()
        {
            BsonSerializer.RegisterSerializer(new ObjectSerializer(_ => true));
        }

        private BaasClient(Uri baseUri, string differentiator, TextWriter output, string? clusterName = null)
        {
            _client.BaseAddress = new Uri(baseUri, "api/admin/v3.0/");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            _clusterName = clusterName;
            Differentiator = differentiator;
            _output = output;
        }

        public static async Task<BaasClient> Docker(Uri baseUri, string differentiator, TextWriter output)
        {
            var result = new BaasClient(baseUri, differentiator, output);

            await result.Authenticate("local-userpass", new
            {
                username = "unique_user@domain.com",
                password = "password"
            });

            var groupDoc = await result.GetAsync<BsonDocument>("auth/profile");
            result._groupId = groupDoc!["roles"].AsBsonArray[0].AsBsonDocument["group_id"].AsString;

            return result;
        }

        public static async Task<BaasClient> Atlas(Uri baseUri, string differentiator, TextWriter output, string clusterName, string apiKey, string privateApiKey, string groupId)
        {
            var result = new BaasClient(baseUri, differentiator, output, clusterName);
            await result.Authenticate("mongodb-cloud", new
            {
                username = apiKey,
                apiKey = privateApiKey
            });

            result._groupId = groupId;

            return result;
        }

        public static async Task<(BaasClient? Client, Uri? BaseUrl, string[] RemainingArgs)> CreateClientFromArgs(string[] args, TextWriter output)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var (extracted, remaining) = Utils.ExtractArguments(args, "baasaasapikey", "baasurl", "baascluster", "baasapikey", "baasprivateapikey", "baasprojectid", "baasdifferentiator");

            if (extracted.TryGetValue("baasaasapikey", out var baaSaasApiKey) && string.IsNullOrEmpty(baaSaasApiKey))
            {

            }

            if (!extracted.TryGetValue("baasurl", out var baseUrl) || string.IsNullOrEmpty(baseUrl))
            {
                return (null, null, remaining);
            }

            var baseUri = new Uri(baseUrl);
            var baasCluster = extracted.GetValueOrDefault("baascluster");
            var differentiator = extracted.GetValueOrDefault("baasdifferentiator", "local");

            var client = string.IsNullOrEmpty(baasCluster)
                ? await Docker(baseUri, differentiator, output)
                : await Atlas(baseUri, differentiator, output, baasCluster!, extracted["baasapikey"], extracted["baasprivateapikey"], extracted["baasprojectid"]);

            return (client, baseUri, remaining);
        }

        public string GetSyncDatabaseName(string appType = AppConfigType.Default) => $"Sync_{Differentiator}_{appType}";

        private async Task Authenticate(string provider, object credentials)
        {
            var authDoc = await PostAsync<BsonDocument>($"auth/providers/{provider}/login", credentials);

            _refreshToken = authDoc!["refresh_token"].AsString;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authDoc["access_token"].AsString);
        }

        public async Task<IDictionary<string, BaasApp>> GetOrCreateApps()
        {
            var apps = await GetApps();

            _output.WriteLine($"Found {apps.Length} apps.");

            var result = new Dictionary<string, BaasApp>();
            await GetOrCreateApp(result, AppConfigType.Default, apps, CreateDefaultApp);
            await GetOrCreateApp(result, AppConfigType.FlexibleSync, apps, CreateFlxApp);
            await GetOrCreateApp(result, AppConfigType.IntPartitionKey, apps, name => CreatePbsApp(name, "long"));
            await GetOrCreateApp(result, AppConfigType.UUIDPartitionKey, apps, name => CreatePbsApp(name, "uuid"));
            await GetOrCreateApp(result, AppConfigType.ObjectIdPartitionKey, apps, name => CreatePbsApp(name, "objectId"));

            return result;
        }

        private async Task GetOrCreateApp(IDictionary<string, BaasApp> result, string name, BaasApp[] apps, Func<string, Task<BaasApp>> creator)
        {
            var app = apps.SingleOrDefault(a => a.Name.StartsWith(name));
            if (app == null)
            {
                app = await creator(name);
            }
            else
            {
                _output.WriteLine($"Found {app.Name} with id {app.ClientAppId}.");
            }

            result[app.Name] = app;
        }

        private async Task<BaasApp> CreateDefaultApp(string name)
        {
            var app = await CreatePbsApp(name, "string", setupCollections: true);

            var authFuncId = await CreateFunction(app, "authFunc", @"exports = (loginPayload) => {
                      return loginPayload[""realmCustomAuthFuncUserId""];
                    };");

            await CreateFunction(app, "triggerClientResetOnSyncServer", TriggerClientResetOnSyncServerFuncSource, runAsSystem: true);
            await CreateFunction(app, "confirmationInfo", ConfirmationInfoFuncSource, runAsSystem: true);
            await CreateFunction(app, "resetInfo", ResetPasswordInfoFuncSource, runAsSystem: true);

            await CreateFunction(app, "documentFunc", @"exports = function(first, second){
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

            await CreateFunction(app, "mirror", @"exports = function(arg){
                return arg;
            };");

            await CreateFunction(app, "sumFunc", @"exports = function(...args) {
                return args.reduce((a,b) => a + b, 0);
            };");

            await EnableProvider(app, "api-key");

            await EnableProvider(app, "custom-function", new
            {
                authFunctionName = "authFunc",
                authFunctionId = authFuncId
            });

            await EnableProvider(app, "custom-token", new
            {
                audience = "my-audience",
                signingAlgorithm = "RS256",
                useJWKURI = false,
                signingKey = "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAntjcGTEsm1r7UqEYgovi\nUX3SV6+26ExRHFOGfUVXG+nUejq5Px/vYHl5f0w+MBZ5Pz8IlyTuPod2zm8iyR/I\npqreOjpNH+RdmMQuJohNdzXPUHCHkcZWIU84cpI2ap+/W/0GubHxg6ItHllsDun/\n9Tgc47sJGRLwGrH7JAE/IsUDLdA+ayl18IBE5aq4SqdXbqLQw6wi+xVj4PF+ITpp\n3ZHg3lJUN2QIe2ewdUuesGDkxTM7d4rAO9MuiVQozdViNeW7kYH8JG+WyXRrZX0v\niseQHyOLiAhJrsyk4J/MN0rtm2rzHYFDFaHsQPIkv7n8G7hySJbQfZpPG2JsMQ2L\nywIDAQAB\n-----END PUBLIC KEY-----",
            }, new[]
            {
                new AuthMetadataField("userId", "externalUserId", true),
                new AuthMetadataField("name.first", "first_name"),
                new AuthMetadataField("name.last", "last_name"),
                new AuthMetadataField("jobTitle", "title"),
                new AuthMetadataField("email", "email"),
                new AuthMetadataField("pictureUrl", "picture_url"),
                new AuthMetadataField("gender", "gender"),
                new AuthMetadataField("birthday", "birthday"),
                new AuthMetadataField("minAge", "min_age"),
                new AuthMetadataField("maxAge", "max_age"),
            });

            return app;
        }

        private async Task<BaasApp> CreatePbsApp(string name, string partitionKeyType, bool setupCollections = false)
        {
            _output.WriteLine($"Creating PBS app {name}...");

            var (app, mongoServiceId) = await CreateAppCore(name, new
            {
                sync = new
                {
                    state = "enabled",
                    database_name = GetSyncDatabaseName(name),
                    partition = new
                    {
                        key = "realm_id",
                        type = partitionKeyType,
                        permissions = new
                        {
                            read = true,
                            write = new BsonDocument
                            {
                                {
                                    "%%partition", new BsonDocument
                                    {
                                        { "$ne", "read-only" }
                                    }
                                }
                            },
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
                var (salesSchema, salesRules) = Schemas.Sales(partitionKeyType, Differentiator);
                var (usersSchema, usersRules) = Schemas.Users(partitionKeyType, Differentiator);
                var (foosSchema, foosRules) = Schemas.Foos(partitionKeyType, Differentiator);

                await CreateSchema(app, mongoServiceId, salesSchema, salesRules);
                await CreateSchema(app, mongoServiceId, usersSchema, usersRules);
                await CreateSchema(app, mongoServiceId, foosSchema, foosRules);

                await PatchAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/custom_user_data", new
                {
                    mongo_service_id = mongoServiceId,
                    enabled = true,
                    database_name = $"Schema_{Differentiator}",
                    collection_name = "users",
                    user_id_field = "user_id"
                });
            }

            return app;
        }

        private async Task<BaasApp> CreateFlxApp(string name)
        {
            _output.WriteLine($"Creating FLX app {name}...");

            var (app, mongoServiceId) = await CreateAppCore(name, new
            {
                flexible_sync = new
                {
                    state = "enabled",
                    database_name = GetSyncDatabaseName(name),
                    queryable_fields_names = new[] { "Int64Property", "GuidProperty", "DoubleProperty", "Int", "Guid", "Id", "PartitionLike" },
                }
            });

            await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services/{mongoServiceId}/default_rule", new
            {
                roles = new[]
                {
                    new
                    {
                        name = "all",
                        apply_when = new { },
                        read = true,
                        write = true,
                        insert = true,
                        delete = true,
                        document_filters = new
                        {
                            read = true,
                            write = true,
                        }
                    }
                }
            });

            await CreateFunction(app, "triggerClientResetOnSyncServer", TriggerClientResetOnSyncServerFuncSource, runAsSystem: true);

            return app;
        }

        public async Task SetAutomaticRecoveryEnabled(BaasApp app, bool enabled)
        {
            var services = await GetAsync<BsonArray>($"groups/{_groupId}/apps/{app}/services");
            var mongoServiceId = services!.Single(s => s.AsBsonDocument["name"] == "BackingDB")["_id"].AsString;
            var config = await GetAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services/{mongoServiceId}/config");

            var syncType = config!.Contains("flexible_sync") ? "flexible_sync" : "sync";
            config[syncType]["is_recovery_mode_disabled"] = !enabled;

            // An empty fragment with just the sync configuration is necessary,
            // as the "conf" document that we retrieve has a bunch of extra fields that we are supposed
            // to be use/return to the server when PATCH-ing
            var fragment = new BsonDocument
            {
                [syncType] = config[syncType]
            };

            await PatchAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services/{mongoServiceId}/config", fragment);
        }

        private async Task<(BaasApp App, string MongoServiceId)> CreateAppCore(string name, object syncConfig)
        {
            var doc = await PostAsync<BsonDocument>($"groups/{_groupId}/apps", new { name = $"{name}{_appSuffix}" });
            var appId = doc!["_id"].AsString;
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

            var mongoServiceId = await CreateMongodbService(app, syncConfig);

            await PutAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/sync/config", new
            {
                development_mode_enabled = true,
            });

            return (app, mongoServiceId);
        }

        private async Task EnableProvider(BaasApp app, string type, object? config = null, AuthMetadataField[]? metadataFields = null)
        {
            _output.WriteLine($"Enabling provider {type} for {app.Name}...");

            var url = $"groups/{_groupId}/apps/{app}/auth_providers";

            // Api key is slightly special, thus this annoying custom handling.
            if (type == "api-key")
            {
                var providers = await GetAsync<BsonArray>(url);
                var apiKeyProviderId = providers!.Select(p => p.AsBsonDocument)
                    .Single(p => p["type"] == "api-key")["_id"].AsString;

                await PutAsync<BsonDocument>($"{url}/{apiKeyProviderId}/enable", new { });
            }
            else
            {
                await PostAsync<BsonDocument>(url, new
                {
                    name = type,
                    type,
                    disabled = false,
                    config,
                    metadata_fields = metadataFields,
                });
            }
        }

        private async Task<string> CreateFunction(BaasApp app, string name, string source, bool runAsSystem = false)
        {
            _output.WriteLine($"Creating function {name} for {app.Name}...");

            var response = await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/functions", new
            {
                name,
                run_as_system = runAsSystem,
                can_evaluate = new { },
                @private = false,
                source
            });

            return response!["_id"].AsString;
        }

        private async Task<BaasApp[]> GetApps()
        {
            var response = await GetAsync<BsonArray>($"groups/{_groupId}/apps");
            return response!
                .Select(x => x.AsBsonDocument)
                .Where(doc => doc["name"].AsString.EndsWith(_appSuffix))
                .Select(doc =>
                {
                    var name = doc["name"].AsString;

                    var appName = name[..^_appSuffix.Length];
                    return new BaasApp(doc["_id"].AsString, doc["client_app_id"].AsString, appName);
                })
                .Where(a => a != null)
                .Select(a => a!)
                .ToArray();
        }

        private async Task<string> CreateService(BaasApp app, string name, string type, object config)
        {
            _output.WriteLine($"Creating service {name} for {app.Name}...");

            var response = await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services", new
            {
                name,
                type,
                config
            });

            return response!["_id"].AsString;
        }

        private async Task<string> CreateMongodbService(BaasApp app, object syncConfig)
        {
            var serviceName = _clusterName == null ? "mongodb" : "mongodb-atlas";
            object mongoConfig = _clusterName == null ? new { uri = "mongodb://localhost:26000" } : new { clusterName = _clusterName };

            var mongoServiceId = await CreateService(app, "BackingDB", serviceName, mongoConfig);

            // The cluster linking must be separated from enabling sync because Atlas
            // takes a few seconds to provision a user for BaaS, meaning enabling sync
            // will fail if we attempt to do it with the same request. It's nondeterministic
            // how long it'll take, so we must retry for a while.
            var attempt = 0;
            while (true)
            {
                try
                {
                    await PatchAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services/{mongoServiceId}/config", syncConfig);
                    break;
                }
                catch
                {
                    if (attempt++ < 120)
                    {
                        _output.WriteLine($"Failed to update service after {attempt * 5} seconds. Will keep retrying...");

                        await Task.Delay(5000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return mongoServiceId;
        }

        private async Task CreateSchema(BaasApp app, string mongoServiceId, object schema, object rule)
        {
            _output.WriteLine($"Creating schema for {app.Name}...");

            await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/schemas", schema);
            await PostAsync<BsonDocument>($"groups/{_groupId}/apps/{app}/services/{mongoServiceId}/rules", rule);
        }

        private async Task RefreshAccessTokenAsync()
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, new Uri("auth/session", UriKind.Relative));
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _refreshToken);

            var response = await _client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to refresh access token - {response.StatusCode}: {content}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = BsonSerializer.Deserialize<BsonDocument>(json);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doc["access_token"].AsString);
        }

        private Task<T?> PostAsync<T>(string relativePath, object obj) => SendAsync<T>(HttpMethod.Post, relativePath, obj);

        private Task<T?> GetAsync<T>(string relativePath) => SendAsync<T>(HttpMethod.Get, relativePath);

        private Task<T?> PutAsync<T>(string relativePath, object obj) => SendAsync<T>(HttpMethod.Put, relativePath, obj);

        private Task<T?> PatchAsync<T>(string relativePath, object obj) => SendAsync<T>(new HttpMethod("PATCH"), relativePath, obj);

        private async Task<T?> SendAsync<T>(HttpMethod method, string relativePath, object? payload = null)
        {
            using var message = new HttpRequestMessage(method, new Uri(relativePath, UriKind.Relative));
            if (payload != null)
            {
                message.Content = Utils.GetJsonContent(payload);
            }

            var response = await _client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized && _refreshToken != null)
                {
                    await RefreshAccessTokenAsync();
                    return await SendAsync<T>(method, relativePath, payload);
                }

                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"An error ({response.StatusCode}) occurred while executing {method} {relativePath}: {content}");
            }

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

        private class AuthMetadataField
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

            private static object Metadata(string differentiator, string collectionName) => new
            {
                database = $"Schema_{differentiator}",
                collection = collectionName,
                data_source = "BackingDB"
            };

            private static object GenericBaasRule(string differentiator, string collectionName) => new
            {
                collection = collectionName,
                database = $"Schema_{differentiator}",
                roles = new[] { _defaultRoles }
            };

            public static (object Schema, object Rules) Sales(string partitionKeyType, string differentiator) =>
            (new
            {
                metadata = Metadata(differentiator, "sales"),
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
                },
            },
            GenericBaasRule(differentiator, "sales"));

            public static (object Schema, object Rules) Users(string partitionKeyType, string differentiator) =>
            (new
            {
                metadata = Metadata(differentiator, "users"),
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
            },
            GenericBaasRule(differentiator, "users"));

            public static (object Schema, object Rules) Foos(string partitionKeyType, string differentiator) =>
            (new
            {
                metadata = Metadata(differentiator, "foos"),
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
            },
            GenericBaasRule(differentiator, "foos"));
        }
    }
}
