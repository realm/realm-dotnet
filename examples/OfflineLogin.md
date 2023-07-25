# Offline synced realm in the .NET Realm SDK #

With MongoDB Realm offline-first capabilities, it is always possible to read and write to the database even when using Device Sync, independently from the connection status of the app. In this small article, we are going to see how to open a synced realm depending on the situation in the .NET Realm SDK

There are two methods in the .NET Realm SDK that can be used to open a Realm: `Realm.GetInstance` (synchronously) and `Realm.GetInstanceAsync` (asynchronously). The two methods not only differ in asynchronicity, but have a different behavior when used to open a synced realm.

### GetInstance ###

This method will return as soon as the realm has been opened. For this reason, this method is the recommended one to use in the majority of application, as it works independently from the connection status. Once the realm has been opened, it will continue to synchronize in the background. 


### GetInstanceAsync ###

This method will complete after the realm has been opened and is fully synchronized. For this reason, this method is recommended to use only when it is essential to work with a fully synchronized realm, for example just after the first login of the user on a freshly installed application. 

The caveat with this method, though, is that it will continue to retry connecting to the server even if the device is offline, and could potentially run forever. In order to avoid this, you have two main possibilities:
- Use a cancellation token. The `GetInstanceAsync` method accept an optional `CancellationToken` variable as input, that can be used to cancel the operation after a maximum delay, like in the following example:
    ```csharp
    try
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(4000);
        var configuration = new FlexibleSyncConfiguration(app.CurrentUser);
        var realm = await Realm.GetInstanceAsync(configuration, token);
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


