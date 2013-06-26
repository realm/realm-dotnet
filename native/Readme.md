#native#

This folder and its subfolders contains sourcecode to build the extern "C" dll that the c# language binding uses to access tightdb.
It also contains the C# soucecode that directly interacts with the extern "C" dll

All P/Invoke and marshalling stuff has been put in files in this directory, these files are likely the only ones that have to change materially between various hardware platforms.
All the higher level supporting C# code is in the folder above this one. The higher level supporting C# code might have to change between various .net platforms or .net implementations, so
it is a benefit that the two sets of source code has been split up.

Some CLI platforms (windows phone 7.5 for instance, windows phone 8.0 is sort of halfway there) do not support P/Invoke yet, in case we need to support such a platform before p/invoke gets ported to it, we will have to rely on c++/CLI,
which is currently a microsoft only way of calling into a c++ program. In case we need it, we will have to create a c++/CLI wrapper, in ways similar to tight_c_cs.dll and tightdbcalls.cs that wraps the c++ classes into CLR classes


Versions :

c++ DLL  tight_c_cs.dll

* 32 bit windows release
* 32 bit windows debug
* 64 bit windows release
* 64 bit windows debug
* nn bit linux mono (to be done)   (probably not built in visual studio)
* nn bit windows mono (to be done) 
* nn bit ios mono (to be done)     (probably not built in visual studio)


C# UnsafeNativeMethods.cs (will be linked into the C# binding)

* C# 5.0 / .net 4.5 32 bit release (Using P/Invoke)
* C# 5.0 / .net 4.5 32 bit debug   (Using P/Invoke)
* C# 5.0 / .net 4.5 64 bit release (Using P/Invoke) 
* C# 5.0 / .net 4.5 64 bit debug   (Using P/Invoke) 
* ..Versions tested with other CLI platforms (mono, windowsRT, windows phone, silverlight) to be done



c++/CLI tight_cpp_CLI.dll

windows (to be done)



C# binding source stack :

1. *.NET platform agnostic* User program (written in C#, but in principle any .NET language will work)
2. *C# Generic* C# bindings (written in pure C#, code is platform agnostic)
3. *C# platform dependent* C# bindings (written in C# but marshals data to/from the c++ DLL, and manages platform differences, error handling etc. Also manages C# to C++ marshalling etc)
4. *C++ platform dependent* c++ DLL for C# bindings (written in c++, provides methods specifically for 3. to call, calls on into tightdb, provides error handling , marshalling on the c++ side, etc.)
5. *C++ platform specific* .lib files containing tightdb (core tightdb built in-house)


Physical C# binding stack :

1. *C#* user program written in C# or another .net language
2. *C#* TightDbCSharp.dll contains 2. and 3. from above. 3. is private, shielded from the user
3. *C++* tightdb_c_csxxxxyyz.dll contains 4. and 5. built into a DLL, with extern C exported methods

xxxx = VS veresion, 2010 or 2012
yy = bitness , 32 or 64
z  = debug or release, d or r
