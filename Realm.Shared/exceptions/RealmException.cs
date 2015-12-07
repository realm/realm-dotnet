/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

    /// <summary>
    /// Base for Realm specific exceptions. Use only for catching.
    /// </summary>
    public class RealmException :  Exception {

    internal RealmException(String detailMessage) : base(detailMessage)
    {
    }
}

}  // namespace Realms
