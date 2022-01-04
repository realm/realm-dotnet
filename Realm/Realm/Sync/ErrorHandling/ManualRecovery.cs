using System;

namespace Realms.Sync.ErrorHandling
{
    public class ManualRecovery : ClientResetHandlerBase
    {
        /// <summary>
        /// Callback that indicates a Client Reset has happened.
        /// This should be handled as quickly as possible as any further changes to the Realm will not be synchronized with the server and must be moved manually from the backup Realm to the new one.
        /// </summary>
        public ErrorHandlingCallback OnClientReset { get; set; }
    }
}
