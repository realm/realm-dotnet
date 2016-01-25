Realm for Xamarin Nuget Build
==============================

These are steps for our internal manual process to build.

The process has not been automated yet as the tools need running on both OS X and Windows to complete the build.

We currently produce two NuGet packages. You will usually build them and test using them from a local folder such as `~/LocalRealm`. We will refer to this as the **local test folder**.

Paths below assume you're starting in the root dir `realm-dotnet` checked out from GitHub.

Building the DLLS
-----------------

* go into the Wrappers dir in terminal
* `make clean`
* `make all`
* In Xamarin Studio, open`Realm.sln`
* select Release ARM
* select Realm.XamarinAndroid project and Rebuild
* select Release iPhoneSimulator
* select Realm.XamarinIOS project and Rebuild
* rename `/Users/andydent/dev/Realm/realm-dotnet/Realm.XamarinIOS/bin/iPhoneSimulator/Release/Realm.dll` to `RealmSimulator.dll` so you have a uniquely named DLL for the simulator
* select Release iPhone with a device attached
* select Realm.XamarinIOS project and Rebuild



Once you have your DLLS,


Setting Version Numbers
-----------------------
Edit `NuGet/NuGet.Library/Realm.targets` and update version numbers in the paths.

If the Fody Weaver version number is also changing, edit it in `NuGet/NuGet.Weaver/RealmWeaver.Fody.nuspec`


Building Fody NuGet
-------------------
You **have** to build this using Visual Studio. Open the normal Realm solution and force a rebuild of the `Nuget.Weaver` project. The `NuGetBuild` folder is created by this build.

Copy the `RealmWeaver.Fody.0.71.0.nupkg` generated in `NuGetBuild` to your **local test folder**.

Building Realm NuGet
--------------------
This step is performed from a Windows commandline using the NuGet tool.

You can have a full realm-dotnet tree over on a Windows machine, and copy over the built binaries from an OS X machine. (see below)

Easier is to run in a VM or have the realm-dotnet folder mounted as a shared folder, eg:
`Z:\andydent_Mac\dev\Realm\realm-dotnet` accessible from your Windows environment.

In either case, change to the `NuGet/NuGet.Library` directory containing `Realm.nuspec` and run the command:

`c:\tools\nuget pack -version 0.71.0 -NoDefaultExcludes Realm.nuspec`

The above assumes that the NuGet.exe tool was unpacked into `C:\tools` and you are building version `0.71.0`. The build scripts make no assumptions about the location of NuGet, just using relative paths from the location of the nuspec file.

Done correctly, this creates a package `/Users/andydent/dev/Realm/realm-dotnet/NuGet/NuGet.Library/Realm.0.71.0.nupkg` which you should copy to your **local test folder**.


### Binaries on OS X
If you are copying binaries from an OS X machine to a separate Windows tree, you will need to get:

* `Realm.XamarinAndroid/bin/Release`
* `Realm.XamarinIOS/bin/iPhone/Release`
* `Realm.XamarinIOS/bin/iPhoneSimulator/Release`
* `wrappers/build/Release-*` multiple directories
Testing the packages
--------------------
Set your Xamarin or Visual Studio to use your **local test folder** as a NuGet source.

Create new projects and use the NuGet Package manager to _add_ just the `Realm.0.71.0.nupkg`. It should automatically also add the Fody weaver and Fody in its turn. They will in ask you to overwrite `FodyWeavers.xml` which you normally allow.

You should then be able to build and run code using Realm.

Once you are happy with this, compress your **local test folder** and upload as a Git new release or to NuGet (instructions to be added when we go public).

Useful Stuff
-------------
Some aspects of NuGet builds suffer from the combination of being both **by convention** and **optional** which means you have very little chance of being told **if** or **what** you are doing wrong.


[Xamarin Advanced NuGet](https://developer.xamarin.com/guides/cross-platform/advanced/nuget/) is a key page because it describes the platform tags specific to Xamarin.

We need to use other tags for non-Xamarin .NET use.

[NuGet Targets and Props](https://docs.nuget.org/release-notes/nuget-2.5#automatic-import-of-msbuild-targets-and-props-files) explains the build folder has to be at the top and can then have framework-specific folders. It covers the conventions in detail.