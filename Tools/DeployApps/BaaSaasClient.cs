////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Baas
{
    public class BaaSaasClient
    {
        private const string _baseUrl = "https://us-east-1.aws.data.mongodb-api.com/app/baas-container-service-autzb/endpoint/";
        private readonly HttpClient _client;

        public BaaSaasClient(string apiKey)
        {
            _client = new();
            _client.BaseAddress = new Uri(_baseUrl);
            _client.DefaultRequestHeaders.TryAddWithoutValidation("apiKey", apiKey);
        }

        public async Task<string> GetOrDeployContainer(string differentiator, TextWriter output)
        {
            output.WriteLine("Looking for existing containers on BaaSaas.");
            var containers = await GetContainers();

            if (containers?.Length > 0)
            {
                var userId = await GetCurrentUserId();
                var existingContainer = containers
                    .FirstOrDefault(c => c.CreatorId == userId && c.Tags.Any(t => t.Key == "DIFFERENTIATOR" && t.Value == differentiator));

                if (existingContainer is not null)
                {
                    output.WriteLine($"Container with id {existingContainer.ContainerId} found.");

                    if (!existingContainer.IsRunning)
                    {
                        output.WriteLine($"Waiting for container with id {existingContainer.ContainerId} to be running.");
                        await WaitForContainer(existingContainer.ContainerId);
                    }

                    return existingContainer.HttpUrl;
                }
            }

            output.WriteLine($"No container found, starting a new one.");
            var containerId = await StartContainer(differentiator);

            output.WriteLine($"Container with id {containerId} started, waiting for it to be running.");
            var container = await WaitForContainer(containerId);

            return container.HttpUrl;
        }

        private Task<ContainerInfo[]?> GetContainers()
        {
            return CallEndpointAsync<ContainerInfo[]>(HttpMethod.Get, "listContainers");
        }

        private Task StopContainer(string id)
        {
            return CallEndpointAsync<BsonDocument>(HttpMethod.Post, $"stopContainer?id={id}");
        }

        private async Task<string?> GetCurrentUserId()
        {
            return (await CallEndpointAsync<BsonDocument>(HttpMethod.Get, "userinfo"))!["id"].AsString;
        }

        private async Task<string> StartContainer(string differentiator)
        {
            var response = await CallEndpointAsync<BsonDocument>(HttpMethod.Post, "startContainer", new[]
            {
                new
                {
                    key = "DIFFERENTIATOR",
                    value = differentiator,
                }
            });

            return response?["id"].AsString!;
        }

        private async Task<ContainerInfo> WaitForContainer(string containerId, int maxRetries = 100)
        {
            while (maxRetries > 0)
            {
                maxRetries -= 1;

                var containers = await GetContainers();
                var container = containers.FirstOrDefault(c => c.ContainerId == containerId);

                if (container?.IsRunning == true)
                {
                    return container;
                }

                await Task.Delay(2000);
            }

            throw new Exception($"Container with id={containerId} was not found or ready after {maxRetries} retrues");
        }

        private async Task<T?> CallEndpointAsync<T>(HttpMethod method, string relativePath, object? payload = null)
        {
            using var message = new HttpRequestMessage(method, new Uri(relativePath, UriKind.Relative));

            if (payload is not null)
            {
                message.Content = Utils.GetJsonContent(payload);
            }

            var response = await _client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(json))
            {
                return BsonSerializer.Deserialize<T>(json);
            }

            return default;
        }

        [BsonIgnoreExtraElements]
        public class ContainerInfo
        {
            [BsonElement("id")]
            public string ContainerId { get; set; } = null!;

            [BsonElement("httpUrl")]
            public string HttpUrl { get; set; } = null!;

            [BsonElement("lastStatus")]
            public string LastStatus { get; set; } = null!;

            [BsonElement("tags")]
            public List<Tag> Tags { get; set; } = null!;

            [BsonElement("creatorId")]
            public string CreatorId { get; set; } = null!;

            public bool IsRunning => LastStatus == "RUNNING";
        }

        public class Tag
        {
            [BsonElement("key")]
            public string Key { get; set; } = null!;

            [BsonElement("value")]
            public string Value { get; set; } = null!;
        }
    }
}
