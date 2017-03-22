About Wrappers
==============

Wrappers contains all our native code and its interfaces to C#.

It usually involves a download phase which pulls prebuilt [Core ](https://github.com/realm/realm-core) libraries from a server. 

We have a second C++ layer called [ObjectStore](https://github.com/realm/realm-object-store/) 
which contains many of our cross-platform abstractions and is pulled into Wrappers as a **git submodule**.

Wrappers also contains a small amount of C++ code which provides the mapping from C# to the ObjectStore and Core logic.

Downloading ObjectStore
-----------------------

### Cloning

If you cloned your `realm-dotnet` repository, you can use a git commmand to get the submodule:

1. Open a terminal window in the `realm-dotnet` source directory
1. Enter the command `git submodule update --recursive`


### Direct Download

If you downloaded a zip of the source, you need to go back to github to identify which version of Objectstore is required. There is no git information in the zip file which specifies this.

1. Look in the github repo [wrappers/src](https://github.com/realm/realm-dotnet/tree/master/wrappers/src) and you will see the link to the submodule, eg: `object-store @ fb2ed6a`.
1. Click the link to take you to the tree in ObjectStore
1. Download a zip using the GitHub download button in that tree, eg `realm-object-store-fb2ed6aa0073be4cb0cd059cae407744ee883b77.zip`
1. Unpack its contents into `wrappers/src/object-store`

Building iOS and Android Wrappers on macOS
------------------------------------------

These instructions assume you have either downloaded a zip from gitub of the realm-dotnet source, or checked out a clone, and then downloaded ObjectStore as above.

1. `cd wrappers` 
1. `make clean`
1. `make all` - this will probably download a current version of core binaries, unless you have built recently. The download and subsequent builds will take some time, depending on your system, as it builds a binary wrapper library for each platform including all Android CPU variations.

### Individual Builds

To save time for testing you may want to build only some of the wrappers libraries:

* iOS builds `librealm-wrappers.a` - run `make ios` or `make iosdbg`. Pass `REALM_ENABLE_SYNC=0` to build without sync (default is `1`)
* Android builds `librealm-wrappers.so` - run `make android` or `make androiddbg` Pass `REALM_ENABLE_SYNC=0` to build without sync (default is `1`)

Building Windows wrappers
-------------

You need Visual Studio 2017 with the `C++ Universal Windows Platform tools` and `Visual C++ tools for CMake` components as well as a version of the Windows SDK installed.
Valid Windows platforms (architectures) are `Win32`, `x64`, and `ARM`. You can specify all or a subset to save time when building.

* To build for regular Windows run `.\build.ps1 Windows -Configuration Debug/Release -Platforms Win32, x64`

* To build for Windows Universal run `.\build.ps1 WindowsStore -Configuration Debug/Release -Platforms Win32, x64, ARM`

You can find the CMake-generated Visual Studio project files in `cmake\$Target\$Configuration-$Platform` and use them for debugging.

Sync is not supported on Windows right now.


General Notes
-------------
All builds steps download the required realm components (core and sync) automatically.

**Note** if you have changed the wrappers source and added, deleted or renamed files, you need to update `wrappers.xcodeproj`, `wrappers.vcxproj`, and `jni/Android.mk` for builds to work.
