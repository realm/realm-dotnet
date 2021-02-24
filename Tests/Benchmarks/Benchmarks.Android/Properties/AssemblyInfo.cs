using System.Reflection;
using System.Runtime.InteropServices;
using Android.App;

[assembly: AssemblyTitle("Benchmarks.Android")]
[assembly: AssemblyDescription("Entry point for the Xamarin.Forms app used to run Benchmarks for Realm")]
[assembly: AssemblyCompany("Realm Inc.")]
[assembly: AssemblyProduct("Benchmarks.Android")]
[assembly: AssemblyCopyright("Copyright © 2021")]
[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
