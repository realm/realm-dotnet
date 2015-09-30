About Wrappers
==============

Wrappers is how we map the C# world to the Core and has to be compiled natively for each platform.

The original _proof of concept_ was just in two files `wrapper.h` and `wrappers.cpp`.

For migration of build scripts, `wrappers.cpp` has been retained and #includes all other files.

The Visual Studio project wrappers.vcxproj shows files as they are broken down.

Building Wrappers
-----------------------
* Android - built with the makefile `jni/Android.mk` and will eventually be covered by the `make all` below
* IOS builds `libwrappers.a` via `Makefile` with a download step pulling the core from `http://static.realm.io/downloads/core/realm-core-$(CORE_VER).tar.bz2` 
	* go into the wrappers dir and just `make all`
	* if you have added, deleted or renamed files, you need to update `wrappers.xcodeproj`
* Windows builds `wrappersx86.dll` and `wrappersx64.dll` with the `wrappers.vcxproj` which is included in RealmNet.sln and getting core from the adjacent `realm-core` directory



internal dir
-------------
To make the code more familiar to people comparing it to Java, we have an `internal` dir which corresponds to `realm-java/realm/src/main/java/io/realm/internal`

See the [sheet describing mappings](https://docs.google.com/a/tightdb.com/spreadsheets/d/1nIcG7SQMrfcN5YE2xcKm3Oy6wubqOENvtWTfhhwk4GA/edit?usp=sharing) which explains how files map across and individual progress to this aim.


Files Copied from Java with Changes
-----------------------------------------------

General principle, stuff commented out because not fullly understood if can remove has a //J comment or /*J to open block



* util.hpp
* util.cpp