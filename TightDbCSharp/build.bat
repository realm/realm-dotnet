@echo off
echo Tightdb C# binding c# part Windows Build.
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
echo the contents of the directory will be c# assembly files
echo in various builds.
echo this build will also copy files from the c interface
echo so that a complete C# binding release can be made
echo the files are copied from the c release\files directory
echo so the files used are from the latest release of the c
echo interface (not neccessarily the currently checked out 
echo source)
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
set zipper=%location%..\7z.exe
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
del %filesdestination%\bin /Q
del %filesdestination%\dll /Q
rem we keep the release directory as is - it might contain
rem earlier builds, we should not delete those
:create release directory structure in case it isn't there
echo creating release directory structure
md %location%release
md %location%release\files\
md %location%release\files\dll\
md %location%release\release\
echo copying release files to release directory
:copy the archiver to the release directory
:copy %zipper% %releasedestination%
:copy C# dll assembly files to the files directory
xcopy %location%bin\*.dll %filesdestination%\bin /s
xcopy %location%bin\*.pdb %filesdestination%\bin /s
echo on
:copy C dlls with tightdb core
xcopy %location%..\native\tightdb_c_cs\tightdb_c_cs2012\release\files\*.* %filesdestination%\dll

copy %location%Build_note_Daily.txt %filesdestination%\readme_DailyBuild.txt
copy %location%Install_note.txt %filesdestination%\readme.txt
:create archive
cd %filesdestination%
%zipper% a -tzip -r %releasedestination%\tightdb_csharp_%xdatex%_%xtimex%_%xdeveloperx% *.*
:copy the readme once again, this time outside the archive
copy %location%Install_note.txt %releasedestination%\readme.txt

:revision history
:0.01 
:First version. Basically copies the dll files out from the project and into a release directory
:and then compresses them to a release archive
:0.02 actually works now, 7z path fixed
:0.03 adapted for c# binding use. Currently might copy more files than actually needed
:although we only take dll and pdb files
pause
:0.04 added some readme files to the release