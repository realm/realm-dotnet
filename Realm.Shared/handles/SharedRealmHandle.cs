/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
namespace Realms
{
    internal class SharedRealmHandle : RealmHandle
    {
        [Preserve("Constructor used by marshaling, cannot be removed by linker")]
        public SharedRealmHandle()
        {
        }

        protected override void Unbind()
        {
            NativeSharedRealm.destroy(handle);
        }
    }
}
