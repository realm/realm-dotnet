#tightdb_csharp#

C# language bindings for TightDB  
Ver 0.5

This directory and its subdirectories contain the *developer* version of tightdb_csharp - this is the project that is needed to produce the *customer* version of tightdb_cshap, that is shipped to customers who access tightdb from within C#.  

##Build instructions##


These instructions use file paths as examples, these paths are of course relative, but an example for
the case where You have chekced out tightdb_csharp from github into e:\Wincoder\Develope\tightdb_csharp

1) You need a copy of the release dir from a VS2012 built C++ binding distribution.
Currently this can be obtained by asking Lasse or Dennis or Brian for a VS2012 release. Building the c++ binding in windows with VS2012 (not express) can be done as follows :

a) check out tightdb master to e:\Wincoder\Develope\tighdb
b) open tightDB.sln in VS2012
c) right click "Solution 'TightDB' (8 projects) in Solution Explorer
d) select UpdateVC++ Projects
e) in the popup "UpdateVC++ Compiler and Libraries" click Update
f) wait while VS2012 updates the projects.
g) select Build->Batch Build
h) unmark all checkboxes under build, mark the 4 named TightDB Debug Win32, tightDB Debug x64, TightDB Release Win32, TightDB Release x64 - click Clean.  Output should report Clean : 8 succeeded, 0 failed, 0 skipped
i) select Build->Batch build, same as h) but end up Clicking REBUILD instead of CLEAN
j) look for compiler warnings, verify that any warnings are marked with fixme in the code, report any new warnings to core developers.
j) if the build succeeded, run the 4 projects one by one and make sure the unit tests pass, report non passing tests to core developers
k) build->batch build - unmark all , mark the 4 called tightdb static libraray
l) click clean
m) build->batch build - unmark all , mark the 4 called tightdb static libraray click build all
o) look for compiler warnings as in j
p) now, call winrelease.cmd this should create a release and update the VS2012 release dir contents



2) This release dir should contain a zipped file, for instance :
H:\Wincoder\Develope\tightdb\release\vs2012\release\tightdb_cpp_VS2012___.zip
3) extract that file into H:\Wincoder\Develope\tightdb_csharp\native\libsVS2012 so that libsvs2012 now contain
tightDB32d.lib and 3 other similar named lib files, as well as a src directory structure with header files in it.
4) Open up the tightdb_c_cs2012 solution found in H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012
5) select build->batch build. select the 4 projects called tightdb_c-cs2012
6) click rebuild.
7) when VS2012 shows Rebuild All: 4 succeeded, open file explorer (click start button, click computer) , go to the directory 
H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012 click the file release.cmd
if prompted, type Yes or All where applicable
You have now built the C++ part of the C# binding, and created a release of that, in the 
H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012\release directory

The files in the release directory will be used when running unit tests and examples that use the C# binding, but they are not used when building the binding itself, or building unit tests and examples

To build the binding :

navigate to H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp  and click the TightDbCsharp.sln solution.
select build->batch build and select both configurations. then click rebuild.
when You get Rebuild all: 2 succeeded, open file explorer (click start button, click computer) and navigate to H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp and click release.bat Answer All if asked, or Yes if asked and All cannot be answered (yea' this is a hack - it's work in progress)

At this point You have a release of the C# binding in the H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp\release directory. This release is all that You need to ship to a customer.

The customer will need to reference TightDbCSharp.dll in their .net projects, and furthermore,  they must ensure at runtime that the tightdb_c_csNNX.dll files (4 of them) can be found by their programs. Usually it is far simplest simply to put the 4 tightdb_c_cs dll's the same place where the TightDbCSharp.dll is deployed. In allmost all cases it is sufficient to just put the 2 dll's that end with r.dll - the ones with d.dll are debug versions and should not be used normally at all.
