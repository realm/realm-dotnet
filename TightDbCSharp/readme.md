#TightDbCSharp#

This directory contains the VS2012 project that results in the C# language bindings assembly

The assembly consists of two parts

- ..\native\tightDBCalls.cs - p/invoke calls to tightCSDLL.dll.
- .\*.* The classes that make up the language binding.

Only files in the native directory contain calls to c++ code.

Files in the .\ directory are pure clean C#, The only nonstandard stuff is the handle inside the classes that is used to keep track of what c++ class instances they are shadowing.

To recreate the project file, follow these steps (VS2012 express)

- File->New Project->Visual C#->Portable Class Library
- name:TightDbCSharp
- location:C:\Develope\github\tightcsharp (that is - the PARENT directory of the location of this readme file. Change drive and path to reflect the actual location)
- UNMARK "Create directory for solution"
-  Now don't press OK yet. VS2012 will not create a project if its directory already exists, so You'll have to cheat a little. Follow these instructions very carefully:
- use explorer to view C:\Develope\github\tightcsharp (that is the checked out tightcsharp directory)
- rename the directory TightDbCSharp to _TightDbCSharp (You might need to close this readme file first)
- Press OK in VS2012 

The above creates the tightdbCS directory again, and VS2012 continues to the platform selection window
Now, copy the files in tightcsharp\_tightdbCS to tightcsharp\tightdbCS (open a new explorer window, navigato to _tightdbCS, ctrl+click the files, right-click drag them, select copy in the popup mennu )
and then, delete the _tightdbCS directory again

In the taget frameworks menu, select xbox 360, select .net for windows store apps
Then in the dropdown menues, select the higest version combination that does not result in some versions being reduced. At writing time, these are :
.NET Framework 4.03 and higher
Silverlight 4 and higher
Windows Phone 7 and higher
You will also have to reduce the choices to something that will actually build. As of writing the VS2012 compiler will not build siverlight 5 or 4 apps that use
P/invoke (DllImport fails) even though silverlight 5 in fact does have limited P/Invoke support as of now.
As of writing, the following has been selected :
.NET Framework 4.5
.NET for Windows Store apps
These two selections currently build fine
OK

In the resulting project, mark the Class1.cs file in solution explorer and select delete
Now, in tools->options->Text Editor->All Languages->Tabs
select Smart
Select Insert spaces
then OK

in Solution explorer, right click tightdbCS
select add->existing item->mark and select spec.cs amnd table.cs
select add->existing item->
navigate to ..\native
mark TightDBCalls.cs AND THEN SELECT ADD AS LINK in the add button (don't just click add)
(the above add as link ensures that the file is not copied to this project, but instead we use the file in the "native" directory.)
(TO-BE-DONE on a test environment, create a test project for windows phone 8 and see if it will build TightDBCalls.cs reg. P/Invoke calls)

Now Build->Rebuild application
