@echo off
:  This batch file copies the c++ dll from tightCSDLL to this projects debug dir
:  This enables easy debugging against the very latest build of c++ tightCSDLL
echo Copying c++ dll to tightCSDLL.DLL
: To avoid a file or directory prompt if the target file is not there, we create a file
echo . > bin\debug\tightCSDLL.dll
echo . > bin\debug\tightCSDLL.pdb
xcopy ..\native\tightCSDLL2010\debug\tightCSDLL2010.dll bin\debug\tightCSDLL.dll  /v /y /f
xcopy ..\native\tightCSDLL2010\debug\tightCSDLL2010.dll bin\debug\tightCSDLL.pdb  /v /y /f
echo finished!
pause