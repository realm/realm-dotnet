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

Building iOS, tvOS, and macCatalyst wrappers on macOS
------------------------------------------

Building for iOS required cmake and zlib installed. In case you do not have them installed, you can do it with `brew install cmake zlib`.

You can use `build-apple-platform.ps1` to build for iOS, tvOS, and macCatalyst, specifying one or more of the available platforms, `Device`, `Simulator` or `Catalayst`, and either `Debug` or `Release` configuration.

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

Building macOS wrappers
-------------

You need Xcode 13 (or later) installed.

* To build a universal (x64 and Arm64) binary, run `./build-macos.sh -c=Debug/Release`.

Building Linux wrappers
-------------

`build-linux.sh` automates configuring and building wrappers with CMake. It accepts CMake arguments like `-GNinja`.

  1. For Linux x64 builds you can just build and run `centos.Dockerfile` if you don't have access to a Linux environment:
     * `docker build . -f centos.Dockerfile -t realm-dotnet/wrappers`
     * `docker run --rm -v $(pwd):/source realm-dotnet/wrappers`
  1. For Linux Arm/Arm64 builds you can build and run `debian-multiarch-arm.Dockerfile`:
     * `docker build . -f debian-multiarch-arm.Dockerfile -t realm-dotnet/wrappers-arm`
     * `docker run --rm -v $(pwd):/source realm-dotnet/wrappers-arm -a=arm64/arm`

General Notes
-------------
All builds steps download the required realm components (core and sync) automatically.

**Note** if you have changed the wrappers source and added, deleted or renamed files, you need to update `src/CMakeLists.txt` for builds to work.
