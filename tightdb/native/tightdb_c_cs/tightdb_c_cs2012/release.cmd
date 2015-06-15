@echo off
echo Tightdb C# binding c++ Windows Build.
Echo alpha 0.02
echo ----------------------------------------------------------
echo press any key to populate release directory with
echo a set of DLL files used by C# programs that access tightdb.
echo .
echo In general, relase.cmd will create release directories
echo with release versions of the already built solution in the
echo same directory as where the solution file is.
echo The release version will be placed in a directory called
echo release, in two sub directories files\ and release\
echo files\ will be an uncompressed release of all needed files
echo laid out in a logical way, and release\ will contain
echo a zipped version of the files\directory - named according
echo to the time the winrelease.cmd file was run
echo the zipfile will also contain the username from the
echo computer that generated the zipfile
echo the contents of the directory will be c++ dll files
echo in various builds.
echo these dll's will contain methods called by the C# binding
echo the dll's contain compiled core code
echo This release is in effect our C interface release on windows
echo It is consumed by the C# binding,but could also
echo be adapted to be used by bindings for other 
echo windows languages. biggest task would be to create
echo a header file, describing the contents of the dll
echo ----------------------------------------------------------
:echo parametres (%0)
:echo where this file is located (%~dp0)
:echo on

set location=%~dp0
set reldate=%date:~6,4%-%date:~3,2%-%date:~0,2%
set reltime=%time::=%
set reltime=%reltime: =%
set reltime=%reltime:,=%
set reldeveloper=%USERNAME: =%
set reldeveloper=%reldeveloper:,=%
echo this batchfile is located at %location%
echo -
echo - release date will be set to %reldate%
echo - release time will be set to %reltime%
echo - release user will be set to %reldeveloper%
echo -
echo Press Any key to proceed, or close the window to abort
set filesdestination=%location%release\files
set releasedestination=%location%release\release
set zipper=%location%..\..\..\7z.exe
:dir %filesdestination%
:dir %releasedestination%
:dir %zipper%
:dir %location%
:echo this batchfile is located at %location%
:echo -
:echo Press Any key to proceed, or close the window to abort
:pause
:remove any files from a prior release

echo cleaning up from earlier releases
del %filesdestination% /Q
rem we keep the release directory as is - it might contain
rem earlier builds, we should not delete those
:create release directory structure in case it isn't there

echo creating release directory structure
md %location%release
md %location%release\files\
md %location%release\release\

echo copying release files to release directory
:copy the archiver to the release directory
:copy %zipper% %releasedestination%
:copy dll files to the files directory
copy %location%Win32\debug\*.dll %filesdestination%
copy %location%Win32\release\*.dll %filesdestination%
copy %location%x64\debug\*.dll %filesdestination%
copy %location%x64\release\*.dll %filesdestination%
:create archive
cd %filesdestination%
%zipper% a -tzip -r %releasedestination%\tightdb_c_%reldate%_%reltime%_%reldeveloper% *.*

:revision history
:0.01 
:First version. Basically copies the dll files out from the project and into a release directory
:and then compresses them to a release archive
:0.02 actually works now, 7z path fixed
pause