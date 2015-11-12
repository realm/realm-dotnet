/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet {

 /**
 * RealmException is Realm specific exceptions.
 */
public class RealmException :  Exception {

    public RealmException(String detailMessage) : base(detailMessage)
    {
    }
}

}  // namespace RealmNet
