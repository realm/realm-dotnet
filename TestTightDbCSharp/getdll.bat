@echo off
:  Copy the c++ DLLs to the project dir for each solution platform
:  This enables easy debugging against the very latest build of c++ tightdb_c_cs2010 dll
:  The C# dll should be copied by the VS2012 project on build, as it is referenced as a C# assembly
:  The C++ dll cannot be referenced,so is not copied automatically
:  This script copies all 4 versions of the dll each time a build is made, 
:  so a batch build will run the script 4 times. However, the copy command will
:  not copy the file if the source is the same as the destination (measured by creation date)
:  The c++ DLL that is loaded in the end will be either tightdb_c_cs64.dll or tightdb_c_cs32.dll
:  Which of the two is determined by UintPtr.Size
:  So in each directory where the program is deployed, both tightdb_c_cs64.dll and tightdb_c_cs32.dll should
:  be found (if the user program is AnyCpu).

set projdir=%1
set dllname=tightdb_c_cs2012
echo Copying new c dlls to project bin directory

set sdirx86deb=%projdir%..\native\tightdb_c_cs\%dllname%\Win32\Debug\
set sdirx86rel=%projdir%..\native\tightdb_c_cs\%dllname%\Win32\Release\
set sdirx64deb=%projdir%..\native\tightdb_c_cs\%dllname%\x64\Debug\
set sdirx64rel=%projdir%..\native\tightdb_c_cs\%dllname%\x64\Release\



set ddirx86deb=%projdir%\bin\x86\debug\
set ddirx86rel=%projdir%\bin\x86\release\
set ddirx64deb=%projdir%\bin\x64\debug\
set ddirx64rel=%projdir%\bin\x64\release\
set ddiranydeb=%projdir%\bin\debug\
set ddiranyrel=%projdir%\bin\release\



xcopy %sdirx86deb%%dllname%32d.* %ddirx86deb% /v /y /f /D
xcopy %sdirx86deb%%dllname%32d.* %ddiranydeb%  /v /y /f /D

xcopy %sdirx86rel%%dllname%32r.* %ddirx86rel%  /v /y /f /D
xcopy %sdirx86rel%%dllname%32r.* %ddiranyrel%  /v /y /f /D

xcopy %sdirx64rel%%dllname%64r.* %ddirx64rel%  /v /y /f /D
xcopy %sdirx64rel%%dllname%64r.* %ddiranyrel% /v /y /f /D

xcopy %sdirx64deb%%dllname%64d.* %ddiranydeb%  /v /y /f /D
xcopy %sdirx64deb%%dllname%64d.* %ddirx64deb%  /v /y /f /D
