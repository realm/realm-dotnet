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
using System.Threading;
using System.Threading.Tasks;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    internal static class SessionTokenHelper
    {
        private static ConcurrentDictionary<string, Timer> _timers = new ConcurrentDictionary<string, Timer>();

        public static async Task RefreshAccessToken(Session session)
        {
            var user = session.User;
            try
            {
                var tokenData = await user.RefreshAccessToken(session.ServerUri.AbsolutePath);
                session.Handle.RefreshAccessToken(tokenData.AccessToken, tokenData.ServerUrl);
                ScheduleTokenRefresh(session.Path, tokenData.ExpiryDate);
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

        private static void ScheduleTokenRefresh(string path, DateTimeOffset expireDate)
        {
            var dueTime = expireDate.AddSeconds(-10) - DateTimeOffset.UtcNow;

            if (dueTime < TimeSpan.Zero)
            {
                OnTimerCallback(path);
            }

            _timers.AddOrUpdate(path, p =>
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
                if (_timers.TryRemove(path, out timer))
                {
                    timer.Dispose();
                }
            }
        }
    }
}