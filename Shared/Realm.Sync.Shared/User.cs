////////////////////////////////////////////////////////////////////////////
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
using System.IO;
using System.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Realms.Sync
{
    [Flags]
    public enum LoginMode
    {
        UseExistingAccount = 1,
        CreateAccount = 2
    }

    public enum UserState
    {
        LoggedOut,
        Active,
        Error
    }

    public class User
    {
        private static readonly MediaTypeHeaderValue _applicationJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private static readonly MediaTypeHeaderValue _applicationProblemJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/problem+json; charset=utf-8");

        public string RefreshToken => Handle.RefreshToken;

        public string Identity => Handle.Identity;

        public Uri ServerUri
        {
            get
            {
                var serverUrl = Handle.ServerUrl;
                if (string.IsNullOrEmpty(serverUrl))
                {
                    return null;
                }

                return new Uri(serverUrl);
            }
        }

        public UserState State => Handle.State;

        internal readonly SyncUserHandle Handle;

        internal User(SyncUserHandle handle)
        {
            Handle = handle;
        }

        public static async Task<User> LoginAsync(Credentials credentials, Uri serverUrl, LoginMode loginMode = LoginMode.UseExistingAccount)
        {
            if (credentials.IdentityProvider == Credentials.Providers.AccessToken)
            {
                var identity = (string)credentials.UserInfo[Credentials.Keys.Identity];
                var isAdmin = (bool)credentials.UserInfo[Credentials.Keys.IsAdmin];
                return new User(SyncUserHandle.GetSyncUser(identity, credentials.Token, serverUrl?.AbsoluteUri, isAdmin));
            }

            var result = await MakeAuthRequestAsync(serverUrl, credentials.ToJson(), TimeSpan.FromSeconds(30)).ConfigureAwait(continueOnCapturedContext: false);
            var refresh_token = result["refresh_token"];

            return new User(SyncUserHandle.GetSyncUser(refresh_token["token_data"]["identity"], refresh_token["token"], serverUrl.AbsoluteUri, false));
        }

        public void LogOut()
        {
            Handle.LogOut();
        }

        internal async Task<string> RefreshAccessToken(string path)
        {
            var json = new JsonObject
            {
                ["data"] = RefreshToken,
                ["path"] = path,
                ["provider"] = "realm"
            };

            var result = await MakeAuthRequestAsync(ServerUri, json, TimeSpan.FromSeconds(30)).ConfigureAwait(continueOnCapturedContext: false);
            return result["access_token"]["token"];
        }

        private static async Task<JsonValue> MakeAuthRequestAsync(Uri serverUri, JsonObject body, TimeSpan timeout)
        {
            body["app_id"] = string.Empty; // FIXME

            string requestBody;
            using (var writer = new StringWriter())
            {
                body.Save(writer);
                requestBody = writer.ToString();
            }

            using (var client = new HttpClient { Timeout = timeout })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(serverUri, "auth"));
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                request.Headers.Accept.ParseAdd(_applicationJsonUtf8MediaType.MediaType);
                request.Headers.Accept.ParseAdd(_applicationProblemJsonUtf8MediaType.MediaType);

                var response = await client.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
                if (response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentType.Equals(_applicationJsonUtf8MediaType))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                    {
                        return JsonValue.Load(stream);
                    }
                }
                else if (response.Content.Headers.ContentType.Equals(_applicationProblemJsonUtf8MediaType))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                    {
                        var problem = JsonValue.Load(stream);
                        throw new Exception("AuthError"); // FIXME: parse the error details and throw a more specific exception
                    }
                }
                else
                {
                    throw new Exception($"Unexpected response {response.StatusCode} {response.ReasonPhrase} ({response.Content?.Headers?.ContentType})");
                }
            }
        }
    }
}