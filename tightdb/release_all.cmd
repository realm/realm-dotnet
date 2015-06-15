@echo off
echo before running this file, You should have built :
echo the c++ part of the C# binding
echo the C# part of the C# binding
echo both inside VS2012 ultimate.
echo This batch file will create all parts of the C# relese.
echo from the results of your build.
Echo.
echo after this has been run, You'll have :
echo.
echo A binding release in TightDbCSharp\release\release
echo A binding release image in TightDbCSharp\release\files
echo An examples release in examples.zip
echo Any key to start creating the c++ release
pause
set location=%~dp0
call %location%native\tightdb_c_cs\tightdb_c_cs2012\release.cmd 
set location=%~dp0

echo any key to start creating the c# release
pause
call %location%\Release.cmd
set location=%~dp0

echo any key to start creating the pre-setup examples release zip file
pause
:copy needed C# binding files to example projects - note that c++ and C# libararies are distributed in release versions
:user can still debug his own stuff, but our stuff is not meant to be debugged by the user.

:call %location%update_examples.cmd
:set location=%~dp0

:zip entire examples directory to a zip file
:del %location%examples.zip
:this one fails on my other pc, creates an empty zip file
:%location%7z.exe a -tzip -r %location%examples.zip %location%examples\*.*

echo any key to update documentation directory with new versions of sourcecodes
pause
call %location%update_docs.cmd
set location=%~dp0

echo Finished! Any key....
pause
