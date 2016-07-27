using System;
using Realms;

namespace PurePCLBuildableTest
{
    public static class TestBuildingRealmFromPCL
    {
        public static Realm MakeARealmWithPCL()
        {
            var conf = new RealmConfiguration("ThisIsDeclaredInPCL.realm");
            conf.ObjectClasses = new [] {typeof(ObjectInPCL)};  // only this class in the Realm
            Realm.DeleteRealm(conf);
            var ret = Realm.GetInstance(conf);
            ret.Write(() => ret.CreateObject<ObjectInPCL>());
            return ret;
        }
    }
}

