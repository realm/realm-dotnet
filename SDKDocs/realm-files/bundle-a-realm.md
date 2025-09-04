# Bundle Realm Files - .NET SDK
You might want to seed your mobile app with some initial data that will be available
to users on the initial launch of the app. To do this, you:

- Build a temporary realm app,
- Make a copy of an existing realm (with only the data you want bundled), and then
- Bundle the Realm file in your app's shared project.

In your production app (the one that will use this bundled realm when first
loading), you add a few lines of code to extract the realm and save it in the
app data. The following sections provide more information on these steps.

## Create a Realm File for Bundling
1. Create a new project with the same data models as your production app. Open
an existing realm with the data you wish to bundle, or create a new one.
2. Use the `WriteCopy()` method to make a copy of the realm to a new
   location and/or name. The following code demonstrates this:
    ```csharp
    // open an existing realm
    var realm = Realm.GetInstance("myRealm.realm");

    // Create a RealmConfiguration for the *copy*
    var config = new RealmConfiguration("bundled.realm");

    // Make sure the file doesn't already exist
    Realm.DeleteRealm(config);

    // Copy the realm
    realm.WriteCopy(config);

    // Want to know where the copy is?
    var locationOfCopy = config.DatabasePath;

    // open an existing realm
    var existingConfig = new RealmConfiguration("myRealm", user);
    var realm = await Realm.GetInstanceAsync(existingConfig);

    // Create a RealmConfiguration for the *copy*
    var bundledConfig = new RealmConfiguration("bundled.realm");

    // Make sure the file doesn't already exist
    Realm.DeleteRealm(bundledConfig);

    // Copy the realm
    realm.WriteCopy(bundledConfig);

    // Want to know where the copy is?
    var locationOfCopy = existingConfig.DatabasePath;
    ```

## Bundle a Realm File in Your Production Application
Now that you have a copy of the realm with the "seed" data in it, you
need to bundle it with your production application. The process of bundling
depends on whether you are building a mobile app or Unity app:

#### Xamarin

1. Navigate to the path you specified for the new realm, and then drag the
newly-created realm file to the shared MAUI/Xamarin project in Visual
Studio.
2. When prompted, choose Copy the file to the directory.
3. In the shared project, right-click the realm file you just added, choose
Build Action, and then choose EmbeddedResource.

#### Unity

1. Open your production Unity project.
2. In the Project tab, copy the new realm file to the **Assets** folder.
Assets stored here are available to the app via the
`Application.dataPath` property.

> **NOTE:**
> Non-encrypted realm files are cross-platform compatible, which
is why you can bundle the file in the shared project.
>

## Open a Realm from a Bundled Realm File
Now that you have a copy of the realm included with your app, you need to
add code to use it. The code you add depends on the type of app:

#### Xamarin

Before you deploy your app with the bundled realm, you need to add code to
extract the realm from the embedded resources, save it to the app's data
location, and then open this new realm in the app. The following code shows how
you can do this during start-up of the app. Note that:

- this code only runs when no realm file is found at the specified
location (typically only on the initial use of the app), and

```csharp
// If you are using a local realm
var config = RealmConfiguration.DefaultConfiguration;

// Extract and copy the realm
if (!File.Exists(config.DatabasePath))
{
    using var bundledDbStream = Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("bundled.realm");
    using var databaseFile = File.Create(config.DatabasePath);
    bundledDbStream!.CopyTo(databaseFile);
}

// Open the Realm:
var realm = Realm.GetInstance(config);

```

#### Unity

The embedded realm is initialized like any other realm in a Unity
project:

```csharp
// After copying the above created file to the project folder,
// we can access it in Application.dataPath

// If you are using a local realm
var config = RealmConfiguration.DefaultConfiguration;

if (!File.Exists(config.DatabasePath))
    FileUtil.CopyFileOrDirectory(Path.Combine(Application.dataPath,
        "bundled.realm"), config.DatabasePath);
}

var realm = Realm.GetInstance(config);

```

