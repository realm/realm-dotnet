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
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    internal static class AuthenticationHelper
    {
        private static readonly string AppId = string.Empty; // FIXME
        private static readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() => new HttpClient { Timeout = TimeSpan.FromSeconds(30) });

        private static readonly MediaTypeHeaderValue _applicationJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private static readonly MediaTypeHeaderValue _applicationProblemJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/problem+json; charset=utf-8");

        // Returns a Tuple<userId, refreshToken>
        public static async Task<UserLoginData> LoginAsync(Credentials credentials, Uri serverUrl)
        {
            var body = credentials.ToDictionary();
            body["app_id"] = AppId;
            var result = await MakeAuthRequestAsync(HttpMethod.Post, new Uri(serverUrl, "auth"), body)
                                    .ConfigureAwait(continueOnCapturedContext: false);
            var refreshToken = result["refresh_token"];
            return new UserLoginData
            {
                RefreshToken = refreshToken["token"].Value<string>(),
                UserId = refreshToken["token_data"]["identity"].Value<string>(),
            };
        }

        // Due to https://bugzilla.xamarin.com/show_bug.cgi?id=20082 we can't use dynamic deserialization.
        public static async Task<JObject> MakeAuthRequestAsync(HttpMethod method, Uri uri, IDictionary<string, object> body = null, string authHeader = null)
        {
            HttpResponseMessage response;
            using (var request = new HttpRequestMessage(method, uri))
            {
                if (body != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                }

                request.Headers.Accept.ParseAdd(_applicationJsonUtf8MediaType.MediaType);
                request.Headers.Accept.ParseAdd(_applicationProblemJsonUtf8MediaType.MediaType);

                if (!string.IsNullOrEmpty(authHeader))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);
                }

                response = await _client.Value.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
            }

            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.Equals(_applicationJsonUtf8MediaType))
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                return JObject.Parse(json);
            }

            var errorJson = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            Exception ex;
            string helpLink = null;
            string errorMessage;
            ErrorCode errorCode;
            try
            {
                var problem = JObject.Parse(errorJson);
                errorCode = ErrorCodeHelper.GetErrorCode(problem["code"].Value<int>()) ?? ErrorCode.Unknown;
                errorMessage = problem["title"].Value<string>();
                helpLink = problem["type"].Value<string>();
            }
            catch
            {
                errorCode = ErrorCode.Unknown;
                errorMessage = "An HTTP exception has occurred.";
            }

            ex = new HttpException(response.StatusCode, response.ReasonPhrase, errorJson, errorMessage, errorCode)
            {
                HelpLink = helpLink
            };

            throw ex;
        }

        private class TokenRefreshData
        {
            public string UserId { get; set; }

            public string RealmPath { get; set; }

            public Uri ServerUrl { get; set; }
        }

        public class UserLoginData
        {
            public string UserId { get; set; }

            public string RefreshToken { get; set; }
        }
    }
}