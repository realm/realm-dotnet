/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

/// <summary>
/// Exception thrown when a file exists but doesn't appear to be a Realm database, may indicate corruption.
/// </summary>
public class RealmInvalidDatabaseException : RealmFileAccessErrorException {

    internal RealmInvalidDatabaseException(String message) : base(message)
    {
    }
}

} // namespace Realms
