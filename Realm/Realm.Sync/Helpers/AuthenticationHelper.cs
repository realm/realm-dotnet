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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    internal static class AuthenticationHelper
    {
        private const int ErrorContentTruncationLimit = 256 * 1024;
        private static readonly string AppId = string.Empty; // FIXME
        private static readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() => new HttpClient { Timeout = TimeSpan.FromSeconds(30) });

        private static readonly ConcurrentDictionary<string, Timer> _tokenRefreshTimers = new ConcurrentDictionary<string, Timer>();
        private static readonly DateTimeOffset _date_1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly MediaTypeHeaderValue _applicationJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private static readonly MediaTypeHeaderValue _applicationProblemJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/problem+json; charset=utf-8");

        private static readonly HashSet<HttpStatusCode> _connectivityStatusCodes = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.NotFound,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.RequestTimeout,
        };

        public static async Task RefreshAccessTokenAsync(Session session, bool reportErrors = true)
        {
            var user = session.User;
            if (user == null)
            {
                return;
            }

            try
            {
                var json = new Dictionary<string, object>
                {
                    ["data"] = user.RefreshToken,
                    ["path"] = session.ServerUri.AbsolutePath,
                    ["provider"] = "realm",
                    ["app_id"] = AppId
                };

                var result = await MakeAuthRequestAsync(HttpMethod.Post, new Uri(user.ServerUri, "auth"), json)
                                        .ConfigureAwait(continueOnCapturedContext: false);

                var accessToken = result["access_token"];

                session.Handle.RefreshAccessToken(accessToken["token"].Value<string>(), accessToken["token_data"]["path"].Value<string>());
                ScheduleTokenRefresh(user.Identity, user.ServerUri, session.Path, _date_1970.AddSeconds(accessToken["token_data"]["expires"].Value<long>()));
            }
            catch (HttpException ex) when (_connectivityStatusCodes.Contains(ex.StatusCode))
            {
                // 30 seconds is an arbitrarily chosen value, consider rationalizing it.
                ScheduleTokenRefresh(user.Identity, user.ServerUri, session.Path, DateTimeOffset.UtcNow.AddSeconds(30));
            }
            catch (Exception ex)
            {
                if (reportErrors)
                {
                    var sessionException = new SessionException("An error has occurred while refreshing the access token.",
                                                                ErrorCode.BadUserAuthentication,
                                                                ex);

                    Session.RaiseError(session, sessionException);
                }
            }
            finally
            {
                // session.User creates a new user each time, so it's safe to dispose the handle here.
                // It won't actually corrupt the state of the session.
                user.Handle.Dispose();
            }
        }

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
                IsAdmin = refreshToken["token_data"]["is_admin"].Value<bool>()
            };
        }

        public static Task ChangePasswordAsync(User user, string password, string otherUserId = null)
        {
            var json = new Dictionary<string, object>
            {
                ["data"] = new Dictionary<string, object>
                {
                    ["new_password"] = password
                }
            };

            if (otherUserId != null)
            {
                json["user_id"] = otherUserId;
            }

            return MakeAuthRequestAsync(HttpMethod.Put, new Uri(user.ServerUri, "auth/password"), json, user.RefreshToken);
        }

        public static async Task<UserInfo> RetrieveInfoForUserAsync(User user, string provider, string providerId)
        {
            var uri = new Uri(user.ServerUri, $"/auth/users/{provider}/{providerId}");
            try
            {
                var response = await MakeAuthRequestAsync(HttpMethod.Get, uri, authHeader: user.RefreshToken)
                                        .ConfigureAwait(continueOnCapturedContext: false);

                var accounts = response["accounts"].Children<JObject>()
                                                   .Select(j => new AccountInfo
                                                   {
                                                       Provider = j["provider"].Value<string>(),
                                                       ProviderUserIdentity = j["provider_id"].Value<string>()
                                                   })
                                                   .ToArray();

                var metadata = response["metadata"].Children<JObject>()
                                                   .ToDictionary(j => j["key"].Value<string>(), j => j["value"].Value<string>());

                return new UserInfo
                {
                    Identity = response["user_id"].Value<string>(),
                    IsAdmin = response["is_admin"].Value<bool>(),
                    Accounts = accounts,
                    Metadata = metadata
                };
            }
            catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public static async Task LogOutAsync(Uri serverUri, string refreshToken)
        {
            var uri = new Uri(serverUri, "/auth/revoke");
            var body = new Dictionary<string, object>
            {
                ["token"] = refreshToken
            };

            await MakeAuthRequestAsync(HttpMethod.Post, uri, body, refreshToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        public static Task UpdateAccountAsync(Uri serverUri, string action, string email = null, IDictionary<string, string> data = null)
        {
            data = data ?? new Dictionary<string, string>();
            data["action"] = action;

            var body = new Dictionary<string, object>
            {
                ["data"] = data
            };

            if (!string.IsNullOrEmpty(email))
            {
                body["provider_id"] = email;
            }

            var updateUri = new Uri(serverUri, "auth/password/updateAccount");

            return MakeAuthRequestAsync(HttpMethod.Post, updateUri, body);
        }

        private static void ScheduleTokenRefresh(string userId, Uri authServerUrl, string path, DateTimeOffset expireDate)
        {
            var dueTime = expireDate.AddSeconds(-10) - DateTimeOffset.UtcNow;
            var timerState = new TokenRefreshData
            {
                RealmPath = path,
                UserId = userId,
                ServerUrl = authServerUrl
            };

            if (dueTime < TimeSpan.Zero)
            {
                OnTimerCallback(timerState);
            }

            _tokenRefreshTimers.AddOrUpdate(
                path,
                p => new Timer(OnTimerCallback, timerState, dueTime, TimeSpan.FromMilliseconds(-1)),
                (p, old) =>
                {
                    old.Dispose();
                    return new Timer(OnTimerCallback, timerState, dueTime, TimeSpan.FromMilliseconds(-1));
                });
        }

        private static void OnTimerCallback(object state)
        {
            var data = (TokenRefreshData)state;

            try
            {
                var user = User.GetLoggedInUser(data.UserId, data.ServerUrl);
                if (user != null &&
                    user.Handle.TryGetSession(data.RealmPath, out var sessionHandle))
                {
                    var session = new Session(sessionHandle);
                    RefreshAccessTokenAsync(session, reportErrors: false).ContinueWith(_ =>
                    {
                        user.Handle.Close();
                        session.CloseHandle();
                    });
                }
            }
            catch
            {
            }
            finally
            {
                if (_tokenRefreshTimers.TryRemove(data.RealmPath, out var timer))
                {
                    timer.Dispose();
                }
            }
        }

        // Due to https://bugzilla.xamarin.com/show_bug.cgi?id=20082 we can't use dynamic deserialization.
        public static async Task<JObject> MakeAuthRequestAsync(HttpMethod method, Uri uri, IDictionary<string, object> body = null, string authHeader = null)
        {
            var request = new HttpRequestMessage(method, uri);
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

            var response = await _client.Value.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.Equals(_applicationJsonUtf8MediaType))
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                return JObject.Parse(json);
            }

            var errorJson = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            Exception ex;
            try
            {
                var problem = JObject.Parse(errorJson);

                var code = ErrorCodeHelper.GetErrorCode(problem["code"].Value<int>()) ?? ErrorCode.Unknown;

                ex = new AuthenticationException(code, response.StatusCode, response.ReasonPhrase, errorJson, problem["title"].Value<string>())
                {
                    HelpLink = problem["type"].Value<string>()
                };
            }
            catch
            {
                ex = new HttpException(response.StatusCode, response.ReasonPhrase, errorJson);
            }

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

            public bool IsAdmin { get; set; }
        }
    }
}