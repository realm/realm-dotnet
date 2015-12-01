/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

/// <summary>
/// Exception thrown when a file doesn't exists when trying to open without a create option.
/// </summary>
public class RealmFileNotFoundException : RealmFileAccessErrorException {

    internal RealmFileNotFoundException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
