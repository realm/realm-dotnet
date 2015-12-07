/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

/// <summary>
/// Exception when you try to Add, update or Remove a persisted object without a write transaction active.
/// </summary>
public class RealmOutsideTransactionException :  RealmException {

    internal RealmOutsideTransactionException(String message) : base(message)
    {
    }
}

} // namespace Realms
