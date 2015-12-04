/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

namespace Realms
{
/// <summary>
/// Exception thrown when you're trying to use Attach to persist an object but it's in another Realm.
/// </summary>
public class RealmObjectOwnedByAnotherRealmException : RealmException
{
    internal RealmObjectOwnedByAnotherRealmException(string detailMessage) : base(detailMessage)
    {

    }
}

}
