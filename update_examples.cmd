@echo off
echo This cmd file will update the examples solutions with newest released c++ dll's and c# libraries
echo note that examples will be running with release versions of the c# library and c++ dll
echo but of course they will still be able to be debugged - User just can't debug down into the binding itself
echo and core will always run non-debug, full speed
echo ...Updating C# libraries
set location=%~dp0
copy %location%TightDbCSharp\release\files\bin\AnyCpu\Release\tightDbCSharp.* %location%examples\lib
echo ...Updating c++ dll files
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\DynamicTable\bin\Debug
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\DynamicTable\bin\Release
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\TutorialSolution\bin\Debug
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\TutorialSolution\bin\Release
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\PerformanceTest\bin\Debug
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\PerformanceTest\bin\Release
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\Experimental\bin\Debug
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\Experimental\bin\Release
copy %location%\TightDBCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\UnityExample\
copy %location%TightDbCSharp_3.5\bin\AnyCpu\Release\tightDbCSharp.* %location%examples\UnityExample\
copy %location%\UnityGettingStarted.txt %location%examples\UnityExample\
copy %location%\Unitytutorialsource.cs %location%examples\UnityExample\
pause