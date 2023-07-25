# Offline synced realm in the .NET Realm SDK

With MongoDB Realm offline-first capabilities, it is always possible to read and write to the database even when using Device Sync, independently from the connection status of the app. In this small article, we are going to see how to open a synced realm depending on the situation in the .NET Realm SDK

There are two methods in the .NET Realm SDK that can be used to open a Realm: `Realm.GetInstance` (synchronously) and `Realm.GetInstanceAsync` (asynchronously). The two methods not only differ in asynchronicity, but have a different behavior when used to open a synced realm. We will first take a look at those two methods on their own, then show a recommended flow for opening a realm. 

#### GetInstance

This method will return as soon as the realm has been opened. For this reason, this method can be safely used in the the majority of the application, as it works independently from the connection status. Once the realm has been opened, it will continue to synchronize in the background. 


#### GetInstanceAsync

This method will complete after the realm has been opened and is fully synchronized. For this reason, this method is recommended to use only when it is essential to work with a fully synchronized realm, for example just after the first login of the user on a freshly installed application. 

The caveat with this method, though, is that it will continue to retry connecting to the server even if the device is offline, and could potentially run forever. In order to avoid this, you have two main possibilities:
- Use a cancellation token. The `GetInstanceAsync` method accept an optional `CancellationToken` variable as input, that can be used to cancel the operation after a maximum delay, like in the following example:
    ```csharp
    try
    {
        var cts = new CancellationTokenSource(4000);
        var configuration = new FlexibleSyncConfiguration(app.CurrentUser);
        var realm = await Realm.GetInstanceAsync(configuration, cts.Token);
    }
    catch (TaskCanceledException tce)
    {
        Console.WriteLine("GetInstanceAsync timeout");
    }
    ```
    This is the recommended approach in most of the cases, as it allows to configure the most appropriate timeout for the operation, depending on the specific application scenario.
- Set `CancelAsyncOperationsOnNonFatalErrors` to `true` on `FlexibleSyncConfiguration`. When set to `true`, async operations such as `Realm.GetInstanceAsync` or `Session.WaitForUploadAsync` will throw an exception whenever a non-fatal error, such as a timeout occurs. The timeouts can be customized by modifying the values in `AppConfiguration.SyncTimeoutOptions`, that contains a series of properties that control sync timeouts, such as the connection timeout and others. For example:
    ```csharp
    var appConfiguration = new AppConfiguration(config.AppId);
    appConfiguration.SyncTimeoutOptions.ConnectTimeout = TimeSpan.FromSeconds(4);
    app = Realms.Sync.App.Create(appConfiguration);

    // ....

    try
    {
        var configuration = new FlexibleSyncConfiguration(app.CurrentUser)
        {
            CancelAsyncOperationsOnNonFatalErrors = true
        };
        var realm = await Realm.GetInstanceAsync(configuration);
    }
    catch (Exception ex)
    {
        Console.WriteLine("GetInstanceAsync timeout");
    }
    ```
    In most cases this approach could be an overkill, as the exception would be raised for any kind of non-fatal errors, even for just a brief connection issue. For this reason it is recommended to use the previous approach, as it allows to retry in the specified time frame.

### Recommended flow

Given the differences between `GetInstance` and `GetInstanceAsync` that we have presented, here is an example of a recommended flow that uses both methods to open a realm, and that can be used in most applications:

```csharp
var user = app.CurrentUser;
var realmConfig = GetRealmConfig();

Realm realm;

if (user == null)
{
    // There is no current user, so show the login screen and wait for the user to login.
    await PresentLoginScreen();

    // Creates a CancellationTokenSource that will be cancelled after 4 seconds.
    var cts = new CancellationTokenSource(4000);

    try
    {
        // The user just logged in, so we probably still have connectivity here.
        // Therefore we can get the realm asynchronously, with all the data synchronized.
        realm = await Realm.GetInstanceAsync(realmConfig, cts.Token);
    }
    catch (TaskCanceledException)
    {
        // If there are connectivity issues, get the realm synchronously
        realm = Realm.GetInstance(realmConfig);
    }
}
else
{
    // The user is already logged in, so open the realm straight away
    // with the available data. If the application needs the latest
    // available data, then use GetInstanceAsync here instead.
    realm = Realm.GetInstance(realmConfig);
}
```