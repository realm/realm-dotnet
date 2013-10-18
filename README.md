#tightdb_csharp#

C# language binding for TightDB

Ver 0.5

This directory and its subdirectories contain the VS2012 solution that is needed to produce the C# binding for tightdb

##Build instructions##

These instructions use some concrete file paths as examples,
it is assumed that You have chekced out tightdb_csharp from github into e:\Wincoder\Develope\tightdb_csharp.
You can of course check out anywhere You like and it will stil work, if You replace the above path with Your
chosen path. The solution itself uses relative paths and is not dependent on any specific location.
However, if You chose to build core itself too, core and the TightDBCSharp binding must be located side by side,
such that ..\tightdb from the C# binding will be the correct path to the core checkout directory.
(or else you cannot have the core release updated automatically by running a batchfile and will have to do some
manual work)

You need a copy of the release dir from a VS2012 built C++ binding distribution.  
Currently this can be obtained by asking Lasse or Dennis or Brian for a VS2012 release.

If You have a copy of the release dir, skip all the way down to 19

##Core Build##

Building the c++ binding in windows with VS2012 (not express) can be done as follows :  

1. check out tightdb master to a directory alongside the csharp binding, e.g. if this file is located at  
  E:\Wincoder\Develope\tightdb_csharp\README.md  
  then make sure the tightDB.sln file is in  
  E:\Wincoder\Develope\tightdb\tightDB.sln    
2. open tightDB.sln in VS2012
3. right click "Solution 'TightDB' (8 projects) in Solution Explorer
4. select UpdateVC++ Projects
5. in the popup "UpdateVC++ Compiler and Libraries" click Update
6. wait while VS2012 updates the projects.
7. select Build->Batch Build
8. unmark all checkboxes under build, mark the 4 named TightDB Debug Win32, tightDB Debug x64, TightDB Release Win32, TightDB Release x64 - click Clean.  Output should report Clean : 8 succeeded, 0 failed, 0 skipped
9. select Build->Batch build, click REBUILD 
10. look for compiler warnings, verify that any warnings are marked with fixme in the code, report any new warnings to core developers.
11. if the build succeeded, run the 4 projects one by one and make sure the unit tests pass, report non passing tests to core developers
12. build->batch build - unmark all , mark the 4 called tightDB static libraray
13. click clean
14. build->batch build - click build all
15. look for compiler warnings as in j
16. now, in explorer, locate and call Windows\winrelease2012.cmd (relative to the solution file) this should create a release and update the winows\VS2012 release dir contents
17. navigate to the CSHARP binding and run the batch file that updates the native part of the CHSARP binding from the core release :
18. in explorer navigate to, and run E:\Wincoder\Develope\tightdb_csharp\native\libsVS2012\copyfromcore.cmd Answer All if asked

##C++ DLL BUILD##  

1. If You did not build core Yourself, extract a VS2012 release into H:\Wincoder\Develope\tightdb_csharp\native\libsVS2012 so that libsvs2012 now contain tightDB32d.lib and 3 other similar named lib files, as well as a src directory structure with header files in it.
2. Open up the tightdb_c_cs2012 solution found in H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012 
3. select build->batch build. select the 4 projects called tightdb_c-cs2012
4. click clean.  select build->batch build. click rebuild.
5. when VS2012 shows Rebuild All: 4 succeeded, mark the solution in solution explorer, right click, select Open folder in file explorer. doubleclick the file release.cmd  
if prompted, type Yes or All where applicable  

You have now built the C++ part of the C# binding, and created a release of that, in the  
H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012\release directory  

The files in the release directory will be used when running unit tests and examples that use the C# binding, but they are not used when building the binding itself, or building unit tests and examples

##C SHARP BINDING BUILD##

1. navigate to H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp  and open the TightDbCsharp.sln solution VS2012

2. select build->batch build and select all 30 configurations. then click clean.

3. select build->batch build and click rebuild (wait until you get Rebuild all: 30 succeeded)

4 mark the solution in solution explorer, right click, select Open folder in file explorer.

5 If you want to make a release - do the following :

##C SHARP BINDING RELEASE##

1. Convert Install_Note.txt and Build_Note_Daily.txt to windows line endings (Use Notepad++ to edit->Eol conversion->Windows Endings)

2. Doubleclick the file release_all.cmd.  

Answer All if asked, or Yes if asked and All cannot be answered. press enter a few times.

At this point You have a release of the C# binding in the H:\Wincoder\Develope\tightdb_csharp\release directory. This release is all that You need to ship to a customer.

The customer will need to reference TightDbCSharp.dll in their .net projects, and furthermore,  they must ensure at runtime that the tightdb_c_csNNX.dll files (4 of them) can be found by their programs. Usually it is far simplest simply to put the 4 tightdb_c_cs dll's the same place where the TightDbCSharp.dll is deployed. In almost all cases it is sufficient to just deploy the 2 dll's that end with r.dll - the ones with d.dll are debug versions and should not be used normally at all.

