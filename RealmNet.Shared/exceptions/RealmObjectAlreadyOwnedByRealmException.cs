/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

namespace RealmNet
{
/// <summary>
/// Exception thrown when you're trying to use Attach to persist an object but it's already been added to this Realm.
/// </summary>
public class RealmObjectAlreadyOwnedByRealmException : RealmException
{
    internal RealmObjectAlreadyOwnedByRealmException(string detailMessage) : base(detailMessage)
    {

    }
}

}
