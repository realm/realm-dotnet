tightdb_csharp
=============

C# language bindings for TightDB

Ver 0.003

This directory and its subdirectories contain the DEVELOPER version of tightdb_csharp - tihs is the project that is needed to
produce the CUSTOMER version of tightdb_cshap, that is shipped to customers who access tightdb from within C#.
Build instructions :

0) make sure You have visual studio 2010 express installed (as a minimum)
1) open a commandline window, cd to the directory tighttdb_csharp
2) run the builddist.bat file (will produce fresh dll files)
3) run the createdist.bat file in this directory - it will copy the needed files to the DIST directory, which will then contain all that
is needed for a user install
4) (to be done) - run the createinstalldist.bat file  - this will create an archive wiht the DIST directory files, ready for deployment


A user install will contain :


a - the tightCSDLL dll file (a c++ DLL), which has the tightdb.lib files built into it
b - the tightdbcalls.dll file (a c# managed DLL), which exposes flat C# call headers with sane C# types, and calls on to tightCSDLL with c++ and c type headers and calls. Also takes care of error handling, memory issues , mashalling etc.
c - the tightdbCS.dll file (a C# managed DLL), which contains the classes a C# tightdb user will ues, and which cals tighdbcalls.dll to get things done
d - the unit test for tightdbCS (to be done)
e - a project with some sample code similar to the web tutorial (to be done)
f - a readme.txt file explaining step-by-step how to create a new project that uses tightdbCS, as well as how to integrate tightdbCS in an already created project
