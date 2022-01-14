using System;
using Realms.Sync.Exceptions;

namespace Realms.Sync.ErrorHandling
{
    public class ClientResetHandlerBase
    {
        /// <summary>
        /// Callback triggered when there is a Client Reset error.
        /// </summary>
        /// <param name="session">
        /// The <see cref="Session"/> where the error happened on.
        /// </param>
        /// <param name="clientResetException">
        /// The specific <see cref="ClientResetException"/>.
        /// </param>
        public delegate void ClientResetCallback(Session session, ClientResetException clientResetException);
    }

}
