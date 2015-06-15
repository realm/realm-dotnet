#tightdb_csharp#

C# language binding for TightDB

Ver 0.1.5

This directory and its subdirectories contain the VS2012 solution that is needed to produce the C# binding for tightdb.

This document describe how to build the binding from the sources.

For a general overview of the .net binding and pointeres to tasks related to maintainance and development, open documentation.txt

##Build instructions##

These instructions use some concrete file paths as examples,
it is assumed that you have chekced out tightdb_csharp from github into e:\Wincoder\Develope\tightdb_csharp.
You can of course check out anywhere you like and it will stil work, if you replace the above path with your
chosen path. The solution itself uses relative paths and is not dependent on any specific location.
However, if you chose to build core itself too, core and the TightDBCSharp binding must be located side by side,
such that ..\tightdb from the C# binding will be the correct path to the core checkout directory.
(or else you cannot have the core release updated automatically by running a batchfile and will have to do some
manual work)

You need a copy of the release dir from a VS2012 built C++ binding distribution.

If you have a copy of the core c++ binding release, skip down to C++ DLL BUILD

##Core Build##

Building the c++ binding in windows with VS2012 (not express) can be done as follows :  

0. (prerequisite) Install Visual Leak Detector from https://vld.codeplex.com/
1. check out tightdb master to a directory alongside the csharp binding.
	e.g. if this file is located at E:\tightdb_csharp\README.md  
  then make sure the tightDB.sln file is in E:\tightdb\tightDB.sln    
2. open tightDBVS2012.sln in VS2012

*Execute core testcases:*
3. select Build->Batch Build
4. unmark all checkboxes under build, mark the 4 named
	TightDBVS2012 Debug Win32,
	tightDBVS2012 Debug x64, 
	TightDBVS2012 Release Win32, 
	TightDBVS2012 Release x64 
	- click Clean.  Output should report Clean : 8 succeeded, 0 failed, 0 skipped
5. select Build->Batch build, click REBUILD 
6. look for compiler warnings, verify that any warnings are marked with fixme in the code, report any new warnings to core developers.
7. if the build succeeded, run the 4 projects one by one and make sure the unit tests pass, report non passing tests to core developers.
If a project does not compile, check debug drop down , configuration manager, only the two last ones should be ticked.

*Build static libraries:*
8. build->batch build 
	- unmark all, mark the 4 called 'tightDB static library'
	- click clean
	- build->batch build - click build all
11. look for compiler warnings as in 6.

*Copy files:*
12. in explorer, run E:\tightdb\Windows\winrelease2012.cmd
	(this should create a release and update the windows\VS2012 release dir)
13. in explorer, run E:\tightdb_csharp\native\libsVS2012\copyfromcore.cmd. Answer 'All' if asked
	(updates the native part of the CHSARP binding from the core release)


##C++ DLL BUILD##  

1. Change the number in the file tightdb_c_cs.cpp to reflect the version being built.
	(Currently the format is simply YYMMDDHHMM.)
2. If you did not build core yourself, extract a VS2012 release into E:\tightdb_csharp\native\libsVS2012
	so that libsvs2012 now contain tightDB32d.lib and 3 other similar named lib files
	as well as a src directory structure with header files in it.
3. Open up the tightdb_c_cs2012 solution found in E:\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012 
4. select build->batch build.
	- select the 4 projects called tightdb_c-cs2012
	- click clean.
	- select build->batch build. click rebuild.
6. when VS2012 shows Rebuild All: 4 succeeded, mark the solution in solution explorer:
	- right click, select Open folder in file explorer. doubleclick the file release.cmd;
		if prompted, type Yes or All where applicable  

You have now built the C++ part of the C# binding, and created a release in 
the E:\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012\release directory.

The files in the release directory will be used when running unit tests and examples that use the C# binding,
but they are not used when building the binding itself, or building unit tests and examples.


##C SHARP BINDING BUILD##

1. Please change the string in Toolbox.Cs so that it reflects the build.
Currently the string is simply YYMMDDHHMM

2. navigate to E:\tightdb_csharp\TightDbCSharp  and open the TightDbCsharp.sln solution VS2012

3. select build->batch build and select all 30 configurations. then click clean.3

4. select build->batch build and click rebuild (wait until you get Rebuild all: 30 succeeded)

5 If you want to make a release - do the following:


##C SHARP UNIT TEST RUNNING

The project TighDbCSharpTest contains all the unit tests. However, I have found no unit test runner that reliably
will run these tests on varying platforms and bitness settings. Instead, We have 3 console projects, written with
NUnitLite - these projects will be used for running unit tests, until some day NUnit or other framework will work
reliably. Note that the code in the TighDBCSharpTest project is linked to from the commandline programs - don't delete
the source code.

0.
In Solution Explorer, locate the Test project, right click, select Set As Startup Project

1.
- Select Release/x86 in solution configuration/platform
- click the green run arrow
	A console window will show, and run the unit tests.
	If VS stops at a breakpoit that accidentially has been left over,´just press Continue to go on.
	Test names are printed as they are executed.
	Eventually the message Tests run: nnn. passed : nnn is shown.
- Make sure that all test listed in Errors and Failures are tests known to fail (due to core bugs or know binding bugs)
- Verify also at the very top of the console output, that 
	Process Running as: 32bit
	Debug or Release is Release
	Enter to close the commandline window

2. Select Release / x64		in solution configuration/platform. Test as above 1.
3. Select Release / AnyCpu	in solution configuration/platform. Test as above 1.
4. Select Debug   / AnyCpu	in solution configuration/platform. Test as above 1.
5. Select Debug   / x64		in solution configuration/platform. Test as above 1.
6. Select Debug   / x86		in solution configuration/platform. Test as above 1.

7. right click test_net35 in solution explorer and select Set as startup project.
Then repeat the steps 1 to 6 with this project as startup project.
At the first run, verify that this line is in the info dump at the top:
Built for .net version : V3.5

8. right click test_net40 in solution explorer and select Set as startup project.
Then repeat the steps 1 to 6 with this project as startup project.
At the first run, verify that this line is in the info dump at the top:
Built for .net version : V4.0

If everything checked out alright, go on to the release phase:


##C SHARP BINDING RELEASE##

1. mark the solution in solution explorer, right click, select Open folder in file explorer.

2. Convert Install_Note.txt and Build_Note_Daily.txt to windows line endings (Use Notepad++) edit->Eol conversion->Windows Format)

3. Doubleclick the file release_all.cmd.  

Answer All if asked, or Yes if asked and All cannot be answered. press enter a few times.

At this point you have a release of the C# binding in the E:\tightdb_csharp\release directory.
This release is all that you need to ship to a customer.

The customer will need to reference TightDbCSharp.dll in their .net projects, and furthermore,
they must ensure at runtime that the tightdb_c_csNNX.dll files (4 of them) can be found by their programs.
Usually it is far simplest simply to put the 4 tightdb_c_cs dll's the same place where the TightDbCSharp.dll is deployed.
In almost all cases it is sufficient to just deploy the 2 dll's that end with r.dll -
the ones with d.dll are debug versions and should not be used normally at all.

