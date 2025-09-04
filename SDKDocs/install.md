# Install the .NET SDK
You can use the Realm SDK for .NET to develop apps in C# .NET with several frameworks, including
[.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui),
[Xamarin](https://dotnet.microsoft.com/apps/xamarin),
[Avalonia UI](https://avaloniaui.net/),
[UWP](https://docs.microsoft.com/en-us/windows/uwp/get-started/),
[Unity](https://unity.com/), and others.

For more information about specific version support for .NET, .NET MAUI, UWP, and
Xamarin, see Platform and Framework Compatibility.

> **NOTE:**
> Integrating the .NET SDK with Unity has different prerequisites and
> install steps than the ones below. Learn how to Integrate the SDK
> with Unity.
>

## Prerequisites
Before getting started, ensure you have installed [Visual Studio](https://visualstudio.microsoft.com/downloads/) (2015 Update 2 or later)

## Installation
> **TIP:**
> The SDK uses Realm Core database for device data persistence. When
> you install the .NET SDK, the package names reflect Realm naming.
>

Follow these steps to add the .NET SDK to your project.

> **IMPORTANT:**
> If you have a multi-platform solution, be sure to install the SDK
> for *all of the platform projects*, even if the given project
> doesn't contain any SDK-specific code.
>

### Mac

#### Open the NuGet Package Manager
In the Solution Explorer, right-click your solution and select
**Manage NuGet Packages...** to open the NuGet
Package management window.

> **NOTE:**
> Adding the package at the Solution level allows you to add it to
> every project in one step.
>

#### Add the Realm Package
In the search bar, search for **Realm**. Select the
result and click Add Package.

If you are using Xamarin, you may be prompted to select which projects
use the Realm package.

Select all of the projects, and then click Ok.

### Windows

#### Open the NuGet Package Manager
In the Solution Explorer, right-click your solution and
select **Manage NuGet Packages for Solution...**
to open the NuGet Package management window.

#### Add the Realm Package
In the search bar, search for **Realm**. Select the
result and click Install.

When prompted, select all projects and click Ok.

#### Add the Realm Weaver to FodyWeavers.xml
> **NOTE:**
> You can skip this step if you were *not* already using [Fody](https://github.com/Fody/Fody) in your project. Visual Studio will generate a properly-configured `FodyWeavers.xml` file for you when you first build.
>

If your project was already using [Fody](https://github.com/Fody/Fody), you must manually add the
Realm weaver to your `FodyWeavers.xml` file.

When done, your `FodyWeavers.xml` file should look similar to:

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
