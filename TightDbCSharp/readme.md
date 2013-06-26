#TightDbCSharp#

This directory contains the VS2012 Ultimate project that results in the C# language bindings assembly
it also contains the build.bat file that must be run after VS2012 has built the project. build.bat will assemble a release in the release directory, ready for deployment.

to build :

1) Open VS2012

2) select Build->Batch Build

3) mark both configurations

4) hit rebuild

5) vie solution explorer, right click TightDbCsharp, selct Open in File Explorer

6) in the file explorer window that popped up, double click build.bat

7) answer All if asked, and Yes if asked and All is not an option

8) Your release is ready for deployment in the release directory. The zip file in release\release is all You need to ship, the directory structure in release\files is just the uncompressed contents of the zip file.

