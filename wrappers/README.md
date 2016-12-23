About Wrappers
==============

Wrappers is how we map the C# world to the Core and has to be compiled natively for each platform.

Building Wrappers
-----------------------
* iOS builds `librealm-wrappers.a` - run `make ios` or `make iosdbg`. Pass `REALM_ENABLE_SYNC=0` to build without sync (default is `1`)
* Android builds `librealm-wrappers.so` - run `make android` or `make androiddbg` Pass `REALM_ENABLE_SYNC=0` to build without sync (default is `1`)
* Windows builds `realm-wrappers.dll` for x86 and x64. Open `wrappers.vcxproj` (or `Realm.sln`) and build for both architectures in the desired configuration. Sync is not supported on Windows right now.

All builds steps download the required realm components (core and sync) automatically.

**Note** if you have added, deleted or renamed files, you need to update `wrappers.xcodeproj`, `wrappers.vcxproj`, and `jni/Android.mk` for builds to work.
