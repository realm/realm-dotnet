#TightCSDLL#

This directory contains c++ project and files that are used to build the tightCSDLL dll file.

The tightCSDLL file contains extern "C" P/Invoke methods, that call on into c++ tightdb

The C# binding will use the methods in the DLL access tightdb.

This directory contains two solutions that links to the same sourcefiles :
tightCSDLL2012.sln - used to build with visual studio 2012 (not there yet, awaiting new lib files)
tightCSDLL2010.sln - used to build with visual studio 2010


###Getting VS2010 or VS2012 ready to build the project###

These are instructions to get going from a fresh version of visual studio 2010, without using the existing project file.(thus starting from no dependencies on anything)  

The end result will be a project directory below the directory where this file is, with project and build related stuff, You can have VS2010 and VS2012 side by side and share the same source files. The slightly weird directory layout is because VS2010 insists on naming a directoy the same as the project, even when You tell it not to. VS2012 has this issue fixed, but having 2012 in this dir, and 2010 in a subdir would be an unbalanced mess. At least we now have a balanced mess.

+(VS2012:stuff that is different with VS2012 are written like this)+  

Prerequesites :  

A directory structure like this :

C:\develope\github\tightcsharp  (this path is used as an example throughout this document)

so that this readme.txt file is located in

c:\develope\github\tightcsharp\native

It is assumed that You have checked out the csharp binding into the 
c:\develope\github\tightcsharp directory
This readme.md file should then be located at 
C:\Develope\github\tightcsharp\native\tightCSDLL\readme.md


Visual studio 2010 Express installed (or better)  
+or+  
Visual studio 2012 Express installed (or better)  

The library files needed are :
- tightdb32.lib
- tightdb32d.lib
- tightdb64.lib
- tightdb64d.lib
place VS2012 Library files in ..\..\libs\libsVS2012 (aka c:\develope\github\tightcsharp\libs\libsVS2012)  
place VS2010 Library files in ..\..\libs\libsVS2010 (aka c:\develope\github\tightcsharp\libs\libsVS2010)  

tightdb header files for tightdb<32|64>[d].lib:
taking the header files from the unix build will do, You probably also need the windows header files-these are not in the unix build,
and not on the github java binding branch, You have to obtain them from someone at tightdb.

The tightdb header files are assumed to be found in  
..\..\..\..\tightdb     (aka c:\develope\tightdb )
..\..\..\..\tightdb\win32  (aka c:\develope\tightdb\win32 )

note that the header files are looked for in a directory that is not a part of this project tree.  

#Setting up VS2010 or 2012 to build the c++ binding#

Now, start Microsoft C++ 2010 Express (or 2012 Express)

mark tools->settings->expert settings if not already marked


file->new project->general.  
select empty project (select in this document means singleclick or otherwise make the item highligted, not doubleclick or otherwise activating its function)  
name:tightCSDLL2010  
+(vs2012:  name:tightCSDLL2012)+  
location:same directory as this readme.txt file  
untick "create directory for solution"  
click OK  (this should create a new directory tightCSDLL\tightCSDLL2010)


In Solution Explorer   
right click tightCSDLL2010->header files
select add->existing.  
navigate up one directory to Native
navigate down one to tightCSDLL
mark and select the following files:  
- stdafx.h
- tightCSDLL.h

right click tightCSDLL->Source Files.
select add->existing  
mark and select the following files:  
- stdafx.s
- TightCSDLL.s



Set a reference to tightdb32.lib in visual studio:  

View Solution explorer. 
right click tightCSDLL2010 (VS2012:tightCSDLL2012) - select properties  
select configuration properties->VC++ Directories
select Library Directories, click the down error and select edit  
type in ..\..\libs2010\   (aka C:\develope\github\tightdbcs\libs\libs2010)
+( vs2012:type in ..\..\libs2012\ (aka C:\develope\github\tightdbcs\libs\libs2010) )+


Configuration properties->linker->input
select additional dependencies, the arrow down symbol, edit  
type TightDB32d.lib
click apply  


select configuration properties->General
select Configuration Type, change it to Dynamic Library (.dll)


select configuration properties->c/C++->Preprocessor
select Preprocessor Definitions, click arrow down and edit  
type in TIGHTCSDLL_EXPORTS  
click OK

in  configuration properties->c/c++->additional include directories

click arrow down,select edit. Add:  
..\..\..\..\tightdb\  
..\..\..\..\tightdb\win32\  

(note - the C/c++ steps must be done after some c++ files have been added to the project, as the c/c++ menu is hidden until some c++ files are added)


Then... Try to build!




If You get "error LNK2038 _MSC_VER value 1600 doesn't match value 1700 link errors when linking,
it is because the .lib files have been built with VS 2010, and are now being linked with VS 2012
This is not possible, You'll have to either use VS2010 to link, or use VS2012 to rebuild new .lib files
The latter option is only possible if You have the entire source-tree for tightdb

I have not verified that below settings are essential for a successfully working build,
but they were set to this instead of defaults,so I figured I better mention them.
in project property pages->c/c++->preprocessor->Preprocessor Definitions, define these:
TIGHTDB_DEBUG -  only if this is a debug build
EXAMPLE_EXPORTS - not sure what this one does

in code generation set
Enable Function-Level Linking to Yes/Gy
Enable minimal rebuild to No(/Gm-)

in precompiled headers
set precompiled header to Not  Using Precompiled Headers //not quite sure why we set this, we do include 

stdafx.h
