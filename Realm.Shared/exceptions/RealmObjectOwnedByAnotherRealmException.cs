/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

namespace Realms
{
/// <summary>
/// Exception thrown when you're trying to use Manage but the object is already managed by a different Realm.
/// </summary>
public class RealmObjectOwnedByAnotherRealmException : RealmException
{
    internal RealmObjectOwnedByAnotherRealmException(string detailMessage) : base(detailMessage)
    {

    }
}

}
