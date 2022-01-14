using System;
using Realms.Sync.Exceptions;

namespace Realms.Sync.ErrorHandling
{
    public class SyncErrorHandler
    {
        /// <summary>
        /// Triggered when an error occurs in a session.
        /// Until full deprecation, this callback still calls into <see cref="Session.Error"/> for backward compatibility.
        /// </summary>
        public delegate void SessionErrorCallback(Session session, SessionException error);

        public SessionErrorCallback OnError { get; set; }
    }
}
