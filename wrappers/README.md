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

Building Wrappers on macOS
--------------------------

These instructions assume you may have downloaded a zip from gitub of the realm-dotnet source, or checked out a clone and then downloaded ObjectStore as above.

1. `cd wrappers` 
1. `make clean`
1. `make all` - this will probably download a current version of core binaries, unless you have built recently. The download and subsequent builds will take some time, depending on your system, as it builds a binary wrapper library for each platform including all Android CPU variations.

### Individual Builds

To save time for testing you may want to build only some of the wrappers libraries:

* iOS builds `librealm-wrappers.a` - run `make ios` or `make iosdbg`. Pass `REALM_ENABLE_SYNC=0` to build without sync (default is `1`)
* Android builds `librealm-wrappers.so` - run `make android` or `make androiddbg` Pass `REALM_ENABLE_SYNC=0` to build without sync (default is `1`)
* Windows builds `realm-wrappers.dll` for x86 and x64. Open `wrappers.vcxproj` (or `Realm.sln`) and build for both architectures in the desired configuration. Sync is not supported on Windows right now.

All builds steps download the required realm components (core and sync) automatically.

**Note** if you have changed the wrappers source and added, deleted or renamed files, you need to update `wrappers.xcodeproj`, `wrappers.vcxproj`, and `jni/Android.mk` for builds to work.
