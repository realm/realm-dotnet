This is a binary relase of the TightDb Engine, for Microsoft Visual Studio.

to use tightdb in Visual Studio C++ solutions, You will have to reference the LIB 
and header files enclosed in this archive.
Furthermore any compiler settings that change ABI layout, must match those used 
to build tightDb core.

In the examples directory you will find an empty C++ solution with a console project 
that uses tightdb. You can use that solution as a starting point,
or adapt Your own project settings, using the example project settings as a guide.

It is critically important that You reference debug libraries in debug builds, and
release binaries in release builds, and that the 64/32 bitness matches up correctly.

Please note that the free visual studio c++ express 2010 does not support 
64 bit builds, and that the example solution includes 64 bit support. 
You will not be able to get the sample solution to work using the free visual studio
express c++ 2010 You will need the commercial Visual studio professional 2010 or 
the free visual studio 2012 express.

(Please also note that the example solution has not yet been created, so it is not
distributed in the current pre-release windows release)