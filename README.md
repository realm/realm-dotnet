# .NET/Xamarin Language Binding for Realm

Main Points
---------------
The working code is in the realm subdir with tightdb retained as legacy because it still contains a lot of documentation and partial implementations from which we're drawing.

The top solution `Realm.sln` allows you to build for all the platforms and different test environments. We currently support IOS and Android as our major platforms. You can also build Win32 with Visual Studio and Mac with Xamarin Studio.

See `realm/doc/RealmDotNetProjects.md` for a description of the different solutions and projects that provides a lot more detail.

The following instructions are valid as of 21 Aug 2015 but are **under construction** and, in particular, we are moving towards having automated download of the code needed by the Wrappers.


Adding Binaries
---------------------
If you are building this solution from a checkout of the dotnet repo, you need to add binaries sourced separately into the following locations:

### For all builds

Tools:
  RealmNetWeaver.dll

### For Win32 builds

* wrappers:
	* build:
		* wrappersx64-Debug.dll
		* wrappersx32-Debug.dll
	* wrappers.lib
	* core:
		* Realm32d.lib  ... and other variants

### other still to be clarified

* wrappers:
	* build:
		* Release-ios-universal:
			* libwrappers.a
		* Release-iphoneos:
			* libwrappers.a
		* Release-iphonesimulator:
			* libwrappers.a
		* Release-android...
	* core:
		* librealm-ios-dbg.a
		* librealm-ios.a
		* librealm.a
		* Realm32d.lib
		* Realm?????.lib for release and x64?????
	* jni:
		* Android.mk
		* Application.mk
		* armeabi:
			* librealm-android.a
		* armeabi-v7a:
			* librealm-android.a
		* armeabi64:
			* librealm-android.a
		* mips:
			* librealm-android.a
		* x86:
			* librealm-android.a
	* libwrappers.dylib

	
	
	
