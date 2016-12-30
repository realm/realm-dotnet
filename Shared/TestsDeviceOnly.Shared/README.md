# README for TestsDeviceOnly.Shared

Contains files which logically belong in `Tests.Shared` but will not build when that is 
in turn included in `SharedTests.PCL`.

Include this project directly into your device projects such as 
`Platform.XamarinIOS/Tests.XamarinIOS`.
