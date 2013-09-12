@echo off
echo Tightdb C# binding c++ Windows Build.
Echo alpha 0.02
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
echo in various builds.
echo This release is in effect our C interface release on windows
echo It is consumed by the C# binding,but could also
echo be used by bindings for other windows languages.
echo ----------------------------------------------------------
:echo parametres (%0)
:echo where this file is located (%~dp0)
:echo on

set location=%~dp0
set date=xdatex
set time=xtimex
set developer=xdeveloperx
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
%zipper% a -tzip -r %releasedestination%\tightdb_c_%xdatex%_%xtimex%_%xdeveloperx% *.*

:revision history
:0.01 
:First version. Basically copies the dll files out from the project and into a release directory
:and then compresses them to a release archive
:0.02 actually works now, 7z path fixed
pause