using System;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;

class SyncedRealm
{
    public static Realm realm;

    public static async Task OpenRealm()
    {
        // You can find the app id in your MongoDB Realm app in Atlas.
        var app = App.Create("3d_chess-sjdkk");
        // For this example we can just randomly create a new user when we start
        // the game since all users will access the same game.
        var email = Guid.NewGuid().ToString();
        var password = "password";
        await app.EmailPasswordAuth.RegisterUserAsync(email, password);
        var user = await app.LogInAsync(Credentials.EmailPassword(email, password));
        var config = new SyncConfiguration("3d_chess_partition_key", user);
        realm = await Realm.GetInstanceAsync(config);
    }
}
