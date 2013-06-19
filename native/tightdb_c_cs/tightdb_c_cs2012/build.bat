@echo off
echo Tightdb C# binding c++ Windows Build.
Echo alpha 0.01
echo ----------------------------------------------------------
Echo Currently this batch file will create a release version of
echo the already built source in the source directory used by
echo the project residing where the batch 
echo file resides. The release version will be placed in 
echo (git checkout directory)\release\cppfiles\
echo and (git checkout directory)\release\cpprelease\
echo the directories are created if they does not exist.
echo prior contents of release tree will be deleted, 
echo except .7z files.
echo the contents of the directory will be c++ dll files
echo in various builds, as well as the C# assembly in various
echo builds
echo in other words, release will contain 
echo the c++ part of the binding to be consumed by a customer, 
echo using echo at least visual studio 2012 express on windows
echo the batch file that creates the C# part of the binding
echo will wrap both c++ and c# stuff up into a file for the 
echo binding customer
echo ----------------------------------------------------------
echo parametres (%0)
echo where this file is located (%~dp0)
set location=%~dp0
set date=xdatex
set time=xtimex
set developer=xdeveloperx
echo this batchfile is located at %location%
echo -
echo Press Any key to proceed, or close the window to abort
pause
:remove any files from a prior release
echo cleaning up from earlier releases
del %location%release\files\cppfiles /Q
del %location%release\files\cppfiles\Win32 /Q
del %location%release\files\cppfiles\x64 /Q
rem we keep the release directory as is - it might contain
rem earlier builds, we should not delete those
:create release directory structure in case it isn't there
echo creating release directory structure
md %location%release
md %location%release\cppfiles\
md %location%release\cppfiles\win32
md %location%release\cppfiles\x64
echo copying release files to release directory
:copy the archiver to the release directory
copy %location%7z.exe %location%\release\vs2012\release
;copy dll files to the files directory
copy %location%native\tightdb_c_cs\tightdb_c_cs2012\Win32\*.dll %location%\release\cppfiles\
copy %location%native\tightdb_c_cs\tightdb_c_cs2012\x64\*.dll %location%\release\cppfiles\
:revision history
:0.01 
:First version. Basically copies the dll files out from the project and into a release directory
:for further processing by the C# binding release build.bat file
