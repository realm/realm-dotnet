@echo off
:  Copy the c++ DLL to the project dir
:  This enables easy debugging against the very latest build of c++ tightCSDLL
:  The C# dll should be copied by the VS2012 project on build, as it is referenced as a C# assembly
:  The C++ dll cannot be referenced,so is not copied automatically
echo Copying c++ dll to tightCSDLL.DLL
: To avoid a file or directory prompt if the target file is not there, we create a file
echo . > bin\debug\tightCSDLL.dll
echo . > bin\debug\tightCSDLL.pdb
xcopy ..\native\tightCSDLL2010\debug\tightCSDLL2010.dll bin\debug\tightCSDLL.dll  /v /y /f
xcopy ..\native\tightCSDLL2010\debug\tightCSDLL2010.pdb bin\debug\tightCSDLL.pdb  /v /y /f

echo Copying c# dll to tightdbCS.dll
: To avoid a file or directory prompt if the target file is not there, we create a file
echo . > bin\debug\tightdbCS.dll
echo . > bin\debug\tightdbCS.pdb
xcopy ..\tightdbCS\bin\debug\tightdbCS.dll bin\debug\tightdbCS.dll  /v /y /f
xcopy ..\tightdbCS\bin\debug\tightdbCS.dll bin\debug\tightdbCS.pdb /v /y /f



echo finished!
pause