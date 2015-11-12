/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

/**
Class for reporting problems with Realm files.
You can catch any of the subclasses independently but any File-level
error which could be handled by an application descends from these.
*/
public class RealmOutsideTransactionException :  RealmException {
    public RealmOutsideTransactionException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
