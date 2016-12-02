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
using System.Collections.Generic;
using System.Threading;

namespace Realms.Sync
{
    public enum SessionState
    {
        WaitingForAccessToken = 0,
        Active,
        Dying,
        Inactive,
        Error,
    }

    /// <summary>
    /// An object encapsulating a Realm Object Server session. Sessions represent the communication between the client (and a local Realm file on disk), and the server (and a remote Realm at a given URL stored on a Realm Object Server).
    /// Sessions are always created by the SDK and vended out through various APIs. The lifespans of sessions associated with Realms are managed automatically.
    /// </summary>
    public class Session
    {
        private static readonly ConcurrentDictionary<SessionHandle, Session> _sessions = new ConcurrentDictionary<SessionHandle, Session>(new SessionHandle.Comparer());

        private List<ErrorEventArgs> _aggregatedErrors = new List<ErrorEventArgs>();

        private event EventHandler<ErrorEventArgs> _error;

        public event EventHandler<ErrorEventArgs> Error
        {
            add
            {
                Interlocked.Exchange(ref _aggregatedErrors, null)?.ForEach(error => value(this, error));
                _error += value;
            }

            remove
            {
                _error -= value;
            }
        }

        /// <summary>
        /// Gets the <see cref="SyncConfiguration"/> that is responsible for controlling the session.
        /// </summary>
        public SyncConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the <see cref="Uri"/> describing the remote Realm which this session connects to and synchronizes changes with.
        /// </summary>
        public Uri ServerUri => new Uri(Handle.ServerUri);

        /// <summary>
        /// Gets the session’s current state.
        /// </summary>
        public SessionState State => Handle.State;

        /// <summary>
        /// Gets the <see cref="User"/> defined by the <see cref="SyncConfiguration"/> that is used to connect to the Realm Object Server.
        /// </summary>
        public User User => new User(Handle.User);

        internal readonly SessionHandle Handle;

        private Session(SessionHandle handle)
        {
            Handle = handle;
        }

        internal void RaiseError(Exception error)
        {
            var args = new ErrorEventArgs(error);
            _error?.Invoke(this, args);
            _aggregatedErrors?.Add(args);
        }

        internal static Session SessionForRealm(Realm realm)
        {
            System.Diagnostics.Debug.Assert(realm.Config is SyncConfiguration, "Realm must be opened with a SyncConfiguration");

            return SessionForPointer(SessionHandle.SessionForRealm(realm.SharedRealmHandle));
        }

        internal static Session SessionForPointer(IntPtr sessionPtr)
        {
            var tempHandle = new SessionHandle();
            tempHandle.SetHandle(sessionPtr);

            var shouldDispose = true;
            var session = _sessions.GetOrAdd(tempHandle, handle =>
            {
                shouldDispose = false;
                return new Session(handle);
            });

            if (shouldDispose)
            {
                tempHandle.Dispose();
            }

            return session;
        }
    }
}
