Benchmarkr
==========
There are Lies, Damn Lies and Benchmarks - this an attempt of the latter.

Benchmarks are inherently flawed and can basically express anything you want, so this is no attempt at giving a comprehensive comparison of the performance of Realm or other solutions. It's just a quick reality check on a very limited number of operations to give us some indicators that we are not messing up the great raw performance of Realm underlying database engine and that it seems likely that we are not radically slower than other solutions.

Specifically, it compares a few simple operations for Realm, SQLite, and Couchbase:
* Insert 150k objects in one transaction
* Query 450k objects and count the 100k matches
* Query 450k objects and enumerate all 100k matching objects

How to run
----------
Run it and press the Start button to see benchmarks appear after a possibly long number of seconds.
It's been observed that Couchbase might not be able to complete this test on some devices.
In that case it's advised to reduce the number of objects used.

Interpret the results
----------------------
The UI is world class awful and just lists the number of milliseconds each operation took, so smaller is better.

Different Builds
----------------
The original `Benchmarkr.sln` builds using the public NuGet versions of Realm and RealmWeaver.

The `BenchmarkrLocalRealm.sln` builds using an **adjacent** set of Realm projects, expecting the `realm-dotnet` working directory to be sibling. This makes it possible to benchmark current versions of Realm without having to build the PCL.

Note that the separate projects for `BenchmarkrLocalRealm.sln` reuse all the source from the original but are in parallel dirs so that NuGet can have individual `packages.config` files which exclude the `Realm` and `RealmWeaver` packages.

