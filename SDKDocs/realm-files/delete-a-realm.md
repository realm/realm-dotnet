# Delete a Realm - .NET SDK
In some circumstances, you might need to delete a realm file and its auxiliary files. If
you are developing or debugging the app, you might manually delete the realm
file, but doing so when the app is running and any realm instances are still
open can cause data corruption.

To delete a realm file while the app is running, you can use the
`DeleteRealm(configuration)`
method to safely do so. The following code demonstrates this:

```csharp
var config = new RealmConfiguration("FileWeThrowAway.realm");
Realm.DeleteRealm(config);
var freshRealm = Realm.GetInstance(config);

```
