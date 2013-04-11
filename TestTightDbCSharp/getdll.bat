@echo off
:  Copy the c++ DLLs to the project dir for each solution platform
:  This enables easy debugging against the very latest build of c++ tightdb_c_cs2010 dll
:  The C# dll should be copied by the VS2012 project on build, as it is referenced as a C# assembly
:  The C++ dll cannot be referenced,so is not copied automatically
:  This script copies all 4 versions of the dll each time a build is made, 
:  so a batch build will run the script 4 times. However, the copy command will
:  not copy the file if the source is the same as the destination (measured by creation date)
:  note that the C# will dynamically load the needed bitness of the dll from .\x64 or .\x32 so we put the relevant dll there when
:  creating an x64 or x86 assembly, and we put both dll's there if it is an AnyCpu release
:  this deployment scheme will be simplified later on. It was built before we implemented support for dynamic loading of the correct c++ DLL
:  current setup is useful for testing all combinations of c++ dll and c# assembly bitness

set projdir=%1
echo Copying new c++ dlls to tightdb_c_cs
set sdirx86deb=%projdir%..\native\tightdb_c_cs\tightdb_c_cs2010\Win32\Debug\
set sdirx86rel=%projdir%..\native\tightdb_c_cs\tightdb_c_cs2010\Win32\Release\
set sdirx64deb=%projdir%..\native\tightdb_c_cs\tightdb_c_cs2010\x64\Debug\
set sdirx64rel=%projdir%..\native\tightdb_c_cs\tightdb_c_cs2010\x64\Release\
set ddirx86deb=%projdir%\bin\x86\debug\
set ddirx86rel=%projdir%\bin\x86\release\
set ddirx64deb=%projdir%\bin\x64\debug\
set ddirx64rel=%projdir%\bin\x64\release\
set ddiranydeb=%projdir%\bin\debug\
set ddiranyrel=%projdir%\bin\release\


xcopy %sdirx86deb%tightdb_c_cs201032d.* %ddirx86deb%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx86rel%tightdb_c_cs201032r.* %ddirx86rel%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64deb%tightdb_c_cs201064d.* %ddirx64deb%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64rel%tightdb_c_cs201064r.* %ddirx64rel%tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64deb%tightdb_c_cs201064d.* %ddiranydeb%\x64\tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64rel%tightdb_c_cs201064r.* %ddiranyrel%\x64\tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx86deb%tightdb_c_cs201032d.* %ddiranydeb%\x86\tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx86rel%tightdb_c_cs201032r.* %ddiranyrel%\x86\tightdb_c_cs.*  /v /y /f /D
:assume that we run 32 bit on AnyCpu
:xcopy %sdirx86deb%tightdb_c_cs201032d.* %ddiranydeb%\tightdb_c_cs.*  /v /y /f /D
:xcopy %sdirx86rel%tightdb_c_cs201032r.* %ddiranyrel%\tightdb_c_cs.*  /v /y /f /D

:assume that we run 64 bit on AnyCpu
xcopy %sdirx64deb%tightdb_c_cs201064d.* %ddiranydeb%\tightdb_c_cs.*  /v /y /f /D
xcopy %sdirx64rel%tightdb_c_cs201064r.* %ddiranyrel%\tightdb_c_cs.*  /v /y /f /D
