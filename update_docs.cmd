@echo off
:currently nothing to do as we now have indirection via doc\tutorial.path
:echo this batch file updates the doc directory with new versions of tutorial and other sorucefiles
:echo this could've been done with symbolic links but there is a windows/linux/github/gitex issue
:pause
:set location=%~dp0
:copy %location%examples\TutorialSolution\Tutorial.cs %location%doc\tutorial\tutorial.cs
:note! the one in doc is in lowercase!