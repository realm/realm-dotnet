NuGetReleaseTests
=============

The separate solution `NuGetReleaseTests.sln` reuses our shared test code normally run
within the main `Realm.sln` but pulls in Realm via our NuGet packages. 

This means we can easily run the unit test suite again over the release version of the PCL
to ensure no problems would present themselves to users which got past other builds of tests.

Note as a side-effect of using NuGet to add Realm, it adds the`Realm.PCL` project to the solution, even though we're not using a PCL anywhere.

Using this Solution
-------------------
The solution as bundled in our source repo does **not** include Realm.

You should:
1. Duplicate the entire folder `NuGetReleaseTests`
2. In the duplicated  `NuGetReleaseTests.sln` use NuGet to add the Realm package to both the IOS and Android projects.
3. Build.

This ensures we replicate the experience of starting out with NuGet to add packages.

