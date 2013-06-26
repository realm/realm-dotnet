#Directory : LibsVS2010#

(to be done, the issue is with the c++ part. The C# part ought to work, but we may need to do a 
special compile, targetting th earlier .net version shipped with vs2010)

This directory is used when building the windows version of the C# bindings for use with VS2010

As VS2010 will only build using VS2010 built libs, and VS2012 will only build using VS2012 build libs - this directory exists in two versions, called LIBSVS2010 and LIBSVS2012

###WINDOWS###

Put the VS2010 built .lib files with tightDB in here, namely:

- tightdb32.lib
- tightdb64.lib
- tightdb32d.lib
- tightdb64d.lib

They are referenced from the tightCSDLL solution. The tightCSDLL solution creates the tighCSDLL.DLL file, which is uesd as a P/invoke bridge between C# and C++

The .lib files can only be opbtained from a tightdb employee with access to the windows build of tightdb - we don't have them for download anywhere.
