#Directory : LibsVS2012#

This directory is used when building the c++ dll used by the windows version of the C# bindings

As VS2010 will only build using VS2010 built libs, and VS2012 will only build using VS2012 build libs - this directory exists in two versions, called LIBSVS2010 and LIBSVS2012

###WINDOWS###

Get a windows-release. (1)
Unzil the .zip file here. It will result in the following:

- a directory called src
- tightdb32.lib
- tightdb64.lib
- tightdb32d.lib
- tightdb64d.lib

The lib files are referenced from the tightCSDLL solution. 
The tightCSDLL solution creates the tighCSDLL.DLL file, which is uesd as a P/invoke bridge between C# and C++
The header files inside src are also needed to build the dll, and the header files need to be exactly the header files used to build the lib files,
therefore they all come in a .zip file release to ensure everything matches up.


(1) To create a release:
The release can be done this way :

- in the main tightdb repository, build lib files by building the 4 lib file projects in a batch build.
- after successfull builds copy the src directory down into the lib directoyry
- and then zip the entire lib directory to a zip file.



