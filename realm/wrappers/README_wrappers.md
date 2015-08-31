About Wrappers
==============

Wrappers is how we map the C# world to the Core and has to be compiled natively for each platform.

The original _proof of concept_ was just in two files `wrapper.h` and `wrappers.cpp`.

For migration of build scripts, `wrappers.cpp` has been retained and includes all other files.

internal dir
-------------
To make the code more familiar to people comparing it to Java, we have an `internal` dir which corresponds to `realm-java/realm/src/main/java/io/realm/internal`

See the [sheet describing mappings](https://docs.google.com/a/tightdb.com/spreadsheets/d/1nIcG7SQMrfcN5YE2xcKm3Oy6wubqOENvtWTfhhwk4GA/edit?usp=sharing) which explains how files map across and individual progress to this aim.
