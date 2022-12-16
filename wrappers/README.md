About Wrappers
==============

Wrappers contains all our native code and its interfaces to C#.

It has a reference to the [Realm Core](https://github.com/realm/realm-core) repository as a **git submodule**.

Wrappers also contains a small amount of C++ code which provides the mapping from C# to the Core logic.

Downloading Realm Core
-----------------------

### Cloning

If you cloned your `realm-dotnet` repository, you can use a git command to get the submodule:

1. Open a terminal window in the `realm-dotnet` source directory
1. Enter the command `git submodule update --init --recursive`


### Direct Download

If you downloaded a zip of the source, you need to go back to github to identify which version of Core is required. There is no git information in the zip file which specifies this.

1. Look in the github repo [wrappers](https://github.com/realm/realm-dotnet/tree/main/wrappers) and you will see the link to the submodule, eg: `realm-core @ 802aa43`.
1. Click the link to take you to the tree in Core
1. Download a zip using the GitHub download button in that tree, eg `realm-core-fb2ed6aa0073be4cb0cd059cae407744ee883b77.zip`
1. Unpack its contents into `wrappers/src/realm-core`

Building iOS wrappers on macOS
------------------------------------------

Prerequisites:
1. Install cmake and zlib: `brew install cmake zlib`.

These instructions assume you have either downloaded a zip from gitub of the realm-dotnet source, or checked out a clone, and then downloaded Realm Core as above.

1. `cd wrappers`
1. `build-ios.sh` - this will probably download a current version of core binaries, unless you have built recently. The download and subsequent builds will take some time, depending on your system, as it builds a binary wrapper library for both device and simulator.

Building Android wrappers
-------------

Building for Android uses CMake with a toolchain file. You can either configure CMake with an Android toolchain file manually, or build with `build-android.sh`. By default it will build for armeabi-v7a, arm64-v8a, x86, and x86_64. You can specify a single ABI to build by passing `--arch=$ABI`. You can also choose a build configuration by passing `--configuration=$CONFIG`. The script also accepts CMake arguments like `-GNinja`.

You need to have the Android NDK installed, version r10e, and set an environment variable called `ANDROID_NDK_HOME` pointing to its location.

Building Windows wrappers
-------------

You need Visual Studio 2017 (or later) with the `C++ Universal Windows Platform tools` and `Visual C++ tools for CMake` components as well as a version of the Windows SDK installed.

Valid Windows platforms (architectures) are `Win32`, `x64`, and `ARM`. You can specify all or a subset to save time when building.

* To build for regular Windows run `.\build.ps1 Windows -Configuration Debug/Release -Platforms Win32, x64`

* To build for Windows Universal run `.\build.ps1 WindowsStore -Configuration Debug/Release -Platforms Win32, x64, ARM`

You can find the CMake-generated Visual Studio project files in `cmake\$Target\$Configuration-$Platform` and use them for debugging.

Building .NET Core wrappers for macOS and Linux
-------------

`build.sh` automates configuring and building wrappers with CMake. It accepts CMake arguments like `-GNinja`.

For Linux builds you can just build and run `centos.Dockerfile` if you don't have access to a Linux environment:

1. `docker build . -f centos.Dockerfile -t realm-dotnet/wrappers`
1. `docker run -v path/to/wrappers:/source realm-dotnet/wrappers`

General Notes
-------------
All builds steps download the required realm components (core and sync) automatically.

**Note** if you have changed the wrappers source and added, deleted or renamed files, you need to update `src/CMakeLists.txt` for builds to work.

