/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
namespace Realms
{
    internal class LinkListHandle:RealmHandle
    {
        internal LinkListHandle(RealmHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            NativeLinkList.destroy(handle);
        }
        
    }
}
