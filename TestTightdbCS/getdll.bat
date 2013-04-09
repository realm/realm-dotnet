echo off
:  Copy the c++ DLLs to the project dir for each solution platform
:  This enables easy debugging against the very latest build of c++ tightdb_c_cs2010 dll
:  The C# dll should be copied by the VS2012 project on build, as it is referenced as a C# assembly
:  The C++ dll cannot be referenced,so is not copied automatically
:  This script copies all 4 versions of the dll each time a build is made, so a batch build will run the script 4 times. However, the copy command will
:  not copy the file if the source is the same as the destination (measured by creation date)
echo Copying c++ dlls to tightCSDLL.DLL
echo below is a dir of the current directory when building
dir
set sdirx86deb=..\..\..\..\native\tightdb_c_cs\tightdb_c_cs2010\Win32\Debug\
set sdirx86rel=..\..\..\..\native\tightdb_c_cs\tightdb_c_cs2010\Win32\Release\
set sdirx64deb=..\..\..\..\native\tightdb_c_cs\tightdb_c_cs2010\x64\Debug\
set sdirx64rel=..\..\..\..\native\tightdb_c_cs\tightdb_c_cs2010\x64\Release\
set ddirx86deb=..\..\..\bin\x86\debug\
set ddirx86rel=..\..\..\bin\x86\release\
set ddirx64deb=..\..\..\bin\x64\debug\
set ddirx64rel=..\..\..\bin\x64\release\

xcopy %sdirx86deb%tightdb_c_cs201032d.* %ddirx86deb%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx86rel%tightdb_c_cs201032r.* %ddirx86rel%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64deb%tightdb_c_cs201064d.* %ddirx64deb%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64rel%tightdb_c_cs201064r.* %ddirx64rel%tightdb_c_cs.*  /v /y /f /D


