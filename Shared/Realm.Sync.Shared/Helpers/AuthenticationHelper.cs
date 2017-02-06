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
using System.Collections.Concurrent;
using System.IO;
using System.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    internal static class AuthenticationHelper
    {
        private const int ErrorContentTruncationLimit = 256 * 1024;
        
        private static readonly ConcurrentDictionary<string, Timer> _tokenRefreshTimers = new ConcurrentDictionary<string, Timer>();
        private static readonly DateTimeOffset _date_1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly MediaTypeHeaderValue _applicationJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private static readonly MediaTypeHeaderValue _applicationProblemJsonUtf8MediaType = MediaTypeHeaderValue.Parse("application/problem+json; charset=utf-8");

        public static async Task RefreshAccessToken(Session session)
        {
            var user = session.User;
            try
            {
                var json = new JsonObject
                {
                    ["data"] = user.RefreshToken,
                    ["path"] = session.ServerUri.AbsolutePath,
                    ["provider"] = "realm"
                };

                var result = await MakeAuthRequestAsync(user.ServerUri, json, TimeSpan.FromSeconds(30)).ConfigureAwait(continueOnCapturedContext: false);
                var access_token = result["access_token"];

                session.Handle.RefreshAccessToken(access_token["token"], access_token["token_data"]["path"]);
                ScheduleTokenRefresh(session.Path, _date_1970.AddSeconds(access_token["token_data"]["expires"]));
            }
            catch (Exception ex)
            {
                // TODO: if http exception - retry instead of reporting it.

                var sessionException = new SessionException("An error has occurred while refreshing the access token.",
                                                            ErrorCode.BadUserAuthentication,
                                                            ex);

                Session.RaiseError(session, sessionException);
            }
            finally
            {
                user.Handle.Dispose();
            }
        }

        // Returns a Tuple<userId, refreshToken>
        public static async Task<Tuple<string, string>> Login(Credentials credentials, Uri serverUrl)
        {
            var result = await MakeAuthRequestAsync(serverUrl, credentials.ToJson(), TimeSpan.FromSeconds(30)).ConfigureAwait(continueOnCapturedContext: false);
            var refresh_token = result["refresh_token"];
            return Tuple.Create((string)refresh_token["token_data"]["identity"], (string)refresh_token["token"]);
        }

        private static void ScheduleTokenRefresh(string path, DateTimeOffset expireDate)
        {
            var dueTime = expireDate.AddSeconds(-10) - DateTimeOffset.UtcNow;

            if (dueTime < TimeSpan.Zero)
            {
                OnTimerCallback(path);
            }

            _tokenRefreshTimers.AddOrUpdate(path, p =>
            {
                return new Timer(OnTimerCallback, path, dueTime, TimeSpan.FromMilliseconds(-1));
            }, (p, old) =>
            {
                old.Dispose();
                return new Timer(OnTimerCallback, path, dueTime, TimeSpan.FromMilliseconds(-1));
            });
        }

        private static void OnTimerCallback(object state)
        {
            var path = (string)state;

            try
            {
                var session = Session.Create(path);
                if (session != null)
                {
                    RefreshAccessToken(session);
                }
            }
            catch
            {
            }
            finally
            {
                Timer timer;
                if (_tokenRefreshTimers.TryRemove(path, out timer))
                {
                    timer.Dispose();
                }
            }
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
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Accept.ParseAdd(_applicationJsonUtf8MediaType.MediaType);
                request.Headers.Accept.ParseAdd(_applicationProblemJsonUtf8MediaType.MediaType);

                var response = await client.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
                if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.Equals(_applicationJsonUtf8MediaType))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                    {
                        return JsonValue.Load(stream);
                    }
                }

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (response.Content.Headers.ContentType.Equals(_applicationProblemJsonUtf8MediaType))
                    {
                        var problem = JsonValue.Load(stream);

                        var code = ErrorCodeHelper.GetErrorCode((int)problem["code"]) ?? ErrorCode.Unknown;

                        throw new AuthenticationException(code, response.StatusCode, response.ReasonPhrase, problem.ToString(), (string)problem["title"]);
                    }

                    var content = ReadContent(stream, ErrorContentTruncationLimit, $"Response too long. Truncated to first {ErrorContentTruncationLimit} characters:{Environment.NewLine}");
                    throw new HttpException(response.StatusCode, response.ReasonPhrase, content);
                }
            }
        }

        private static string ReadContent(Stream stream, int maxNumberOfCharacters, string prefixIfExceeded)
        {
            using (var sr = new StreamReader(stream))
            {
                var sb = new StringBuilder();

                int current;
                while ((current = sr.Read()) > 0)
                {
                    sb.Append((char)current);
                    if (sb.Length > maxNumberOfCharacters - 1)
                    {
                        sb.Insert(0, prefixIfExceeded);
                        break;
                    }
                }

                return sb.ToString();
            }
        }
    }
}