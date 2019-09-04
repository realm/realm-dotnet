using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// Enum describing what should happen in case of a Client Resync.
    /// </summary>
    /// <remarks>
    /// A Client Resync is triggered if the device and server cannot agree on a common shared history
    /// for the Realm file, thus making it impossible for the device to upload or receive any changes.
    /// This can happen if the server is rolled back or restored from backup.
    /// <br/>
    /// IMPORTANT: Just having the device offline will not trigger a Client Resync.
    /// </remarks>
    public enum ClientResyncMode : byte
    {
        /// <summary>
        /// Realm will compare the local Realm with the Realm on the server and automatically transfer
        /// any changes from the local Realm that makes sense to the Realm provided by the server.
        /// <br/>
        /// This is the default mode for fully synchronized Realms. It is not yet supported by
        /// Query-based Realms.
        /// </summary>
        RecoverLocalRealm = 0,

        /// <summary>
        /// The local Realm will be discarded and replaced with the server side Realm.
        /// All local changes will be lost.
        /// <br/>
        /// This mode is not yet supported by Query-based Realms.
        /// </summary>
        DiscardLocalRealm = 1,

        /// <summary>
        /// A manual Client Resync is also known as a Client Reset.
        /// <br/>
        /// A <see cref="ClientResetException"/> will be sent to <see cref="Session.Error"/>,
        /// triggering a Client Reset. Doing this provides a handle to both the old and new Realm file, enabling
        /// full control over which changes to move, if any.
        /// <br/>
        /// This is the only supported mode for Query-based Realms.
        /// </summary>
        Manual = 2,
    }
}
