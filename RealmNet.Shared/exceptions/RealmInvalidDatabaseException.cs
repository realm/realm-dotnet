/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

public class RealmInvalidDatabaseException : RealmFileAccessErrorException {

    public RealmInvalidDatabaseException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
