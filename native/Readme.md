#native#

This directory contains C# code that interacts with the tightCSDLL The C# code is not meant to be called by the user of the c# binding, but by the C# binding itself. All P/Invoke and marshalling stuff has been put in files in this directory, these files are likely the only ones that have to change materially between various versions of the C# binding.

Versions :

- 64bit windows - microsoft .net (to be done)
- 32bit windows - microsoft .net (in progress)
- 64bit windows - mono (to be done)
- 32bit windows - mono (to be done)
- 64bit linux (possibly in several flavors) (to be done)
- 32bit linux (possibly in several flavors) (to be done)


C# binding source stack :

1. *.NET platform agnostic* User program (written in C#, but in principle any .NET language will work)
2. *C# Generic* C# bindings (written in pure C#, code is platform agnostic)
3. *C# platform dependent* C# bindings (written in C# but marshals data to/from the c++ DLL, and manages platform differences, error handling etc. Also manages C# to C++ marshalling etc)
4. *C++ platform dependent* c++ DLL for C# bindings (written in c++, provides methods specifically for 3. to call, calls on into tightdb, provides error handling , marshalling on the c++ side, etc.)
5. *C++ platform specific* .lib files containing tightdb (built on the platform by tightdb)


Physical C# binding stack :

1. *C#* user program written in C# or another .net language
2. *C#* tightdbCSHARP.dll contains 2. and 3. from above. 3. is private, shielded from the user
3. *C++* tightCSDLL.dll contains 4. and 5. built into a DLL, with extern C exported methods

Thus, the only expensive calls are the ones between 2. and 3. in the physical stack (these ar P/Invoke calls)