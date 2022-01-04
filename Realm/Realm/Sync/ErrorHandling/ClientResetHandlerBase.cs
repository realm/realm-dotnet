using System;
using Realms.Sync.Exceptions;

namespace Realms.Sync.ErrorHandling
{
    public class ClientResetHandlerBase
    {
        /// <summary>
        /// Callback triggered when there is a Client Reset error.
        /// </summary>
        /// <param name="realm">
        /// The <see cref="Realm"/> under synchronization where the error happened on.
        /// </param>
        /// <param name="clientResetException">
        /// The specific <see cref="ClientResetException"/>.
        /// </param>
        public delegate void ClientResetCallback(Realm realm, ClientResetException clientResetException);
    }

}
