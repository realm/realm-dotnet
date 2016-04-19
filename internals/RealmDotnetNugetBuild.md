Realm for Xamarin Nuget Build
==============================

These are steps for our internal manual process to build.

The process has not been automated yet as the tools need running on both OS X and Windows to complete the build. 

We currently produce Realm as a single NuGet package. You will usually build it and test using it from a local folder such as `~/LocalRealm`. We will refer to this as the **local test folder**.

Paths below assume you're starting in the root dir `realm-dotnet` checked out from GitHub.

NuGet Features used
-------------------

We update the Fody standard file `FodyWeavers.xml` automatically using the XDT engine as described in [Configuration File and Source Code Transformations][ConfigsAndTransforms].

There's a pair of files in `NuGet.Library/content`:

* `FodyWeavers.xml.install.xdt`
* `FodyWeavers.xml.uninstall.xdt`


Setting Version Numbers
-----------------------
The Fody Weaver version number **must** follow the main Realm.dll even if there were no changes to Fody code.

**Note** You need to change version numbers below before building DLLs!

* Update the `AssemblyVersion` and `AssemblyFileVersion` version in `AssemblyInfo.cs` in five locations (platforms + PCL + RealmWeaver):
  * `RealmWeaver/AssemblyInfo.cs`
  * `Realm.PCL/Properties/AssemblyInfo.cs`
  * `Realm.Win32/Properties/AssemblyInfo.cs`
  * `Realm.XamarinAndroid/Properties/AssemblyInfo.cs`
  * `Realm.XamarinIOS/Properties/AssemblyInfo.cs`
* Edit `NuGet/NuGet.Library/Realm.targets` and update version numbers in **all** the paths.

**At NuGet build time,** you will also have to change the version number you use in the `nuget` command line.


Building the DLLS
-----------------

Follow the _Building Realm Steps_ section in `README.md` if you haven't built them already.



Building Realm NuGet
--------------------
This step is performed from a Windows commandline using the NuGet tool.

You can have a full realm-dotnet tree over on a Windows machine, and copy over the built binaries from an OS X machine. (see below)

Easier is to run in a VM or have the realm-dotnet folder mounted as a shared folder, eg:
`Z:\andydent_Mac\dev\Realm\realm-dotnet` accessible from your Windows environment.

In either case, change to the `NuGet/NuGet.Library` directory containing `Realm.nuspec` and run the command:

`c:\tools\nuget pack -version 0.72.0 -NoDefaultExcludes Realm.nuspec`

The above assumes that the NuGet.exe tool was unpacked into `C:\tools` and you are building version `0.72.0`. The build scripts make no assumptions about the location of NuGet, just using relative paths from the location of the nuspec file.

Done correctly, this creates a package `/Users/andydent/dev/Realm/realm-dotnet/NuGet/NuGet.Library/Realm.0.72.0.nupkg` which you should copy to your **local test folder**.

### Binaries on OS X
If you are copying binaries from an OS X machine to a separate Windows tree, you will need to get:

* `NuGet/NuGet.Library` to do the building of the NuGet
* `Realm.PCL/bin/Release`
* `Realm.XamarinAndroid/bin/Release`
* `Realm.XamarinIOS/bin/iPhone/Release`
* `RealmWeaver/bin/Release`
* `wrappers/build/Release-android` to get the multiple `libwrappers.so` in subdirs

Testing the packages
--------------------
Set your Xamarin or Visual Studio to use your **local test folder** as a NuGet source.

Create new projects and use the NuGet Package manager to _add_ just the `Realm.0.72.0.nupkg`. It should automatically also add Fody in its turn. They will in ask you to overwrite `FodyWeavers.xml` which you normally allow.

You should then be able to build and run code using Realm.

Once you are happy with this, compress your **local test folder** and upload as a Git new release or to NuGet (instructions to be added when we go public).

Private Upload Note
-------------------
Whilst we are zipping private builds, the naming convention is, eg: `realm-dotnet-0.73.0.zip` for the version number `0.73.0`.


Useful Stuff
-------------
Some aspects of NuGet builds suffer from the combination of being both **by convention** and **optional** which means you have very little chance of being told **if** or **what** you are doing wrong.


[Xamarin Advanced NuGet][XamNNG] is a key page because it describes the platform tags specific to Xamarin.

We need to use other tags for non-Xamarin .NET use.

[NuGet Targets and Props][NGTargetsNProps] explains the build folder has to be at the top and can then have framework-specific folders. It covers the conventions in detail.


[ConfigsAndTransforms]: 
http://docs.nuget.org/create/configuration-file-and-source-code-transformations
[XamNNG]: https://developer.xamarin.com/guides/cross-platform/advanced/nuget/
[NGTargetsNProps]: https://docs.nuget.org/release-notes/nuget-2.5#automatic-import-of-msbuild-targets-and-props-files