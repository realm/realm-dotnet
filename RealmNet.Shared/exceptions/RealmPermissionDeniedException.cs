/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

public class RealmPermissionDeniedException : RealmFileAccessErrorException {

    public RealmPermissionDeniedException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
