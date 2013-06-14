#Directory : LibsVS2012#

This is a marker readme do not delete it, or this directory will disappear

This directory is used when building the windows version of the C# bindings  

As VS2010 will only build using VS2010 built libs, and VS2012 will only build using VS2012 build libs - this directory exists in two versions, called LIBSVS2010 and LIBSVS2012

You should manually put in this directory the c++ binding build.

The files are :

src\
- tightdb32.lib
- tightdb64.lib
- tightdb32d.lib
- tightdb64d.lib

These files are usually to be found in a .rar file, created by whoever built the libraries
