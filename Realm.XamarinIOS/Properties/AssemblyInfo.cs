using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;  // for InternalsVisibleTo

using Foundation;

// This attribute allows you to mark your assemblies as “safe to link”.
// When the attribute is present, the linker—if enabled—will process the assembly
// even if you’re using the “Link SDK assemblies only” option, which is the default for device builds.

[assembly: LinkerSafe]

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle("Realm.XamarinIOS")]
[assembly: AssemblyDescription("Realm is a mobile database: a replacement for Core Data & SQLite")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright © 2016 Realm")]
[assembly: AssemblyCompany("Realm Inc.")]
[assembly: AssemblyProduct("Realm C#")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

[assembly: InternalsVisibleTo("IntegrationTestsXamarinIOS")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("0.74.1.0")]
[assembly: AssemblyFileVersion("0.74.1.0")]
