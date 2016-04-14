PCLbasedTests
=============

The separate solution `PCLbasedTests.sln` reuses our shared test code normally run
within the main `Realm.sln` but pulls in Realm via  PCL. 

This means we can easily run the unit test suite again over the release version of the PCL
to ensure no problems would present themselves to users which got past other builds of tests.

