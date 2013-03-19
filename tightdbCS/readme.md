#tightdbCS#

This directory contains the VS2012 project that results in the C# language bindings DLL that users reference.

The DLL consists of two parts

- ..\native\tightDBCalls.cs - p/invoke calls to tightCSDLL.dll.
- .\ - The classes that make up the language binding.

Only files in the native directory contain calls to c++ code.
Files in this directory are pure clean C#, The only nonstandard stuff is the handle inside the classes that is used to keep track of what c++ class instances they are shadowing.
