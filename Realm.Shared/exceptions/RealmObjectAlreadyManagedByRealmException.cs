/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

namespace Realms
{
/// <summary>
/// Exception thrown when you're trying to use Manage but the object has already been added to this Realm.
/// </summary>
public class RealmObjectAlreadyManagedByRealmException : RealmException
{
    internal RealmObjectAlreadyManagedByRealmException(string detailMessage) : base(detailMessage)
    {

    }
}

}
