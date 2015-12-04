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
Builds `RealmWeaver.Fody`

### NuGet Project ###
Builds our NuGet package, not yet ready

### AssemblyToProcess Project ###
Used only by `WeaverTests.cs` **not currently in use as needs fixing**

### Wrappers Project ###
Builds the wrappers for pure Windows (libwrappers on other platforms downloaded by external scripts)
