#TightDbCSharp#

This directory contains the VS2012 project that results in the C# language bindings assembly

The assembly consists of two parts

- ..\native\tightDBCalls.cs - p/invoke calls to tightdb_c_cs.dll.
- .\*.* The classes that make up the language binding.

Only files in the native directory contain calls to c++ code.

Files in the .\ directory are pure clean C#, The only nonstandard stuff is the handle inside the classes that is used to keep track of what c++ class instances they are shadowing.

To recreate the project file, follow these steps (VS2012 express)

- File->New Project->Visual C#->Class Library
- name:TightDbCSharp
- location:C:\Develope\github\tightcsharp (that is - the PARENT directory of the location of this readme file. Change drive and path to reflect the actual location)
- UNMARK "Create directory for solution"
-  Now don't press OK yet. VS2012 will not create a project if its directory already exists, so You'll have to cheat a little. Follow these instructions very carefully:
- use explorer to view C:\Develope\github\tightcsharp (that is the checked out tightcsharp directory)
- rename the directory TightDbCSharp to _TightDbCSharp (You might need to close this readme file first)
- Press OK in VS2012 

The above creates the tightdbCS directory again

Now, copy the files in tightcsharp\_TightDbCSharp to tightcsharp\TightDbCSharp :

- click tightdbcsharp\TightDbCSharp in an explorer window - it should show an empty folder
- open another explorer window, 
- navigate to _tightdbCS, 
- ctrl+A to select all files and directories 
- right-click drag them to the empty folder, select copy in the popup mennu 
- and then, delete the _tightdbCS directory again

- In solution explorer, in the resulting project, mark the Class1.cs file and select delete

- Now, in tools->options->Text Editor->All Languages->Tabs
- select Smart
- Select Insert spaces
- then OK

in Solution explorer, right click TightDbCSharp
- select add->existing item->mark and select spec.cs amnd table.cs accept with ADD
- select add->existing item-> navigate to ..\native mark NativeCalls.cs AND THEN SELECT ADD AS LINK in the add button (don't just click add)

(the above add as link ensures that the file is not copied to this project, but instead we use the file in the "native" directory.)
(TO-BE-DONE on a test environment, create a test project for windows phone 8 and see if it will build TightDBCalls.cs reg. P/Invoke calls)

Now do a Build->Rebuild application to check that all is okay

At this point You should have a working AnyCpu version of TightDbCSharp assembly that passes all the unit tests in TestTightdbCS.sln (also AnyCpu) on windows 7, 64 bit, running as a 64 bit process if started from the 64 bit Nunit unit tester.

Select build->batch build, mark the two projects and click build.

If the build is successfull,  You will now have two files called :
TightDbCSharp\Debug\TightDbCsharp.dll
TightDbCSharp\Release\TightDbCsharp.dll
Both these files are of the "AnyCpu" type - they will work on 32 as well as 64 bit - but the end user must ensure that the correct tightdb_c_cs.dll file has also been deployed. What file to use depends on wether the program will run as 32 or as 64 bit, and wether the program should run in debug or release mode.
