# .NET/Xamarin Language Binding for Realm

Main Points
---------------
The working code is in the `realm` subdir with `tightdb` retained as legacy because it still contains a lot of documentation and partial implementations from which we're drawing.

The top solution `RealmNet.sln` allows you to build for all the platforms and different test environments. We currently support IOS and Android as our major platforms. You can also build Win32 with Visual Studio and Mac with Xamarin Studio.

See `realm/doc/RealmDotNetProjects.md` for a description of the different solutions and projects that provides a lot more detail.

The following instructions are valid as of 9 Sep 2015 but are **under construction** and, in particular, we are moving towards having automated download of the code needed by the Wrappers.


Adding Binaries
---------------------
If you are building this solution from a checkout of the dotnet repo, you need to add binaries sourced separately into the following locations:

### For all builds

Tools:
  RealmNetWeaver.dll

### For Win32 builds

* wrappers - Build an adjacent `realm-core\build\vs2013\realm.sln` configurations + platforms
	* Static Lib, debug **platforms** x86 and x64
	* Static Lib, release **platforms** x86 and x64

### other still to be clarified

