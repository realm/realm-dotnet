/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

/// <summary>
/// Exception thrown when a file exists with the same name as you to create a new one
/// </summary>
public class RealmFileExistsException : RealmFileAccessErrorException {

    internal RealmFileExistsException(String message) : base(message)
    {
    }
}

} // namespace Realms
