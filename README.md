#tightdb_csharp#

C# language bindings for TightDB  
Ver 0.006  


This directory and its subdirectories contain the **developer** version of tightdb_csharp - tihs is the project that is needed to produce the **customer** version of tightdb_cshap, that is shipped to customers who access tightdb from within C#.  

##Build instructions##

0) make sure You have visual studio 2010 express installed (as a minimum)  
1) open a commandline window, cd to the directory tighttdb_csharp  
2) run the builddist.bat file (will produce fresh dll files)  
3) run the createdist.bat file in this directory - it will copy the needed files to the DIST directory, which will then contain all that  
is needed for a user install  
4) (to be done) - run the createinstalldist.bat file  - this will create an archive wiht the DIST directory files, ready for deployment  

##User install contents##

- the tightCSDLL dll file (a c++ DLL), which has the tightdb.lib files built into it  
- the tightdbCS.dll file (a C# managed DLL), which contains the classes a C# tightdb user will use, and which calls tightCSDLL to get things done. The marshalling calls to tightCSDLL are placed in a protected area of the source, the customer do not have access to these calls, only the provided C# classes and methods.
- the unit test for tightdbCS (to be done)  
- a project with some sample code similar to the web tutorial (to be done)  
- a readme.txt file explaining step-by-step how to create a new project that uses tightdbCS, as well as how to integrate tightdbCS in an already created project
- a support.txt file that contains links and contact info for the tightdb company