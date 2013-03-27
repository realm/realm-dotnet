¤TestPinvoke#

Not used as part of a build. This prject is a test project used to test various platform and c#/c++ marshalling issues.
The project is a simple commandline project that is used to debug calls from c# to the c++ dll we build up.
the dll contains specific test methods for this test program.
The idea is that running this program will not in any way touch tightdb code in the dll so we can test the c#/c++ interop stuff here,
and if this breaks on some platform, we know it has nothing to do with tightdb.
Why is this neccesary?  Because interop and marshalling behave differently with 32 and 64 bits, and with different compilers, and with different
platforms. It's quite easy to write an interop marshal that works on some combinations but not on otheres.
platforms to consider : C# side  :

C# stack (TightDBCSHARP) could be :

- 32 bit mono on windows
- 32 bit mono on linux
- 32 bit mono on mac is underway
- 32 bit windows.net on windows
- 64 bit mono on windows
- 64 bit mono on linux
- 64 bit mono on mac is underway
- 64 bit windows.net on windows

.net version running could be one of

http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions 

+LOTS+ of versions, I suggest we concentrate on :

- 4.5 RTM (released 2012-08-02)
- 4.0 (released 2010-04-12)

If we see customer demand, we might consider going back to 3.5, or further back

The tightCSDLL.dll (and the .lib files built into it) can be 

- 32bit VS2010
- 32bit VS2012
- 64bit VS2010
- 64bit VS2012
- various Mono builds (platforms and bit size) (I don't know Yet what compiler would've made those dll's)



(far from every combo of above groups have been tested as of now)




references of interest reg. c# and c++ interop with p/invoke :

the leveldb interoplangiage bind for c# https://github.com/meebey/leveldb-sharp/blob/master/Native.cs
the leveldb interop language bind for c#, c++ part :  https://code.google.com/p/leveldb/source/browse/db/c.cc

This language bind shows by example how a multi-platform bind can be made. They manage exceptions and errors by returning a string that is null if stuff went okay, or the errror message if an exception or error condition had been raised on the c++ side.
This can then be handled generally on the c# side, by acting on the value of the string. In a no-error situation the overhead is not very large (just a check for null pointer value or empty string)

the dumpbin tool.  start button->visual studio2012->visual studio tools->developer command prompt
in this command prompt, cd to your dll, then type dumpbin MYDLL.DLL /exports - this will list the method signatures found in that dll.

http://clrinterop.codeplex.com/releases/view/14120
a super neat program that will give you a c# code snippet that matches a extern C code snippet You paste in. works with structs and functions.
Of course the  suggested c# code is not neccesarily the smartest or most portable, but it's a good starting point.


http://www.mono-project.com/Interop_with_Native_Libraries
A good overview of several platform issues and fixes from the mono project. argumentation for why we have to use extern C ABI

http://clrinterop.codeplex.com/
msft website with several tools and tips

introductionary overview, list of useful tools
http://blogs.microsoft.co.il/blogs/sasha/archive/2012/01/24/managed-unmanaged-interoperability-in-the-2010s-part-1-p-invoke-managed-to-unmanaged.aspx

msdn article on pinvoke from c# (i find it pretty confusing)
http://msdn.microsoft.com/da-dk/magazine/cc164193(en-us).aspx

...more to come
