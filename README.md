#tightdb_csharp#

C# language bindings for TightDB  
Ver 0.2

This directory and its subdirectories contain the *developer* version of tightdb_csharp - this is the project that is needed to produce the *customer* version of tightdb_cshap, that is shipped to customers who access tightdb from within C#.  

##Build instructions##


These instructions use file paths as examples, these paths are of course relative, but an example for
the case where You have chekced out tightdb_csharp from github into H:\Wincoder\Develope\tightdb_csharp

1) You need a copy of the release dir from a VS2012 built C++ binding distribution.
Currently this can be obtained by asking Lasse or Dennis or Brian for a VS2012 release. Building the c++ binding in windows with VS2012 (not express) is not documented for now.

2) This release dir should contain a zipped file, for instance :
H:\Wincoder\Develope\tightdb\release\vs2012\release\tightdb_cpp_VS2012___.zip
3) extract that file into H:\Wincoder\Develope\tightdb_csharp\libsVS2012 so that libsvs2012 now contain
tightDB32d.lib and 3 other similar named lib files, as well as a src directory structure with header files in it.
4) Open up the tightdb_c_cs2012 solution found in H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012
5) select build->batch build. select the 4 projects called tightdb_c-cs2012
6) click rebuild.
7) when VS2012 shows Rebuild All: 4 succeeded, open file explorer (click start button, click computer) , go to the directory 
H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012 click the file build.bat
if prompted, type Yes or All where applicable
You have now built the C++ part of the C# binding, and created a release of that, in the 
H:\Wincoder\Develope\tightdb_csharp\native\tightdb_c_cs\tightdb_c_cs2012\release directory

The files in the release directory will be used by programs that use the C# binding, but they are not used when building the binding itself.


To build the binding :

navigate to H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp  and click the TightDbCsharp.sln solution.
select build->batch build and select both configurations. then click rebuild.
when You get Rebuild all: 2 succeeded, open file explorer (click start button, click computer) and navigate to H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp and click build.bat. Anser All if asked, or Yes if asked and All cannot be answered (yea' this is a hack - it's work in progress)

At this point You have a release of the C# binding in the H:\Wincoder\Develope\tightdb_csharp\TightDbCSharp\release directory. This release is all that You need to ship to a customer.

