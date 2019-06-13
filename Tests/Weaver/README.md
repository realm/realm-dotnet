About WeaverTests
=================

The WeaverTests are not included in the standard runner apps, unlike most of the adjacent tests.

Instead, they are run within a unit test runner, in Xamarin Studio just with 
Run - Run Unit Tests.

They are also run as part of our regular CI.

The tests run the Weaver _"in-process"_ and provide a **simulated** environment for the weaver.

Thus, although we attempt to provide coverage with them, they are not a substitute for
ensuring you can weave with a live app.

Running the normal unit tests on a device or simulator will go through the weaving process
for the classes declared. That may miss some edge cases.

eg: the normal unit tests, by definition, are always building an environment where 
there are **some** RealmObject classes declared.