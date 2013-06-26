@echo off
echo before running this file, You should have built :
echo the c++ part of the C# binding
echo the C# part of the c# binding
echo both inside VS2012 ultimate.
echo This batch file will create all parts of the C# relese.
echo from the results of your build.
Echo.
echo after this has been run, You'll have :
echo.
echo A binding release in TightDbCSharp\release\release
echo An examples release in examples.zip
echo Any key to start creating the c++ release
pause
set location=%~dp0
call %location%native\tightdb_c_cs\tightdb_c_cs2012\build.bat
set location=%~dp0
echo any key to start creating the c# release
pause
call %location%TightDbCSharp\build.bat
set location=%~dp0
echo any key to start creating the pre-setup examples release zip file
pause
:copy needed C# binding files to example projects - note that c++ and C# libararies are distributed in release versions
:user can still debug his own stuff, but our stuff is not meant to be debugged by the user.
copy %location%TightDbCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\TutorialSolution\bin\Release
copy %location%TightDbCSharp\release\files\dll\tightdb_c_cs2012??r.dll %location%examples\TutorialSolution\bin\Debug
copy %location%TightDbCSharp\release\files\bin\Release\*.* %location%examples\lib
:zip entire examples directory to a zip file
del %location%examples.zip
%location%7z.exe a -tzip -r %location%examples.zip %location%examples*.*
echo Finished! Any key....
pause