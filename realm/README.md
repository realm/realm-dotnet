# .NET/Xamarin Language Binding for Realm

See doc/RealmDotNetProjects.md for a description of the different solutions and projects.

If you are building this solution from a checkout of the dotnet repo, you need to add binaries sourced separately into the following locations:

For all builds
--------------
Tools:
  RealmNetWeaver.dll

For Win32 builds
----------------
wrappers:
  build:
    wrappersx64-Debug.dll
    wrappersx32-Debug.dll
    wrappers.lib
  core:
    Realm32d.lib  ... and other variants

other still to be clarified
---------------------------

wrappers:
	build:
		Release-ios-universal:
			libwrappers.a
		Release-iphoneos:
			libwrappers.a
		Release-iphonesimulator:
			libwrappers.a
		Release-android...
	core:
		librealm-ios-dbg.a
		librealm-ios.a
		librealm.a
		Realm32d.lib
		Realm?????.lib for release and x64?????
	jni:
		Android.mk
		Application.mk
		armeabi:
			librealm-android.a
		armeabi-v7a:
			librealm-android.a
		armeabi64:
			librealm-android.a
		mips:
			librealm-android.a
		x86:
			librealm-android.a
	libwrappers.dylib

	
	
	
