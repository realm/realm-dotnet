Abstract tests
==========

This project is a PCL that contains a number of abstract test classes. They cannot be run directly 
because they are integration tests, meant to be run with an instance of core. Thus, the specialized 
projects like IntegrationTests.Win32 provide instances of these classes, and that is where you
can actually run the tests. 

This has the unfortunate implication that you cannot click on a single test in the IDE and run it,
but this is the best solution we have found so far.
