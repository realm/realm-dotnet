/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

///<summary>
/// Base for catching exceptions with Realm files, typically problems from which an app would recover</summary>
///<remarks>
///You can catch any of the subclasses independently but any File-level
///error which could be handled by an application descends from these.
///</remarks>
public class RealmFileAccessErrorException :  RealmException {
    internal RealmFileAccessErrorException(String message) : base(message)
    {
    }
}

} // namespace RealmNet
