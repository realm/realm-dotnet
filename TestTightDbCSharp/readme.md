#TestTightDbCSharp#

This solution contains unit tests for the TightDbCSharp binding

The solution also contains stubs that call the individual tests directly, making it possible to debug the unit tests

To create this solution from scratch, You would need to do the following :


- File->New Project->Visual C#->Console Application
- name:TestTightDbCSharp
- location:C:\Develope\github\tightcsharp\ (that is - the PARENT directory of the location of this readme file. Change drive and path to reflect the actual location)
- UNMARK "Create directory for solution"
- The dropdown should state "create new solution"
-  Now don't press OK yet. VS2012 will not create a project if its directory already exists, so You'll have to cheat a little. Follow these instructions very carefully:
- use explorer to view C:\Develope\github\tightcsharp (that is the checked out tightcsharp directory)
- rename the directory TightDbCSharp to _TightDbCSharp (You might need to close this readme file first)
- Press OK in VS2012 

The above creates the TestTightDbCSharp directory again.
Now, copy the files in Tightcsharp\_TightDbCSharp to tightcsharp\TightDbCSharp :

- click tightdbcsharp\TightDbCSharp in an explorer window - You will see a few directories and some files.
- open another explorer window, 
- navigate to _tightdbCS, 
- ctrl+A to select all files and directories 
- right-click drag them to the empty folder, select copy in the popup mennu 
- and then, delete the _tightdbCS directory again


in solution explorer :

- right click the project TightDbCSharp, select Add reference
- navigate to, and add C:\Program Files (x86)\NUnit 2.6.2\bin\framework\nunit.framework.dll
- right click the project TightDbCSharp, select Add reference
- navigate and add C:\Develope\github\tightcsharp\TightDbCSharp\bin\Debug\TightDbCSharp.dll

- right click the project TightDbCSharp, select properties
- in pre-build event command line, write $(ProjectDir)\getdll.bat
- in click code analysis
- set the configuration dropdown to release
- set the platform to all platforms
- DESELECT enable code analysis on build
- set the configuration dropdown to debug
- SELECT enable code analysis on build
- select "Microsoft all rules"

build->batch build, select both and click build

The test can be run in VS or You can open nunit and select the TestTightDbCSharp.dll and click run to run the unit tests