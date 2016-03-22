Solutions
==============

After refactoring of the original Realm.sln - we now have Realm.sln as the main one.


Solutions - Platform Specific
------------------------------

The only platform-specific solutiion left at present is `RealmWin.sln` for easier testing of just Win32 without unloading other projects, in Visual Studio.


Projects - Platform specific 
----------------------------
All the **Realm** projects depend on Realm.Shared.shproj  and provide Realm for that platform.

### Realm.Win32 ###
References:

* Realm.Shared.shproj
* Mono.Android
* mscorlib
* System
* System.Xml
* System.Xml.Linq

Includes Realm core via  `wrappersx86.dll` and `wrappersx64.dll` being built alongside the wrappers.vcxproj native c++ project

**Warning** if you are building these targets make sure you have checked out your `realm-core` repo at a matching tag for the core versions being used in other builds. It is generally not advisable to buid core against the HEAD.


### Realm.XamarinAndroid  ###
References:

* Realm.Shared.shproj
* Mono.Android
* mscorlib
* System
* System.Xml
* System.Xml.Linq

Includes Realm core via `libwrappers.so`


### Realm.XamarinIOS ###
References:

* Realm.Shared.shproj
* System
* Xamarin.IOS

Includes Realm core via `libwrappers.a` 


### Realm.XamarinMAC  ###
References:

* Realm.Shared.shproj
* System
* System.Core
* Xamarin.Mac

Includes Realm core via `libwrappers.dylib` 


Shared Projects
---------------
These are `.shproj` projects which are a compile-time inclusion of files into the assembly using them and do not generate an assembly themselves.

### IntegrationTests.Shared.shproj ###
Testing classes used directly in all the Integration Test projects

### Realm.Shared.shproj ###
The core database classes


Test Projects
-----------------
* IntegrationTests.Win32 - allows tests to be run with TestDriven.net 🚀
* IntegrationTests.XamarinIOS - GUI test runner
* IntegrationTests.XamarinAndroid - GUI test runner

Demo Apps
---------------
* Playground.XamarinIOS
* Playground.XamarinAndroid

Other
-----

### RealmWeaver Project ###
Builds `RealmWeaver.Fody` weaving tool. Only needed when building and running tests from this project. Users of our NuGet distribution get it included.

### NuGet Project ###
The project `Nuget.Weaver` builds our NuGet project for `RealmWeaver.Fody` as documented in detail in RealmDotnetNugetBuild.md.

### PCL Distribution Project ###
The `Realm.PCL` project builds our PCL variant which is used in the _bait-and-switch_ pattern to allow PCL libraries to use Realm whilst platform-specific libraries are linked to the final app projects. See `RealmDotNetPCL.md` for more details.

### AssemblyToProcess Project ###
Used only by `WeaverTests.cs` **not currently in use as needs fixing**

### Wrappers Project ###
Builds the wrappers for pure Windows (libwrappers on other platforms built with core downloaded by external scripts as documented in `../README.md`)
