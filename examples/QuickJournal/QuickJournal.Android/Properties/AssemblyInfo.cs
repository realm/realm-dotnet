using System.Reflection;
using System.Runtime.InteropServices;
using Android.App;

[assembly: AssemblyTitle("QuickJournal.Android")]
[assembly: AssemblyDescription("A trivial sample that demonstrates how to use the Realm database.")]
[assembly: AssemblyCompany("Realm")]
[assembly: AssemblyProduct("QuickJournal.Android")]
[assembly: AssemblyCopyright("Copyright © Realm 2020")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
