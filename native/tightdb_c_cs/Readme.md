#tightdb_c_cs#

This directory contains c++ project and files that are used to build the tightdb_c_cs.dll file for various platform configurations

The tightdb_c_cs.dll file contains extern "C" methods, that call on into c++ tightdb

The C# binding use the extern "c" methods in the DLL to access tightdb.

This directory contains two solutions that links to the same sourcefiles :
tightCSDLL2012.sln - used to build with visual studio 2012 (not there yet, awaiting new lib files)
tightCSDLL2010.sln - used to build with visual studio 2010


###Getting VS2010 or VS2012 ready to build the project###

These are instructions to get going from a fresh version of visual studio 2010, without using the existing project file.(thus starting from no dependencies on anything)

The end result will be a project directory below the directory where this file is, with project and build related stuff, You can have VS2010 and VS2012 side by side and share the same source files. The slightly weird directory layout is because VS2010 insists on naming a directoy the same as the project, even when You tell it not to.
VS2012 has had this issue fixed, but having 2012 in this dir, and 2010 in a subdir would be an unbalanced mess. At least we now have a balanced mess.

Prerequesites :  

A directory structure like this :

C:\develope\github\tightcsharp  (this path is used as an example throughout this document)

so that this readme.txt file is located in

c:\develope\github\tightcsharp\native

It is assumed that You have checked out the csharp binding into the 
c:\develope\github\tightcsharp directory
This readme.md file should then be located at 
C:\Develope\github\tightcsharp\native\tightdb_c_cs\readme.md


Visual studio 2010 Express installed (or better)  
or
Visual studio 2012 Express installed (or better) (however, not tested until we get .lib files built with vs2012)

The library files needed are :
- tightdb32.lib
- tightdb32d.lib
- tightdb64.lib
- tightdb64d.lib

place VS2012 Library files in ..\..\libs\libsVS2012 (aka c:\develope\github\tightcsharp\libs\libsVS2012)  
place VS2010 Library files in ..\..\libs\libsVS2010 (aka c:\develope\github\tightcsharp\libs\libsVS2010)  

tightdb header files for tightdb<32|64>[d].lib:

Taking the header files from the unix build will do, You probably also need the windows header files-these are not in the unix build,and not on the github java binding branch, You have to obtain them from someone at tightdb.

The tightdb header files are assumed to be found in  
- ..\..\..\..\tightdb     (aka c:\develope\tightdb )
- ..\..\..\..\tightdb\win32  (aka c:\develope\tightdb\win32 )

note that the header files are looked for in a directory that is not a part of this project tree.  

###Setting up VS2010 or 2012 to build the c++ binding###

Now, start Microsoft C++ 2010 Express (or 2012 Express)


mark tools->settings->expert settings if not already marked


-file->new project->general.  
-mark empty project 
-name:tightdb_c_cs2010
-(vs2012:  name:tightdb_c_cs2012)
-location:same directory as this readme.txt file (on this pc c:\Develope\github\tightcsharp\native\tightdb_c_cs) 
-untick "create directory for solution"  
-click OK  (this should create a set of project files inside C:\Develope\github\tightcsharp\native\tightdb_c_cs\tightdb_c_cs2010)

If not already open, select view->solution explorer

-In Solution Explorer   
-right click tightdb_c_cs_2010->header files
-select add->existing.  
-navigate up one directory to tightdb_c_cs
-mark and select the following files:
- -stdafx.hpp
- -tightdb_c_cs.hpp
-click Add


-right click tightdb_c-cs_2010->Source Files.
-select add->existing  
-mark and select the following files:  
-- stdafx.cpp
-- TightCSDLL.cpp
-click Add




Then...


Now, Check that You have 64 bit c++ compiler support in vs2010 (You need to have opened a c++ project with c++ files in it to check that)

-in solution explorer, right click tightdb_c_cs2010, select properties
-in tightdb_c_cs2010 property pages, click configuration manager.
-in the Platform column, click Win32
-if You've got "new" as an option, and if selecting New gets You a small
dialog from where You can select X64 in the New platform dropdown, You have 64 bit c++ support)

-If You don't have 64 bit compiler support (and You are running on a 64 bit OS) then this is a copy of the machine install notes on the machine where I fixed the issue:
-
Installation with VS 2010 Express

The ordinary versions of VS 2010 Express does not have support for building x64 versions of c++ programs - but there is a version of VS 2010 Express that do have this support, You have to obtain it from the "Windows SDK 7 for .net framework 4" kit
The road i went down was a bit winding, there are probably better ways to get to the goal, but this works :
1) install visual studio 2010 express
2) in windows control panel, uninstall visual studio 2010 redistributable 32 and 64 bit  (two programs must be uninstalled)
3) install "Windows SDK 7.1 for Windows 7 and .net Framework 4" from http://www.microsoft.com/en-us/download/confirmation.aspx?id=8279

Even though the SDK install fails towards the end of the installation, VS2010 will now have an option to compile to itanium and x64 however, the property pages for 64 bit builds are empty, to fix this, install this update :
4) http://www.microsoft.com/en-us/download/details.aspx?id=4422



Set up the general settings that apply to all kinds of builds  :

First create 64 bit builds as well as the preinstalled 32 bit builds:

-In Solution explorer, right click tightdb_c_cs2010, select properties
-in tightdb_c_cs2010 property pages, click configuration manager.
-in configuration manager, in project contexts-Platform column, click Win32
you get a "new" option. click that.
-In the "New Project Platfor" dialog, select x64, and select "Copy settings from Win32". select "create new solution platform.
-In the platform column, select Win32 again.
-In the Active Solution Platform dropdown, select Win32,
-In the Active solution configuration dropdown, select Debug.
-Click close. Click OK to get rid of the final dialog.


View->Property Manager
(You should now have tightdb_c_cs2010 as a root in a tree, with these 4 nodes :
-Debug|Win32 
-Release|Win32 
-Debug|x64
-Release|x64
)



right click tightdb_c_cs2010
-select properties 
-In "tightdb_c_cs2010 property pages" select configuration properties->General.
-In the "Configuration" left dropdown box on top of the window  select "All Configurations"
-in the "platform" dropdown on top of the window, select All Platforms.


Set up stuff that is the same for all 4 configurations:
-select Configuration Type, change it to Dynamic Library (.dll)
-click Apply.


-select configuration properties->VC++ Directories
-select Library Directories, click the down error and select edit  
-type in ..\..\libsVS2010\   (aka C:\develope\github\tightdbcs\libs\libsVS2010)
( vs2012:type in ..\..\libs2012\ (aka C:\develope\github\tightdbcs\libs\libsVS2012) )


-select configuration properties->c/C++->Preprocessor
-select Preprocessor Definitions, click arrow down and edit  
-type in TIGHTDB_C_CS_EXPORTS  
-click apply

in  configuration properties->c/c++->General->additional include directories

-click arrow down,select edit. Add:  
-..\..\..\..\..\tightdb\  
-..\..\..\..\..\tightdb\win32\  
-click apply



Now, this next setting is different for each build combination,It sets up the linker for the specific build to select the correct .lib file with tightdb.

The first two in below list is the settings of Configuration: and Platform: dropdowns. The third column is what must be typed into  Configurration properties->linker->input->Additional Depedencies
-Debug  ,Win32 ,TightDB32d.lib
-Debug  ,x64   ,TightDB64d.lib
-Release,Win32 ,TightDB32.lib
-Release,Win64 ,TightDB64.lib

-click apply each time before changing the dropdowns


Now, let's have the resulting files have slightly different names, and lets put the 32 bit files in a seperate directory like the 64 bit ones are put : The first two in below list is the settings of Configuration: and Platform: dropdowns. The third column is  what must be typed into Configurration properties->General->Target Name

-Debug  ,Win32 ,$(ProjectName)32d
-Debug  ,x64   ,$(ProjectName)64d
-Release,Win32 ,$(ProjectName)32
-Release,Win64 ,$(ProjectName)64
-click away from the editbox and apply each time before changing the dropdowns

(balance the tree structure - put 32 and 64 bit versions side by side) 
-Select "all configurations" in the left dropdown, and select win32 in the right one
-in configuration properties->general->output directory, type in :
-$(SolutionDir)$(Platform)\$(Configuration)\
-in configuration properties->general->Intermediate Directory, type in
-$(Platform)\$(Configuration)\

(note:VS2010 express only)
-This setting is for the 64 bit builds only.
-Select all configurations in the left dropdown, and select x64 in the right one.
-in configuration properties->general select Windows7.1.SDK



Then... Try to build!

Project->build->batch build->mark all 4, click build.


If batch build succeeds, you should find some new files ():

-C:\Develope\github\tightcsharp\native\tightdb_c_cs\tightdb_c_cs2010\Win32\Debug\tightdb_c_cs201032d.dll
-C:\Develope\github\tightcsharp\native\tightdb_c_cs\tightdb_c_cs2010\Win32\Release\tightdb_c_cs201032.dll
-C:\Develope\github\tightcsharp\native\tightdb_c_cs\tightdb_c_cs2010\x64\Debug\tightdb_c_cs201064d.dll
-C:\Develope\github\tightcsharp\native\tightdb_c_cs\tightdb_c_cs2010\x64\Release\tightdb_c_cs201064.dll

This file in its various versions is the c++ part of the language bind.




If You get "error LNK2038 _MSC_VER value 1600 doesn't match value 1700 link errors when linking,
it is because the .lib files have been built with VS 2010, and are now being linked with VS 2012
This is not possible, You'll have to either use VS2010 to link, or use VS2012 to rebuild new .lib files
The latter option is only possible if You have the entire source-tree for tightdb

I have not verified that below settings are essential for a successfully working build,
but they were set to this instead of defaults in the jni c++ project,so I figured I better mention them.
in project property pages->c/c++->preprocessor->Preprocessor Definitions, define these:
TIGHTDB_DEBUG -  only if this is a debug build
EXAMPLE_EXPORTS - not sure what this one does

in code generation set
Enable Function-Level Linking to Yes/Gy
Enable minimal rebuild to No(/Gm-)

in precompiled headers
set precompiled header to Not  Using Precompiled Headers //not quite sure why we set this, we do include 
stdafx.h
