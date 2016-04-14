NuGetReleaseTests
=============

The separate solution `NuGetReleaseTests.sln` reuses our shared test code normally run
within the main `Realm.sln` but pulls in Realm via our NuGet packages. 

This means we can easily run the unit test suite again over the release version of the PCL
to ensure no problems would present themselves to users which got past other builds of tests.

Note as a side-effect of using NuGet to add Realm, it adds the`Realm.PCL` project to the solution, even though we're not using a PCL anywhere.

