/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

namespace RealmNet
{
    public class RealmObjectAlreadyOwnedByRealmException : RealmException
    {
        public RealmObjectAlreadyOwnedByRealmException(string detailMessage) : base(detailMessage)
        {

        }
    }
}
