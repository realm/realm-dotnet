# Realm SDK for .NET
Use the Realm SDK for .NET to develop cross-platform mobile and
desktop apps with C# and MAUI.

## Get Started with the .NET SDK
These docs provide minimal-explanation code examples of how to work
with the Realm SDK for .NET.
Use the SDK's open-source database - Realm - as an object store on the
device.

> **TIP:** See the [Quick Start](quick-start.md) to integrate
> Realm into your app and get started.
>

You can use the Realm SDK for .NET to develop apps in C# .NET with several frameworks, including
[.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui),
[Xamarin](https://dotnet.microsoft.com/apps/xamarin),
[Avalonia UI](https://avaloniaui.net/),
[UWP](https://docs.microsoft.com/en-us/windows/uwp/get-started/),
[Unity](https://unity.com/), and others.

### Install the .NET SDK
To get started, use NuGet to install the .NET SDK in your solution.
Then, import the SDK in your source files to get started.

### Define an Object Schema
Use C# to idiomatically define an object schema.

### Configure & Open a Database
You can configure your database to do things like populate initial
data on load, use an encryption key to secure data, and more.

To begin working with your data, configure and open a database.

### Read and Write Data
You can:
- Create, read, update, and delete objects from the database on the device.
- Construct complex queries to filter data using idiomatic LINQ Syntax
  or the database's Realm Query Language (RQL).

### React to Changes
Live objects mean that your data is always up-to-date.

Register a change listener to react to changes and perform logic like updating your UI.

## Recommended Reading
### .NET API Reference
Generate reference docs for the SDK's .NET APIs from the [Docs/](/../Docs/Readme.md) directory.

### Integrate the SDK with Unity
Find out how to [integrate the SDK with your Unity project](unity.md).

### Example Projects
Explore engineering and expert-provided example projects to learn best
practices and common development patterns using the .NET SDK:

- [Realm .NET Samples](https://github.com/realm/realm-dotnet-samples) GitHub repo