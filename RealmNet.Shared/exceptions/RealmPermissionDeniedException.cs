/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

/// <summary>
/// Exception when you can't open an existing realm file, or create a new one.
/// </summary>
/// <remarks>
/// May be seen in testing if you have crashed a unit test but an external test runner is still going.
/// </remarks>
public class RealmPermissionDeniedException : RealmFileAccessErrorException {

    internal RealmPermissionDeniedException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
