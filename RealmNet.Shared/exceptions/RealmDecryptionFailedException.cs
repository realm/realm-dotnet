/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

public class RealmDecryptionFailedException : RealmFileAccessErrorException {

    public RealmDecryptionFailedException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
