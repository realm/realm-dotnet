using System;

namespace Realms.Sync.ErrorHandling
{
    public class DiscardLocalResetHandler : ClientResetHandlerBase
    {
        /// <summary>
        /// Callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <param name="before">
        /// Read-only backup <see cref="Realm"/> in its state before the reset.
        /// </param>
        /// <param name="after">
        /// <see cref="Realm"/> state to become after the reset.
        /// </param>
        public delegate void BeforeResetCallback(Realm before, Realm after);

        /// <summary>
        /// Callback that indicates a Client Reset just happened.
        /// </summary>
        /// <param name="realm">
        /// The <see cref="Realm"/> after the reset.
        /// </param>
        public delegate void AfterResetCallback(Realm realm);

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset is about to happen.
        /// Among other things, you can use this call to temporarily store the before Realm as a backup and in the <see cref="OnAfterReset"/> callback merge the changes, if necessary.
        /// </summary>
        public BeforeResetCallback OnBeforeReset { get; set; }

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset just happened. Special custom actions can be taken at this point like merging local changes if the "before" realm was stored during <see cref="BeforeResetCallback">.
        /// </summary>
        public AfterResetCallback OnAfterReset { get; set; }

        /// <summary>
        /// Gets or sets the callback triggered when an error has occurred that makes the operation unable to complete, for example in the case of a destructive schema change.
        /// </summary>
        public ClientResetCallback ManualResetFallback { get; set; }
    }
}
