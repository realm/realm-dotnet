# Quick Start for Unity - .NET SDK
This page contains information on how to install and integrate Realm into
your Unity project.

## Prerequisites
- Unity [2020.3.12f1 (LTS)](https://unity3d.com/get-unity/download/archive)

> **NOTE:**
> The Realm .NET SDK may be compatible
with other versions of Unity, but `2020.3.12f1 (LTS)` is the version that
the Realm team uses for testing and development. We recommend
using this version to ensure your project works with Realm and
that the install steps match the steps below since Unity's UI often changes
between versions.
>

## Install
Realm provides various ways to install the Realm
.NET SDK for use with Unity. Experienced Unity developers may
find installing Realm manually with a tarball to be intuitive.
However, we recommend installing the Realm .NET SDK via npm since it
provides
[notifications of version updates through Unity's package manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@2.0/manual/index.html#PackManUpdate).

#### Npm

##### Add NPM as a Scoped Registry
Before you can download and use Realm within your Unity project, you
must add [NPM](https://www.npmjs.com/) as a [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html). Adding NPM as a scoped
registry configures Unity to communicate with NPM, allowing you to install
packages, such as Realm.

Open the [Unity package manager](https://docs.unity3d.com/Manual/upm-ui.html) by clicking the
Window tab on the top of the Unity menu. Click Package
Manager from the Window dropdown. Then, click the gear icon on
the right-hand corner. Select the Advanced Project
Settings option from the dropdown.

Fill out the scoped registry form with the details below and click the save
button.

```text
name = NPM
URL = https://registry.npmjs.org
Scope(s) = io.realm.unity
```


##### Add Realm to the Project Manifest
Now that Unity can install the Realm .NET SDK from NPM, you need
to add Realm as a dependency in your  project's [manifest file](https://docs.unity3d.com/Manual/upm-manifestPrj.html). Open
"Packages/manifest.json" file in [Visual Studio](https://docs.microsoft.com/en-us/visualstudio/) or another text editor. At
the bottom of the dependency's object, add the field, "io.realm.unity" and
its value, the Realm .NET version number you want to use in
quotes.

```json
"io.realm.unity": "<version-number>"
```

Remember to replace the `<version-number>` above with the actual version
number. You can find the latest release version at the
[realm-dotnet GitHub repo](https://github.com/realm/realm-dotnet).
Your full manifest file should look something like the following:

```none
{
 "dependencies": {
   ...
   "io.realm.unity": "10.21.0"
 },
 "scopedRegistries": [
   {
     "name": "NPM",
     "url": "https://registry.npmjs.org/",
     "scopes": [
       "io.realm.unity"
     ]
   }
 ]
}
```

When you save this file, Unity downloads the specified version of the
Realm .NET SDK package from the NPM registry.

##### Verify the Realm Dependency and Check for Updates
To verify that the Realm package has been downloaded from NPM,
open your Unity package manager by clicking the Window tab on the
top of the Unity menu. Click Package Manager from the
Window dropdown. You should see Realm on the "Packages: In
Project" tab.

If you see a green check icon next to the version number of the
Realm package, that means your package is up-to-date. However, if
you see the up arrow icon, a new version of the package is available. Clicking
it gives you the option to upgrade to the latest release.

#### Tarball

##### Download the Latest Realm .NET SDK Release
Before you begin using Realm within your Unity project, you must
download the Realm .NET SDK.

Navigate to the [realm-dotnet repository releases](https://github.com/realm/realm-dotnet/releases) page, and scroll down to the release you want
to use in your project. If you are unsure of which release to use, you can use
the one labeled **"latest release"** on the left row.

Scroll down to the **"Assets"** dropdown of the release and click the link
labeled **"io.realm.unity-<version-number>.tgz"** to download the SDK.

##### Add the Tarball to Your Projects Package Manager
Move your downloaded Realm .NET SDK tarball inside of your
project. You can do this by dragging and dropping the file into your project's
folder. Copying the tarball to your project folder and committing it to version
control ensures other developers working on the project can just clone the
repository and build without manually downloading the Realm dependency.

Next, you must load the tarball into your project using the [Unity package manager](https://docs.unity3d.com/Manual/upm-ui.html).

To open the package manager, click the Window tab on the top of
the Unity menu. Click Package Manager from the Window
dropdown. Once the package manager model opens, click the + icon
in the top left corner of the model. Select the Add package from
tarball... option.

Select your **"io.realm.unity-bundled-&lt;version-number&gt;.tgz"** file to
begin importing your project.

## Import Realm
[Create a C# script](https://docs.unity3d.com/Manual/CreatingAndUsingScripts.html) or use a C#
script you have already created. Open that script in [Visual Studio](https://docs.microsoft.com/en-us/visualstudio/) or another text editor and add
the following line to import your Realm package:

```csharp
using Realms;
```

## Using Realm in Your Unity Project
When developing with Realm .NET SDK, the API
methods are the same regardless of whether you use Unity or another platform.
However, since Unity has some [scripting restrictions](https://docs.unity3d.com/Manual/ScriptingRestrictions.html), you should keep
the following additional considerations in mind when developing your project:

### Managed Code Stripping
Unity performs [managed code stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html),
discarding any unused code from a build to reduce binary size. This may lead to issues when
deserializing [BSON](https://www.mongodb.com/docs/manual/reference/bson-types/) into C# classes. For platforms
that use [IL2CPP](https://docs.unity3d.com/Manual/IL2CPP.html), such as iOS,
managed code stripping is enabled by default. When working with BSON, use
the [[Preserve] attribute](https://docs.unity3d.com/ScriptReference/Scripting.PreserveAttribute.html)
to prevent managed code stripping on types properties that are only populated by
the serializer. Since those properties use
[reflection](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection),
Unity cannot statically infer that the property setter is used. This means that
unless you apply the `[Preserve] attribute`, Unity will strip those properties
away.

### Using Realm While the Application is Quitting
The Realm .NET SDK cannot be accessed within the
[AppDomain.DomainUnload Event](https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.domainunload?view=net-5.0) or
the [Application.quitting](https://docs.unity3d.com/ScriptReference/Application-quitting.html) event.
This means you cannot write data to a Realm while the player application is quitting.
If you need to store some data just before the app exits, consider using the
[Application.wantsToQuit](https://docs.unity3d.com/ScriptReference/Application-wantsToQuit.html)
event instead.

## Additional Examples
The Realm community has created many projects that demonstrate the usage
of the Realm .NET SDK:

- [dodoTV42 Youtube Channel: How to SAVE and LOAD data in Unity3D with Realm SDK](https://www.youtube.com/watch?v=8jo_S02HLkI)
