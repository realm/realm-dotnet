/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

namespace RealmNet
{
    public class RealmObjectOwnedByAnotherRealmException : RealmException
    {
        public RealmObjectOwnedByAnotherRealmException(string detailMessage) : base(detailMessage)
        {

        }
    }
}
