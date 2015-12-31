/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

/// <summary>
/// Exception when you can't open an existing realm file because the format differs from your current class declarations.
/// </summary>
/// <remarks>
/// Typically triggered when you open the same Realm name, or use GetInstance() with no name, 
    /// and don't delete old files. <seealso href="https://realm.io/docs/xamarin/latest/#migrations">Read more at Migrations.</seealso>
/// </remarks>
public class RealmFormatUpgradeRequiredException : RealmFileAccessErrorException {

    internal RealmFormatUpgradeRequiredException(String message) : base(message)
    {
    }
}

} // namespace Realms
