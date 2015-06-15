:@echo off
echo This cmd file will update the examples solutions with newest released c++ dll's and c# libraries
echo note that examples will be running with release versions of the c# library and c++ dll
echo but of course they will still be able to be debugged - User just can't debug down into the binding itself
echo and core will always run non-debug, full speed
echo ...Updating C# libraries
set location=%~dp0
:First, delte all unneccessary files from examples
del DynamicTable\bin\*.* /s /q
del DynamicTable\obj\*.* /s /q
del TutorialSolution\bin\*.* /s /q
del TutorialSolution\obj\*.* /s /q
del PerformanceTest\bin\*.* /s /q
del PerformanceTest\obj\*.* /s /q
del Experimental\bin\*.* /s /q
del Experimental\obj\*.* /s /q
del UnityExample\bin\*.* /s /q
del UnityExample\obj\*.* /s /q

:then, update examples with newest tightdb binding
xcopy %location%release\files\tightDB\NET45\*.* %location%examples\DynamicTable\* /s /y
xcopy %location%release\files\tightDB\NET45\*.* %location%examples\TutorialSolution\* /s /y
xcopy %location%release\files\tightDB\NET45\*.* %location%examples\PerformanceTest\* /s /y
xcopy %location%release\files\tightDB\NET45\*.* %location%examples\Experimental\* /s /y
:unity in 32 bit currently only works with .net35 and lower so use that binding
xcopy %location%release\files\tightDB\NET35\*.* %location%examples\UnityExample\* /s /y
:copy %location%UnityGettingStarted.txt %location%examples\UnityExample\
:copy %location%Unitytutorialsource.cs %location%examples\UnityExample\
pause