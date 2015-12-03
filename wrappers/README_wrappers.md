About Wrappers
==============

Wrappers is how we map the C# world to the Core and has to be compiled natively for each platform.

There are additional variants for release vs debug and the iPhone Simulator vs device.

Building Wrappers
-----------------------
* IOS and Android build `libwrappers.a` and `libwrappers.so` via `Makefile` with a download step pulling the core from `http://static.realm.io/downloads/core/realm-core-$(CORE_VER).tar.bz2` 
* go into the wrappers dir and just `make all`
* Windows builds `wrappersx86.dll` and `wrappersx64.dll` with the `wrappers.vcxproj` which is included in RealmNet.sln and getting core from the adjacent `realm-core` directory

**Note** if you have added, deleted or renamed files, you need to update `wrappers.xcodeproj` for IOS builds to work

