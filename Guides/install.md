# Install the .NET SDK

You can use the Realm SDK for .NET to develop apps in C# with [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui) targeting Android, iOS, macOS (Mac Catalyst), and Windows. The SDK also supports [Unity](https://unity.com/) for game development.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.107 or later)
- .NET MAUI workload:
  ```
  dotnet workload install maui
  ```
- Platform-specific tooling:
  - **iOS / macOS:** Xcode (latest stable, Mac required)
  - **Android:** Android SDK (installed via Visual Studio or Android Studio)
  - **Windows:** Windows App SDK (installed automatically with the MAUI workload)

> **NOTE:**
> Integrating the .NET SDK with Unity has different prerequisites and install steps. See the Unity guide for details.

## Installation

Follow these steps to add the .NET SDK to your MAUI project.

> **IMPORTANT:**
> Install the `Realm` NuGet package for the shared project of your MAUI solution. The single-project MAUI model handles all platforms from one project file.

### Via .NET CLI

```
dotnet add package Realm
```

### Via NuGet Package Manager (Visual Studio)

1. In Solution Explorer, right-click your project and select **Manage NuGet Packages…**
2. Search for **Realm** and click **Install**.

### Add the Realm Weaver to FodyWeavers.xml

> **NOTE:**
> You can skip this step if you were *not* already using [Fody](https://github.com/Fody/Fody) in your project. Visual Studio will generate a properly-configured `FodyWeavers.xml` file for you when you first build.

If your project was already using [Fody](https://github.com/Fody/Fody), add the Realm weaver manually:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Realm />
</Weavers>
```

## Import the SDK

Add the following line to the top of your source files to use the SDK:

```csharp
using Realms;
```
