using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using UnityEngine;

class SyncedRealm
{
    public static Realm realm;

    public static async Task OpenRealm()
    {
        Debug.Log("SyncedRealm OpenRealm");
        var app = App.Create("3d_chess-sjdkk");
        Debug.Log("A");
        var user = await app.LogInAsync(Credentials.Anonymous());
        Debug.Log("B");
        var config = new SyncConfiguration("3d_chess_partition_key", user);
        Debug.Log("C");
        realm = await Realm.GetInstanceAsync(config);
        Debug.Log("D");
        //Thread.Sleep(5000);
        Debug.Log(realm.Config.DatabasePath);
    }
}


//class SyncedRealm
//{
//    public static Realm realm;

//    public static async void OpenRealm()
//    {
//        var app = App.Create("mangodb-vbrvs");
//        var user = await app.LogInAsync(Credentials.Anonymous());
//        var config = new SyncConfiguration("mangoDB", user);
//        realm = await Realm.GetInstanceAsync(config);
//        Debug.Log(realm.Config.DatabasePath);
//    }
//}
