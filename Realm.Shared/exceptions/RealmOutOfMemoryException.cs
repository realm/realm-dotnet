/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

/// <summary>
/// Exception when Realm's run out of memory, shut down your application rather than trying to continue.
/// </summary>
public class RealmOutOfMemoryException :  RealmException {

    internal RealmOutOfMemoryException(String message) : base(message)
    {
    }
}

}  // namespace Realms
