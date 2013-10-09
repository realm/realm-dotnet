:@echo off
echo This cmd file will update the examples solutions with newest released c++ dll's and c# libraries
echo note that examples will be running with release versions of the c# library and c++ dll
echo but of course they will still be able to be debugged - User just can't debug down into the binding itself
echo and core will always run non-debug, full speed
echo ...Updating C# libraries
set location=%~dp0
copy %location%release\files\lib\NET45\tightDbCSharp.* %location%examples\lib\NET45
copy %location%release\files\lib\NET45\tightDbCSharp.* %location%examples\lib\NET40
copy %location%release\files\lib\NET45\tightDbCSharp.* %location%examples\lib\NET35
echo ...Updating c++ dll files
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\DynamicTable\bin\Debug
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\DynamicTable\bin\Release
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\TutorialSolution\bin\Debug
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\TutorialSolution\bin\Release
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\PerformanceTest\bin\AnyCpu
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\PerformanceTest\bin\X64
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\PerformanceTest\bin\X86
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\Experimental\bin\Debug
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\Experimental\bin\Release
copy %location%release\files\dll\tightdb_c_cs2012??r.dll %location%examples\UnityExample\
copy %location%UnityGettingStarted.txt %location%examples\UnityExample\
copy %location%Unitytutorialsource.cs %location%examples\UnityExample\
pause