@echo off
echo Tightdb C# binding c# part Windows Build.
Echo alpha 0.1.1
echo ----------------------------------------------------------
Echo Currently this batch file will create a release version of
echo the already built source of tightDbCSharp
echo The release version will be placed in 
echo (git checkout directory)\release\files\
echo and (git checkout directory)\release\release\
echo the directories are created if they does not exist.
echo prior contents of release tree will be deleted, 
echo except .zip files.
echo the contents of the directory will be c# assembly files
echo in various builds.
echo ----------------------------------------------------------
:echo parametres (%0)
:echo where this file is located (%~dp0)
:echo on
set location=%~dp0
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
set filesdestination=%location%release\files\TightDB
set readmedestination=%location%release\files
set releasedestination=%location%release\release
set zipper=%location%7z.exe
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
:readmedestination is the files directory get rid of that directory and its subdirs
del %readmedestination% /Q /S
rem we keep the release directory as is - it might contain
rem earlier builds, we should not delete those
:create release directory structure in case it isn't there
echo creating release directory structure
md %location%release
md %location%release\files\
md %location%release\files\tightDB\
md %location%release\files\tightDB\NET35\
md %location%release\files\tightDB\NET40\
md %location%release\files\tightDB\NET45\
md %location%release\release\
echo copying release files to release directory
:copy the archiver to the release directory
:copy %zipper% %releasedestination%
:copy C# dll assembly files to the files directory
xcopy %location%TightDBCSharp\bin\AnyCpu\Release\*.dll %filesdestination%\NET45 /s /y
xcopy %location%TightDBCSharp\bin\AnyCpu\Release\*.pdb %filesdestination%\NET45 /s /y
xcopy %location%TightDBCSharp_NET40\bin\AnyCpu\Release\*.dll %filesdestination%\NET40 /s /y
xcopy %location%TightDBCSharp_NET40\bin\AnyCpu\Release\*.pdb %filesdestination%\NET40 /s /y
xcopy %location%TightDBCSharp_NET35\bin\AnyCpu\Release\*.dll %filesdestination%\NET35 /s /y
xcopy %location%TightDBCSharp_NET35\bin\AnyCpu\Release\*.pdb %filesdestination%\NET35 /s /y

:copy C dlls with tightdb core
xcopy %location%native\tightdb_c_cs\tightdb_c_cs2012\release\files\tightdb_c_cs201232r.dll %filesdestination%\NET35 /y
xcopy %location%native\tightdb_c_cs\tightdb_c_cs2012\release\files\tightdb_c_cs201264r.dll %filesdestination%\NET35 /y
xcopy %location%native\tightdb_c_cs\tightdb_c_cs2012\release\files\tightdb_c_cs201232r.dll %filesdestination%\NET40 /y
xcopy %location%native\tightdb_c_cs\tightdb_c_cs2012\release\files\tightdb_c_cs201264r.dll %filesdestination%\NET40 /y
xcopy %location%native\tightdb_c_cs\tightdb_c_cs2012\release\files\tightdb_c_cs201232r.dll %filesdestination%\NET45 /y
xcopy %location%native\tightdb_c_cs\tightdb_c_cs2012\release\files\tightdb_c_cs201264r.dll %filesdestination%\NET45 /y

copy %location%Build_note_Daily.txt %readmedestination%\readme_DailyBuild.txt
copy %location%Install_note.txt %readmedestination%\readme.txt


set location=%~dp0
call %location%update_examples.cmd
set location=%~dp0
xcopy %location%examples\*.* %readmedestination%\examples\* /s /y

:create archive
cd %readmedestination%
%zipper% a -tzip -r %releasedestination%\tightdb_csharp_%reldate%_%reltime%_%reldeveloper% *.*
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
:0.05 create bin directory
:0.06 removed an echo on
:0.07 now copies from the new Release\AnyCpu directory
:0.07 now creates binaries for .net 3.5 .net 4.0 and .net 4.5
:0.08 changed to add install_note to releases
:0.09 changed directory structure to make it even easier to deploy tightdb in an existing project
:0.10 Added examples directory to the standard release