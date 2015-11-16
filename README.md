# .NET/Xamarin Language Binding for Realm

Main Points
---------------
The top solution `RealmNet.sln` allows you to build for all the platforms and different test environments. We currently support IOS and Android as our major platforms. You can also build Win32 with Visual Studio and Mac with Xamarin Studio.

See `doc/RealmDotNetProjects.md` for a description of the different solutions and projects that provides a lot more detail.

The following instructions are valid as of 6 Nov 2015 but are **under construction** and, in particular, we are moving towards having automated download of the code needed by the Wrappers.


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

### iOS and OS X wrappers

Build using the `Makefile` in wrappers which downloads the _current_ static core library and builds the wrapper configurations, including running `lipo` to combine libraries.

Use `make all` to build all the wrappers. If you just want to update a Debug build, use `make iosdbg`.

The `RealmNet.XamarinIOS.csproj` includes Release or Debug libraries depending on configuration.

### android Wrappers 

currently don't build (see issue 164) as they don't have a download step



Debugging Wrappers
------------------

Under Visual Studio, you should be able to seamlessly debug from the managed world of most of the binding down into the C++ wrappers provided you _Enable native debugging_ in the `Realmnet.Win32.csproj` options.

To debug wrappers on IOS, say for the `IntegrationTests.IOS` project 

* you must first have run that project to get it onto the IOS Simulator, say from Xamarin Studio. 
* After the test app is on the simulator, _stop running from Xamarin Studio_ - only one debugger can connect at a time.
* Manually launch the test app on the simulator
* open the `wrappers.xcodeproj` in XCode and manually connect to the process using the menu item Debug - Attach to Process. Scroll down through the System Processes until you find `IntegrationTestsXamarionIOS.` 
* The debugger should show the live process and allow you to set breakpoints which will cause the app to pause when they are hit, just as if you had launched a native app from inside XCode.