// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("NUnit Software")]
[assembly: AssemblyProduct("NUnit 3 Xamarin Runner")]
[assembly: AssemblyCopyright("Copyright (C) 2017 Charlie Poole")]
[assembly: AssemblyTrademark("NUnit is a trademark of NUnit Software")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("3.6.1")]
[assembly: AssemblyFileVersion("3.6.1.0")]

[assembly: ComVisible(false)]


#if DEBUG
#if __IOS__
[assembly: AssemblyConfiguration("iOS Debug")]
#elif __DROID__
[assembly: AssemblyConfiguration("Android Debug")]
#elif WINDOWS_UWP
[assembly: AssemblyConfiguration("Universal Windows Debug")]
#else
[assembly: AssemblyConfiguration("Debug")]
#endif
#else
#if __IOS__
[assembly: AssemblyConfiguration("iOS")]
#elif __DROID__
[assembly: AssemblyConfiguration("Android")]
#elif WINDOWS_UWP
[assembly: AssemblyConfiguration("Universal Windows")]
#else
[assembly: AssemblyConfiguration("")]
#endif
#endif