# Platform and Framework Compatibility - .NET SDK
## Target Platforms
The following table shows which .NET
versions you can use with the SDK on your target platforms:

|Target Platform|Supported Version(s)|
| --- | --- |
|Debian 8 or later RHEL 7.1 or later Ubuntu 16.04 or later|.NET Core 2.0 or later (.NET Framework 5.0 or later)|
|Windows 8.1 or later|.NET Core 2.0 or later (.NET 5.0 Framework or later) .NET Framework 4.6.1 or later MAUI requires Windows 11 and Windows 10 version 1809 or higher, using Windows UI Library (WinUI) 3.|
|Universal Windows Platform (UWP)|.NET Standard 2.0 or later (Fall Creators Update)|
|macOS|.NET Core 2.0 or later (.NET 5.0 Framework or later) Xamarin.Mac for macOS 10.11 or later. MAUI requires macOS 10.15 or later, using Mac Catalyst.|
|iOS|Xamarin.iOS for iOS 9 or later. MAUI requires iOS 11 or later.|
|Android|Xamarin.Android for Android 4.1 (API level 16) or later. MAUI requires Android 5.0 (API level 21) or later.|
|tvOS|Xamarin and Unity for tvOS 9.0 or later. For more information on developing for tvOS, see Build for tvOS .|

> **NOTE:**
> The source generator models in .NET SDK v10.18.0 and later
> require the following:
>
> - .NET Core 2.0 or later (.NET Framework 5.0 or later)
> - C# 9.0 or later
>
> If you are targeting an older version of .NET Framework, your object
> models must derive from the `RealmObject`, `EmbeddedObject`, or
> `AsymmetricObject` base classes required by the old source
> generator.
>
> The following demonstrates how you can adjust your current object models for compatibility with older .NET Frameworks:
>
> ```csharp
> public partial class Person : IRealmObject // Current model
>
> public class Person : RealmObject // Adjusted to inherit from RealmObject
> ```
>
> For more information, refer to Object Models - .NET SDK.
>

## Development Environments
You can use the following development environments to build apps with
the .NET SDK:

- Visual Studio 2015 Update 2 or higher for Windows
- Visual Studio for Mac 7.0 or higher
- Unity [2020.3.12f1 (LTS)](https://unity3d.com/get-unity/download/archive)

> **NOTE:**
> The .NET SDK may be compatible with
> other versions of Unity, but `2020.3.12f1 (LTS)` is the version the
> SDK team uses for testing and development. We recommend using
> this version to ensure your project works with the .NET SDK and that
> the install steps match the Integrate Realm with Unity documentation steps because Unity's UI
> often changes between versions.
>

## Android Deployment
Due to some instruction set limitations, the SDK does not support
deploying Android apps to the `armeabi` ABI. Because default templates often
have different ABI settings for Debug and Release modes, your app may throw
a `System.TypeInitializationException` exception in Release mode but not when
it was running in Debug mode.

To avoid this, verify the ABI settings for both Debug and Release modes. To check
and change the settings, follow the steps in the
[Visual Studio CPU Architectures](https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/cpu-architectures)
page.

Unless you have a good reason to avoid linking other ABIs, we recommend
checking all of the settings other than `armeabi`.

## Limitations
The SDK has limits imposed to balance flexibility with performance. The SDK
throws an exception during app initialization if the following limits are
exceeded:

- Class names can't exceed 57 bytes in length.
- Property names can't exceed 63 bytes in length.

In addition, for iOS apps, the total size of all open Realm files cannot be
larger than the amount of memory your application is allowed to map in iOS. This
varies per device, and depends on how fragmented the memory space on the device is.
If you need to store more data than is allowed, you can split your data into
multiple Realm files, open a realm only when needed, and close it when it is
no longer needed.

For more information, see [Open Radar 17119975](http://www.openradar.me/17119975).
