Solutions
==============

After refactoring of the original Realm.sln.


RealmFoundation.sln
-------------------
Builds and tests a couple of key projects included as DLLs in the others.
- RealmNet
- RealmNet.Tests, (using MockCoreProvider rather than a background)
- InteropShared
- RealmNetWeaver
- RealmNetWeaver.Tests
- Nuget


Solutions - Platform Specific
------------------------------

The platform-specific solutions let us build a GUI test program and run unit tests specifically against that platform. 

All have the pattern of including projects for platform _Blah_:

- RealmNet
- RealmNet.tests
- InteropShared
- Interop.Blah
- Playground.Blah

Currently, our platform-specific solutions are:

- RealmWin
- RealmPureNetLINQ - runs as _AnyCPU_ for quick debugging of LINQ stuff with mock core
- RealmXamarinAndroid
- RealmXamarinIOS
- RealmXamarinMac


Projects - Platform specific 
----------------------------
All the platform interop projects depend on InteropShared and are used in the above.

### Interop.Win32 project ###
References:

- InteropShared
- Mono.Android
- mscorlib
- RealmNet
- System
- System.Xml
- System.Xml.Linq

Includes Realm core via  `wrappersx86.dll` and `wrappersx64.dll` being built alongside the wrappers.vcxproj native c++ project


### Interop.XamarinAndroid project ###
References:

- InteropShared
- Mono.Android
- mscorlib
- RealmNet
- System
- System.Xml
- System.Xml.Linq

Includes Realm core via `libwrappers.so`


### Interop.XamarinIOS project ###
References:

- InteropShared
- RealmNet
- System
- Xamarin.IOS

Includes Realm core via `libwrappers.a` 


### Interop.XamarinMAC project ###
References:

- InteropShared
- RealmNet
- System
- System.Core
- Xamarin.Mac

Includes Realm core via `libwrappers.dylib` 


Shared Projects
---------------
### InteropShared project ###
Provides the core classes used by the other Interop.

### RealmNet project ###
The core database classes

### RealmNet.Tests ###
Tests for the core database classes, will run against the current platform. 
Regarded as _integration tests_ because going usually through a layer to exercise the backend


Other
-----

### RealmNetWeaver Project ###
Builds `RealmNetWeaver.Fody`

### NuGet Project ###
Builds our NuGet package, not yet ready

### AssemblyToProcess Project ###

### IntegrationTests Project ###

### Wrappers Project ###
Builds the wrappers for pure Windows (libwrappers on other platforms built by external scripts)
