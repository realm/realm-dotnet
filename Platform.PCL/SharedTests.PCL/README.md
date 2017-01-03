# README for SharedTests.PCL

Project to wrap our unit tests which are in shared projects `Shared/Tests.Shared` 
and `Shared/Tests.Sync.Shared`.

To ensure full testing of code built as PCL occurs, simply pulling those unit test suites
directly in here gives the best coverage.

This PCL in turn is used in specific device builds such as `Platform.XamarinIOS/TestsInPCL.XamarinIOS`.
