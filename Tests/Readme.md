# Tests

## Structure

The tests are structured as a multi-targeted project that is then run directly or referenced by a number of test runners. All tests for the SDK
are located in the `Realm.Tests` project. This project produces either an executable for console targets, such as .NET Core and .NET Framework
or a library for the platform-specific test runner to consume.

For iOS, Android, and UWP, we're using a [Xamarin.Forms based test runner](https://github.com/nirinchev/nunit.xamarin) that is forked from
[nunit/nunit.xamarin](https://github.com/nunit/nunit.xamarin) and maintained by us. It has a UI that allows us to drill down the test tree
and execute a single test or a suite of tests.

For Xamarin.Mac, we're using a standalone runner that has no functionality other than automatically running the tests.

For Unity, we're building `Realm.Tests` as a .NET Standard 2.0 application that is referencing Unity's custom NUnit build and we're executing
it either in the Editor or as a standalone player.

## Test Explorer

If you're using Visual Studio for Windows and running the tests with the test explorer, it might be a good idea to manually modify `Realm.Tests.csproj`
and set `<LocalDev>true</LocalDev>`. What this will do is exclude all target frameworks except for `net461`. This dramatically speeds up VS builds as
it forces it to only build for one platform.

## Unity

The Unity tests are somewhat special, not only because they don't use MSBuild and Visual Studio to build and run, but because they need to be run
for two different scripting engines and two different compatibility levels.

### Setup

To setup the unity tests, run the `SetupUnityPackage` project with `tests` argument: `dotnet run --project Tools/SetupUnityPackage/ -- tests`.
What this will do is build `Realm.Tests` with `/p:UnityBuild=true` which will convert the project to a .NET Standard 2.0 library and switch the
NUnit references from the NuGet package to Unity's custom build. Then it'll copy the .dll and the .pdb to `Tests.Unity/Assets/Tests` where they'll
be picked up by the Unity test runner. Finally, it'll download all NuGet dependencies of the test project and drop them in `Tests.Unity/Assets/Dependencies`.

### Running the tests in the editor

When running the tests in the editor, you can use the built-in test explorer to drill down and execute one or more tests.

### Building a standalone test player

If you want to build the player but not run it, you can invoke the editor from the command line like:

```powershell
Start-Process -Wait 'C:\Program Files\Unity\Hub\Editor\2021.2.0a8\Editor\Unity.exe' "-runTests -batchmode -projectPath Tests\Tests.Unity -testPlatform StandaloneWindows64 testSettingsFile .\.TestConfigs\Mono-Net4.json"
```

The important arguments here are:
* `testPlatform` which should be a valid value from the [`BuildTarget` enum](https://docs.unity3d.com/ScriptReference/BuildTarget.html). It allows
you to select the platform for which the executable will be created.
* `testSettingsFile` which allows you to modify player configuration options, such as the scripting backend and the API Profile.
Refer to the [Unity docs](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html#testsettingsfile) for
details on the format and valid config options of the settings file.

Once the editor completes the build, it will exit automatically. The built player should be located in `Tests.Unity/PlayModeTestPlayer`.

### Running a standalone test player

When you run the tests, it's usually valuable to explicitly provide a log file for the output, otherwise the logs will be stored in the default
location. To specify custom log file location, invoke the executable with `-logFile ./myrun.log`. The test player will only output the "run started"
and "run finished" messages at the `Error` level, so individual test runs will not show up in the in-game console, but they'll show up in the log file.
Currently only completed tests are logged, but you can change that if necessary by implementing `TestManager.TestStarted`.