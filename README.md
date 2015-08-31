# .NET/Xamarin Language Binding for Realm

Main Points
---------------
The working code is in the realm subdir with tightdb retained as legacy because it still contains a lot of documentation and partial implementations from which we're drawing.

The top solution `Realm.sln` allows you to build for all the platforms and different test environments. We currently support IOS and Android as our major platforms. You can also build Win32 with Visual Studio and Mac with Xamarin Studio.

See `realm/doc/RealmDotNetProjects.md` for a description of the different solutions and projects that provides a lot more detail.

The following instructions are valid as of 21 Aug 2015 but are **under construction** and, in particular, we are moving towards having automated download of the code needed by the Wrappers.


Adding Supporting Binaries
---------------------
If you are building this solution from a checkout of the dotnet repo, you need to add binaries sourced separately into the following locations:

### For all builds

Tools:
  RealmNetWeaver.dll

OR

Build the RealmNetWeaver project, including its NuGet download of Fody.

### For Win32 builds

* wrappers:
	* build:
		* wrappersx64-Debug.dll
		* wrappersx32-Debug.dll
	* wrappers.lib
	* core:
		* Realm32d.lib  ... and other variants

OR

Build Wrappers.vcxproj but this will require you to have the realm-core directory checked out adjacent to realm-dotnet. This is very much an interim, Realm-internal-team solution for now.

### Other Platforms Should Download Files	 ###

Running a `make core` in the wrappers directory will download and unpack headers and binaries for

* IOS
* Android (coming very soon)
